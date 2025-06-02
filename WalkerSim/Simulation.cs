using System;
using System.Diagnostics;
using System.Threading;

namespace WalkerSim
{
    internal class TimeMeasurement
    {
        private Stopwatch _sw = new Stopwatch();
        private float[] _samples = new float[64];

        private int _index = 0;
        private int _count = 0;

        public void Add(float time)
        {
            _samples[_index % _samples.Length] = time;
            _index++;
            _count = System.Math.Min(_count + 1, _samples.Length);
        }

        public void Reset()
        {
            _index = 0;
            _count = 0;
        }

        public void Restart()
        {
            _sw.Restart();
        }

        public float Capture()
        {
            var elapsed = (float)_sw.Elapsed.TotalSeconds;
            Add(elapsed);
            return elapsed;
        }

        public float Average
        {
            get
            {
                if (_count == 0)
                    return 0.0f;
                float sum = 0.0f;
                for (int i = 0; i < _count; i++)
                {
                    sum += _samples[i];
                }
                return sum / _count;
            }
        }
    }

    internal partial class Simulation
    {
        public static Simulation Instance = new Simulation();

        public float TimeScale = 1.0f;

        public const int TicksPerSecond = 30;
        public const float TickRate = 1f / TicksPerSecond;
        public const int TickRateMs = 1000 / TicksPerSecond;

        private Thread _thread;
        private bool _running = false;
        private bool _shouldStop = false;
        private bool _paused = false;
        private bool _isFastAdvancing = false;

        private Vector3[] _groupStarts = new Vector3[0];

        private int _maxAllowedAliveAgents = 64;

        TimeMeasurement _updateTime = new TimeMeasurement();

        public bool EditorMode = false;

        public void Stop()
        {
            if (_thread == null)
            {
                return;
            }

            _shouldStop = true;
            _thread.Join();
            _thread = null;
            _running = false;
            _nextAutoSave = DateTime.MaxValue;

            Logging.Out("Simulation stopped.");
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

            _running = true;
            _shouldStop = false;
            _thread = new Thread(ThreadUpdate);
            _thread.Start();

            Logging.Out("Started Simulation.");
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

                SetupGrid();
                Populate();
                SetupProcessors();

                _simTime.Reset();
                _updateTime.Reset();
            }
        }

        public void SetPaused(bool paused)
        {
            if (_paused != paused)
            {
                if (paused)
                    Logging.Out("Paused simulation.");
                else
                    Logging.Out("Resuming simulation.");
            }
            _paused = paused;
        }

        public void EntityKilled(int entityId)
        {
            if (_state.Active.TryGetValue(entityId, out var agent))
            {
                MarkAgentDead(agent);
            }
        }

        public bool LoadMapData(string directoryPath)
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
                var max = Config.WorldLocation.RandomPOI;
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
            var maxDistance = MathEx.Clamp((float)_state.Config.GroupSize * 2.0f, 10.0f, 500.0f);

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
            var maxAgents = (int)System.Math.Ceiling(sqrKm * config.PopulationDensity);
            maxAgents = MathEx.Clamp(maxAgents, 1, Limits.MaxAgents);

            _state.GroupCount = maxAgents / config.GroupSize;
            if (maxAgents % config.GroupSize != 0)
            {
                _state.GroupCount++;
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

        private void ThreadUpdate()
        {
            if (_state.Config.FastForwardAtStart && _state.Ticks == 0)
            {
                Logging.Out("Advancing simulation for {0} ticks...", Simulation.Limits.TicksToAdvanceOnStartup);

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

                Logging.Out("... done, took {0}.", elapsed);
            }

            _updateTime.Restart();

            float accumulator = 0;
            while (!_shouldStop)
            {
                var timeElapsed = _updateTime.Capture();
                _updateTime.Restart();

                var elapsedMs = timeElapsed;
                var scaledDt = (float)(timeElapsed * TimeScale);

                if (_paused)
                {
                    Thread.Sleep(1);
                    continue;
                }

                accumulator = System.Math.Min(accumulator + scaledDt, 2.0f);

                if (accumulator < TickRate)
                {
                    if (_shouldStop)
                        break;

                    Thread.Sleep(1);
                    continue;
                }

                while (accumulator >= TickRate)
                {
                    _simTime.Restart();

                    Tick();

                    _simTime.Capture();

                    if (_shouldStop)
                        break;

                    CheckAgentSpawn();
                    accumulator -= TickRate;
                }

                CheckAutoSave();

                if (_shouldStop)
                    break;
            }

            _running = false;
            _shouldStop = false;
        }

        // Called from the main thread, this should be invoked from GameUpdate.
        public void GameUpdate(float deltaTime)
        {
            if (!_running || _shouldStop || _paused)
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

        private int SecondsToTicks(int seconds)
        {
            return seconds * TicksPerSecond;
        }

        private int MinutesToTicks(int minutes)
        {
            return SecondsToTicks(minutes * 60);
        }
    }
}
