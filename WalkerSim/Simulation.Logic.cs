using System.Collections.Generic;
using System.Diagnostics;

namespace WalkerSim
{
    internal partial class Simulation
    {
        private float averageTickTime = 0f;
        private float lastTickTimeMs = 0f;

        private Stopwatch tickWatch = new Stopwatch();

        // Singleton instances to avoid GCing.
        private FixedBufferList<Agent> _nearby = new FixedBufferList<Agent>(Limits.MaxQuerySize);
        private List<EventData> _events = new List<EventData>();

        // NOTE: This must be a prime number.
        private const uint IteratorIncrement = 7;

        // If this is true then the simulation will be deterministic given it has equal configurations.
        public bool Deterministic = false;

        // If DeterministicLoop is set to true this will be the amount of how many agents it will update per tick.
        private const int MaxUpdateCountPerTick = 2000;


        public void Tick()
        {
            tickWatch.Restart();

            UpdateWindDirection();
            UpdateEvents();

            var agents = _state.Agents;
            var numUpdates = 0;
            var maxUpdates = System.Math.Min(
                System.Math.Min(MaxUpdateCountPerTick * TimeScale, agents.Count),
                4000
            );

            while (true)
            {
                var agentIndex = (int)(_state.SlowIterator % agents.Count);
                var agent = agents[agentIndex];

                _state.SlowIterator += IteratorIncrement;

                if (agent.CurrentState == Agent.State.Wandering)
                {
                    UpdateAgent(agent);
                }
                else if (agent.CurrentState == Agent.State.Respawning)
                {
                    RespawnAgent(agent);
                }

                var exitLoop = false;
                if (Deterministic)
                {
                    exitLoop = numUpdates >= maxUpdates;
                }
                else
                {
                    exitLoop = tickWatch.Elapsed.TotalSeconds >= TickRate;
                }

                if (exitLoop)
                {
                    break;
                }

                numUpdates++;
            }

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

            // Sanity check, omitted for release builds.
            CheckPositionInBounds(agent.Position);

            float maxNeighborDistance = _state.MaxNeighbourDistance;

            _nearby.Clear();
            QueryCellsLockFree(agent.Position, agent.Index, maxNeighborDistance, _nearby);

            var curVel = agent.Velocity * 0.95f;

            var processorGroup = _processors[agent.Group];
            for (int i = 0; i < processorGroup.Entries.Count; i++)
            {
                var processor = processorGroup.Entries[i];

                var addVel = processor.Handler(_state, agent, _nearby, processor.Distance, processor.Power);
                addVel.Validate();

                curVel += addVel;
            }

            curVel.Validate();
            agent.Velocity = curVel;

            ApplyMovement(agent, deltaTime * TimeScale, processorGroup.SpeedScale);

            //BounceOffWalls(ref agent);
            Warp(agent);
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

        public void ApplyMovement(Agent agent, float deltaTime, float power)
        {
            // Cap the deltaTime
            deltaTime = System.Math.Min(deltaTime, 10.0f);

            // Keep the Z axis clean.
            agent.Position.Z = 0;
            agent.Velocity.Z = 0;

            var pos = agent.Position;
            pos.Validate();

            var vel = agent.Velocity;
            vel.Validate();

            var dir = Vector3.Normalize(vel);
            pos += (dir * power) * deltaTime;
            pos.Validate();

            agent.Velocity = vel;
            agent.Position = pos;
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
