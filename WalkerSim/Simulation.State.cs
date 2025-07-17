using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public class State
        {
            public uint Version = SaveVersion;
            public Config Config = Config.GetDefault();
            public List<Agent> Agents = new List<Agent>();
            public Vector3 WorldMins = Vector3.Zero;
            public Vector3 WorldMaxs = Vector3.Zero;
            public string WorldName = string.Empty;
            public MapData MapData = null;
            public List<int>[] Grid = null;
            public ConcurrentDictionary<int, Player> Players = new ConcurrentDictionary<int, Player>();
            public List<EventData> Events = new List<EventData>();
            public List<EventData> EventsTemp = new List<EventData>();
            public Dictionary<int, Agent> Active = new Dictionary<int, Agent>();
            public WalkerSim.Random PRNG;
            public uint SlowIterator = 0;
            public Vector3 WindDir = new Vector3(1, 0, 0);
            public Vector3 WindDirTarget = new Vector3(1, 0, 0);
            public float WindTime = 0;
            public uint Ticks = 0;
            public uint TickNextWindChange = 0;
            public int GroupCount = 0;
            public float MaxNeighbourDistance = 0;
            public int[] AgentsNearPOICounter;
            public int POIIterator = 0;
            // Statistics
            public int FailedSpawns = 0;
            public int SuccessfulSpawns = 0;
            public int TotalDespawns = 0;
            // Game State, not saved.
            public bool IsBloodmoon = false;
            public bool IsDayTime = true;

            public void Reset()
            {
                Config = new Config();
                Agents = new List<Agent>();
                WorldMins = Vector3.Zero;
                WorldMaxs = Vector3.Zero;
                Players = new ConcurrentDictionary<int, Player>();
                PRNG = null;
                SoftReset();
            }

            public void SoftReset()
            {
                Events = new List<EventData>();
                Active = new Dictionary<int, Agent>();
                SlowIterator = 0;
                TickNextWindChange = 0;
                Ticks = 0;
                POIIterator = 0;
                FailedSpawns = 0;
                SuccessfulSpawns = 0;
                TotalDespawns = 0;
                WindDir = new Vector3(1, 0, 0);
                WindDirTarget = new Vector3(1, 0, 0);
            }
        }

        State _state = new State();

        public WalkerSim.Random PRNG
        {
            get => _state.PRNG;
        }

        public int GroupCount
        {
            get => _state.GroupCount;
        }

        public int AgentCount
        {
            get => _state.Agents.Count;
        }

        public int ActiveCount
        {
            get => _state.Active.Count;
        }

        public int SuccessfulSpawns
        {
            get => _state.SuccessfulSpawns;
        }

        public int FailedSpawns
        {
            get => _state.FailedSpawns;
        }

        public int TotalDespawns
        {
            get => _state.TotalDespawns;
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

        public string WorldName
        {
            get => _state.WorldName;
        }

        public uint Ticks
        {
            get => _state.Ticks;
        }

        public uint SlowIterator
        {
            get => _state.SlowIterator;
        }

        public uint TickNextWindChange
        {
            get => _state.TickNextWindChange;
        }

        public float WindTime
        {
            get => _state.WindTime;
        }

        public Vector3 WindDirection
        {
            get => _state.WindDir;
        }

        public Vector3 WindDirectionTarget
        {
            get => _state.WindDirTarget;
        }

        public float MaxNeighbourDistance
        {
            get => _state.MaxNeighbourDistance;
        }

        public bool Running
        {
            get => _running;
        }

        public float AverageSimTime => _simTime.Average;

        public float AverageUpdateTime => _updateTime.Average;

        public bool Paused => IsPaused();

        public bool IsBloodmoon => _state.IsBloodmoon;

        public bool IsDayTime => _state.IsDayTime;
    }
}
