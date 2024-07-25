using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public delegate int AgentSpawnHandler(Agent agent);

        private AgentSpawnHandler _agentSpawnHandler;

        private ConcurrentQueue<Agent> _pendingSpawns = new ConcurrentQueue<Agent>();

        private DateTime _nextSpawn = DateTime.Now;

        public void SetAgentSpawnHandler(AgentSpawnHandler handler)
        {
            _agentSpawnHandler = handler;
        }

        private void CheckAgentSpawn()
        {
            var nearby = new List<Agent>();
            foreach (var kv in _state.Players)
            {
                var player = kv.Value;

                if (player.EntityId == -1)
                    continue;

                var playerPos = player.Position;

                // Don't activate them when they are in the inner radius.
                var activationBorderSize = 4.0f;

                nearby.Clear();
                QueryNearby(playerPos, -1, player.ViewRadius, nearby);

                foreach (var agent in nearby)
                {
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

            _nextSpawn = now.AddSeconds(0.1);

            Agent agent;
            if (!_pendingSpawns.TryDequeue(out agent))
            {
                return;
            }

            int agentEntityId = -1;
            if (_agentSpawnHandler != null)
            {
                agentEntityId = _agentSpawnHandler(agent);
            }

            if (agentEntityId != -1)
            {
                agent.EntityId = agentEntityId;
                agent.CurrentState = Agent.State.Active;

                AddActiveAgent(agentEntityId, agent);
            }
            else
            {
                // Turn back to wandering, currently not possible to spawn.
                agent.CurrentState = Agent.State.Wandering;
            }
        }
    }
}
