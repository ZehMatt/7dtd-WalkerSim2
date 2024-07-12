using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public delegate bool AgentDespawnHandler(Agent agent);

        private AgentDespawnHandler _agentDespawnHandler;

        private Dictionary<int, Agent> _active = new Dictionary<int, Agent>();

        private List<int> _cleanUpList = new List<int>();


        public IReadOnlyDictionary<int, Agent> Active
        {
            get
            {
                return _active;
            }
        }

        public void SetAgentDespawnHandler(AgentDespawnHandler handler)
        {
            _agentDespawnHandler = handler;
        }

        private void AddActiveAgent(int entityId, Agent agent)
        {
            _active.Add(entityId, agent);
        }

        private void RemoveInactiveAgents()
        {
            _cleanUpList.Clear();

            foreach (var kv in _active)
            {
                if (kv.Value.CurrentState != Agent.State.Active)
                {
                    _cleanUpList.Add(kv.Key);
                }
            }

            foreach (var entityId in _cleanUpList)
            {
                _active.Remove(entityId);
            }
        }

        public void MarkAgentDead(int entityId)
        {
            if (_active.TryGetValue(entityId, out var agent))
            {
                agent.CurrentState = Agent.State.Dead;
            }
        }

        private void CheckAgentDespawn()
        {
            foreach (var kv in _active)
            {
                var agent = kv.Value;
                if (agent.CurrentState != Agent.State.Active)
                    continue;

                var pos = agent.Position;

                var isNearPlayer = false;
                foreach (var ply in players)
                {
                    if (!ply.Value.IsAlive)
                        continue;

                    var dist = agent.GetDistance(ply.Value.Position);

                    // NOTE: It has to be fully outside to avoid constant spawning/despawning.
                    if (dist <= (ply.Value.ViewRadius + 8))
                        continue;

                    isNearPlayer = true;
                    break;
                }

                if (!isNearPlayer)
                    continue;

                if (_agentDespawnHandler != null)
                {
                    _agentDespawnHandler(agent);

                    agent.CurrentState = Agent.State.Wandering;
                }
            }
        }
    }
}
