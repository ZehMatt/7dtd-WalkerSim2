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

        private List<Agent> agents = new List<Agent>();

        public Vector3 WorldMins = Vector3.Zero;
        public Vector3 WorldMaxs = Vector3.Zero;

        public float TimeScale = 1.0f;
        public int MaxAgents = 0;
        public readonly float TickRate = 1f / 40f;
        public readonly int GroupSize = 4;

        private float _accumulator = 0f;
        private int _ticks = 0;
        private System.Random _random;
        private MapData _mapData;
        private bool _initialized = false;

        private Thread _thread;
        private bool _running = false;
        private bool _paused = false;

        private int _groupCount = 0;
        public int GroupCount
        {
            get => _groupCount;
        }

        public MapData MapData
        {
            get => _mapData;
        }

        public IReadOnlyList<Agent> Agents
        {
            get => agents;
        }

        public Vector3 WorldSize
        {
            get => WorldMaxs - WorldMins;
        }

        public void Stop()
        {
            if (_thread == null)
            {
                return;
            }

            _running = false;
            _thread.Join();
        }

        public void Start()
        {
            Stop();

            _running = true;
            _thread = new Thread(() => ThreadUpdate());
            _thread.Start();
        }

        public void Reset(Vector3 worldMins, Vector3 worldMaxs, int maxAgents)
        {
            Stop();

            _random = new System.Random(1);

            WorldMins = worldMins;
            WorldMaxs = worldMaxs;
            MaxAgents = maxAgents;

            SetupGrid();
            Populate();

            _initialized = true;
            _accumulator = 0.0f;
        }

        public void SetPaused(bool paused)
        {
            _paused = paused;
        }

        public void EntityKilled(int entityId)
        {
            if (players.TryGetValue(entityId, out Player ply))
            {
                ply.IsAlive = false;
                return;
            }

            if (_active.TryGetValue(entityId, out var agent))
            {
                agent.EntityId = -1;
                agent.Health = -1;
                agent.EntityClassId = -1;
                agent.CurrentState = Agent.State.Dead;
            }
        }

        public bool LoadMapData(string directoryPath)
        {
            _mapData = MapData.LoadFromFolder(directoryPath);
            if (_mapData == null)
                return false;

            return true;
        }

        Vector3 GetRandomPosition()
        {
            float x0 = (float)_random.NextDouble();
            float y0 = (float)_random.NextDouble();
            float x = Math.Remap(x0, 0f, 1f, WorldMins.X, WorldMaxs.X);
            float y = Math.Remap(y0, 0f, 1f, WorldMins.Y, WorldMaxs.Y);
            return new Vector3(x, y);
        }

        Vector3 GetStartLocation(Vector3[] groupStarts, int index, int groupIndex)
        {
            // TODO: Make this an option.
            if (false)
            {
                float groupOffset = (index % GroupSize) / (float)GroupSize;

                // Spawn in circle.
                float angle = groupOffset * (float)System.Math.PI * 2.0f;
                float radius = 40.0f;
                float offsetX = (float)System.Math.Cos(angle) * radius;
                float offsetY = (float)System.Math.Sin(angle) * radius;

                return groupStarts[groupIndex] + new Vector3(offsetX, offsetY);
            }
            else
            {
                // Pick a random position.
                return GetRandomPosition();
            }
        }

        void Populate()
        {
            agents.Clear();

            _groupCount = MaxAgents / GroupSize;
            if (MaxAgents % GroupSize != 0)
            {
                _groupCount++;
            }

            Vector3[] groupStartPos = new Vector3[_groupCount];
            for (int i = 0; i < groupStartPos.Length; i++)
            {
                groupStartPos[i] = GetRandomPosition();
            }

            for (int index = 0; index < MaxAgents; index++)
            {
                int groupIndex = index / GroupSize;

                var agent = new Agent(index, groupIndex);
                agent.Position = GetStartLocation(groupStartPos, index, groupIndex);

                // Ensure the position is not out of bounds.
                Warp(agent);

                agent.Velocity.X = (float)(_random.NextDouble() * 3f);
                agent.Velocity.Y = (float)(_random.NextDouble() * 3f);

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

                while (_accumulator > TickRate && _running)
                {
                    Tick();

                    _accumulator -= TickRate;
                    _ticks++;
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
            pos.X = Math.Remap(pos.X, WorldMins.X, WorldMaxs.X, min.X, max.X);
            pos.Y = Math.Remap(pos.Y, WorldMins.Y, WorldMaxs.Y, min.Y, max.Y);
            pos.Z = 0;
            return pos;
        }
    }
}
