using System;
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
        private List<Agent> _nearby = new List<Agent>();
        private List<EventData> _events = new List<EventData>();

        private delegate Vector3 MovementProcessorDelegate(State state, Agent agent, List<EventData> events, List<Agent> nearby, float distance, float power);

        class MovementProcessor
        {
            public List<Processor> Entries;
            public float SpeedScale = 1.0f;
        }

        class Processor
        {
            public MovementProcessorDelegate Handler;
            public float Distance;
            public float Power;
        }

        List<MovementProcessor> _processors = new List<MovementProcessor>();

        public Vector3 WindDirection
        {
            get => _state.WindDir;
        }

        private void SetupProcessors()
        {
            _processors.Clear();

            // Zero init the list based on group count.
            for (int i = 0; i < _state.GroupCount; i++)
            {
                _processors.Add(new MovementProcessor());
            }

            foreach (var processorGroup in _state.Config.Processors)
            {
                var processors = new List<Processor>();
                foreach (var processor in processorGroup.Entries)
                {
                    var entry = new Processor();
                    entry.Distance = processor.Distance;
                    entry.Power = processor.Power;
                    switch (processor.Type)
                    {
                        case Config.MovementProcessorType.Flock:
                            entry.Handler = Flock;
                            break;
                        case Config.MovementProcessorType.Align:
                            entry.Handler = Align;
                            break;
                        case Config.MovementProcessorType.Avoid:
                            entry.Handler = Avoid;
                            break;
                        case Config.MovementProcessorType.Group:
                            entry.Handler = Group;
                            break;
                        case Config.MovementProcessorType.GroupAvoid:
                            entry.Handler = GroupAvoid;
                            break;
                        case Config.MovementProcessorType.Wind:
                            entry.Handler = Wind;
                            break;
                        case Config.MovementProcessorType.StickToRoads:
                            entry.Handler = StickToRoads;
                            break;
                        case Config.MovementProcessorType.WorldEvents:
                            entry.Handler = WorldEvents;
                            break;
                        case Config.MovementProcessorType.Invalid:
                            throw new InvalidOperationException("Invalid movement processor type");
                        default:
                            throw new InvalidOperationException("Unhandled movement processor type: " + processor.Type.ToString());

                    }
                    processors.Add(entry);
                }

                if (processorGroup.Group == -1)
                {
                    // Fill all.
                    for (int i = 0; i < _processors.Count; i++)
                    {
                        _processors[i].Entries = processors;
                        _processors[i].SpeedScale = processorGroup.SpeedScale;
                    }
                }
                else
                {
                    // Override.
                    _processors[processorGroup.Group].Entries = processors;
                    _processors[processorGroup.Group].SpeedScale = processorGroup.SpeedScale;
                }
            }

            // Find the maximum query distance.
            _state.MaxNeighbourDistance = 0.0f;

            foreach (var processorList in _processors)
            {
                foreach (var processor in processorList.Entries)
                {
                    _state.MaxNeighbourDistance = System.Math.Max(_state.MaxNeighbourDistance, processor.Distance);
                }
            }
        }

        public void Tick()
        {
            tickWatch.Restart();

            UpdateWindDirection();
            UpdateEvents();

            float maxNeighborDistance = _state.MaxNeighbourDistance;

            // We make a copy of the events to avoid locking/unlocking per agent.
            lock (_state)
            {
                _events.Clear();
                _events.AddRange(_state.Events);
            }

            var now = DateTime.Now;
            var agents = _state.Agents;

            while (tickWatch.Elapsed.TotalSeconds < TickRate)
            {
                var agentIndex = (int)(_state.SlowIterator % agents.Count);
                var agent = agents[agentIndex];

                var deltaTime = now - agent.LastUpdate;
                agent.LastUpdate = now;

                // NOTE: We use a prime number here to have a better distribution of agents.
                _state.SlowIterator += 193;

                if (agent.CurrentState != Agent.State.Wandering)
                    return;

#if DEBUG
                var worldMins = _state.WorldMins;
                var worldMaxs = _state.WorldMaxs;
                Debug.Assert(agent.Index == agentIndex);
                Debug.Assert(agent.Position.X >= worldMins.X);
                Debug.Assert(agent.Position.X <= worldMaxs.X);
                Debug.Assert(agent.Position.Y >= worldMins.Y);
                Debug.Assert(agent.Position.Y <= worldMaxs.Y);
#endif

                _nearby.Clear();
                QueryCells(agent.Position, agent.Index, maxNeighborDistance, _nearby);

                var curVel = Vector3.Zero;

                var processorGroup = _processors[agent.Group];
                for (int i = 0; i < processorGroup.Entries.Count; i++)
                {
                    var processor = processorGroup.Entries[i];

                    var addVel = processor.Handler(_state, agent, _events, _nearby, processor.Distance, processor.Power);
                    addVel.Validate();

                    curVel += addVel;
                }

                curVel.Validate();
                agent.Velocity = curVel;

                ApplyMovement(agent, (float)deltaTime.TotalSeconds, processorGroup.SpeedScale * _speedScale);

                //BounceOffWalls(ref agent);
                Warp(agent);
            }

            // Update the grid, can't do this in parallel since it's not thread safe.
            for (int i = 0; i < agents.Count; i++)
            {
                MoveInGrid(agents[i]);
            }

            tickWatch.Stop();

            double ticks = tickWatch.ElapsedTicks;
            double seconds = ticks / Stopwatch.Frequency;
            double milliseconds = (ticks / Stopwatch.Frequency) * 1000;

            lastTickTimeMs = (float)milliseconds;
            averageTickTime += (float)milliseconds;

            if (_ticks > 1)
                averageTickTime *= 0.5f;

            _ticks++;
        }

        private void UpdateWindDirection()
        {
            var prng = _state.PRNG;

            if (_ticks >= _state.TickNextWindChange)
            {
                var windIncrement = 1.2f * TickRate;

                _state.WindTime += windIncrement;

                // Pick new direction.
                _state.WindDirTarget.X = (float)System.Math.Cos(_state.WindTime);
                _state.WindDirTarget.Y = (float)System.Math.Sin(_state.WindTime);

                // Pick a random delay for the next change.
                _state.TickNextWindChange = _ticks + prng.Next(400, 600);
            }

            // Approach the target direction.
            var delta = _state.WindDirTarget - _state.WindDir;
            var windChangeSpeed = 1.0f;

            _state.WindDir += (delta * windChangeSpeed) * TickRate;
        }

        public float GetTickTime()
        {
            return lastTickTimeMs;
        }

        public float GetAverageTickTime()
        {
            return averageTickTime;
        }

        private static Vector3 Flock(State state, Agent agent, List<EventData> events, List<Agent> nearby, float distance, float power)
        {
            var distanceSqr = distance;
            // point toward the center of the flock (mean flock boid position)
            var mean = Vector3.Zero;
            var count = 0;
            for (int i = 0; i < nearby.Count; i++)
            {
                var neighbor = nearby[i];
                var dist = Vector3.Distance(agent.Position, neighbor.Position);
                if (dist > distanceSqr)
                    continue;
                mean += neighbor.Position;
            }
            if (count == 0)
                return Vector3.Zero;

            mean /= count;

            var center = mean - agent.Position;
            return center * power;
        }

        private static Vector3 Align(State state, Agent agent, List<EventData> events, List<Agent> nearby, float distance, float power)
        {
            var distanceSqr = distance;
            // point toward the center of the flock (mean flock boid position)
            var meanVel = Vector3.Zero;
            var count = 0;
            for (int i = 0; i < nearby.Count; i++)
            {
                var neighbor = nearby[i];
                var dist = Vector3.Distance(agent.Position, neighbor.Position);
                if (dist > distanceSqr)
                    continue;

                meanVel += neighbor.Velocity;
                count++;
            }

            if (count == 0)
                return Vector3.Zero;

            meanVel /= count;

            var delta = meanVel - agent.Velocity;
            return delta * power;
        }

        private static Vector3 Avoid(State state, Agent agent, List<EventData> events, List<Agent> nearby, float distance, float power)
        {
            var distanceSqr = distance;
            // point away as boids get close
            var sumCloseness = Vector3.Zero;
            for (int i = 0; i < nearby.Count; i++)
            {
                var neighbor = nearby[i];
                var dist = Vector3.Distance(agent.Position, neighbor.Position);
                if (dist > distanceSqr)
                    continue;
                var closeness = distanceSqr - agent.GetDistance(neighbor);
                sumCloseness += (agent.Position - neighbor.Position) * closeness;
            }
            return sumCloseness * power;
        }

        private static Vector3 Group(State state, Agent agent, List<EventData> events, List<Agent> nearby, float distance, float power)
        {
            var distanceSqr = distance;
            // point towards same group
            var sumCloseness = Vector3.Zero;
            for (int i = 0; i < nearby.Count; i++)
            {
                var neighbor = nearby[i];
                var distanceAway = Vector3.Distance(agent.Position, neighbor.Position);
                if (distanceAway > distanceSqr)
                    continue;

                float closeness = System.Math.Min(10.0f, distanceSqr - distanceAway);
                if (neighbor.Group == agent.Group)
                {
                    sumCloseness += (neighbor.Position - agent.Position) * closeness;
                }
            }

            return sumCloseness * power;
        }

        private static Vector3 GroupAvoid(State state, Agent agent, List<EventData> events, List<Agent> nearby, float distance, float power)
        {
            var groupCount = state.GroupCount;
            var distanceSqr = distance;

            var sum = Vector3.Zero;
            var count = 0;
            for (int i = 0; i < nearby.Count; i++)
            {
                var neighbor = nearby[i];
                if (neighbor.Group == agent.Group)
                    continue;

                var distanceAway = Vector3.Distance(agent.Position, neighbor.Position);
                if (distanceAway > distanceSqr)
                    continue;

                sum += neighbor.Position;
            }

            if (count == 0)
                return Vector3.Zero;

            var delta = (sum / count) - agent.Position;
            return delta * power;
        }

        private static Vector3 Wind(State state, Agent agent, List<EventData> events, List<Agent> nearby, float distance, float power)
        {
            return state.WindDir * power;
        }

        private static Vector3 StickToRoads(State state, Agent agent, List<EventData> events, List<Agent> nearby, float distance, float power)
        {
            // Remap the position to the roads bitmap.
            if (state.MapData == null)
            {
                return Vector3.Zero;
            }

            var roads = state.MapData.Roads;
            if (roads == null)
            {
                return Vector3.Zero;
            }

            var pos = agent.Position;
            var worldMins = state.WorldMins;
            var worldMaxs = state.WorldMaxs;

            var x = Math.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, roads.Width);
            var y = Math.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, roads.Height);

            int ix = (int)x;
            int iy = (int)y;

            var closest = roads.GetClosestRoad(ix, iy);
            if (closest.Type == RoadType.None)
            {
                return Vector3.Zero;
            }

            float dx = (float)(closest.X - ix);
            float dy = (float)(closest.Y - iy);

            if (closest.Type == RoadType.Asphalt)
            {
                return new Vector3(dx * 0.75f, dy * 0.75f) * power;
            }
            else if (closest.Type == RoadType.Offroad)
            {
                return new Vector3(dx * 0.5f, dy * 0.5f) * power;
            }

            return Vector3.Zero;
        }

        private Vector3 WorldEvents(State state, Agent agent, List<EventData> events, List<Agent> nearby, float distance, float power)
        {
            Vector3 sum = Vector3.Zero;

            float n = 0;
            for (int i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                if (ev.Type == EventType.Noise)
                {
                    var dist = Vector3.Distance(agent.Position, ev.Position);
                    if (dist > ev.Radius)
                    {
                        continue;
                    }

                    var delta = ev.Position - agent.Position;
                    delta.Z = 0;

                    var closeness = 1.0f - ((dist * 0.7f) / ev.Radius);
                    sum += delta * closeness;
                    n++;
                }
            }

            if (n > 0)
            {
                sum /= n;
            }

            return sum * power;
        }

        public void ApplyMovement(Agent agent, float deltaTime, float power)
        {
            // Cap the deltaTime
            deltaTime = System.Math.Min(deltaTime, 0.2f);

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
