using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public delegate bool AgentDespawnHandler(Agent agent);

        private AgentDespawnHandler _agentDespawnHandler;

        private List<int> _cleanUpList = new List<int>();

        public IReadOnlyDictionary<int, Agent> Active
        {
            get
            {
                return _state.Active;
            }
        }

        public void SetAgentDespawnHandler(AgentDespawnHandler handler)
        {
            _agentDespawnHandler = handler;
        }

        private void AddActiveAgent(int entityId, Agent agent)
        {
            _state.Active.Add(entityId, agent);
            Logging.Debug("Added agent with entity id {0} to active list, list size {1}", entityId, _state.Active.Count);
        }

        private void RemoveInactiveAgents()
        {
            _cleanUpList.Clear();

            foreach (var kv in _state.Active)
            {
                if (kv.Value.CurrentState != Agent.State.Active)
                {
                    _cleanUpList.Add(kv.Key);
                }
            }

            foreach (var entityId in _cleanUpList)
            {
                Logging.Debug("Removed agent with entity id {0} from active list", entityId);
                _state.Active.Remove(entityId);
            }
        }

        public void MarkAgentDead(Agent agent)
        {
            agent.ResetSpawnData();

            if (_state.Config.RespawnPosition == Config.WorldLocation.None)
            {
                // No respawn, dead for good.
                agent.CurrentState = Agent.State.Dead;
            }
            else
            {
                // Mark to be respawned.
                agent.CurrentState = Agent.State.Respawning;
            }
        }

        private bool IsInsidePlayerMaxView(Vector3 pos)
        {
            // NOTE: We do not check if the player is alive here, the game plays a death animation
            // and zombies will eat on the player so despawning them the moment the player is killed
            // is not wanted. If the player respawns it will update the position and despawn as usual.

            foreach (var ply in _state.Players)
            {
                var dist = Vector3.Distance(pos, ply.Value.Position);

                // We add a little offset to avoid constant spawn-despawning.
                if (dist >= ply.Value.ViewRadius + 8)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private void CheckAgentDespawn()
        {
            if (_agentDespawnHandler == null)
                return;

            foreach (var kv in _state.Active)
            {
                var agent = kv.Value;
                if (agent.CurrentState != Agent.State.Active)
                    continue;

                var insidePlayerView = IsInsidePlayerMaxView(agent.Position);
                if (insidePlayerView)
                    continue;

                // Handle the despawn.
                _agentDespawnHandler(agent);

                // Activate in simulation.
                agent.CurrentState = Agent.State.Wandering;
            }
        }
    }
}
