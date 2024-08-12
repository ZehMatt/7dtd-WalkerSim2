using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace WalkerSim
{
    internal class Player
    {
        public Vector3 Position;
        public int EntityId;
        public int ViewRadius;
        public bool IsAlive;
    }

    internal partial class Simulation
    {
        public static Simulation Instance = new Simulation();

        public float TimeScale = 1.0f;
        public readonly float TickRate = 1f / 40f;

        private float _accumulator = 0f;
        private int _ticks = 0;

        private bool _initialized = false;

        private Thread _thread;
        private bool _running = false;
        private bool _paused = false;
        private float _speedScale = 1.0f;

        private Vector3[] _groupStarts = new Vector3[0];

        public int GroupCount
        {
            get => _state.GroupCount;
        }

        public Config Config
        {
            get => _state.Config;
        }

        public MapData MapData
        {
            get => _state.MapData;
        }

        public IReadOnlyList<Agent> Agents
        {
            get => _state.Agents;
        }

        public Vector3 WorldSize
        {
            get => _state.WorldMaxs - _state.WorldMins;
        }

        public Vector3 WorldMins
        {
            get => _state.WorldMins;
        }

        public Vector3 WorldMaxs
        {
            get => _state.WorldMaxs;
        }

        public void Stop()
        {
            if (_thread == null)
            {
                return;
            }

            _running = false;
            _thread.Join();
            _thread = null;

            Logging.Out("Simulation stopped.");
        }

        public void FastAdvance(int numTicks)
        {
            var oldScale = _speedScale;
            _speedScale = 1500.0f;
            for (int i = 0; i < numTicks; i++)
            {
                Tick();
            }
            _speedScale = oldScale;
        }

        public void Start()
        {
            Stop();

            _running = true;
            _thread = new Thread(() => ThreadUpdate());
            _thread.Start();

            Logging.Out("Started Simulation.");
        }

        public void Reset(Vector3 worldMins, Vector3 worldMaxs, Config config)
        {
            Stop();

            _state.Config = config;
            _state.PRNG = new System.Random(config.RandomSeed);
            _state.SlowIterator = 0;

            _state.WorldMins = worldMins;
            _state.WorldMaxs = worldMaxs;

            SetupGrid();
            Populate();
            SetupProcessors();

            _initialized = true;
            _accumulator = 0.0f;
        }

        public void SetPaused(bool paused)
        {
            _paused = paused;
        }

        public void EntityKilled(int entityId)
        {
            if (_state.Players.TryGetValue(entityId, out Player ply))
            {
                ply.IsAlive = false;
                return;
            }

            if (_state.Active.TryGetValue(entityId, out var agent))
            {
                agent.EntityId = -1;
                agent.Health = -1;
                agent.EntityClassId = -1;
                agent.CurrentState = Agent.State.Dead;
            }
        }

        public bool LoadMapData(string directoryPath)
        {
            _state.MapData = MapData.LoadFromFolder(directoryPath);
            if (_state.MapData == null)
                return false;

            return true;
        }

        Vector3 GetRandomPosition()
        {
            var prng = _state.PRNG;
            float borderSize = 250;
            float x0 = (float)prng.NextDouble();
            float y0 = (float)prng.NextDouble();
            float x = Math.Remap(x0, 0f, 1f, _state.WorldMins.X + borderSize, _state.WorldMaxs.X - borderSize);
            float y = Math.Remap(y0, 0f, 1f, _state.WorldMins.Y + borderSize, _state.WorldMaxs.Y - borderSize);
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
                res.X = Math.Remap(x0, 0f, 1f, worldMins.X + borderSize, worldMaxs.X - borderSize);
                res.Y = worldMins.Y + borderSize;
            }
            else if (side == 1)
            {
                // Right.
                res.X = worldMaxs.X - borderSize;
                float y0 = (float)prng.NextDouble();
                res.Y = Math.Remap(y0, 0f, 1f, worldMins.Y + borderSize, worldMaxs.Y - borderSize);
            }
            else if (side == 2)
            {
                // Bottom.
                float x0 = (float)prng.NextDouble();
                res.X = Math.Remap(x0, 0f, 1f, worldMins.X + borderSize, worldMaxs.X - borderSize);
                res.Y = worldMaxs.Y - borderSize;
            }
            else if (side == 3)
            {
                // Left.
                res.X = worldMins.X + borderSize;
                float y0 = (float)prng.NextDouble();
                res.Y = Math.Remap(y0, 0f, 1f, worldMins.Y + borderSize, worldMaxs.Y - borderSize);
            }

            return res;
        }

        Vector3 GetRandomPOIPosition()
        {
            var mapData = _state.MapData;
            var prefabs = mapData.Prefabs;
            var decos = prefabs.Decorations;
            var prng = _state.PRNG;

            var selectedIdx = prng.Next(decos.Length);
            return decos[selectedIdx].Position;
        }

        Vector3 GetGroupPosition(int groupIndex)
        {
            return _groupStarts[groupIndex];
        }

        Vector3 GetStartLocation()
        {
            var config = _state.Config;
            var prng = _state.PRNG;

            var startType = config.StartLocation;
            if (startType == Config.SpawnLocation.Mixed)
            {
                var min = Config.SpawnLocation.RandomBorderLocation;
                var max = Config.SpawnLocation.RandomPOI;
                startType = (Config.SpawnLocation)prng.Next((int)min, (int)max + 1);
            }

            switch (startType)
            {
                case Config.SpawnLocation.None:
                    break;
                case Config.SpawnLocation.RandomBorderLocation:
                    return GetRandomBorderPosition();
                case Config.SpawnLocation.RandomLocation:
                    return GetRandomPosition();
                case Config.SpawnLocation.RandomPOI:
                    return GetRandomPOIPosition();
            }

            // This should never happen.
            throw new System.Exception("Bad starting location type");
        }

        Vector3 GetStartLocation(int index, int groupIndex)
        {
            var config = _state.Config;
            var maxDistance = (float)_state.Config.GroupSize * 0.8f;

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

            _state.GroupCount = config.MaxAgents / config.GroupSize;
            if (config.MaxAgents % config.GroupSize != 0)
            {
                _state.GroupCount++;
            }

            _groupStarts = new Vector3[_state.GroupCount];
            for (int i = 0; i < _groupStarts.Length; i++)
            {
                _groupStarts[i] = GetStartLocation();
            }

            for (int index = 0; index < config.MaxAgents; index++)
            {
                int groupIndex = index / config.GroupSize;

                var agent = new Agent(index, groupIndex);
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
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (_running)
            {
                if (_paused)
                {
                    Thread.Sleep(25);
                    continue;
                }

                float deltaTime = (float)sw.Elapsed.TotalSeconds;
                sw.Restart();

                if (deltaTime > 0.1f)
                {
                    deltaTime = 0.1f;
                }

                _accumulator += deltaTime * TimeScale;

                if (_accumulator < TickRate)
                {
                    Thread.Sleep(2);
                }

                while (_accumulator > TickRate && _running)
                {
                    Tick();
                    _accumulator -= TickRate;
                }

                if (!_running)
                    break;

                CheckAgentSpawn();
            }
        }

        public void Update(float deltaTime)
        {
            if (!_initialized)
                return;

            if (!_running)
                return;

            ProcessSpawnQueue();
            CheckAgentDespawn();
            RemoveInactiveAgents();
        }

        public Vector3 RemapPosition2D(Vector3 pos, Vector3 min, Vector3 max)
        {
            var worldMins = _state.WorldMins;
            var worldMaxs = _state.WorldMaxs;

            pos.X = Math.Remap(pos.X, worldMins.X, worldMaxs.X, min.X, max.X);
            pos.Y = Math.Remap(pos.Y, worldMins.Y, worldMaxs.Y, min.Y, max.Y);
            pos.Z = 0;

            return pos;
        }
    }
}
