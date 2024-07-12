using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        private ConcurrentDictionary<int, Player> players = new ConcurrentDictionary<int, Player>();

        public IEnumerable<KeyValuePair<int, Player>> Players
        {
            get => players;
        }

        public void AddPlayer(int entityId, Vector3 pos, int viewRadius)
        {
            Player player = new Player();
            player.EntityId = entityId;
            player.Position = pos;
            player.ViewRadius = viewRadius;
            player.IsAlive = true;

            players.TryAdd(entityId, player);

            Logging.Out("Player added in simulation, entity id: {0}, position: {1}", entityId, player.Position);
        }

        public void RemovePlayer(int entityId)
        {
            if (players.TryRemove(entityId, out var _))
            {
                Logging.Out("Player removed from simulation, entity id: {0}", entityId);
            }
        }

        public void UpdatePlayer(int entityId, Vector3 newPos)
        {
            if (players.TryGetValue(entityId, out var player))
            {
                player.Position = newPos;
            }
        }
    }
}
