using System.Diagnostics;
using System.Threading.Tasks;

namespace WalkerSim
{
    internal partial class Simulation
    {
        private float averageTickTime = 0f;
        private float lastTickTimeMs = 0f;

        private Stopwatch tickWatch = new Stopwatch();

        // Singleton instances to avoid GCing.
        private FixedBufferList<Agent> _nearby = new FixedBufferList<Agent>(Limits.MaxQuerySize);

        // If this is true then the simulation will be deterministic given it has equal configurations.
        public bool Deterministic = false;

        // If DeterministicLoop is set to true this will be the amount of how many agents it will update per tick.
        private const uint MaxUpdateCountPerTick = 2000;


        private void UpdateAgentLogic(Agent agent)
        {
            if (agent.CurrentState == Agent.State.Wandering)
            {
                UpdateAgent(agent);
            }
            else if (agent.CurrentState == Agent.State.Respawning)
            {
                RespawnAgent(agent);
            }

        }

        public void Tick()
        {
            tickWatch.Restart();

            lock (_state)
            {
                UpdateWindDirection();
                UpdateEvents();
                UpatePOICounter();

                var agents = _state.Agents;
                var maxUpdates = MaxUpdateCountPerTick;

                if (EditorMode)
                {
                    // Update in parallel.
                    Parallel.For(0, maxUpdates, i =>
                    {
                        var index = (int)((_state.SlowIterator + (uint)i) % agents.Count);
                        var agent = _state.Agents[index];
                        UpdateAgentLogic(agent);
                    });
                }
                else
                {
                    // Update single threaded.
                    for (int i = 0; i < maxUpdates; i++)
                    {
                        var index = (int)((_state.SlowIterator + (uint)i) % agents.Count);
                        var agent = agents[index];
                        UpdateAgentLogic(agent);
                    }
                }

                _state.SlowIterator += maxUpdates;

                // Update the grid, can't do this in parallel since it's not thread safe.
                for (int i = 0; i < agents.Count; i++)
                {
                    MoveInGrid(agents[i]);
                }

#if DEBUG
                for (int i = 0; i < agents.Count; i++)
                {
                    ValidateAgentInCorrectCell(agents[i]);
                }
#endif

                _state.Ticks++;
            }

            tickWatch.Stop();
        }

        private void UpatePOICounter()
        {
            if (_state.MapData == null)
            {
                return;
            }

            var prefabs = _state.MapData.Prefabs;
            var decos = prefabs.Decorations;

            if (_state.AgentsNearPOICounter == null || _state.AgentsNearPOICounter.Length != decos.Length)
            {
                _state.AgentsNearPOICounter = new int[decos.Length];
            }

            for (int i = 0; i < decos.Length; i++)
            {
                var deco = decos[i];
                _state.AgentsNearPOICounter[i] = QueryNearbyCount(deco.Position, 64, 100);
            }
        }

        [Conditional("DEBUG")]
        private void CheckPositionInBounds(Vector3 pos)
        {
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;
            Debug.Assert(pos.X >= worldMins.X);
            Debug.Assert(pos.X <= worldMaxs.X);
            Debug.Assert(pos.Y >= worldMins.Y);
            Debug.Assert(pos.Y <= worldMaxs.Y);
        }

        private void UpdateAgent(Agent agent)
        {
            var ticksDelta = _state.Ticks - agent.LastUpdateTick;
            var deltaTime = ticksDelta * TickRate;
            agent.LastUpdateTick = _state.Ticks;

            // Might be cleared while its running.
            if (_processors.Count == 0)
            {
                return;
            }

            // Sanity check, omitted for release builds.
            CheckPositionInBounds(agent.Position);

            float maxNeighborDistance = _state.MaxNeighbourDistance;

            FixedBufferList<Agent> nearby = _nearby;

            if (EditorMode)
            {
                // This needs to be thread safe in the editor mode, this will hit the GC harder.
                nearby = new FixedBufferList<Agent>(Limits.MaxQuerySize);
            }
            else
            {
                _nearby.Clear();
            }

            QueryCellsLockFree(agent.Position, agent.Index, maxNeighborDistance, nearby);

            var curVel = agent.Velocity * 0.97f;

            // Check if curVel is near zero.
            if (System.Math.Abs(curVel.X) < 1e-6f)
                curVel.X = 0;
            if (System.Math.Abs(curVel.Y) < 1e-6f)
                curVel.Y = 0;

            var processorGroup = _processors[agent.Group];
            if (processorGroup != null)
            {
                for (int i = 0; i < processorGroup.Entries.Count; i++)
                {
                    var processor = processorGroup.Entries[i];

                    var addVel = processor.Handler(_state, agent, nearby, processor.DistanceSqr, processor.Power);
                    addVel.Validate();

                    curVel += addVel;
                }
            }

            // Clamp the velocity.
            curVel.X = Math.Clamp(curVel.X, -2.0f, 2.0f);
            curVel.Y = Math.Clamp(curVel.Y, -2.0f, 2.0f);

            curVel.Validate();
            agent.Velocity = curVel;

            ApplyMovement(agent, deltaTime, processorGroup.SpeedScale);

            Warp(agent);
        }

        public void ApplyMovement(Agent agent, float deltaTime, float speedScale)
        {
            // Cap the deltaTime
            deltaTime = System.Math.Min(deltaTime, TimeScale);

            // Keep the Z axis clean.
            agent.Position.Z = 0;
            agent.Velocity.Z = 0;

            var pos = agent.Position;
            pos.Validate();

            var vel = agent.Velocity;
            vel.Validate();

            var walkSpeed = 1.4f; // Roughly 1.4m/s
            var realPower = speedScale * walkSpeed;

            var dir = Vector3.Normalize(vel);
            pos += (dir * realPower) * deltaTime;
            pos.Validate();

            agent.Position = pos;
        }

        private void RespawnAgent(Agent agent)
        {
            var startPos = GetRespawnLocation();

            agent.Position = startPos;
            agent.CurrentState = Agent.State.Wandering;
        }

        private void UpdateWindDirection()
        {
            var prng = _state.PRNG;

            if (_state.Ticks >= _state.TickNextWindChange)
            {
                // Pick new direction.
                _state.WindDirTarget.X = (float)System.Math.Cos(prng.NextDouble() * System.Math.PI * 2);
                _state.WindDirTarget.Y = (float)System.Math.Sin(prng.NextDouble() * System.Math.PI * 2);

                // Pick a random delay for the next change.
                _state.TickNextWindChange = _state.Ticks + (uint)prng.Next(2000, 4000);
            }

            // Approach the target direction.
            var delta = _state.WindDirTarget - _state.WindDir;
            var windChangeSpeed = 1.0f;

            _state.WindDir = Vector3.Normalize(_state.WindDir + ((delta * windChangeSpeed) * TickRate));
        }

        public float GetTickTime()
        {
            return lastTickTimeMs;
        }

        public float GetAverageTickTime()
        {
            return averageTickTime;
        }

        private void BounceOffWalls(Agent agent)
        {
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            float turn = .5f;
            if (agent.Position.X <= worldMins.X)
                agent.Velocity.X += turn;

            if (agent.Position.X >= worldMaxs.X)
                agent.Velocity.X -= turn;

            if (agent.Position.Y <= worldMins.Y)
                agent.Velocity.Y += turn;

            if (agent.Position.Y >= worldMaxs.Y)
                agent.Velocity.Y -= turn;

            agent.Position = Vector3.Clamp(agent.Position, worldMins, worldMaxs);
        }

        private void Warp(Agent agent)
        {
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            float BorderSize = 100.0f;
            float EdgeDistance = 100.0f;

            if (agent.Position.X < worldMins.X + BorderSize)
                agent.Position.X = worldMaxs.X - BorderSize - EdgeDistance;

            if (agent.Position.X > worldMaxs.X - BorderSize)
                agent.Position.X = worldMins.X + BorderSize + EdgeDistance;

            if (agent.Position.Y < worldMins.Y + BorderSize)
                agent.Position.Y = worldMaxs.Y - BorderSize - EdgeDistance;

            if (agent.Position.Y > worldMaxs.Y - BorderSize)
                agent.Position.Y = worldMins.Y + BorderSize + EdgeDistance;
        }

    }
}
