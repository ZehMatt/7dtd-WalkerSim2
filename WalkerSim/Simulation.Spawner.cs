using System;
using System.Collections.Concurrent;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public delegate int AgentSpawnHandler(Simulation simulation, Agent agent);

        private AgentSpawnHandler _agentSpawnHandler;

        private ConcurrentQueue<Agent> _pendingSpawns = new ConcurrentQueue<Agent>();

        private DateTime _nextSpawn = DateTime.Now;

        private FixedBufferList<Agent> _nearPlayer = new FixedBufferList<Agent>(512);

        private bool _allowAgentSpawn = true;

        public void SetAgentSpawnHandler(AgentSpawnHandler handler)
        {
            _agentSpawnHandler = handler;
        }

        private void CheckAgentSpawn()
        {
            if (_allowAgentSpawn == false)
            {
                return;
            }

            // Don't activate them when they are in the inner radius.
            var activationBorderSize = 4.0f;

            foreach (var kv in _state.Players)
            {
                var player = kv.Value;

                if (player.EntityId == -1)
                    continue;

                if (player.IsAlive == false)
                    continue;

                var playerPos = player.Position;

                _nearPlayer.Clear();
                QueryCells(playerPos, -1, player.ViewRadius, _nearPlayer);

                for (int i = 0; i < _nearPlayer.Count; i++)
                {
                    var agent = _nearPlayer[i];

                    if (agent.CurrentState != Agent.State.Wandering)
                        continue;

                    var dist = Vector3.Distance(playerPos, agent.Position);
                    if (dist < player.ViewRadius - activationBorderSize)
                        continue;

                    if (dist > player.ViewRadius)
                        continue;

                    Logging.Debug("Agent {0} near player {1} at {2}m, spawning...", agent.Index, player.EntityId, dist);

                    agent.CurrentState = Agent.State.PendingSpawn;
                    _pendingSpawns.Enqueue(agent);
                }
            }
        }

        private void ProcessSpawnQueue()
        {
            var now = DateTime.Now;
            if (now < _nextSpawn)
            {
                return;
            }

            Agent agent;
            if (!_pendingSpawns.TryDequeue(out agent))
            {
                return;
            }

            _nextSpawn = now.AddSeconds(Limits.SpawnDespawnDelay);

            int agentEntityId = -1;
            if (_agentSpawnHandler != null)
            {
                agentEntityId = _agentSpawnHandler(this, agent);
            }
            else
            {
                Logging.Warn("No spawn handler registered");
            }

            if (agentEntityId != -1)
            {
                agent.EntityId = agentEntityId;
                agent.CurrentState = Agent.State.Active;

                AddActiveAgent(agentEntityId, agent);

                _state.SuccessfulSpawns++;
            }
            else
            {
                // Turn back to wandering, currently not possible to spawn.
                agent.CurrentState = Agent.State.Wandering;

                _state.FailedSpawns++;
            }
        }

        public void SetEnableAgentSpawn(bool allowSpawn)
        {
            if (_allowAgentSpawn == allowSpawn)
                return;

            if (!allowSpawn)
            {
                Logging.Info("Agent spawning disabled.");
            }
            else
            {
                Logging.Info("Agent spawning enabled.");
            }

            _allowAgentSpawn = allowSpawn;
        }
    }
}
