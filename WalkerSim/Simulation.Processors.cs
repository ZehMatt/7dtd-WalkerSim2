using System.Collections.Generic;

namespace WalkerSim
{
    public partial class Simulation
    {
        /// <summary>
        /// Deterministic hash for use in parallel processor code instead of shared PRNG.
        /// Returns a positive int derived from agent index, tick, and a salt.
        /// </summary>
        internal static int AgentHash(int agentIndex, uint tick, int salt = 0)
        {
            unchecked
            {
                uint h = (uint)agentIndex;
                h = h * 2654435761u ^ tick;
                h = h * 2654435761u ^ (uint)salt;
                h ^= h >> 16;
                h *= 0x85ebca6bu;
                h ^= h >> 13;
                h *= 0xc2b2ae35u;
                h ^= h >> 16;
                return (int)(h & 0x7FFFFFFFu);
            }
        }
        // Processor structs to avoid lambda allocations
        private struct FlockAnyProcessor : INeighborProcessor
        {
            public Vector3 Mean;
            public int Count;

            public void Process(Agent neighbor)
            {
                Mean += neighbor.Position;
                Count++;
            }
        }

        private struct FlockSameProcessor : INeighborProcessor
        {
            public int AgentGroup;
            public Vector3 Mean;
            public int Count;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group != AgentGroup)
                    return;

                Mean += neighbor.Position;
                Count++;
            }
        }

        private struct FlockOtherProcessor : INeighborProcessor
        {
            public int AgentGroup;
            public Vector3 Mean;
            public int Count;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group == AgentGroup)
                    return;

                Mean += neighbor.Position;
                Count++;
            }
        }

        private struct AlignAnyProcessor : INeighborProcessor
        {
            public Vector3 MeanVel;
            public int Count;

            public void Process(Agent neighbor)
            {
                MeanVel += neighbor.Velocity;
                Count++;
            }
        }

        private struct AlignSameProcessor : INeighborProcessor
        {
            public int AgentGroup;
            public Vector3 MeanVel;
            public int Count;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group != AgentGroup)
                    return;

                MeanVel += neighbor.Velocity;
                Count++;
            }
        }

        private struct AlignOtherProcessor : INeighborProcessor
        {
            public int AgentGroup;
            public Vector3 MeanVel;
            public int Count;

            public void Process(Agent neighbor)
            {
                if (neighbor.Group == AgentGroup)
                    return;

                MeanVel += neighbor.Velocity;
                Count++;
            }
        }

        private struct AvoidAnyProcessor : INeighborProcessor
        {
            public Vector3 AgentPos;
            public float DistanceSqr;
            public Vector3 SumCloseness;

            public void Process(Agent neighbor)
            {
                var delta = AgentPos - neighbor.Position;
                var dist = delta.X * delta.X + delta.Y * delta.Y;
                var closeness = DistanceSqr - dist;
                SumCloseness += delta * closeness;
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

                var delta = AgentPos - neighbor.Position;
                var dist = delta.X * delta.X + delta.Y * delta.Y;
                var closeness = DistanceSqr - dist;
                SumCloseness += delta * closeness;
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

                var delta = AgentPos - neighbor.Position;
                var dist = delta.X * delta.X + delta.Y * delta.Y;
                var closeness = DistanceSqr - dist;
                SumCloseness += delta * closeness;
            }
        }

        internal delegate Vector3 MovementProcessorDelegate(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2);

        class MovementProcessor
        {
            public List<Processor> Entries;
            public float SpeedScale = 1.0f;
            public Config.PostSpawnBehavior PostSpawnBehavior = Config.PostSpawnBehavior.Wander;
            public Config.WanderingSpeed PostSpawnWanderingSpeed = Config.WanderingSpeed.NoOverride;
            public Drawing.Color Color;
        }

        class Processor
        {
            public MovementProcessorDelegate Handler;
            public float Distance;
            public float Power;
            public float Param1;
            public float Param2;
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
            { Config.MovementProcessorType.StickToBiome, StickToBiome },
            { Config.MovementProcessorType.AvoidBiome, AvoidBiome },
        };

        private List<MovementProcessor> _processors = new List<MovementProcessor>();
        private int[] _groupToSystemIndex = System.Array.Empty<int>();

        /// <summary>
        /// Returns the system index (0-based, into Config.Processors) for each spawn group.
        /// </summary>
        public int[] GroupToSystemIndex => _groupToSystemIndex;

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

            // Build the list of movement processors from config.
            var configProcessors = new List<MovementProcessor>();
            var weights = new List<float>();

            foreach (var processorGroup in _state.Config.Processors)
            {
                var processors = new List<Processor>();

                foreach (var processor in processorGroup.Entries)
                {
                    var entry = new Processor();
                    entry.Distance = processor.Distance;
                    entry.Power = processor.Power;
                    entry.Param1 = processor.Param1;
                    entry.Param2 = processor.Param2;
                    entry.Handler = GetProcessorDelegate(processor.Type);

                    processors.Add(entry);
                }

                var group = new MovementProcessor()
                {
                    Entries = processors,
                    SpeedScale = processorGroup.SpeedScale,
                    PostSpawnBehavior = processorGroup.PostSpawnBehavior,
                    PostSpawnWanderingSpeed = processorGroup.PostSpawnWanderSpeed,
                };

                if (processorGroup.Color == "")
                {
                    group.Color = Drawing.Color.Magenta;
                }
                else
                {
                    group.Color = Utils.ParseColor(processorGroup.Color);
                }

                configProcessors.Add(group);
                weights.Add(System.Math.Max(processorGroup.Weight, 0.01f));
            }

            // Distribute spawn groups among systems proportionally by weight.
            // Zero init the list based on group count.
            for (int i = 0; i < _state.GroupCount; i++)
            {
                _processors.Add(null);
            }

            _groupToSystemIndex = new int[_state.GroupCount];

            if (configProcessors.Count == 1)
            {
                // Single system gets all groups.
                for (int i = 0; i < _state.GroupCount; i++)
                {
                    _processors[i] = configProcessors[0];
                    _groupToSystemIndex[i] = 0;
                }
            }
            else
            {
                // Calculate how many groups each system gets based on weight.
                float totalWeight = 0;
                for (int i = 0; i < weights.Count; i++)
                    totalWeight += weights[i];

                // Assign groups proportionally, ensuring each system gets at least 1.
                var groupCounts = new int[configProcessors.Count];
                int assigned = 0;
                for (int i = 0; i < configProcessors.Count; i++)
                {
                    groupCounts[i] = System.Math.Max(1, (int)((_state.GroupCount * weights[i]) / totalWeight));
                    assigned += groupCounts[i];
                }

                // Distribute any remaining (or remove excess) from the largest weight system.
                int remainder = _state.GroupCount - assigned;
                if (remainder != 0)
                {
                    int largestIdx = 0;
                    for (int i = 1; i < weights.Count; i++)
                    {
                        if (weights[i] > weights[largestIdx])
                            largestIdx = i;
                    }
                    groupCounts[largestIdx] += remainder;
                    if (groupCounts[largestIdx] < 1)
                        groupCounts[largestIdx] = 1;
                }

                // Fill the processor list by assigning contiguous ranges.
                int groupIdx = 0;
                for (int i = 0; i < configProcessors.Count && groupIdx < _state.GroupCount; i++)
                {
                    for (int j = 0; j < groupCounts[i] && groupIdx < _state.GroupCount; j++)
                    {
                        _processors[groupIdx] = configProcessors[i];
                        _groupToSystemIndex[groupIdx] = i;
                        groupIdx++;
                    }
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

            // Reset travel state on all agents so stale state from a previous
            // processor configuration (e.g. CityVisitor) doesn't interfere.
            var agents = _state.Agents;
            for (int i = 0; i < agents.Count; i++)
            {
                var agent = agents[i];
                agent.CurrentTravelState = Agent.TravelState.Idle;
                agent.TargetCityIndex = -1;
                agent.RoadNodeTarget = -1;
                agent.ClearRoadNodeHistory();
            }
        }

        internal static Vector3 FlockAny(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            var processor = new FlockAnyProcessor
            {
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

        internal static Vector3 FlockSame(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            var processor = new FlockSameProcessor
            {
                AgentGroup = agent.Group,
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

        internal static Vector3 FlockOther(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            var processor = new FlockOtherProcessor
            {
                AgentGroup = agent.Group,
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

        internal static Vector3 AlignAny(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            var processor = new AlignAnyProcessor
            {
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


        internal static Vector3 AlignSame(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            var processor = new AlignSameProcessor
            {
                AgentGroup = agent.Group,
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

        internal static Vector3 AlignOther(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            var processor = new AlignOtherProcessor
            {
                AgentGroup = agent.Group,
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

        internal static Vector3 AvoidAny(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
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


        internal static Vector3 AvoidSame(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
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

        internal static Vector3 AvoidOther(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
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

        internal static Vector3 Wind(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            return state.WindDir * power;
        }

        internal static Vector3 WindInverted(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            return (state.WindDir * -1.0f) * power;
        }

        // How close (bitmap pixels) to a node before advancing to the next.
        private const float RoadNodeArrivalDist = 6f;
        // Max distance to search for a road node when first entering.
        private const float RoadNodeSearchDist = 40f;
        // Reset road navigation if the agent drifts this far from its target node.
        private const float RoadNodeDriftDist = 60f;
        // Force magnitude scale to keep force similar to old implementation.
        private const float RoadForceScale = 10f;

        internal static Vector3 StickToRoads(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            if (state.MapData == null)
                return Vector3.Zero;

            var roads = state.MapData.Roads;
            if (roads == null)
                return Vector3.Zero;

            var graph = roads.Graph;
            if (graph == null || graph.Nodes.Length == 0)
                return Vector3.Zero;

            var pos = agent.Position;
            var worldMins = state.WorldMins;
            var worldMaxs = state.WorldMaxs;

            // Remap to bitmap coordinates.
            float bx = MathEx.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, roads.Width);
            float by = MathEx.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, roads.Height);

            // If the agent has arrived at a city, stop road navigation entirely
            // so CityVisitor can wander the agent within the city bounds.
            if (agent.CurrentTravelState == Agent.TravelState.Arrived)
            {
                agent.RoadNodeTarget = -1;
                agent.ClearRoadNodeHistory();
                return Vector3.Zero;
            }

            // Check if the agent has a city target from CityVisitor; if so, bias
            // intersection choices toward the city so both processors cooperate.
            float biasX = float.NaN, biasY = float.NaN;
            bool approachingCity = false;
            if (agent.TargetCityIndex >= 0 &&
                agent.CurrentTravelState == Agent.TravelState.Approaching &&
                state.MapData.Cities != null)
            {
                var cities = state.MapData.Cities;
                if (agent.TargetCityIndex < cities.CityList.Count)
                {
                    var city = cities.CityList[agent.TargetCityIndex];
                    biasX = MathEx.Remap(city.Position.X, worldMins.X, worldMaxs.X, 0f, roads.Width);
                    biasY = MathEx.Remap(city.Position.Y, worldMins.Y, worldMaxs.Y, 0f, roads.Height);
                    approachingCity = true;
                }
            }

            // Validate current target is still in range.
            if (agent.RoadNodeTarget >= 0)
            {
                if (agent.RoadNodeTarget >= graph.Nodes.Length)
                {
                    // Graph changed (different map), reset.
                    agent.RoadNodeTarget = -1;
                    agent.ClearRoadNodeHistory();
                }
                else
                {
                    var target = graph.Nodes[agent.RoadNodeTarget];
                    float dtx = target.X - bx;
                    float dty = target.Y - by;
                    float distToTargetSqr = dtx * dtx + dty * dty;

                    if (distToTargetSqr > RoadNodeDriftDist * RoadNodeDriftDist)
                    {
                        // Too far from target, reset navigation.
                        agent.RoadNodeTarget = -1;
                        agent.ClearRoadNodeHistory();
                    }
                    else if (distToTargetSqr < RoadNodeArrivalDist * RoadNodeArrivalDist)
                    {
                        // Arrived at target, push to history and pick next node.
                        agent.PushRoadNodeHistory(agent.RoadNodeTarget);
                        agent.RoadNodeTarget = PickNextRoadNode(graph, agent, agent.Velocity, state.Ticks, biasX, biasY);
                    }
                }
            }

            // Find initial target if we don't have one.
            if (agent.RoadNodeTarget < 0)
            {
                int nearest = graph.FindNearestNode(bx, by);
                if (nearest < 0)
                    return Vector3.Zero;

                var nearestNode = graph.Nodes[nearest];
                float ndx = nearestNode.X - bx;
                float ndy = nearestNode.Y - by;
                if (ndx * ndx + ndy * ndy > RoadNodeSearchDist * RoadNodeSearchDist)
                    return Vector3.Zero; // Too far from any road.

                // We're near this node, pick a connected node to walk toward.
                agent.ClearRoadNodeHistory();
                agent.PushRoadNodeHistory(nearest);
                agent.RoadNodeTarget = PickNextRoadNode(graph, agent, agent.Velocity, state.Ticks, biasX, biasY);

                if (agent.RoadNodeTarget < 0)
                    agent.RoadNodeTarget = nearest; // Isolated node, just attract to it.
            }

            // Steer toward the target node.
            {
                var target = graph.Nodes[agent.RoadNodeTarget];
                float dx = target.X - bx;
                float dy = target.Y - by;
                float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);

                if (dist > 0.1f)
                {
                    dx /= dist;
                    dy /= dist;
                }

                float typeScale = target.Type == RoadType.Asphalt ? 0.75f : 0.5f;

                // When approaching a city, check if the target road node actually
                // gets us closer. If not, return zero and let CityVisitor guide
                // the agent off-road toward the city.
                if (approachingCity)
                {
                    float curDistSqr = (biasX - bx) * (biasX - bx) + (biasY - by) * (biasY - by);
                    float tgtDistSqr = (biasX - target.X) * (biasX - target.X) + (biasY - target.Y) * (biasY - target.Y);
                    if (tgtDistSqr >= curDistSqr)
                        return Vector3.Zero; // Road node is farther from city; let CityVisitor take over.
                }

                return new Vector3(dx * typeScale * RoadForceScale, dy * typeScale * RoadForceScale) * power;
            }
        }

        /// <summary>
        /// Picks the next road node to navigate toward.
        /// Uses the agent's circular history buffer to avoid revisiting recent nodes.
        /// When biasX/biasY are not NaN, intersection choices prefer the node closest
        /// to the bias target (used for CityVisitor cooperation).
        /// </summary>
        internal static int PickNextRoadNode(RoadGraph graph, Agent agent,
            Vector3 velocity, uint tick, float biasX = float.NaN, float biasY = float.NaN)
        {
            // The most recently pushed history entry is the node we just arrived at.
            int arrivedAt = agent.RoadNodeHistoryCount > 0
                ? agent.RoadNodeHistory[(agent.RoadNodeHistoryPos - 1 + Agent.RoadNodeHistorySize) % Agent.RoadNodeHistorySize]
                : -1;

            if (arrivedAt < 0 || arrivedAt >= graph.Nodes.Length)
                return -1;

            var node = graph.Nodes[arrivedAt];
            var connections = node.Connections;

            if (connections.Length == 0)
                return -1; // Isolated node.

            if (connections.Length == 1)
                return connections[0]; // Dead end: traverse back.

            bool hasBias = !float.IsNaN(biasX);

            // At intersections (3+ connections) without a bias target, 33% chance to pick randomly for variety.
            bool pickRandom = !hasBias && connections.Length >= 3 && AgentHash(agent.Index, tick, 0) % 3 == 0;

            // Prepare velocity direction for velocity-aligned selection.
            float velX = velocity.X;
            float velY = velocity.Y;
            float velLen = (float)System.Math.Sqrt(velX * velX + velY * velY);
            if (velLen > 0.01f) { velX /= velLen; velY /= velLen; }

            // Count how many non-history candidates we have.
            int availableCount = 0;
            for (int i = 0; i < connections.Length; i++)
            {
                if (!agent.IsInRoadNodeHistory(connections[i]))
                    availableCount++;
            }

            // All neighbors are already in history — clear and pick any neighbor.
            if (availableCount == 0)
            {
                agent.ClearRoadNodeHistory();
                agent.PushRoadNodeHistory(arrivedAt);
                return connections[AgentHash(agent.Index, tick, 1) % connections.Length];
            }

            int bestIdx = -1;
            float bestScore = float.MaxValue;
            float bestDot = -2f;
            int fallbackIdx = -1;
            int candidateCount = 0;

            for (int i = 0; i < connections.Length; i++)
            {
                int connIdx = connections[i];

                if (agent.IsInRoadNodeHistory(connIdx))
                {
                    if (fallbackIdx < 0) fallbackIdx = connIdx;
                    continue;
                }

                candidateCount++;

                if (pickRandom)
                {
                    if (AgentHash(agent.Index, tick, 100 + i) % candidateCount == 0)
                        bestIdx = connIdx;
                }
                else if (hasBias)
                {
                    var nextNode = graph.Nodes[connIdx];
                    float dx = nextNode.X - biasX;
                    float dy = nextNode.Y - biasY;
                    float distSqr = dx * dx + dy * dy;
                    if (distSqr < bestScore)
                    {
                        bestScore = distSqr;
                        bestIdx = connIdx;
                    }
                }
                else if (velLen > 0.01f)
                {
                    var nextNode = graph.Nodes[connIdx];
                    var currentNode = graph.Nodes[arrivedAt];
                    float dx = nextNode.X - currentNode.X;
                    float dy = nextNode.Y - currentNode.Y;
                    float len = (float)System.Math.Sqrt(dx * dx + dy * dy);
                    if (len > 0) { dx /= len; dy /= len; }

                    float dot = velX * dx + velY * dy;
                    if (dot > bestDot)
                    {
                        bestDot = dot;
                        bestIdx = connIdx;
                    }
                }
                else
                {
                    if (AgentHash(agent.Index, tick, 200 + i) % candidateCount == 0)
                        bestIdx = connIdx;
                }
            }

            return bestIdx >= 0 ? bestIdx : (fallbackIdx >= 0 ? fallbackIdx : connections[0]);
        }

        internal static Vector3 AvoidRoads(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
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

        internal static Vector3 StickToPOIs(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
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


        internal static Vector3 AvoidPOIs(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
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

        internal static Vector3 WorldEvents(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
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

        internal static Vector3 PreferCities(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
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

        internal static Vector3 AvoidCities(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
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

        internal static Vector3 CityVisitor(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            if (state.MapData == null || state.MapData.Cities == null || state.MapData.Cities.CityList.Count == 0)
            {
                return Vector3.Zero;
            }

            var cities = state.MapData.Cities;
            var cityList = cities.CityList;
            int cityCount = cityList.Count;

            // State machine: Idle -> Approaching -> Arrived -> Idle
            if (agent.CurrentTravelState == Agent.TravelState.Idle)
            {
                // Select a new target city
                uint hash = (uint)((agent.Group * 2654435761) ^ ((uint)state.Ticks * 1597334677));
                float randomValue = (hash % 10000) / 10000.0f;
                float targetWeight = randomValue * cities.TotalAreaWeight;

                int targetCityIndex = 0;
                float accumulatedWeight = 0;
                for (int i = 0; i < cityCount; i++)
                {
                    accumulatedWeight += cities.CityAreaWeights[i];
                    if (accumulatedWeight >= targetWeight)
                    {
                        targetCityIndex = i;
                        break;
                    }
                }

                agent.TargetCityIndex = targetCityIndex;
                agent.CurrentTravelState = Agent.TravelState.Approaching;
            }

            if (agent.CurrentTravelState == Agent.TravelState.Approaching)
            {
                var targetCity = cityList[agent.TargetCityIndex];
                var agentPos = agent.Position;

                // Check if we've arrived at the target city
                if (agentPos.X >= targetCity.MinX && agentPos.X <= targetCity.MaxX &&
                    agentPos.Y >= targetCity.MinY && agentPos.Y <= targetCity.MaxY)
                {
                    agent.CurrentTravelState = Agent.TravelState.Arrived;
                    agent.CityTime = state.Ticks;
                    // Fall through to Arrived state handling
                }
                else
                {
                    // Approach the city
                    float closestX = System.Math.Max(targetCity.MinX, System.Math.Min(agentPos.X, targetCity.MaxX));
                    float closestY = System.Math.Max(targetCity.MinY, System.Math.Min(agentPos.Y, targetCity.MaxY));
                    
                    float dx = closestX - agentPos.X;
                    float dy = closestY - agentPos.Y;

                    if (dx != 0 || dy != 0)
                    {
                        float invMag = 1f / (float)System.Math.Sqrt(dx * dx + dy * dy);
                        return new Vector3(dx * invMag * power, dy * invMag * power, 0);
                    }
                    return Vector3.Zero;
                }
            }
            
            if (agent.CurrentTravelState == Agent.TravelState.Arrived)
            {
                // Stay time from Param1 (min minutes) and Param2 (max minutes)
                float minStay = param1 > 0 ? param1 : 20f;
                float maxStay = param2 > param1 ? param2 : minStay;
                float stayMinutes = minStay + (AgentHash(agent.Index, state.Ticks, 300) / (float)0x7FFFFFFF) * (maxStay - minStay);
                ulong cityDuration = Simulation.MinutesToTicks((uint)stayMinutes);
                if ((state.Ticks - agent.CityTime) >= cityDuration)
                {
                    agent.CurrentTravelState = Agent.TravelState.Idle;
                    return Vector3.Zero;
                }

                // Wander within the city
                var targetCity = cityList[agent.TargetCityIndex];
                const float edgeMargin = 20f;
                float cityWidth = targetCity.Bounds.X - (edgeMargin * 2);
                float cityHeight = targetCity.Bounds.Y - (edgeMargin * 2);

                // Change target position every ~60 seconds for more exploration
                uint timeSegment = state.Ticks / 3600;

                // Generate unique target for this time segment using agent group
                uint seedX = (uint)(agent.Group * 2654435761 + timeSegment * 1640531527);
                uint seedY = (uint)(agent.Group * 1597334677 + timeSegment * 3266489917);

                // Map to full city area uniformly
                float targetOffsetX = ((seedX % 10000) / 10000.0f * 2.0f - 1.0f) * (cityWidth / 2);
                float targetOffsetY = ((seedY % 10000) / 10000.0f * 2.0f - 1.0f) * (cityHeight / 2);

                float targetX = targetCity.Position.X + targetOffsetX;
                float targetY = targetCity.Position.Y + targetOffsetY;
                
                float dx = targetX - agent.Position.X;
                float dy = targetY - agent.Position.Y;
                float distSqr = dx * dx + dy * dy;

                if (distSqr > 1f)
                {
                    float invMag = 1f / (float)System.Math.Sqrt(distSqr);
                    float forceMult = System.Math.Min((float)System.Math.Sqrt(distSqr) / 30f, 1f);
                    return new Vector3(dx * invMag * power * forceMult * 0.5f, dy * invMag * power * forceMult * 0.5f, 0);
                }
            }

            return Vector3.Zero;
        }

        // Distance (in SDF pixels) over which the outer biome force ramps from zero to full.
        private const float BiomeForceFalloff = 20f;
        // How far inside (SDF pixels) agents still get a gentle inward nudge.
        private const float BiomeInnerBand = 10f;
        // Strength of the inner-band nudge relative to full power.
        private const float BiomeInnerStrength = 0.25f;

        internal static Vector3 BiomeForce(Simulation sim, State state, Agent agent, float power, float param1, float sign)
        {
            if (state.MapData == null)
                return Vector3.Zero;

            var biomes = state.MapData.Biomes;
            if (biomes == null || biomes.SDFWidth == 0)
                return Vector3.Zero;

            var biomeType = (Biomes.Type)(byte)param1;

            // Remap agent position to biome-map pixel space.
            var pos = agent.Position;
            float bx = MathEx.Remap(pos.X, state.WorldMins.X, state.WorldMaxs.X, 0f, biomes.Width);
            float by = MathEx.Remap(pos.Y, state.WorldMins.Y, state.WorldMaxs.Y, 0f, biomes.Height);

            float sdf = biomes.SampleSDF(biomeType, bx, by);

            // StickToBiome (sign>0): sdf<0 means outside, sdf>0 means inside.
            // AvoidBiome  (sign<0): sdf>0 means inside (bad), sdf<0 means outside (good).
            // 'depth' is positive when the agent is on the wrong side of the boundary.
            float depth = sign > 0 ? -sdf : sdf;

            float strength;
            if (depth > 0f)
            {
                // Wrong side: ramp force up with distance from boundary.
                strength = System.Math.Min(depth / BiomeForceFalloff, 1f);
            }
            else if (-depth < BiomeInnerBand)
            {
                // Right side but near the boundary: gentle nudge to stay inside.
                // Fades linearly from BiomeInnerStrength at boundary to 0 at BiomeInnerBand.
                strength = BiomeInnerStrength * (1f - (-depth / BiomeInnerBand));
            }
            else
            {
                // Deep on the right side: no force needed.
                return Vector3.Zero;
            }

            var grad = biomes.SampleSDFGradient(biomeType, bx, by);
            float len = grad.Magnitude();
            if (len < 0.001f)
                return Vector3.Zero;

            return (grad / len) * (power * sign * strength);
        }

        internal static Vector3 StickToBiome(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            return BiomeForce(sim, state, agent, power, param1, 1f);
        }

        internal static Vector3 AvoidBiome(Simulation sim, State state, Agent agent, float distance, float power, float param1, float param2)
        {
            return BiomeForce(sim, state, agent, power, param1, -1f);
        }

    }
}
