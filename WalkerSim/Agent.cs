namespace WalkerSim
{
    public class Agent : GridObject
    {
        public enum State
        {
            Inactive,
            Dead,
            Wandering,
            PendingSpawn,
            Spawned,
            Respawning,
        }

        public enum SubState
        {
            None,
            Alerted,
        }

        public enum TravelState
        {
            Idle,
            Approaching,
            Arrived,
        }

        [System.Flags]
        public enum DismembermentMask
        {
            None = 0,
            Head = 1 << 0,
            LeftUpperArm = 1 << 1,
            LeftLowerArm = 1 << 2,
            RightUpperArm = 1 << 3,
            RightLowerArm = 1 << 4,
            LeftUpperLeg = 1 << 5,
            LeftLowerLeg = 1 << 6,
            RightUpperLeg = 1 << 7,
            RightLowerLeg = 1 << 8,
            LowerBody = LeftUpperLeg | LeftLowerLeg | RightUpperLeg | RightLowerLeg,
        }

        public enum MoveType
        {
            Normal,
            Crippled,
            Crawling,
        }

        public int Index;
        public int Group;
        public Vector3 Velocity;
        public int EntityId = -1;
        public int EntityClassId = -1;
        public float Health = -1;
        public float MaxHealth = -1;
        public float OriginalMaxHealth = -1;
        public State CurrentState = State.Inactive;
        public uint LastUpdateTick = 0;
        public uint LastSpawnTick = 0;
        public SubState CurrentSubState = SubState.None;
        public uint AlertedTick = 0;
        public Vector3 AlertPosition;
        public ulong TimeToDie = ulong.MaxValue;
        public int TargetCityIndex = -1;
        public uint CityTime = 0;
        public TravelState CurrentTravelState = TravelState.Idle;
        public int RoadNodeTarget = -1;
        public const int RoadNodeHistorySize = 20;
        public ushort[] RoadNodeHistory = new ushort[RoadNodeHistorySize];
        public byte RoadNodeHistoryPos = 0;  // Next write position (circular).
        public byte RoadNodeHistoryCount = 0; // Number of valid entries (max RoadNodeHistorySize).
        public DismembermentMask Dismemberment = DismembermentMask.None;
        public MoveType WalkType = MoveType.Normal;

        public Agent()
        {
            ClearRoadNodeHistory();
        }

        public Agent(int index, int group)
        {
            Index = index;
            Group = group;
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
            CellIndex = -1;
            CurrentState = State.Inactive;
            LastUpdateTick = 0;
            TargetCityIndex = -1;
            CityTime = 0;
            CurrentTravelState = TravelState.Idle;
            RoadNodeTarget = -1;
            ClearRoadNodeHistory();

            ResetSpawnData();
        }

        public void ClearRoadNodeHistory()
        {
            System.Array.Clear(RoadNodeHistory, 0, RoadNodeHistorySize);
            RoadNodeHistoryPos = 0;
            RoadNodeHistoryCount = 0;
        }

        public void PushRoadNodeHistory(int nodeIndex)
        {
            RoadNodeHistory[RoadNodeHistoryPos] = (ushort)nodeIndex;
            RoadNodeHistoryPos = (byte)((RoadNodeHistoryPos + 1) % RoadNodeHistorySize);
            if (RoadNodeHistoryCount < RoadNodeHistorySize)
                RoadNodeHistoryCount++;
        }

        public bool IsInRoadNodeHistory(int nodeIndex)
        {
            for (int i = 0; i < RoadNodeHistoryCount; i++)
            {
                if (RoadNodeHistory[i] == nodeIndex)
                    return true;
            }
            return false;
        }

        public void ResetSpawnData()
        {
            EntityId = -1;
            EntityClassId = -1;
            Health = -1;
            MaxHealth = -1;
            TimeToDie = ulong.MaxValue;
            Dismemberment = DismembermentMask.None;
            WalkType = MoveType.Normal;
        }

        public float GetDistance(Agent other)
        {
            return Vector3.Distance2D(Position, other.Position);
        }

        public float GetDistance(Vector3 other)
        {
            return Vector3.Distance2D(Position, other);
        }
    }
}
