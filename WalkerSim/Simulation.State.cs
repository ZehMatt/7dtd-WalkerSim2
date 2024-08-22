using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public class State
        {
            public Config Config = new Config();
            public List<Agent> Agents = new List<Agent>();
            public Vector3 WorldMins = Vector3.Zero;
            public Vector3 WorldMaxs = Vector3.Zero;
            public MapData MapData = null;
            public List<int>[] Grid = null;
            public ConcurrentDictionary<int, Player> Players = new ConcurrentDictionary<int, Player>();
            public List<EventData> Events = new List<EventData>();
            public Dictionary<int, Agent> Active = new Dictionary<int, Agent>();
            public WalkerSim.Random PRNG;
            public uint SlowIterator = 0;
            public Vector3 WindDir = new Vector3(1, 0, 0);
            public Vector3 WindDirTarget = new Vector3(1, 0, 0);
            public float WindTime = 0;
            public int TickNextWindChange = 0;
            public int GroupCount = 0;
            public float MaxNeighbourDistance = 0;

            public void Reset()
            {
                Config = new Config();
                Agents = new List<Agent>();
                WorldMins = Vector3.Zero;
                WorldMaxs = Vector3.Zero;
                Players = new ConcurrentDictionary<int, Player>();
                Events = new List<EventData>();
                Active = new Dictionary<int, Agent>();
                PRNG = null;
                SlowIterator = 0;
                WindDir = new Vector3(1, 0, 0);
                WindDirTarget = new Vector3(1, 0, 0);
                WindTime = 0;
                TickNextWindChange = 0;
                GroupCount = 0;
            }
        }

        State _state = new State();
    }
}
