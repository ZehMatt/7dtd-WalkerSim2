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

        public Vector3 WindDirection
        {
            get => _state.WindDir;
        }

        public void Tick()
        {
            tickWatch.Restart();

            UpdateWindDirection();
            UpdateEvents();

            float maxNeighborDistance = 750f;

            // We make a copy of the events to avoid locking/unlocking per agent.
            var events = new List<EventData>();
            lock (_state)
            {
                events.AddRange(_state.Events);
            }

            var maxUpdates = 500;
            var now = DateTime.Now;
            var agents = _state.Agents;
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;
            var nearby = new List<Agent>();

            // This is mostly thread-safe, it might race the position but we don't need it super accurate.
            //System.Threading.Tasks.Parallel.For(0, agents.Count, i =>
            //agents.ForEach((agent) =>
            for (int i = 0; i < maxUpdates; i++)
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
                Debug.Assert(agent.Index == agentIndex);
                Debug.Assert(agent.Position.X >= worldMins.X);
                Debug.Assert(agent.Position.X <= worldMaxs.X);
                Debug.Assert(agent.Position.Y >= worldMins.Y);
                Debug.Assert(agent.Position.Y <= worldMaxs.Y);
#endif

                nearby.Clear();
                QueryNearby(agent.Position, agent.Index, maxNeighborDistance, nearby);

                var curVel = agent.Velocity;

                {
                    var addVel = Flock(agent, nearby, 150f, .003f);
                    addVel.Validate();
                    curVel += addVel;
                }

                {
                    var addVel = Align(agent, nearby, 150f, .001f);
                    addVel.Validate();
                    curVel += addVel;
                }

                {
                    var addVel = Avoid(agent, nearby, 50f, .002f);
                    addVel.Validate();
                    curVel += addVel;
                }

                {
                    var addVel = Group(agent, nearby, 250f, .0001f);
                    addVel.Validate();
                    curVel += addVel;
                }

                {
                    var addVel = GroupAvoid(agent, nearby, 150f, .0001f);
                    addVel.Validate();
                    curVel += addVel;
                }

                {
                    var addVel = Wind(agent, nearby, .05f);
                    addVel.Validate();
                    curVel += addVel;
                }

                {
                    var addVel = StickToRoads(agent, nearby, .04f);
                    addVel.Validate();
                    curVel += addVel;
                }

                {
                    var addVel = ProcessEvents(events, agent, .05f);
                    addVel.Validate();
                    curVel += addVel;
                }

                curVel.Validate();
                agent.Velocity = curVel;

                MoveForward(agent, (float)deltaTime.TotalSeconds);
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

        private Vector3 Flock(Agent agent, List<Agent> nearby, float distance, float power)
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

        private Vector3 Align(Agent agent, List<Agent> nearby, float distance, float power)
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

        private Vector3 Avoid(Agent agent, List<Agent> nearby, float distance, float power)
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

        private Vector3 Group(Agent agent, List<Agent> nearby, float distance, float power)
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

        private Vector3 GroupAvoid(Agent agent, List<Agent> nearby, float distance, float power)
        {
            var groupCount = _groupCount;
            var distanceSqr = distance;
            // point away from other groups.
            var sumCloseness = Vector3.Zero;
            for (int i = 0; i < nearby.Count; i++)
            {
                var neighbor = nearby[i];
                var distanceAway = Vector3.Distance(agent.Position, neighbor.Position);
                if (distanceAway > distanceSqr)
                    continue;

                float groupDistance = ((float)Math.Abs(agent.Group - neighbor.Group) / (float)groupCount);
                float groupDislikeFactor = groupDistance * 0.6f;
                float closeness = distanceSqr - distanceAway;

                var delta = agent.Position - neighbor.Position;
                delta.Z = 0;

                sumCloseness += delta * (closeness * groupDislikeFactor);
            }

            return sumCloseness * power;
        }

        private Vector3 Wind(Agent agent, List<Agent> _, float power)
        {
            return _state.WindDir * power;
        }

        private Vector3 StickToRoads(Agent agent, List<Agent> _, float power)
        {
            // Remap the position to the roads bitmap.
            if (MapData == null)
            {
                return Vector3.Zero;
            }

            var roads = MapData.Roads;
            if (roads == null)
            {
                return Vector3.Zero;
            }

            var pos = agent.Position;
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

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

        private Vector3 ProcessEvents(IReadOnlyList<EventData> events, Agent agent, float power)
        {
            Vector3 sum = Vector3.Zero;

            float n = 0;
            foreach (var ev in events)
            {
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

        public void MoveForward(Agent agent, float deltaTime, float minSpeed = 1, float maxSpeed = 5)
        {
            // Keep the Z axis clean.
            agent.Position.Z = 0;
            agent.Velocity.Z = 0;

            var pos = agent.Position;
            pos.Validate();

            var vel = agent.Velocity;
            vel.Validate();

            var speed = vel.Magnitude();
            if (speed == 0f)
            {
                vel = Vector3.One * minSpeed;
            }
            else if (speed > maxSpeed)
            {
                vel = (vel / speed) * maxSpeed;
            }
            else if (speed < minSpeed)
            {
                vel = (vel / speed) * minSpeed;
            }
            vel.Validate();

            float speedScale = 2.0f;

            var dir = Vector3.Normalize(vel);
            pos += (dir * speedScale) * deltaTime;
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
