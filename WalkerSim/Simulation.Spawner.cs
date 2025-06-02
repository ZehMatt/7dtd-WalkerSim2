using System;
using System.Collections.Concurrent;

namespace WalkerSim
{
    internal partial class Simulation
    {
        public struct SpawnData
        {
            public Agent Agent;
            public Config.PostSpawnBehavior PostSpawnBehavior;
            public int ActivatorEntityId;
        }

        public delegate int AgentSpawnHandler(Simulation simulation, SpawnData agent);

        private AgentSpawnHandler _agentSpawnHandler;

        private ConcurrentQueue<SpawnData> _pendingSpawns = new ConcurrentQueue<SpawnData>();

        private DateTime _nextSpawn = DateTime.Now;

        private FixedBufferList<Agent> _nearPlayer = new FixedBufferList<Agent>(512);

        private DateTime _nextSpawnCheck = DateTime.Now;

        private bool _allowAgentSpawn = true;

        private int _nextFakeEntityId = 0;

        public void SetAgentSpawnHandler(AgentSpawnHandler handler)
        {
            _agentSpawnHandler = handler;
        }

        private bool HasReachedMaximumSpawnedAgents()
        {
            return _pendingSpawns.Count + _state.Active.Count >= _maxAllowedAliveAgents;
        }

        private int GetCountActiveNearby(Vector3 position, float radius)
        {
            var count = 0;

            foreach (var kv in _state.Active)
            {
                var agent = kv.Value;
                if (agent.CurrentState != Agent.State.Active)
                    continue;

                var dist = Vector3.Distance2D(position, agent.Position);
                if (dist <= radius)
                {
                    count++;
                }
            }

            return count;
        }

        private void CheckAgentSpawn()
        {
            if (_allowAgentSpawn == false)
            {
                return;
            }

            if (_nextSpawnCheck > DateTime.Now)
            {
                // We don't have to run this every tick/frame, agents typically don't move that fast.
                return;
            }

            _nextSpawnCheck = DateTime.Now.AddMilliseconds(200);

            // Don't activate them when they are in the inner radius.
            var activationBorderSize = 8.0f;

            var maxActivePerPlayer = _maxAllowedAliveAgents;
            if (_state.Players.Count > 0)
            {
                // If there are players, we allow more active agents per player.
                maxActivePerPlayer = _maxAllowedAliveAgents / _state.Players.Count;
            }

            foreach (var kv in _state.Players)
            {
                if (HasReachedMaximumSpawnedAgents())
                {
                    // We have reached the maximum amount of agents alive, do not spawn more.
                    Logging.Debug($"Maximum amount of agents alive reached, max: {_maxAllowedAliveAgents}, pending spawns: {_pendingSpawns.Count}, active agents: {_state.Active.Count}");

                    // Increase delay, no need to try again so soon when it is not possible to spawn more agents.
                    _nextSpawnCheck = DateTime.Now.AddMilliseconds(2000);

                    return;
                }

                var player = kv.Value;

                if (player.EntityId == -1)
                    continue;

                if (player.IsAlive == false)
                    continue;

                if (DateTime.Now < player.NextPossibleSpawnTime)
                {
                    //Logging.Debug("Player {0} is not alive long enough to spawn agents, skipping...", player.EntityId);
                    continue;
                }

                var activeNearby = GetCountActiveNearby(player.Position, player.ViewRadius);
                if (activeNearby >= maxActivePerPlayer)
                {
                    // Too many active agents nearby, do not spawn more.
                    Logging.Debug("Player {0} has too many active agents nearby ({1}), skipping spawn...", player.EntityId, activeNearby);

                    // Delay the test for this player.
                    player.NextPossibleSpawnTime = DateTime.Now.AddSeconds(1);

                    continue;
                }

                var playerPos = player.Position;

                _nearPlayer.Clear();
                QueryCells(playerPos, -1, player.ViewRadius, _nearPlayer);

                for (int i = 0; i < _nearPlayer.Count; i++)
                {
                    var agent = _nearPlayer[i];

                    if (agent.CurrentState != Agent.State.Wandering)
                        continue;

                    var dist = Vector3.Distance2D(playerPos, agent.Position);
                    if (dist < player.ViewRadius - activationBorderSize)
                        continue;

                    if (dist > player.ViewRadius)
                        continue;

                    // TODO: We are not handling overflow of Ticks but it takes a lot of time to get there.
                    var spawnDelta = _state.Ticks - agent.LastSpawnTick;
                    if (spawnDelta < Limits.MinSpawnDelayTicks)
                    {
                        // The actual spawning might fail and to avoid trying to spawn the same agent
                        // too often we skip it for a while.

                        Logging.Debug("Agent {0} was spawned too recently, skipping spawn, last spawn tick: {1}, current tick: {2}, delta: {3}",
                            agent.Index, agent.LastSpawnTick, _state.Ticks, spawnDelta);

                        continue;
                    }

                    Logging.Debug("Agent {0} near player {1} at {2}m, spawning...", agent.Index, player.EntityId, dist);

                    agent.LastSpawnTick = _state.Ticks;
                    agent.CurrentState = Agent.State.PendingSpawn;

                    var processorGroup = _processors[agent.Group];
                    var spawnData = new SpawnData()
                    {
                        Agent = agent,
                        PostSpawnBehavior = processorGroup != null ? processorGroup.PostSpawnBehavior : Config.PostSpawnBehavior.Wander,
                        ActivatorEntityId = player.EntityId
                    };
                    _pendingSpawns.Enqueue(spawnData);

                    // Bail out, in case there are more left in the activation border it will be handled next tick.
                    // This also gives other players a chance to spawn agents.
                    break;
                }
            }

#if DEBUG
            if (_pendingSpawns.Count > 100)
            {
                Logging.Warn("Excessive amount of pending spawns: {0}", _pendingSpawns.Count);
            }
#endif
        }

        private void ProcessSpawnQueue()
        {
            var now = DateTime.Now;
            if (now < _nextSpawn)
            {
                return;
            }

            SpawnData spawnData;
            if (!_pendingSpawns.TryDequeue(out spawnData))
            {
                return;
            }

            var agent = spawnData.Agent;
            _nextSpawn = now.AddSeconds(Limits.SpawnDespawnDelay);

            int agentEntityId = -1;
            if (_agentSpawnHandler != null)
            {
                agentEntityId = _agentSpawnHandler(this, spawnData);
            }
            else
            {
                if (EditorMode)
                {
                    agentEntityId = _nextFakeEntityId;
                    _nextFakeEntityId++;
                }
                else
                {
                    Logging.Err("No spawn handler registered");
                }
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
