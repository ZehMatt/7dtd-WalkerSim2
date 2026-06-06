using System;
using System.Threading;

namespace WalkerSim
{
    public partial class Simulation
    {
        public static Simulation Instance = new Simulation();

        public float TimeScale = 1.0f;

        private Thread _thread;
        private bool _running = false;
        private bool _shouldStop = false;
        private bool _pauseRequested = false;
        private volatile bool _gamePaused = false;
        private Vector3[] _groupStarts = new Vector3[0];
        private int[] _groupSizes = new int[0];

        private int _maxAllowedAliveAgents = 64;
        private float _moveSpeedDay = 1.0f;
        private float _moveSpeedNight = 1.0f;
        private float _moveSpeedRageDay = 2.5f;
        private float _moveSpeedRageNight = 2.5f;

        public int MaxAllowedAliveAgents
        {
            get
            {
                return _maxAllowedAliveAgents;
            }
        }

        TimeMeasurement _updateTime = new TimeMeasurement();

        public bool EditorMode = false;

        public void Stop()
        {
            if (_thread == null)
            {
                return;
            }

            Logging.CondInfo(Config.LoggingOpts.General, () => "Stopping simulation...");

            _shouldStop = true;
            _thread.Join();
            _thread = null;
            _running = false;
            _nextAutoSave = DateTime.MaxValue;
        }

        public void Shutdown()
        {
            Stop();

            _state.Players.Clear();
            _state.Agents.Clear();
            _state.Spawned.Clear();
            _state.Events.Clear();
            _state.IsBloodmoon = false;
            _state.IsDayTime = true;

            // Prevent auto save writing empty state.
            _autoSaveFile = null;
        }

        public void Advance(uint numTicks)
        {
            if (_running)
            {
                throw new Exception("Can't advance the simulation while its running");
            }

            for (uint i = 0; i < numTicks; i++)
            {
                Tick();
            }
        }

        public void Start()
        {
            Stop();

            if (_autoSaveInterval != -1)
            {
                _nextAutoSave = DateTime.UtcNow.AddSeconds(_autoSaveInterval);
            }

            Logging.CondInfo(Config.LoggingOpts.General, () => "Starting simulation...");

            _running = true;
            _shouldStop = false;
            _thread = new Thread(ThreadUpdate);
            _thread.Start();
        }

        public void SetWorldSize(Vector3 worldMins, Vector3 worldMaxs)
        {
            lock (_state)
            {
                _state.WorldMins = worldMins;
                _state.WorldMaxs = worldMaxs;

                UpdateGrid();
            }
        }

        public void SetWorldName(string worldName)
        {
            lock (_state)
            {
                _state.WorldName = worldName;
            }
        }

        public void SetMaxAllowedAliveAgents(int maxAlive)
        {
            _maxAllowedAliveAgents = maxAlive;
        }

        public void Reset(Config config)
        {
            Stop();

            lock (_state)
            {
                _state.SoftReset();
                _state.Config = config;
                _state.PRNG = new WalkerSim.Random(config.RandomSeed);
                // The game presents day 1 at the start, the mod stores that 1-based value via
                // worldTime/24000 + 1. The editor self-advances time, so start it at day 1 too,
                // otherwise the first day stays in the population ramp's clamped-flat region.
                _state.GameTime = EditorMode ? 1.0 : 0.0;
                _state.WindDir = new Vector3(1, 0, 0);
                _state.WindDirTarget = new Vector3(1, 0, 0);
                _state.WindTime = 0;

                if (config.Processors.Count == 0)
                {
                    _state.Agents.Clear();
                    return;
                }

                SetupGrid();

                // SetupProcessors computes the group partition (GroupCount, per-group sizes
                // and system mapping) from the per-system GroupSize and Weight, so it must run
                // before BuildGroupStarts/Populate which depend on it.
                SetupProcessors();
                BuildGroupStarts();
                Populate();

                _simTime.Reset();
                _updateTime.Reset();
            }
        }

        public void SetPaused(bool paused)
        {
            if (_pauseRequested != paused)
            {
                if (paused)
                {
                    Logging.Out("Paused simulation.");
                }
                else
                {
                    Logging.Out("Resuming simulation.");
                }
            }
            _pauseRequested = paused;
        }

        public void EntityKilled(int entityId)
        {
            if (_state.Spawned.TryGetValue(entityId, out var agent))
            {
                MarkAgentDead(agent);
            }
        }

        public bool LoadMapData(string directoryPath, string worldName)
        {
            lock (_state)
            {
                var mapData = MapData.LoadFromFolder(directoryPath);
                if (mapData == null)
                {
                    return false;
                }

                _state.MapData = mapData;

                if (WorldSize != mapData.WorldSize)
                {
                    SetWorldSize(mapData.WorldMins, mapData.WorldMaxs);
                }

                SetWorldName(worldName);

                return true;
            }
        }

        Vector3 GetRandomPosition(Random prng)
        {
            float borderSize = 250;
            float x0 = (float)prng.NextDouble();
            float y0 = (float)prng.NextDouble();
            float x = MathEx.Remap(x0, 0f, 1f, _state.WorldMins.X + borderSize, _state.WorldMaxs.X - borderSize);
            float y = MathEx.Remap(y0, 0f, 1f, _state.WorldMins.Y + borderSize, _state.WorldMaxs.Y - borderSize);
            return new Vector3(x, y);
        }

        Vector3 GetRandomBorderPosition(Random prng)
        {
            Vector3 res = new Vector3();

            float borderSize = 250;
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            // Select border side.
            int side = prng.Next(0, 4);
            if (side == 0)
            {
                // Top.
                float x0 = (float)prng.NextDouble();
                res.X = MathEx.Remap(x0, 0f, 1f, worldMins.X + borderSize, worldMaxs.X - borderSize);
                res.Y = worldMins.Y + borderSize;
            }
            else if (side == 1)
            {
                // Right.
                res.X = worldMaxs.X - borderSize;
                float y0 = (float)prng.NextDouble();
                res.Y = MathEx.Remap(y0, 0f, 1f, worldMins.Y + borderSize, worldMaxs.Y - borderSize);
            }
            else if (side == 2)
            {
                // Bottom.
                float x0 = (float)prng.NextDouble();
                res.X = MathEx.Remap(x0, 0f, 1f, worldMins.X + borderSize, worldMaxs.X - borderSize);
                res.Y = worldMaxs.Y - borderSize;
            }
            else if (side == 3)
            {
                // Left.
                res.X = worldMins.X + borderSize;
                float y0 = (float)prng.NextDouble();
                res.Y = MathEx.Remap(y0, 0f, 1f, worldMins.Y + borderSize, worldMaxs.Y - borderSize);
            }

            return res;
        }

        Vector3 GetRandomPOIPosition(Random prng)
        {
            var mapData = _state.MapData;
            if (mapData == null)
            {
                // Can be null in viewer.
                return GetRandomBorderPosition(prng);
            }

            var prefabs = mapData.Prefabs;
            var decos = prefabs.Decorations;
            if (decos.Length == 0)
            {
                // No decorations, fallback to random border position.
                return GetRandomBorderPosition(prng);
            }

            // Weighted selection, the bigger the decoration, the more likely it is to be selected.
            var totalArea = 0.0f;
            foreach (var deco in decos)
            {
                totalArea += deco.Bounds.X * deco.Bounds.Y;
            }

            var selectedArea = 0.0f;
            var selectedDeco = decos[0];
            var rand = (float)prng.NextDouble() * totalArea;
            for (int i = 0; i < decos.Length; i++)
            {
                var deco = decos[i];
                var area = deco.Bounds.X * deco.Bounds.Y;
                selectedArea += area;
                if (selectedArea >= rand)
                {
                    selectedDeco = deco;
                    break;
                }
            }

            var bounds = selectedDeco.Bounds;
            var pos = selectedDeco.Position;

            var offset = new Vector3(
                -(bounds.X / 2) + ((float)prng.NextDouble() * bounds.X),
                -(bounds.Y / 2) + ((float)prng.NextDouble() * bounds.Y)
                );

            return pos + offset;
        }

        Vector3 GetRandomCityPosition(Random prng)
        {
            var mapData = _state.MapData;
            if (mapData == null)
            {
                // Can be null in viewer.
                return GetRandomBorderPosition(prng);
            }

            var cities = mapData.Cities;
            if (cities.CityList.Count == 0)
            {
                // No cities, fallback to random border position.
                return GetRandomBorderPosition(prng);
            }

            // Weighted selection by actual cell-count area (precomputed), so
            // irregular shapes don't get inflated by their bounding box.
            var selectedCity = cities.CityList[0];
            if (cities.TotalAreaWeight > 0f)
            {
                float rand = (float)prng.NextDouble() * cities.TotalAreaWeight;
                float acc = 0f;
                for (int i = 0; i < cities.CityList.Count; i++)
                {
                    acc += cities.CityAreaWeights[i];
                    if (acc >= rand)
                    {
                        selectedCity = cities.CityList[i];
                        break;
                    }
                }
            }

            // Pick a random POI from the city and jitter within its footprint.
            // This keeps spawns on actual occupied cells for non-rectangular cities.
            if (selectedCity.POIs.Count == 0)
            {
                return selectedCity.Position;
            }

            var poi = selectedCity.POIs[prng.Next(selectedCity.POIs.Count)];
            var offset = new Vector3(
                -(poi.Bounds.X / 2) + ((float)prng.NextDouble() * poi.Bounds.X),
                -(poi.Bounds.Y / 2) + ((float)prng.NextDouble() * poi.Bounds.Y)
                );

            return poi.Position + offset;
        }

        Vector3 GetGroupCenter(int groupIndex)
        {
            var agents = _state.Agents;
            var sum = Vector3.Zero;
            int count = 0;

            for (int i = 0; i < agents.Count; i++)
            {
                var agent = agents[i];
                if (agent.Group == groupIndex && agent.CurrentState == Agent.State.Wandering)
                {
                    sum += agent.Position;
                    count++;
                }
            }

            // Fall back to original group start if no active agents in this group.
            if (count == 0)
            {
                return _groupStarts[groupIndex];
            }

            return sum * (1f / count);
        }

        Vector3 GetWorldLocation(Config.WorldLocation worldLoc, Config.WorldBiome biome, Random prng)
        {
            if (worldLoc == Config.WorldLocation.Mixed)
            {
                var min = Config.WorldLocation.RandomBorderLocation;
                var max = Config.WorldLocation.RandomCity;
                worldLoc = (Config.WorldLocation)prng.Next((int)min, (int)max + 1);
            }

            if (biome == Config.WorldBiome.Any || !HasBiomeData())
            {
                return GetLocationPosition(worldLoc, prng);
            }

            // Rejection-sample the chosen location type until it lands in the target biome.
            var biomeType = MapBiome(biome);
            const int maxAttempts = 64;
            for (int i = 0; i < maxAttempts; i++)
            {
                var pos = GetLocationPosition(worldLoc, prng);
                if (GetBiomeAt(pos) == biomeType)
                {
                    return pos;
                }
            }

            // Biome absent or too sparse for this location type, accept the last attempt.
            return GetLocationPosition(worldLoc, prng);
        }

        Vector3 GetLocationPosition(Config.WorldLocation worldLoc, Random prng)
        {
            switch (worldLoc)
            {
                case Config.WorldLocation.RandomBorderLocation:
                    return GetRandomBorderPosition(prng);
                case Config.WorldLocation.RandomLocation:
                    return GetRandomPosition(prng);
                case Config.WorldLocation.RandomPOI:
                    return GetRandomPOIPosition(prng);
                case Config.WorldLocation.RandomCity:
                    return GetRandomCityPosition(prng);
            }

            // This should never happen.
            throw new System.Exception("Bad starting location type");
        }

        bool HasBiomeData()
        {
            var mapData = _state.MapData;
            return mapData != null && mapData.Biomes != null && mapData.Biomes.Width != 0;
        }

        Biomes.Type GetBiomeAt(Vector3 pos)
        {
            var biomes = _state.MapData.Biomes;
            int bx = (int)MathEx.Remap(pos.X, _state.WorldMins.X, _state.WorldMaxs.X, 0f, biomes.Width);
            int by = (int)MathEx.Remap(pos.Y, _state.WorldMins.Y, _state.WorldMaxs.Y, 0f, biomes.Height);
            return biomes.GetBiomeType(bx, by);
        }

        static Biomes.Type MapBiome(Config.WorldBiome biome)
        {
            switch (biome)
            {
                case Config.WorldBiome.Snow:
                    return Biomes.Type.Snow;
                case Config.WorldBiome.PineForest:
                    return Biomes.Type.PineForest;
                case Config.WorldBiome.Desert:
                    return Biomes.Type.Desert;
                case Config.WorldBiome.Wasteland:
                    return Biomes.Type.Wasteland;
                case Config.WorldBiome.BurntForest:
                    return Biomes.Type.BurntForest;
                default:
                    return Biomes.Type.Invalid;
            }
        }

        private Config.WorldLocation GetSystemStartPosition(int group)
        {
            if (group >= 0 && group < _groupToSystemIndex.Length)
            {
                int sys = _groupToSystemIndex[group];
                if (sys >= 0 && sys < _state.Config.Processors.Count)
                {
                    return _state.Config.Processors[sys].StartPosition;
                }
            }
            return Config.WorldLocation.RandomLocation;
        }

        private Config.WorldLocation GetSystemRespawnPosition(int group)
        {
            if (group >= 0 && group < _groupToSystemIndex.Length)
            {
                int sys = _groupToSystemIndex[group];
                if (sys >= 0 && sys < _state.Config.Processors.Count)
                {
                    return _state.Config.Processors[sys].RespawnPosition;
                }
            }
            return Config.WorldLocation.None;
        }

        private Config.WorldBiome GetSystemStartBiome(int group)
        {
            if (group >= 0 && group < _groupToSystemIndex.Length)
            {
                int sys = _groupToSystemIndex[group];
                if (sys >= 0 && sys < _state.Config.Processors.Count)
                {
                    return _state.Config.Processors[sys].StartBiome;
                }
            }
            return Config.WorldBiome.Any;
        }

        private Config.WorldBiome GetSystemRespawnBiome(int group)
        {
            if (group >= 0 && group < _groupToSystemIndex.Length)
            {
                int sys = _groupToSystemIndex[group];
                if (sys >= 0 && sys < _state.Config.Processors.Count)
                {
                    return _state.Config.Processors[sys].RespawnBiome;
                }
            }
            return Config.WorldBiome.Any;
        }

        Vector3 GetStartLocationForGroup(int group)
        {
            var loc = GetSystemStartPosition(group);
            if (loc == Config.WorldLocation.None)
            {
                loc = Config.WorldLocation.RandomLocation;
            }

            return GetWorldLocation(loc, GetSystemStartBiome(group), _state.PRNG);
        }

        Vector3 GetRespawnLocationForGroup(int group)
        {
            return GetWorldLocation(GetSystemRespawnPosition(group), GetSystemRespawnBiome(group), _state.PRNG);
        }

        void BuildGroupStarts()
        {
            var config = _state.Config;
            var prng = new Random(config.RandomSeed);

            _groupStarts = new Vector3[_state.GroupCount];
            for (int i = 0; i < _groupStarts.Length; i++)
            {
                var loc = GetSystemStartPosition(i);
                if (loc == Config.WorldLocation.None)
                {
                    loc = Config.WorldLocation.RandomLocation;
                }

                _groupStarts[i] = GetWorldLocation(loc, GetSystemStartBiome(i), prng);
            }
        }

        private int ComputeMaxAgents()
        {
            var sqrKm = (WorldSize.X / 1000.0f) * (WorldSize.Y / 1000.0f);
            if (sqrKm <= 0)
            {
                return 0;
            }

            var maxAgents = (int)System.Math.Ceiling(sqrKm * _state.Config.PopulationDensity);
            return MathEx.Clamp(maxAgents, 1, Limits.MaxAgents);
        }

        private int GetConfiguredGroupSize(int group)
        {
            if (group >= 0 && group < _groupToSystemIndex.Length)
            {
                int sys = _groupToSystemIndex[group];
                if (sys >= 0 && sys < _state.Config.Processors.Count)
                {
                    return System.Math.Max(1, _state.Config.Processors[sys].GroupSize);
                }
            }
            return Config.DefaultGroupSize;
        }

        Vector3 GetStartLocation(int index, int groupIndex)
        {
            var config = _state.Config;

            // Give each agent 2 meters distance to each other.
            var maxDistance = MathEx.Clamp((float)GetConfiguredGroupSize(groupIndex) * 6.0f, 16.0f, 500.0f);

            if (config.StartAgentsGrouped)
            {
                // Spawn in circle.
                float angle = (float)_state.PRNG.NextDouble() * (float)System.Math.PI * 2.0f;
                float radius = (float)_state.PRNG.NextDouble() * maxDistance;
                float offsetX = (float)System.Math.Cos(angle) * radius;
                float offsetY = (float)System.Math.Sin(angle) * radius;

                return _groupStarts[groupIndex] + new Vector3(offsetX, offsetY);
            }
            else
            {
                return GetStartLocationForGroup(groupIndex);
            }
        }

        void Populate()
        {
            var agents = _state.Agents;

            agents.Clear();
            SetupGrid();

            if (_state.GroupCount == 0)
            {
                return;
            }

            int totalAgents = 0;
            for (int g = 0; g < _state.GroupCount; g++)
            {
                totalAgents += _groupSizes[g];
            }

            if (totalAgents == 0)
            {
                return;
            }

            // If population ramp is configured, only a fraction of agents start as Wandering.
            var popFraction = GetPopulationFraction();
            var targetTotal = popFraction >= 1f ? totalAgents : (int)(totalAgents * popFraction);

            // Pre-compute active count per group proportional to each group's size.
            var activeForGroup = new int[_state.GroupCount];
            if (popFraction >= 1f)
            {
                for (int g = 0; g < _state.GroupCount; g++)
                {
                    activeForGroup[g] = _groupSizes[g];
                }
            }
            else
            {
                int assigned = 0;
                for (int g = 0; g < _state.GroupCount; g++)
                {
                    int groupTarget = (int)(_groupSizes[g] * popFraction);
                    activeForGroup[g] = groupTarget;
                    assigned += groupTarget;
                }
                // Distribute any rounding remainder across groups.
                for (int g = 0; assigned < targetTotal && g < _state.GroupCount; g++)
                {
                    if (activeForGroup[g] < _groupSizes[g])
                    {
                        activeForGroup[g]++;
                        assigned++;
                    }
                }
            }

            int index = 0;
            for (int g = 0; g < _state.GroupCount; g++)
            {
                int groupSize = _groupSizes[g];
                for (int indexInGroup = 0; indexInGroup < groupSize; indexInGroup++)
                {
                    var agent = new Agent(index, g);
                    agent.LastUpdateTick = _state.Ticks;
                    agent.Position = GetStartLocation(index, g);

                    if (indexInGroup < activeForGroup[g])
                    {
                        agent.CurrentState = Agent.State.Wandering;
                    }

                    // Ensure the position is not out of bounds.
                    Warp(agent);

                    agents.Add(agent);

                    MoveInGrid(agent);

                    index++;
                }
            }
        }

        private bool IsPaused()
        {
            if (_pauseRequested || _gamePaused)
            {
                return true;
            }

            // Don't simulate with no registered players.
            if (_state.Players.Count == 0 && !EditorMode)
            {
                return true;
            }

            if (Config.PauseDuringBloodmoon && _state.IsBloodmoon)
            {
                // Pause during bloodmoon if configured.
                return true;
            }

            return false;
        }

        private void ThreadUpdate()
        {
            Logging.CondInfo(Config.LoggingOpts.General, () => "Started simulation.");

            _updateTime.Restart();

            float accumulator = 0;
            float unscaledAccumulator = 0;
            bool ranTicks = false;
            while (!_shouldStop)
            {
                var timeElapsed = _updateTime.Elapsed();
                // Only the cycles that actually ran ticks are meaningful as "update time";
                // recording the idle spins would drag the average below tick time.
                if (ranTicks)
                {
                    _updateTime.Add(timeElapsed);
                }

                _updateTime.Restart();
                ranTicks = false;

                var scaledDt = (float)(timeElapsed * TimeScale);

                if (IsPaused())
                {
                    Thread.Sleep(100);
                    continue;
                }

                accumulator = System.Math.Min(accumulator + scaledDt, 20.0f);
                unscaledAccumulator = System.Math.Min(unscaledAccumulator + timeElapsed, 20.0f);

                uint unscaledTicks = (uint)(unscaledAccumulator / Constants.TickRate);
                _state.UnscaledTicks += unscaledTicks;
                unscaledAccumulator -= unscaledTicks * Constants.TickRate;

                if (accumulator < Constants.TickRate)
                {
                    if (_shouldStop)
                    {
                        break;
                    }

                    // Yield to prevent busy-wait CPU spike, but don't sleep (too slow on Mono)
                    if (TimeScale <= 1.0f)
                    {
                        Thread.Sleep(0);
                    }
                }

                while (accumulator >= Constants.TickRate)
                {
                    Tick();

                    accumulator -= Constants.TickRate;

                    if (_shouldStop)
                    {
                        break;
                    }

                    ranTicks = true;
                }

                // Don't tie this to ticks, this should work with any time scale and tick rate.
                CheckAgentSpawn();

                if (_shouldStop)
                {
                    break;
                }
            }

            _running = false;
            _shouldStop = false;

            Logging.CondInfo(Config.LoggingOpts.General, () => "Simulation stopped.");
        }

        // Called from the main thread, this should be invoked from GameUpdate.
        public void GameUpdate(float deltaTime)
        {
            if (!_running || _shouldStop || IsPaused())
            {
                return;
            }

            ProcessSpawnQueue();
            CheckAgentDespawn();
        }

        public Vector3 RemapPosition2D(Vector3 pos, Vector3 min, Vector3 max)
        {
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            pos.X = MathEx.Remap(pos.X, worldMins.X, worldMaxs.X, min.X, max.X);
            pos.Y = MathEx.Remap(pos.Y, worldMins.Y, worldMaxs.Y, min.Y, max.Y);
            pos.Z = 0;

            return pos;
        }

        public void ReloadConfig(Config config)
        {
            lock (_state)
            {
                _state.Config = config;

                if (config.Processors.Count == 0)
                {
                    _state.Agents.Clear();
                    return;
                }

                var prevGroupSizes = _groupSizes;
                var prevGroupToSystem = _groupToSystemIndex;

                SetupProcessors();

                // Rebuild group start positions so a live per-system StartPosition change takes
                // effect for future respawns. Deterministic, so it's a no-op when unchanged.
                BuildGroupStarts();

                // A GroupSize/Weight change re-partitions the same population into different
                // groups; density/world changes alter the agent count and need a full rebuild.
                bool partitionChanged = !IntArraysEqual(prevGroupSizes, _groupSizes)
                    || !IntArraysEqual(prevGroupToSystem, _groupToSystemIndex);

                if (partitionChanged)
                {
                    int totalAgents = 0;
                    for (int g = 0; g < _state.GroupCount; g++)
                    {
                        totalAgents += _groupSizes[g];
                    }

                    if (_state.Agents.Count != totalAgents)
                    {
                        // Agent count changed (e.g. first populate or a density change); rebuild.
                        Populate();
                        return;
                    }

                    // Same population, different grouping: re-bucket the existing agents into the
                    // new groups in place so they keep their positions and the simulation keeps
                    // running, instead of resetting everyone back to spawn.
                    int idx = 0;
                    for (int g = 0; g < _state.GroupCount; g++)
                    {
                        int size = _groupSizes[g];
                        for (int i = 0; i < size; i++)
                        {
                            _state.Agents[idx++].Group = g;
                        }
                    }
                }
                else if (_state.Agents.Count == 0)
                {
                    Populate();
                    return;
                }

                // Only clear travel state whose owning processor is no longer present for the
                // agent's group, so a stale CityVisitor/StickToRoads target can't strand an
                // agent. Editing an unrelated parameter keeps the processor, so its travel
                // state is preserved (e.g. a CityVisitor group keeps heading to its city).
                var agents = _state.Agents;
                for (int i = 0; i < agents.Count; i++)
                {
                    var agent = agents[i];

                    if (!GroupHasProcessor(agent.Group, Config.MovementProcessorType.CityVisitor))
                    {
                        agent.CurrentTravelState = Agent.TravelState.Idle;
                        agent.TargetCityIndex = -1;
                        agent.CityArrivalDay = 0;
                    }

                    if (!GroupHasProcessor(agent.Group, Config.MovementProcessorType.StickToRoads))
                    {
                        agent.RoadNodeTarget = -1;
                        agent.ClearRoadNodeHistory();
                    }
                }
            }
        }

        private static bool IntArraysEqual(int[] a, int[] b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        public Drawing.Color GetGroupColor(int groupIndex)
        {
            if (groupIndex >= _processors.Count)
            {
                return Drawing.Color.Gray;
            }

            var proc = _processors[groupIndex];
            if (proc == null)
            {
                return Drawing.Color.Gray;
            }

            return proc.Color;
        }

        static public uint MillisecondsToTicks(uint milliseconds)
        {
            return (milliseconds * Constants.TicksPerSecond) / 1000;
        }

        static public uint SecondsToTicks(uint seconds)
        {
            return seconds * Constants.TicksPerSecond;
        }

        static public uint MinutesToTicks(uint minutes)
        {
            return SecondsToTicks(minutes * 60);
        }

        public double GetSimulationTimeSeconds()
        {
            return _state.UnscaledTicks / (double)Constants.TicksPerSecond;
        }

        public void SetIsBloodmoon(bool bloodmoon)
        {
            _state.IsBloodmoon = bloodmoon;
        }

        public void SetIsDayTime(bool isDay)
        {
            _state.IsDayTime = isDay;
        }

        public void SetGameTime(double gameTime)
        {
            _state.GameTime = gameTime;
        }

        public void SetGamePaused(bool paused)
        {
            _gamePaused = paused;
        }

        public void SetMoveSpeeds(float daySpeed, float nightSpeed, float daySpeedRage, float nightSpeedRage)
        {
            _moveSpeedDay = daySpeed;
            _moveSpeedNight = nightSpeed;
            _moveSpeedRageDay = daySpeedRage;
            _moveSpeedRageNight = nightSpeedRage;
        }
    }

}
