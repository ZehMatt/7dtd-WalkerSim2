using System;
using System.Collections.Generic;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public delegate bool AgentDespawnHandler(Simulation simulation, Agent agent);

        private AgentDespawnHandler _agentDespawnHandler;

        private DateTime _nextDespawn = DateTime.Now;

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

            var log = Config.LoggingOpts.Spawns || EditorMode || Utils.IsDebugMode();
            Logging.CondInfo(log, "Added agent with entity id {0} to active list, list size {1}", entityId, _state.Active.Count);
        }

        public void MarkAgentDead(Agent agent)
        {
            _state.Active.Remove(agent.EntityId);
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
            var viewRadius = Config.SpawnActivationRadius;

            // NOTE: We do not check if the player is alive here, the game plays a death animation
            // and zombies will eat on the player so despawning them the moment the player is killed
            // is not wanted. If the player respawns it will update the position and despawn as usual.
            foreach (var ply in _state.Players)
            {
                var dist = Vector3.Distance2D(pos, ply.Value.Position);

                // We add a little offset to avoid constant spawn-despawning.
                if (dist >= viewRadius + 8)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private void CheckAgentDespawn()
        {
            var now = DateTime.Now;
            if (now < _nextDespawn)
            {
                return;
            }

            _nextDespawn = now.AddSeconds(Limits.SpawnDespawnDelay);

            var log = Config.LoggingOpts.Despawns || EditorMode || Utils.IsDebugMode();
            foreach (var kv in _state.Active)
            {
                var agent = kv.Value;
                if (agent.CurrentState != Agent.State.Active)
                    continue;

                var insidePlayerView = IsInsidePlayerMaxView(agent.Position);
                if (insidePlayerView)
                    continue;

                Logging.CondInfo(log, "Agent {0} is outside player view, despawning {1}...", agent.Index, agent.EntityId);

                // Handle the despawn.
                if (_agentDespawnHandler != null)
                {
                    _agentDespawnHandler(this, agent);
                }

                // Activate in simulation.
                agent.CurrentState = Agent.State.Wandering;
                agent.EntityId = -1;
                // Reset spawn timestamp, allow immediate respawning in case the player backtracks.
                agent.LastSpawnTick = 0;

                _state.Active.Remove(kv.Key);
                _state.TotalDespawns++;

                break;
            }
        }
    }
}
