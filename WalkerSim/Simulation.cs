using System;
using System.Threading;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public static Simulation Instance = new Simulation();

        public float TimeScale = 1.0f;

        private Thread _thread;
        private bool _running = false;
        private bool _shouldStop = false;
        private bool _pauseRequested = false;
        private bool _gamePaused = false;
        private bool _isFastAdvancing = false;

        private Vector3[] _groupStarts = new Vector3[0];

        private int _maxAllowedAliveAgents = 64;
        private float _moveSpeedDay = 1.0f;
        private float _moveSpeedNight = 1.0f;
        private float _moveSpeedRageDay = 2.5f;
        private float _moveSpeedRageNight = 2.5f;

        public int MaxAllowedAliveAgents
        {
            get { return _maxAllowedAliveAgents; }
        }

        TimeMeasurement _updateTime = new TimeMeasurement();

        public bool EditorMode = false;

        public void Stop()
        {
            if (_thread == null)
            {
                return;
            }

            Logging.CondInfo(Config.LoggingOpts.General, "Stopping simulation...");

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
            _state.Active.Clear();
            _state.Events.Clear();
            _state.IsBloodmoon = false;
            _state.IsDayTime = true;

            // Prevent auto save writing empty state.
            _autoSaveFile = null;
        }

        public void Advance(uint numTicks)
        {
            if (_running)
                throw new Exception("Can't advance the simulation while its running");

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
                _nextAutoSave = DateTime.Now.AddSeconds(_autoSaveInterval);
            }

            Logging.CondInfo(Config.LoggingOpts.General, "Starting simulation...");

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

                if (config.Processors.Count == 0)
                {
                    _state.Agents.Clear();
                    return;
                }

                SetupGrid();
                Populate();
                SetupProcessors();

                _simTime.Reset();
                _updateTime.Reset();
            }
        }

        public void SetPaused(bool paused)
        {
            if (_pauseRequested != paused)
            {
                if (paused)
                    Logging.Out("Paused simulation.");
                else
                    Logging.Out("Resuming simulation.");
            }
            _pauseRequested = paused;
        }

        public void EntityKilled(int entityId)
        {
            if (_state.Active.TryGetValue(entityId, out var agent))
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
                    return false;

                _state.MapData = mapData;

                if (WorldSize != mapData.WorldSize)
                {
                    SetWorldSize(mapData.WorldMins, mapData.WorldMaxs);
                }

                SetWorldName(worldName);

                return true;
            }
        }

        Vector3 GetRandomPosition()
        {
            var prng = _state.PRNG;
            float borderSize = 250;
            float x0 = (float)prng.NextDouble();
            float y0 = (float)prng.NextDouble();
            float x = MathEx.Remap(x0, 0f, 1f, _state.WorldMins.X + borderSize, _state.WorldMaxs.X - borderSize);
            float y = MathEx.Remap(y0, 0f, 1f, _state.WorldMins.Y + borderSize, _state.WorldMaxs.Y - borderSize);
            return new Vector3(x, y);
        }

        Vector3 GetRandomBorderPosition()
        {
            Vector3 res = new Vector3();

            float borderSize = 250;
            var prng = _state.PRNG;
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

        Vector3 GetRandomPOIPosition()
        {
            var mapData = _state.MapData;
            if (mapData == null)
            {
                // Can be null in viewer.
                return GetRandomBorderPosition();
            }

            var prefabs = mapData.Prefabs;
            var decos = prefabs.Decorations;
            if (decos.Length == 0)
            {
                // No decorations, fallback to random border position.
                return GetRandomBorderPosition();
            }

            var prng = _state.PRNG;

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

        Vector3 GetRandomCityPosition()
        {
            var mapData = _state.MapData;
            if (mapData == null)
            {
                // Can be null in viewer.
                return GetRandomBorderPosition();
            }

            var cities = mapData.Cities;
            if (cities.CityList.Count == 0)
            {
                // No cities, fallback to random border position.
                return GetRandomBorderPosition();
            }

            // Weighted selection, the bigger the city, the more likely it is to be selected.
            var totalArea = 0.0f;
            foreach (var city in cities.CityList)
            {
                totalArea += city.Bounds.X * city.Bounds.Y;
            }

            var prng = _state.PRNG;
            var selectedArea = 0.0f;
            var selectedCity = cities.CityList[0];
            var rand = (float)prng.NextDouble() * totalArea;

            for (int i = 0; i < cities.CityList.Count; i++)
            {
                var city = cities.CityList[i];
                var area = city.Bounds.X * city.Bounds.Y;
                selectedArea += area;
                if (selectedArea >= rand)
                {
                    selectedCity = city;
                    break;
                }
            }

            var bounds = selectedCity.Bounds;
            var pos = selectedCity.Position;

            var offset = new Vector3(
                -(bounds.X / 2) + ((float)prng.NextDouble() * bounds.X),
                -(bounds.Y / 2) + ((float)prng.NextDouble() * bounds.Y)
                );

            return pos + offset;
        }

        Vector3 GetGroupPosition(int groupIndex)
        {
            return _groupStarts[groupIndex];
        }

        Vector3 GetWorldLocation(Config.WorldLocation worldLoc)
        {
            var config = _state.Config;
            var prng = _state.PRNG;

            if (worldLoc == Config.WorldLocation.Mixed)
            {
                var min = Config.WorldLocation.RandomBorderLocation;
                var max = Config.WorldLocation.RandomCity;
                worldLoc = (Config.WorldLocation)prng.Next((int)min, (int)max + 1);
            }

            switch (worldLoc)
            {
                case Config.WorldLocation.None:
                    break;
                case Config.WorldLocation.RandomBorderLocation:
                    return GetRandomBorderPosition();
                case Config.WorldLocation.RandomLocation:
                    return GetRandomPosition();
                case Config.WorldLocation.RandomPOI:
                    return GetRandomPOIPosition();
                case Config.WorldLocation.RandomCity:
                    return GetRandomCityPosition();
            }

            // This should never happen.
            throw new System.Exception("Bad starting location type");
        }

        Vector3 GetStartLocation()
        {
            var config = _state.Config;
            return GetWorldLocation(config.StartPosition);
        }

        Vector3 GetRespawnLocation()
        {
            var config = _state.Config;
            return GetWorldLocation(config.RespawnPosition);
        }

        Vector3 GetStartLocation(int index, int groupIndex)
        {
            var config = _state.Config;

            // Give each agent 2 meters distance to each other.
            var maxDistance = MathEx.Clamp((float)_state.Config.GroupSize * 6.0f, 16.0f, 500.0f);

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
                return GetStartLocation();
            }
        }

        void Populate()
        {
            var agents = _state.Agents;
            var config = _state.Config;
            var prng = _state.PRNG;

            agents.Clear();

            var sqrKm = (WorldSize.X / 1000.0f) * (WorldSize.Y / 1000.0f);
            if (sqrKm == 0)
            {
                return;
            }

            var maxAgents = (int)System.Math.Ceiling(sqrKm * config.PopulationDensity);
            maxAgents = MathEx.Clamp(maxAgents, 1, Limits.MaxAgents);

            _state.GroupCount = (maxAgents + (config.GroupSize - 1)) / config.GroupSize;

            if (_state.GroupCount == 0)
            {
                return;
            }

            _groupStarts = new Vector3[_state.GroupCount];
            for (int i = 0; i < _groupStarts.Length; i++)
            {
                _groupStarts[i] = GetStartLocation();
            }

            for (int index = 0; index < maxAgents; index++)
            {
                int groupIndex = index / config.GroupSize;

                var agent = new Agent(index, groupIndex);
                agent.LastUpdateTick = _state.Ticks;
                agent.Position = GetStartLocation(index, groupIndex);

                // Ensure the position is not out of bounds.
                Warp(agent);

                agent.Velocity.X = (float)(prng.NextDouble() * 3f);
                agent.Velocity.Y = (float)(prng.NextDouble() * 3f);

                agents.Add(agent);

                MoveInGrid(agent);
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
            Logging.CondInfo(Config.LoggingOpts.General, "Started simulation.");

            if (Config.FastForwardAtStart && _state.Ticks == 0)
            {
                Logging.CondInfo(Config.LoggingOpts.General,
                    "Advancing simulation for {0} ticks...",
                    Simulation.Limits.TicksToAdvanceOnStartup);

                _isFastAdvancing = true;

                var elapsed = Utils.Measure(() =>
                {
                    var oldTimeScale = TimeScale;
                    TimeScale = 128.0f;
                    for (uint num = 0u; num < Simulation.Limits.TicksToAdvanceOnStartup && !_shouldStop; num++)
                    {
                        Tick();
                    }
                    TimeScale = oldTimeScale;
                });

                _isFastAdvancing = false;

                Logging.CondInfo(Config.LoggingOpts.General, "... done, took {0}.", elapsed);
            }

            _updateTime.Restart();

            float accumulator = 0;
            while (!_shouldStop)
            {
                var timeElapsed = _updateTime.Capture();
                _updateTime.Restart();

                var elapsedMs = timeElapsed;
                var scaledDt = (float)(timeElapsed * TimeScale);

                if (IsPaused())
                {
                    Thread.Sleep(100);
                    continue;
                }

                accumulator = System.Math.Min(accumulator + scaledDt, 10.0f);

                if (accumulator < Constants.TickRate)
                {
                    if (_shouldStop)
                        break;

                    Thread.Sleep(1);
                    continue;
                }

                while (accumulator >= Constants.TickRate)
                {
                    Tick();

                    accumulator -= Constants.TickRate;

                    if (_shouldStop)
                        break;
                }

                if (_shouldStop)
                    break;
            }

            _running = false;
            _shouldStop = false;

            Logging.CondInfo(Config.LoggingOpts.General, "Simulation stopped.");
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

                if (_state.Agents.Count == 0)
                {
                    Populate();
                }

                SetupProcessors();
            }
        }

        public Drawing.Color GetGroupColor(int groupIndex)
        {
            if (groupIndex >= _processors.Count)
            {
                return Drawing.ColorTable.GetColorForIndex(groupIndex);
            }

            var proc = _processors[groupIndex];
            if (proc == null)
            {
                return Drawing.ColorTable.GetColorForIndex(groupIndex);
            }

            return _processors[groupIndex].Color;
        }

        private uint MillisecondsToTicks(uint milliseconds)
        {
            return (milliseconds * Constants.TicksPerSecond) / 1000;
        }

        private uint SecondsToTicks(uint seconds)
        {
            return seconds * Constants.TicksPerSecond;
        }

        private uint MinutesToTicks(uint minutes)
        {
            return SecondsToTicks(minutes * 60);
        }

        public double GetSimulationTimeSeconds()
        {
            return _state.Ticks / (double)Constants.TicksPerSecond;
        }

        public void SetIsBloodmoon(bool bloodmoon)
        {
            _state.IsBloodmoon = bloodmoon;
        }

        public void SetIsDayTime(bool isDay)
        {
            _state.IsDayTime = isDay;
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
