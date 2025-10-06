using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WalkerSim
{
    internal partial class Simulation
    {
        // Singleton instances to avoid GCing.
        private FixedBufferList<Agent> _nearby = new FixedBufferList<Agent>(Limits.MaxQuerySize);

        // If DeterministicLoop is set to true this will be the amount of how many agents it will update per tick.
        private const uint MaxUpdateCountPerTick = 2000;

        private ThreadLocal<FixedBufferList<Agent>> QueryBuffer = new ThreadLocal<FixedBufferList<Agent>>(() =>
        {
            return new FixedBufferList<Agent>(Limits.MaxQuerySize);
        });

        private TimeMeasurement _simTime = new TimeMeasurement();

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

        private void UpdateAgents()
        {
            var agents = _state.Agents;
            var maxUpdates = MaxUpdateCountPerTick;

            if (agents.Count > 0)
            {
                if (EditorMode || _isFastAdvancing)
                {
                    // Update in parallel.
                    Parallel.For(0, maxUpdates, i =>
                    {
                        var index = (int)((_state.SlowIterator + (uint)i) % agents.Count);
                        var agent = agents[index];
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
            }

#if DEBUG
            for (int i = 0; i < agents.Count; i++)
            {
                ValidateAgentInCorrectCell(agents[i]);
            }
#endif
        }

        public void Tick()
        {
            _simTime.Restart();

            try
            {
                lock (_state)
                {
                    CheckAgentSpawn();
                    UpdateWindDirection();
                    UpdateEvents();
                    UpatePOICounter();
                    UpdateAgents();
                    CheckAutoSave();
                }
            }
            catch (System.Exception ex)
            {
                Logging.Exception(ex);
            }

            _state.Ticks++;

            _simTime.Capture();
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

            // When single threaded we use the local buffer.
            FixedBufferList<Agent> nearby = _nearby;

            if (EditorMode)
            {
                // Use the thread local buffer for editor mode.
                nearby = QueryBuffer.Value;
            }
            else if (_isFastAdvancing)
            {
                // It seems we can't use QueryBuffer for Unity/Mono, this doesn't seem to be working,
                // and the instance is never set, allocate temporary one.
                nearby = new FixedBufferList<Agent>(Limits.MaxQuerySize);
            }

            nearby.Clear();

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
            curVel.X = MathEx.Clamp(curVel.X, -2.0f, 2.0f);
            curVel.Y = MathEx.Clamp(curVel.Y, -2.0f, 2.0f);

            curVel.Validate();
            agent.Velocity = curVel;

            if (_isFastAdvancing)
            {
                deltaTime *= 2.0f;
            }

            UpdateAwareness(agent);
            ApplyMovement(agent, deltaTime, processorGroup.SpeedScale);

            Warp(agent);
        }

        public void UpdateAwareness(Agent agent)
        {
            if (agent.CurrentSubState == Agent.SubState.Alerted)
            {
                var ticksAlerted = _state.Ticks - agent.AlertedTick;

                if (ticksAlerted > SecondsToTicks(30))
                {
                    agent.CurrentSubState = Agent.SubState.None;
                }
            }
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
                // Calculate new direction within 90 degrees of current direction
                var currentAngle = System.Math.Atan2(_state.WindDir.Y, _state.WindDir.X);
                var maxAngleChange = System.Math.PI / 2; // 90 degrees max
                var newAngle = currentAngle + (prng.NextDouble() - 0.5) * maxAngleChange;

                _state.WindDirTarget.X = (float)System.Math.Cos(newAngle);
                _state.WindDirTarget.Y = (float)System.Math.Sin(newAngle);

                // Keep original timing logic
#if true
                var minWindTime = MinutesToTicks(4);
                var maxWindTime = MinutesToTicks(8);
#else
                var minWindTime = SecondsToTicks(5);
                var maxWindTime = SecondsToTicks(10);
#endif
                _state.TickNextWindChange = _state.Ticks + (uint)prng.Next(minWindTime, maxWindTime);
            }

            var maxRotation = 0.001f;
            var currentDir = new Vector3(_state.WindDir.X, _state.WindDir.Y);
            var targetDir = new Vector3(_state.WindDirTarget.X, _state.WindDirTarget.Y);

            currentDir = Vector3.Normalize(Vector3.Lerp(currentDir, targetDir, maxRotation));

            _state.WindDir.X = currentDir.X;
            _state.WindDir.Y = currentDir.Y;
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
