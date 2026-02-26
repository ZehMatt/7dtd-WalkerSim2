using System.Diagnostics;
using System.Threading.Tasks;

namespace WalkerSim
{
    internal partial class Simulation
    {
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
            var agentCount = agents.Count;
            var maxUpdates = Constants.MaxUpdateCountPerTick;

            if (agentCount > 0)
            {
                if (EditorMode || _isFastAdvancing)
                {
                    // Update in parallel.
                    Parallel.For(0, maxUpdates, i =>
                    {
                        var index = (_state.SlowIterator + i) % agentCount;
                        var agent = agents[(int)index];
                        UpdateAgentLogic(agent);
                    });
                }
                else
                {
                    // Update single threaded.
                    var slowIterator = _state.SlowIterator;
                    for (uint i = 0; i < maxUpdates; i++)
                    {
                        var index = (slowIterator + i) % agentCount;
                        var agent = agents[(int)index];
                        UpdateAgentLogic(agent);
                    }
                }

                _state.SlowIterator += maxUpdates;

                // Update the grid, can't do this in parallel since it's not thread safe.
                for (int i = 0; i < agentCount; i++)
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
                UpdateWindDirection();
                UpdateEvents();
                UpatePOICounter();
                UpdateAgents();
                CheckAutoSave();
            }
            catch (System.Exception ex)
            {
                Logging.Exception(ex);
            }

            _simTime.Capture();

            _state.Ticks++;

            // Log profiling data every 10 seconds
            if (_state.Ticks % (Constants.TicksPerSecond * 10) == 0)
            {
                PerformanceCounters.Report();
            }
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

            if (decos.Length == 0)
            {
                return;
            }

            // Update only one POI per tick to spread the cost
            var deco = decos[_state.POIIterator];
            _state.AgentsNearPOICounter[_state.POIIterator] = QueryNearbyCount(deco.Position, 64, 100);

            _state.POIIterator = (_state.POIIterator + 1) % decos.Length;
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
            var ticks = _state.Ticks;
            var ticksDelta = ticks - agent.LastUpdateTick;
            var deltaTime = ticksDelta * Constants.TickRate;
            agent.LastUpdateTick = ticks;

            // Might be cleared while its running.
            var processorCount = _processors.Count;
            if (processorCount == 0)
            {
                return;
            }

            // Sanity check, omitted for release builds.
            CheckPositionInBounds(agent.Position);

            var curVel = agent.Velocity * 0.9999f;

            // Check if curVel is near zero.
            if (System.Math.Abs(curVel.X) < 1e-6f)
                curVel.X = 0;
            if (System.Math.Abs(curVel.Y) < 1e-6f)
                curVel.Y = 0;

            // Safe access to processors - may be modified by editor thread
            var processorGroup = (agent.Group >= 0 && agent.Group < _processors.Count) ? _processors[agent.Group] : null;
            if (processorGroup != null)
            {
                var entries = processorGroup.Entries;
                var entryCount = entries.Count;

                for (int i = 0; i < entryCount; i++)
                {
                    var processor = entries[i];
                    var addVel = processor.Handler(this, _state, agent, processor.Distance, processor.Power);
                    addVel.Validate();
                    curVel += addVel;
                }
            }

            // Clamp the velocity.
            curVel.X = MathEx.Clamp(curVel.X, -2.0f, 2.0f);
            curVel.Y = MathEx.Clamp(curVel.Y, -2.0f, 2.0f);

            curVel.Validate();
            agent.Velocity = curVel;

            UpdateAwareness(agent);
            ApplyMovement(agent, deltaTime, processorGroup?.SpeedScale ?? 1.0f);
            Warp(agent);
        }

        public void UpdateAwareness(Agent agent)
        {
            if (agent.CurrentSubState == Agent.SubState.Alerted)
            {
                var ticks = _state.Ticks;
                var ticksAlerted = ticks - agent.AlertedTick;

                if (ticksAlerted > SecondsToTicks(30))
                {
                    agent.CurrentSubState = Agent.SubState.None;
                }
            }
        }

        public void ApplyMovement(Agent agent, float deltaTime, float speedScale)
        {
            // Cap the deltaTime
            deltaTime = System.Math.Min(deltaTime, Constants.TickRate * 128.0f);

            // Keep the Z axis clean.
            agent.Position.Z = 0;
            agent.Velocity.Z = 0;

            var pos = agent.Position;
            pos.Validate();

            var vel = agent.Velocity;
            vel.Validate();

            float walkSpeed;
            if (_state.IsDayTime)
            {
                walkSpeed = agent.CurrentSubState == Agent.SubState.Alerted ? _moveSpeedRageDay : _moveSpeedDay;
            }
            else
            {
                walkSpeed = agent.CurrentSubState == Agent.SubState.Alerted ? _moveSpeedRageNight : _moveSpeedNight;
            }

            // Slow the movement speed for crippled and crawler walk types.
            if (agent.WalkType == 5 /* cWalkTypeCripple = 5; */)
            {
                walkSpeed *= 0.5f;
            }
            else if (agent.WalkType == 0x15 /* cWalkTypeCrawler = 0x15 */)
            {
                walkSpeed *= 0.25f;
            }

            if (_isFastAdvancing)
            {
                walkSpeed *= 64.0f;
            }
            else
            {
                walkSpeed *= MathEx.Clamp(TimeScale, 1.0f, 32.0f);
            }

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
                // Use Gaussian-like distribution for more natural wind changes
                // Combine two random values to approximate normal distribution (central limit theorem)
                var currentAngle = System.Math.Atan2(_state.WindDir.Y, _state.WindDir.X);
                var rand1 = prng.NextDouble() - 0.5; // [-0.5, 0.5]
                var rand2 = prng.NextDouble() - 0.5; // [-0.5, 0.5]
                var gaussianLike = (rand1 + rand2) * 2.0; // Approximate normal distribution centered at 0, range ~[-2, 2] but mostly [-1, 1]

                var maxAngleChange = System.Math.PI / 3; // 60 degrees max
                var angleOffset = gaussianLike * maxAngleChange;
                var newAngle = currentAngle + angleOffset;

                _state.WindDirTarget.X = (float)System.Math.Cos(newAngle);
                _state.WindDirTarget.Y = (float)System.Math.Sin(newAngle);

                var minWindTime = (int)MinutesToTicks(1);
                var maxWindTime = (int)MinutesToTicks(3);

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
