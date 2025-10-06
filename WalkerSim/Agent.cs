namespace WalkerSim
{
    internal class Agent : GridObject
    {
        public enum State
        {
            Dead,
            Wandering,
            PendingSpawn,
            Active,
            Respawning,
        }

        public enum SubState
        {
            None,
            Alerted,
        }

        public int Index;
        public int Group;
        public Vector3 Velocity;
        public int EntityId = -1;
        public int EntityClassId = -1;
        public int Health = -1;
        public State CurrentState = State.Dead;
        public uint LastUpdateTick = 0;
        public uint LastSpawnTick = 0;
        public SubState CurrentSubState = SubState.None;
        public uint AlertedTick = 0;
        public Vector3 AlertPosition;

        public Agent()
        {
        }

        public Agent(int index, int group)
        {
            Index = index;
            Group = group;
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
            CellIndex = -1;
            CurrentState = State.Wandering;
            LastUpdateTick = 0;

            ResetSpawnData();
        }

        public void ResetSpawnData()
        {
            EntityId = -1;
            EntityClassId = -1;
            Health = -1;
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
