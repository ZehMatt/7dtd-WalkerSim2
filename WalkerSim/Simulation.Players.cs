using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        internal class Player
        {
            public Vector3 Position;
            public int EntityId;
            public bool IsAlive;
            public uint NextPossibleSpawnTime;
        }

        public IEnumerable<KeyValuePair<int, Player>> Players
        {
            get => _state.Players;
        }

        public int PlayerCount
        {
            get => _state.Players.Count;
        }

        public void AddPlayer(int entityId, Vector3 pos, uint spawnDelay)
        {
            Player player = new Player();
            player.EntityId = entityId;
            player.Position = pos;
            player.IsAlive = true;
            player.NextPossibleSpawnTime = Ticks + spawnDelay;

            _state.Players.TryAdd(entityId, player);

            Logging.CondInfo(Config.LoggingOpts.General,
                "Player added in simulation, entity id: {0}, position: {1}, spawn delay: {2} ticks",
                entityId,
                player.Position,
                spawnDelay);
        }

        public void RemovePlayer(int entityId)
        {
            if (_state.Players.TryRemove(entityId, out var player))
            {
                player.IsAlive = false;

                Logging.CondInfo(Config.LoggingOpts.General,
                    "Player removed from simulation, entity id: {0}",
                    entityId);
            }
        }

        public void UpdatePlayer(int entityId, Vector3 newPos, bool isAlive)
        {
            if (_state.Players.TryGetValue(entityId, out var player))
            {
                player.Position = newPos;
                player.IsAlive = isAlive;
            }
        }

        public void NotifyPlayerSpawned(int entityId, uint spawnDelay)
        {
            if (_state.Players.TryGetValue(entityId, out var player))
            {
                Logging.CondInfo(Config.LoggingOpts.General,
                    "Player spawned in simulation, entity id: {0}, position: {1}, spawn activation delay: {2} ticks",
                    entityId,
                    player.Position,
                    spawnDelay);

                player.IsAlive = true;
                player.NextPossibleSpawnTime = Ticks + spawnDelay;
            }
        }

        public bool HasPlayer(int entityId)
        {
            return _state.Players.ContainsKey(entityId);
        }
    }
}
