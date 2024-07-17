using System;

namespace WalkerSim
{
    internal class Agent
    {
        public enum State
        {
            Wandering,
            PendingSpawn,
            Active,
            Dead,
        }

        public int Index;
        public int Group;
        public Vector3 Position;
        public Vector3 Velocity;
        public int CellIndex;
        public int EntityId;
        public int EntityClassId;
        public int Health;
        public State CurrentState;
        public DateTime LastUpdate;

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
            LastUpdate = DateTime.Now;

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
            return Vector3.Distance(Position, other.Position);
        }

        public float GetDistance(Vector3 other)
        {
            return Vector3.Distance(Position, other);
        }
    }
}
