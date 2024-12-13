using System;
using System.Collections.Generic;

namespace WalkerSim
{
    public class WalkerSimMod : IModApi
    {
        static DateTime _lastUpdate = DateTime.Now;

        void IModApi.InitMod(Mod _modInstance)
        {
            // Set up logging.
            Logging.SetHandler(Logging.Level.Info, Log.Out);
            Logging.SetHandler(Logging.Level.Warning, Log.Warning);
            Logging.SetHandler(Logging.Level.Error, Log.Error);

            // Setup the simulation window.
            SimulationWindow.Init();

            // Setup hooks.
            // TODO: Look into delaying this so clients can still use this with EAC when joining servers.
            //       IsHost() is not reliable at this point of time.
            Hooks.Init();

            // Register for events.
            ModEvents.GameAwake.RegisterHandler(GameAwake);
            ModEvents.GameStartDone.RegisterHandler(GameStartDone);
            ModEvents.GameUpdate.RegisterHandler(GameUpdate);
            ModEvents.GameShutdown.RegisterHandler(GameShutdown);
            ModEvents.EntityKilled.RegisterHandler(EntityKilled);

            // 1.2 broke PlayerSpawnedInWorld so we have to maintain the player list in a different way.
            // ModEvents.PlayerSpawnedInWorld.RegisterHandler(PlayerSpawnedInWorld);
            // ModEvents.PlayerDisconnected.RegisterHandler(PlayerDisconnected);

            Simulation.Instance.SetAgentSpawnHandler(SpawnManager.SpawnAgent);
            Simulation.Instance.SetAgentDespawnHandler(SpawnManager.DespawnAgent);
        }

        static void GameAwake()
        {
            Logging.Out("GameAwake");
        }

        internal static Config LoadConfiguration()
        {
            var worldFolder = PathAbstractions.WorldsSearchPaths.GetLocation(GamePrefs.GetString(EnumGamePrefs.GameWorld)).FullPath;
            Logging.Out("World Folder: {0}", worldFolder);

            var worldFolderConfig = System.IO.Path.Combine(worldFolder, "WalkerSim.xml");
            if (System.IO.File.Exists(worldFolderConfig))
            {
                Logging.Out("Found WalkerSim config for world, loading configuration from: {0}", worldFolderConfig);
                return Config.LoadFromFile(worldFolderConfig);
            }

            // Get the assembly path.
            var assemblyPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var defaultConfigPath = System.IO.Path.Combine(assemblyPath, "WalkerSim.xml");

            if (System.IO.File.Exists(defaultConfigPath))
            {
                Logging.Out("Loading default config from: {0}", defaultConfigPath);
                var config = Config.LoadFromFile(defaultConfigPath);
                if (config == null)
                {
                    Logging.Err("Failed to load default config, using defaults.");
                    return new Config();
                }
                return config;
            }

            Logging.Out("No config found, using defaults.");
            return Config.GetDefault();
        }

        static string GetSimulationSaveFile()
        {
            var saveFilePath = GameIO.GetSaveGameDir();
            return System.IO.Path.Combine(saveFilePath, "walkersim.bin");
        }

        static void ResetSimulation()
        {
            var world = GameManager.Instance.World;
            var simulation = Simulation.Instance;

            // Remove all active zombies as they will have no connection with the simulation anymore.
            foreach (var kv in simulation.Active)
            {
                world.RemoveEntity(kv.Key, EnumRemoveEntityReason.Despawned);
            }

            var config = LoadConfiguration();
            simulation.Reset(config);
            simulation.SetFastAdvanceAtStart(true);
        }

        internal static void RestartSimulation()
        {
            Logging.Out("Restarting simulation...");

            ResetSimulation();

            var simulation = Simulation.Instance;
            simulation.Start();
        }

        static void CompareConfig()
        {
            var config = LoadConfiguration();
            if (config == null)
                return;

            var simConfig = Simulation.Instance.Config;
            if (!simConfig.Compare(config))
            {
                Logging.Out("Configuration on disk is different than the configuration in the saved state. In order to apply the changes the simulation must be restarted, this can be done using 'walkersim restart' in the console.");
            }
        }

        static void InitializeSimulation()
        {
            var simulation = Simulation.Instance;

            // Load the map data
            {
                string worldFolder = PathAbstractions.WorldsSearchPaths.GetLocation(GamePrefs.GetString(EnumGamePrefs.GameWorld)).FullPath;
                Logging.Out("World Folder: {0}", worldFolder);

                Logging.Out("Loading Map Data...");
                var loaded = false;
                var elapsed = Utils.Measure(() =>
                {
                    loaded = simulation.LoadMapData(worldFolder);
                });

                if (loaded)
                    Logging.Out("Map Data Loaded in {0}.", elapsed);
                else
                    Logging.Err("Failed to load map data");
            }

            // Set world size
            {
                var world = GameManager.Instance.World;
                world.GetWorldExtent(out Vector3i min, out Vector3i max);

                var worldMins = new Vector3(min.x, min.x, min.y);
                var worldMaxs = new Vector3(max.x, max.x, max.y);

                simulation.SetWorldSize(worldMins, worldMaxs);
            }

            // Load or create the state.
            {
                var simFile = GetSimulationSaveFile();

                if (System.IO.File.Exists(simFile) && simulation.Load(simFile))
                {
                    Logging.Out("Using existing simulation from: {0}", simFile);
                    simulation.SetFastAdvanceAtStart(false);

                    CompareConfig();
                }
                else
                {
                    Logging.Out("No previous simulation found, starting new.");
                    ResetSimulation();
                }

                simulation.EnableAutoSave(simFile, 60.0f);
            }

            Logging.Out("Initialized Simulation World, World Size: {0}, {1}; Agents: {2}",
                simulation.WorldMins,
                simulation.WorldMaxs,
                simulation.Agents.Count);

            // Simulation will be resumed in GameUpdate, there are a couple conditions such as 
            // the requirement to have players before its resumed.
            simulation.SetPaused(true);

            simulation.Start();
        }

        static bool IsHost()
        {
            if (GameManager.IsDedicatedServer)
                return true;

            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                return true;

            return false;
        }

        static void GameStartDone()
        {
            if (!IsHost())
            {
                // If we are just a client we also avoid using hooks to not mess with EAC.
                Logging.Out("WalkerSim disabled, not host.");
                return;
            }

            InitializeSimulation();
        }

        static void UpdatePlayerPositions()
        {
            var world = GameManager.Instance.World;
            var players = world.Players.list;

            var simulation = Simulation.Instance;

            foreach (var player in players)
            {
                var entityId = player.entityId;
                var pos = player.GetPosition();

                // NOTE: Unity uses Y for vertical axis, we don't, screw that.
                simulation.UpdatePlayer(entityId, VectorUtils.ToSim(pos), player.IsAlive());
            }
        }

        static bool IsEntityDead(Entity entity, int entityId)
        {
            if (entity == null)
            {
                Logging.Debug("Entity not found: {0}", entityId);
                return true;
            }

            if (!entity.IsAlive())
            {
                Logging.Debug("Entity dead: {0}", entityId);
                return true;
            }

            return false;
        }

        static void UpdateActiveAgents()
        {
            var world = GameManager.Instance.World;

            var simulation = Simulation.Instance;

            foreach (var kv in simulation.Active)
            {
                var agent = kv.Value;
                if (agent.EntityId == -1)
                {
                    Logging.Debug("Agent has no entity id, skipping.");
                    continue;
                }

                var entity = world.GetEntity(agent.EntityId);
                if (IsEntityDead(entity, agent.EntityId))
                {
                    // Mark as dead so it will be sweeped.
                    simulation.MarkAgentDead(agent);
                }
                else
                {
                    // Update position.
                    var newPos = entity.GetPosition();
                    agent.Position = VectorUtils.ToSim(newPos);
                    agent.Position.Validate();
                }
            }
        }

        static void UpdateSimulation()
        {
            var now = DateTime.Now;
            var diff = now - _lastUpdate;

            _lastUpdate = now;

            var simulation = Simulation.Instance;
            simulation.GameUpdate((float)diff.TotalSeconds);
        }

        static void MaintainPlayerList(Simulation simulation, World world)
        {
            var players = world.Players.list;

            var maxViewDistance = GamePrefs.GetInt(EnumGamePrefs.ServerMaxAllowedViewDistance);
            var viewRadius = (maxViewDistance * 16) / 2;

            // Add new players.
            foreach (var player in players)
            {
                var entityId = player.entityId;
                if (!simulation.HasPlayer(entityId))
                {
                    simulation.AddPlayer(player.entityId, VectorUtils.ToSim(player.position), viewRadius);
                }
            }

            // Check who has gone missing.
            List<int> missingPlayers = null;
            foreach (var kv in simulation.Players)
            {
                var entityId = kv.Key;
                if (players.FindIndex(p => p.entityId == entityId) == -1)
                {
                    if (missingPlayers == null)
                    {
                        missingPlayers = new List<int>();
                    }
                    missingPlayers.Add(entityId);
                }
            }

            // Remove missing players.
            if (missingPlayers != null)
            {
                foreach (var entityId in missingPlayers)
                {
                    simulation.RemovePlayer(entityId);
                }
            }
        }

        static void GameUpdate()
        {
            if (!IsHost())
            {
                return;
            }

            var world = GameManager.Instance.World;
            if (world == null)
                return;

            var simulation = Simulation.Instance;
            if (simulation == null)
                return;

            // 1.2 broke PlayerSpawnedInWorld so we have to maintain the player list in a different way.
            // Remove this at a later point in time.
            MaintainPlayerList(simulation, world);

            // Check for enemy spawning
            {
                var allowEnemySpawn = GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode);

                simulation.SetEnableAgentSpawn(allowEnemySpawn);
            }

            // Check for pausing.
            {
                var isPaused = GameManager.Instance.IsPaused();
                if (simulation.PlayerCount == 0)
                {
                    isPaused = true;
                }

                // TODO: Validate if this is correct, there seems to be various ways to check this.
                if (simulation.Config.PauseDuringBloodmoon && world.isEventBloodMoon)
                {
                    isPaused = true;
                }

                simulation.SetPaused(isPaused);

                if (isPaused)
                {
                    return;
                }
            }

            try
            {
                UpdatePlayerPositions();
                UpdateActiveAgents();
                UpdateSimulation();
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
            }
        }

        static void GameShutdown()
        {
            var simulation = Simulation.Instance;
            simulation.AutoSave();
            simulation.Stop();
        }

        static int GetPlayerEntityId(ClientInfo _cInfo)
        {
            if (_cInfo != null)
                return _cInfo.entityId;

            // On a local host this is set to null, grab id from player list.
            var world = GameManager.Instance.World;
            var player = world.Players.list[0];

            return player.entityId;
        }

        static void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            if (!IsHost())
            {
                return;
            }

            var simulation = Simulation.Instance;

            var maxViewDistance = GamePrefs.GetInt(EnumGamePrefs.ServerMaxAllowedViewDistance);
            var viewRadius = (maxViewDistance * 16) / 2;

            Logging.Out("Max View Distance: {0}", maxViewDistance);
            Logging.Out("View Radius: {0}", viewRadius);

            switch (_respawnReason)
            {
                case RespawnType.JoinMultiplayer:
                case RespawnType.EnterMultiplayer:
                case RespawnType.NewGame:
                case RespawnType.LoadedGame:
                    var entityId = GetPlayerEntityId(_cInfo);
                    // NOTE: Unity uses Y for vertical axis, we don't, screw that.
                    simulation.AddPlayer(entityId, VectorUtils.ToSim(_pos), viewRadius);
                    break;
            }
        }

        static void PlayerDisconnected(ClientInfo _cInfo, bool _bShutdown)
        {
            if (!IsHost())
            {
                return;
            }

            var simulation = Simulation.Instance;

            var entityId = GetPlayerEntityId(_cInfo);

            simulation.RemovePlayer(entityId);
        }

        static void EntityKilled(Entity _entity, Entity _source)
        {
            if (!IsHost())
            {
                return;
            }

            var simulation = Simulation.Instance;
            var entityId = _entity.entityId;

            simulation.EntityKilled(entityId);
        }

        internal static void NotifyNoise(Entity instigator, UnityEngine.Vector3 position, string clipName, float volumeScale)
        {
            if (!IsHost())
            {
                return;
            }

            if (!AIDirectorData.FindNoise(clipName, out AIDirectorData.Noise noise) || instigator is EntityEnemy)
            {
                return;
            }

            if (noise.heatMapStrength == 0.0f)
            {
                return;
            }

#if false
            // Log all variables from noise.
            Logging.Info("Noise: {0}, Volume: {1}, Duration: {2}, MuffledWhenCrouched: {3}, HeatMapStrength: {4}, HeatMapWorldTimeToLive: {5}, volumeScale: {6}.",
                               clipName,
                               noise.volume,
                               noise.duration,
                               noise.muffledWhenCrouched,
                               noise.heatMapStrength,
                               noise.heatMapWorldTimeToLive,
                               volumeScale);
#endif

            if (instigator != null)
            {
                if (instigator.IsIgnoredByAI())
                {
                    return;
                }
            }

            var simulation = Simulation.Instance;
            var distance = noise.volume * volumeScale * 3.8f;

            simulation.AddSoundEvent(VectorUtils.ToSim(position), distance, noise.duration * 20);
        }
    }
}
