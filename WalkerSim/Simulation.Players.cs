using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        internal class Player
        {
            public Vector3 Position;
            public int EntityId;
            public int ViewRadius;
            public bool IsAlive;
        }

        public IEnumerable<KeyValuePair<int, Player>> Players
        {
            get => _state.Players;
        }

        public int PlayerCount
        {
            get => _state.Players.Count;
        }

        public void AddPlayer(int entityId, Vector3 pos, int viewRadius)
        {
            Player player = new Player();
            player.EntityId = entityId;
            player.Position = pos;
            player.ViewRadius = viewRadius;
            player.IsAlive = true;

            _state.Players.TryAdd(entityId, player);

            Logging.Out("Player added in simulation, entity id: {0}, position: {1}", entityId, player.Position);
        }

        public void RemovePlayer(int entityId)
        {
            if (_state.Players.TryRemove(entityId, out var _))
            {
                Logging.Out("Player removed from simulation, entity id: {0}", entityId);
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

        public bool HasPlayer(int entityId)
        {
            return _state.Players.ContainsKey(entityId);
        }
    }
}
