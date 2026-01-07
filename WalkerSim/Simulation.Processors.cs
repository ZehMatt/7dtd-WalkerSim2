using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        // Processor structs to avoid lambda allocations
        private struct FlockAnyProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public float DistanceSqr;
            public Vector3 Mean;
            public int Count;

            public void Process(Agent neighbor)
            {
                var dist = Vector3.Distance2DSqr(AgentPos, neighbor.Position);
                if (dist <= DistanceSqr)
                {
                    Mean += neighbor.Position;
                    Count++;
                }
            }
        }

        private struct FlockSameProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public int AgentGroup;
            public float DistanceSqr;
            public Vector3 Mean;
            public int Count;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group != AgentGroup)
                    return;

                var dist = Vector3.Distance2DSqr(AgentPos, neighbor.Position);
                if (dist <= DistanceSqr)
                {
                    Mean += neighbor.Position;
                    Count++;
                }
            }
        }

        private struct FlockOtherProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public int AgentGroup;
            public float DistanceSqr;
            public Vector3 Mean;
            public int Count;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group == AgentGroup)
                    return;

                var dist = Vector3.Distance2DSqr(AgentPos, neighbor.Position);
                if (dist <= DistanceSqr)
                {
                    Mean += neighbor.Position;
                    Count++;
                }
            }
        }

        private struct AlignAnyProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public float DistanceSqr;
            public Vector3 MeanVel;
            public int Count;

            public void Process(Agent neighbor)
            {
                var dist = Vector3.Distance2DSqr(AgentPos, neighbor.Position);
                if (dist <= DistanceSqr)
                {
                    MeanVel += neighbor.Velocity;
                    Count++;
                }
            }
        }

        private struct AlignSameProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public int AgentGroup;
            public float DistanceSqr;
            public Vector3 MeanVel;
            public int Count;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group != AgentGroup)
                    return;

                var dist = Vector3.Distance2DSqr(AgentPos, neighbor.Position);
                if (dist <= DistanceSqr)
                {
                    MeanVel += neighbor.Velocity;
                    Count++;
                }
            }
        }

        private struct AlignOtherProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public int AgentGroup;
            public float DistanceSqr;
            public Vector3 MeanVel;
            public int Count;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group == AgentGroup)
                    return;

                var dist = Vector3.Distance2DSqr(AgentPos, neighbor.Position);
                if (dist <= DistanceSqr)
                {
                    MeanVel += neighbor.Velocity;
                    Count++;
                }
            }
        }

        private struct AvoidAnyProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public float DistanceSqr;
            public Vector3 SumCloseness;

            public void Process(Agent neighbor)
            {
                var dist = Vector3.Distance2DSqr(AgentPos, neighbor.Position);
                if (dist <= DistanceSqr)
                {
                    var closeness = DistanceSqr - dist;
                    SumCloseness += (AgentPos - neighbor.Position) * closeness;
                }
            }
        }

        private struct AvoidSameProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public int AgentGroup;
            public float DistanceSqr;
            public Vector3 SumCloseness;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group != AgentGroup)
                    return;

                var dist = Vector3.Distance2DSqr(AgentPos, neighbor.Position);
                if (dist <= DistanceSqr)
                {
                    var closeness = DistanceSqr - dist;
                    SumCloseness += (AgentPos - neighbor.Position) * closeness;
                }
            }
        }

        private struct AvoidOtherProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public int AgentGroup;
            public float DistanceSqr;
            public Vector3 SumCloseness;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group == AgentGroup)
                    return;

                var dist = Vector3.Distance2DSqr(AgentPos, neighbor.Position);
                if (dist <= DistanceSqr)
                {
                    var closeness = DistanceSqr - dist;
                    SumCloseness += (AgentPos - neighbor.Position) * closeness;
                }
            }
        }

        private delegate Vector3 MovementProcessorDelegate(Simulation sim, State state, Agent agent, float distance, float power);

        class MovementProcessor
        {
            public List<Processor> Entries;
            public float SpeedScale = 1.0f;
            public int Group = -1;
            public Config.PostSpawnBehavior PostSpawnBehavior = Config.PostSpawnBehavior.Wander;
            public Config.WanderingSpeed PostSpawnWanderingSpeed = Config.WanderingSpeed.NoOverride;
            public Drawing.Color Color;
        }

        class Processor
        {
            public MovementProcessorDelegate Handler;
            public float Distance;
            public float Power;
        }

        private readonly Dictionary<Config.MovementProcessorType, MovementProcessorDelegate> ProcessorTypeToDelegateMap = new Dictionary<Config.MovementProcessorType, MovementProcessorDelegate>()
        {
            { Config.MovementProcessorType.FlockAnyGroup, FlockAny },
            { Config.MovementProcessorType.AlignAnyGroup, AlignAny },
            { Config.MovementProcessorType.AvoidAnyGroup, AvoidAny },
            { Config.MovementProcessorType.FlockSameGroup, FlockSame },
            { Config.MovementProcessorType.AlignSameGroup, AlignSame },
            { Config.MovementProcessorType.AvoidSameGroup, AvoidSame },
            { Config.MovementProcessorType.FlockOtherGroup, FlockOther },
            { Config.MovementProcessorType.AlignOtherGroup, AlignOther },
            { Config.MovementProcessorType.AvoidOtherGroup, AvoidOther },
            { Config.MovementProcessorType.Wind, Wind },
            { Config.MovementProcessorType.WindInverted, WindInverted },
            { Config.MovementProcessorType.StickToRoads, StickToRoads },
            { Config.MovementProcessorType.AvoidRoads, AvoidRoads },
            { Config.MovementProcessorType.StickToPOIs, StickToPOIs },
            { Config.MovementProcessorType.AvoidPOIs, AvoidPOIs },
            { Config.MovementProcessorType.WorldEvents, WorldEvents },
            { Config.MovementProcessorType.PreferCities, PreferCities },
            { Config.MovementProcessorType.AvoidCities, AvoidCities },
            { Config.MovementProcessorType.CityVisitor, CityVisitor },
        };

        private List<MovementProcessor> _processors = new List<MovementProcessor>();

        private MovementProcessorDelegate GetProcessorDelegate(Config.MovementProcessorType type)
        {
            return ProcessorTypeToDelegateMap[type];
        }

        private void SetupProcessors()
        {
            if (_state.GroupCount == 0)
            {
                return;
            }

            _processors.Clear();

            if (_state.Config.Processors.Count == 0)
            {
                // Nothing to do.
                return;
            }

            // Zero init the list based on group count.
            for (int i = 0; i < _state.GroupCount; i++)
            {
                _processors.Add(null);
            }

            // Build a list of groups, some specify the exact group some are generic.
            List<MovementProcessor> genericGroups = new List<MovementProcessor>();
            List<MovementProcessor> specificGroups = new List<MovementProcessor>();

            foreach (var processorGroup in _state.Config.Processors)
            {
                var processors = new List<Processor>();

                if (processorGroup.Group != -1)
                {
                    if (processorGroup.Group >= _state.GroupCount)
                    {
                        Logging.Err("A processor group specifies an invalid group index, available groups: {0}, specified: {1}, fallback to any.", _state.GroupCount, processorGroup.Group);

                        // Fallback to any group.
                        processorGroup.Group = -1;
                    }
                }

                foreach (var processor in processorGroup.Entries)
                {
                    var entry = new Processor();
                    entry.Distance = processor.Distance;
                    entry.Power = processor.Power;
                    entry.Handler = GetProcessorDelegate(processor.Type);

                    processors.Add(entry);
                }

                var group = new MovementProcessor()
                {
                    Entries = processors,
                    SpeedScale = processorGroup.SpeedScale,
                    Group = processorGroup.Group,
                    PostSpawnBehavior = processorGroup.PostSpawnBehavior,
                    PostSpawnWanderingSpeed = processorGroup.PostSpawnWanderSpeed,
                };

                if (processorGroup.Color == "")
                {
                    // Assign a default color of purple.
                    group.Color = Drawing.Color.Magenta;
                }
                else
                {
                    group.Color = Utils.ParseColor(processorGroup.Color);
                }

                if (group.Group < 0)
                    genericGroups.Add(group);
                else
                    specificGroups.Add(group);
            }

            // Fill the generic ones but alternate.
            if (genericGroups.Count > 0)
            {
                for (int i = 0; i < _state.GroupCount; i++)
                {
                    var group = genericGroups[i % genericGroups.Count];
                    var newGroup = new MovementProcessor()
                    {
                        Entries = group.Entries,
                        SpeedScale = group.SpeedScale,
                        Group = group.Group,
                        Color = group.Color,
                        PostSpawnBehavior = group.PostSpawnBehavior,
                        PostSpawnWanderingSpeed = group.PostSpawnWanderingSpeed,
                    };
                    _processors[i] = newGroup;
                }
            }

            // Fill specific ones.
            for (int i = 0; i < specificGroups.Count; i++)
            {
                var group = specificGroups[i];

                if (group.Group >= _processors.Count)
                {
                    Logging.Warn("Group specifies the group index {0} but there are only a total of {1} groups.", group.Group, _processors.Count);
                }
                else
                {
                    var newGroup = new MovementProcessor()
                    {
                        Entries = group.Entries,
                        SpeedScale = group.SpeedScale,
                        Group = group.Group,
                        Color = group.Color,
                        PostSpawnBehavior = group.PostSpawnBehavior,
                        PostSpawnWanderingSpeed = group.PostSpawnWanderingSpeed,
                    };
                    _processors[group.Group] = newGroup;
                }
            }

            // Colorize all the groups who are still transparent
            for (int i = 0; i < _processors.Count; i++)
            {
                var group = _processors[i];
                if (group == null)
                    continue;

                if (group.Color == Drawing.Color.Transparent)
                {
                    group.Color = Drawing.ColorTable.GetColorForIndex(i);
                }
            }

            // Find the maximum query distance.
            _state.MaxNeighbourDistance = 0.0f;

            foreach (var processorList in _processors)
            {
                if (processorList == null)
                    continue;

                foreach (var processor in processorList.Entries)
                {
                    _state.MaxNeighbourDistance = System.Math.Max(_state.MaxNeighbourDistance, processor.Distance);
                }
            }
        }

        private static Vector3 FlockAny(Simulation sim, State state, Agent agent, float distance, float power)
        {
            var processor = new FlockAnyProcessor
            {
                AgentPos = agent.Position,
                DistanceSqr = distance * distance,
                Mean = Vector3.Zero,
                Count = 0
            };

            sim.ForEachNearby(agent.Position, agent.Index, distance, ref processor);

            if (processor.Count == 0)
                return Vector3.Zero;

            processor.Mean /= processor.Count;
            var center = processor.Mean - agent.Position;
            return center * power;
        }

        private static Vector3 FlockSame(Simulation sim, State state, Agent agent, float distance, float power)
        {
            var processor = new FlockSameProcessor
            {
                AgentPos = agent.Position,
                AgentGroup = agent.Group,
                DistanceSqr = distance * distance,
                Mean = Vector3.Zero,
                Count = 0
            };

            sim.ForEachNearby(agent.Position, agent.Index, distance, ref processor);

            if (processor.Count == 0)
                return Vector3.Zero;

            processor.Mean /= processor.Count;
            var center = processor.Mean - agent.Position;
            return center * power;
        }

        private static Vector3 FlockOther(Simulation sim, State state, Agent agent, float distance, float power)
        {
            var processor = new FlockOtherProcessor
            {
                AgentPos = agent.Position,
                AgentGroup = agent.Group,
                DistanceSqr = distance * distance,
                Mean = Vector3.Zero,
                Count = 0
            };

            sim.ForEachNearby(agent.Position, agent.Index, distance, ref processor);

            if (processor.Count == 0)
                return Vector3.Zero;

            processor.Mean /= processor.Count;
            var center = processor.Mean - agent.Position;
            return center * power;
        }

        private static Vector3 AlignAny(Simulation sim, State state, Agent agent, float distance, float power)
        {
            var processor = new AlignAnyProcessor
            {
                AgentPos = agent.Position,
                DistanceSqr = distance * distance,
                MeanVel = Vector3.Zero,
                Count = 0
            };

            sim.ForEachNearby(agent.Position, agent.Index, distance, ref processor);

            if (processor.Count == 0)
                return Vector3.Zero;

            processor.MeanVel /= processor.Count;
            var delta = processor.MeanVel - agent.Velocity;
            return delta * power;
        }


        private static Vector3 AlignSame(Simulation sim, State state, Agent agent, float distance, float power)
        {
            // point toward the center of the flock (mean flock boid position)
            var meanVel = Vector3.Zero;
            var distanceSqr = distance * distance;

            var processor = new AlignSameProcessor
            {
                AgentPos = agent.Position,
                AgentGroup = agent.Group,
                DistanceSqr = distanceSqr,
                MeanVel = Vector3.Zero,
                Count = 0
            };

            sim.ForEachNearby(agent.Position, agent.Index, distance, ref processor);

            if (processor.Count == 0)
                return Vector3.Zero;

            processor.MeanVel /= processor.Count;
            var delta = processor.MeanVel - agent.Velocity;
            return delta * power;
        }

        private static Vector3 AlignOther(Simulation sim, State state, Agent agent, float distance, float power)
        {
            var processor = new AlignOtherProcessor
            {
                AgentPos = agent.Position,
                AgentGroup = agent.Group,
                DistanceSqr = distance * distance,
                MeanVel = Vector3.Zero,
                Count = 0
            };

            sim.ForEachNearby(agent.Position, agent.Index, distance, ref processor);

            if (processor.Count == 0)
                return Vector3.Zero;

            processor.MeanVel /= processor.Count;
            var delta = processor.MeanVel - agent.Velocity;
            return delta * power;
        }

        private static Vector3 AvoidAny(Simulation sim, State state, Agent agent, float distance, float power)
        {
            var processor = new AvoidAnyProcessor
            {
                AgentPos = agent.Position,
                DistanceSqr = distance * distance,
                SumCloseness = Vector3.Zero
            };

            sim.ForEachNearby(agent.Position, agent.Index, distance, ref processor);

            return processor.SumCloseness * power;
        }


        private static Vector3 AvoidSame(Simulation sim, State state, Agent agent, float distance, float power)
        {
            var processor = new AvoidSameProcessor
            {
                AgentPos = agent.Position,
                AgentGroup = agent.Group,
                DistanceSqr = distance * distance,
                SumCloseness = Vector3.Zero
            };

            sim.ForEachNearby(agent.Position, agent.Index, distance, ref processor);

            return processor.SumCloseness * power;
        }

        private static Vector3 AvoidOther(Simulation sim, State state, Agent agent, float distance, float power)
        {
            var processor = new AvoidOtherProcessor
            {
                AgentPos = agent.Position,
                AgentGroup = agent.Group,
                DistanceSqr = distance * distance,
                SumCloseness = Vector3.Zero
            };

            sim.ForEachNearby(agent.Position, agent.Index, distance, ref processor);

            return processor.SumCloseness * power;
        }

        private static Vector3 Wind(Simulation sim, State state, Agent agent, float distance, float power)
        {
            return state.WindDir * power;
        }

        private static Vector3 WindInverted(Simulation sim, State state, Agent agent, float distance, float power)
        {
            return (state.WindDir * -1.0f) * power;
        }

        private static Vector3 StickToRoads(Simulation sim, State state, Agent agent, float distance, float power)
        {
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

            // Remap the position to the roads bitmap.
            var x = MathEx.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, roads.Width);
            var y = MathEx.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, roads.Height);

            int ix = (int)x;
            int iy = (int)y;

            var closest = roads.GetClosestRoad(ix, iy, (int)agent.Velocity.X, (int)agent.Velocity.Y);
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

        private static Vector3 AvoidRoads(Simulation sim, State state, Agent agent, float distance, float power)
        {
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

            // Remap the position to the roads bitmap.
            var x = MathEx.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, roads.Width);
            var y = MathEx.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, roads.Height);

            int ix = (int)x;
            int iy = (int)y;

            var closest = roads.GetClosestRoad(ix, iy, (int)agent.Velocity.X, (int)agent.Velocity.Y);
            if (closest.Type == RoadType.None)
            {
                return Vector3.Zero;
            }

            float dx = (float)(ix - closest.X);
            float dy = (float)(iy - closest.Y);

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

        private static Vector3 StickToPOIs(Simulation sim, State state, Agent agent, float distance, float power)
        {
            if (state.MapData == null)
            {
                return Vector3.Zero;
            }

            var prefabs = state.MapData.Prefabs;
            var decos = prefabs.Decorations;

            var closestIdx = -1;
            var closestDist = float.MaxValue;
            for (int i = 0; i < decos.Length; i++)
            {
                var deco = decos[i];

                if (state.AgentsNearPOICounter != null)
                {
                    if (state.AgentsNearPOICounter[i] >= 64)
                        continue;
                }

                var dist = Vector3.Distance2DSqr(agent.Position, deco.Position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIdx = i;
                }
            }

            if (closestIdx == -1)
            {
                return Vector3.Zero;
            }

            var direction = Vector3.Normalize(decos[closestIdx].Position - agent.Position);
            return direction * power;
        }


        private static Vector3 AvoidPOIs(Simulation sim, State state, Agent agent, float distance, float power)
        {
            if (state.MapData == null)
            {
                return Vector3.Zero;
            }

            var prefabs = state.MapData.Prefabs;
            var decos = prefabs.Decorations;

            var sumCloseness = Vector3.Zero;
            for (int i = 0; i < decos.Length; i++)
            {
                var deco = decos[i];
                var dist = Vector3.Distance2DSqr(agent.Position, deco.Position);
                if (dist > distance)
                {
                    continue;
                }

                var closeness = distance - dist;
                sumCloseness += (agent.Position - deco.Position) * closeness;
            }

            return sumCloseness * power;
        }

        private static Vector3 WorldEvents(Simulation sim, State state, Agent agent, float distance, float power)
        {
            var events = state.EventsTemp;

            Vector3 sum = Vector3.Zero;
            Vector3 eventCenter = Vector3.Zero;
            float n = 0;
            for (int i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                if (ev.Type == EventType.Noise)
                {
                    var dist = Vector3.Distance2D(agent.Position, ev.Position);
                    if (dist > ev.Radius)
                    {
                        continue;
                    }

                    eventCenter += ev.Position;
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
                eventCenter /= n;

                agent.CurrentSubState = Agent.SubState.Alerted;
                agent.AlertedTick = state.Ticks;
                agent.AlertPosition = eventCenter;
            }

            return sum * power;
        }

        private static Vector3 PreferCities(Simulation sim, State state, Agent agent, float distance, float power)
        {
            if (state.MapData == null || state.MapData.Cities == null)
            {
                return Vector3.Zero;
            }

            var cities = state.MapData.Cities;
            var city = cities.GetCityAt(agent.Position);

            if (city != null)
            {
                // Agent is inside a city - create smooth wandering motion using sine/cosine
                // Each agent gets unique movement pattern based on their ID

                // Create agent-specific frequencies and phases
                // Using prime number mixing to avoid pattern synchronization between agents
                float phaseX = (agent.Index * 2654435761u) % 10000 / 10000.0f * 6.28318530718f; // 0 to 2*PI
                float phaseY = (agent.Index * 1597334677u) % 10000 / 10000.0f * 6.28318530718f;

                // Different frequencies for more organic movement (avoid circular patterns)
                float freqX = 0.002f + ((agent.Index * 2246822519u) % 1000 / 10000.0f) * 0.001f; // 0.002-0.003
                float freqY = 0.0015f + ((agent.Index * 3266489917u) % 1000 / 10000.0f) * 0.001f; // 0.0015-0.0025

                // Calculate smooth offset based on time
                float timeScale = state.Ticks;
                float offsetX = (float)System.Math.Sin(timeScale * freqX + phaseX);
                float offsetY = (float)System.Math.Cos(timeScale * freqY + phaseY);

                // Scale offsets to wander within city bounds with margin
                const float edgeMargin = 20f;
                float wanderRadiusX = (city.Bounds.X - 2 * edgeMargin) * 0.3f; // Use 30% of available space
                float wanderRadiusY = (city.Bounds.Y - 2 * edgeMargin) * 0.3f;

                // Calculate target position: city center + smooth wandering offset
                float targetX = city.Position.X + offsetX * wanderRadiusX;
                float targetY = city.Position.Y + offsetY * wanderRadiusY;
                var targetPos = new Vector3(targetX, targetY, 0);

                // Move toward the smoothly changing target position
                var direction = targetPos - agent.Position;
                direction.Z = 0;

                float distToTarget = Vector3.Magnitude(direction);
                if (distToTarget > 1f)
                {
                    direction = Vector3.Normalize(direction);
                    // Gentle force proportional to distance
                    float forceMult = System.Math.Min(distToTarget / 30f, 1f);
                    return direction * power * forceMult * 0.5f;
                }

                return Vector3.Zero;
            }
            else
            {
                // Agent is outside city - find nearest city and move toward its edge
                Cities.City nearestCity = null;
                float nearestDist = float.MaxValue;

                foreach (var c in cities.CityList)
                {
                    // Calculate distance to nearest point on city boundary
                    float closestX = System.Math.Max(c.MinX, System.Math.Min(agent.Position.X, c.MaxX));
                    float closestY = System.Math.Max(c.MinY, System.Math.Min(agent.Position.Y, c.MaxY));
                    float dist = Vector3.Distance2D(agent.Position, new Vector3(closestX, closestY, 0));

                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestCity = c;
                    }
                }

                if (nearestCity != null)
                {
                    // Move toward nearest edge of city
                    float closestX = System.Math.Max(nearestCity.MinX, System.Math.Min(agent.Position.X, nearestCity.MaxX));
                    float closestY = System.Math.Max(nearestCity.MinY, System.Math.Min(agent.Position.Y, nearestCity.MaxY));
                    var direction = new Vector3(closestX, closestY, 0) - agent.Position;
                    direction.Z = 0;

                    if (direction.X != 0 || direction.Y != 0)
                    {
                        direction = Vector3.Normalize(direction);
                        return direction * power;
                    }
                }

                return Vector3.Zero;
            }
        }

        private static Vector3 AvoidCities(Simulation sim, State state, Agent agent, float distance, float power)
        {
            if (state.MapData == null || state.MapData.Cities == null)
            {
                return Vector3.Zero;
            }

            var cities = state.MapData.Cities;
            var city = cities.GetCityAt(agent.Position);

            if (city != null)
            {
                // Agent is inside a city - push toward nearest exit
                float distToLeft = agent.Position.X - city.MinX;
                float distToRight = city.MaxX - agent.Position.X;
                float distToBottom = agent.Position.Y - city.MinY;
                float distToTop = city.MaxY - agent.Position.Y;

                float minDist = System.Math.Min(System.Math.Min(distToLeft, distToRight), System.Math.Min(distToBottom, distToTop));

                Vector3 exitDirection = Vector3.Zero;
                if (minDist == distToLeft)
                    exitDirection = new Vector3(-1, 0, 0);
                else if (minDist == distToRight)
                    exitDirection = new Vector3(1, 0, 0);
                else if (minDist == distToBottom)
                    exitDirection = new Vector3(0, -1, 0);
                else if (minDist == distToTop)
                    exitDirection = new Vector3(0, 1, 0);

                return exitDirection * power;
            }
            else
            {
                // Agent is outside city - apply repulsion if near (within distance)
                Vector3 repulsion = Vector3.Zero;

                foreach (var c in cities.CityList)
                {
                    // Calculate distance to nearest point on city boundary
                    float closestX = System.Math.Max(c.MinX, System.Math.Min(agent.Position.X, c.MaxX));
                    float closestY = System.Math.Max(c.MinY, System.Math.Min(agent.Position.Y, c.MaxY));
                    float dist = Vector3.Distance2D(agent.Position, new Vector3(closestX, closestY, 0));

                    if (dist < distance)
                    {
                        // Push away from city edge
                        var direction = agent.Position - new Vector3(closestX, closestY, 0);
                        direction.Z = 0;

                        if (direction.X != 0 || direction.Y != 0)
                        {
                            direction = Vector3.Normalize(direction);
                            var closeness = distance - dist;
                            repulsion += direction * closeness;
                        }
                    }
                }

                return repulsion * power;
            }
        }

        private static Vector3 CityVisitor(Simulation sim, State state, Agent agent, float distance, float power)
        {
            if (state.MapData == null || state.MapData.Cities == null || state.MapData.Cities.CityList.Count == 0)
            {
                return Vector3.Zero;
            }

            var cities = state.MapData.Cities;

            // Offset each agent's time by their group using prime number for staggering
            long tickOffset = agent.Group * 37;
            long adjustedTime = state.Ticks + tickOffset;

            int cityCount = cities.CityList.Count;

            // Fixed 10-minute rotation cycle through cities
            int cityRotationTime = sim.MinutesToTicks(10);
            int currentCityIndex = (int)((adjustedTime / cityRotationTime) % cityCount);
            int targetCityIndex = (currentCityIndex + agent.Group) % cityCount;
            var targetCity = cities.CityList[targetCityIndex];

            var currentCity = cities.GetCityAt(agent.Position);

            // Two branches: in target city or not
            if (currentCity == targetCity)
            {
                // In target city - wander across full area
                float phaseX = (agent.Group * 2654435761u) % 10000 / 10000.0f * 6.28318530718f;
                float phaseY = (agent.Group * 1597334677u) % 10000 / 10000.0f * 6.28318530718f;

                float freqX = 0.002f + ((agent.Group * 2246822519u) % 1000 / 10000.0f) * 0.001f;
                float freqY = 0.0015f + ((agent.Group * 3266489917u) % 1000 / 10000.0f) * 0.001f;

                float timeScale = state.Ticks;
                float offsetX = (float)System.Math.Sin(timeScale * freqX + phaseX);
                float offsetY = (float)System.Math.Cos(timeScale * freqY + phaseY);

                const float edgeMargin = 20f;
                float wanderRadiusX = (targetCity.Bounds.X / 2) - edgeMargin;
                float wanderRadiusY = (targetCity.Bounds.Y / 2) - edgeMargin;

                float targetX = targetCity.Position.X + offsetX * wanderRadiusX;
                float targetY = targetCity.Position.Y + offsetY * wanderRadiusY;
                var targetPos = new Vector3(targetX, targetY, 0);

                var direction = targetPos - agent.Position;
                direction.Z = 0;

                float distToTarget = Vector3.Magnitude(direction);
                if (distToTarget > 1f)
                {
                    direction = Vector3.Normalize(direction);
                    float forceMult = System.Math.Min(distToTarget / 30f, 1f);
                    return direction * power * forceMult * 0.5f;
                }

                return Vector3.Zero;
            }
            else
            {
                // Not in target city - approach it
                float closestX = System.Math.Max(targetCity.MinX, System.Math.Min(agent.Position.X, targetCity.MaxX));
                float closestY = System.Math.Max(targetCity.MinY, System.Math.Min(agent.Position.Y, targetCity.MaxY));
                var direction = new Vector3(closestX, closestY, 0) - agent.Position;
                direction.Z = 0;

                if (direction.X != 0 || direction.Y != 0)
                {
                    direction = Vector3.Normalize(direction);
                    return direction * power;
                }

                return Vector3.Zero;
            }
        }

    }
}
