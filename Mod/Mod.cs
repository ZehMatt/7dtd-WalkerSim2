using System;

namespace WalkerSim
{
    public class WalkerSimMod : IModApi
    {
        static DateTime lastUpdate = DateTime.Now;

        void IModApi.InitMod(Mod _modInstance)
        {
            // Set up logging.
            Logging.SetHandler(Logging.Level.Info, Log.Out);
            Logging.SetHandler(Logging.Level.Warning, Log.Warning);
            Logging.SetHandler(Logging.Level.Error, Log.Error);

            // Setup the simulation window.
            SimulationWindow.Init();

            // Setup hooks.
            Hooks.Init();

            // Register for events.
            ModEvents.GameAwake.RegisterHandler(GameAwake);
            ModEvents.GameStartDone.RegisterHandler(GameStartDone);
            ModEvents.GameUpdate.RegisterHandler(GameUpdate);
            ModEvents.GameShutdown.RegisterHandler(GameShutdown);
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(PlayerSpawnedInWorld);
            ModEvents.PlayerDisconnected.RegisterHandler(PlayerDisconnected);
            ModEvents.EntityKilled.RegisterHandler(EntityKilled);

            Simulation.Instance.SetAgentSpawnHandler(SpawnManager.SpawnAgent);
            Simulation.Instance.SetAgentDespawnHandler(SpawnManager.DespawnAgent);
        }

        static void GameAwake()
        {
        }

        static Config LoadConfiguration()
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
            return new Config();
        }

        static void InitializeSimWorld()
        {
            var simulation = Simulation.Instance;

            var world = GameManager.Instance.World;
            world.GetWorldExtent(out Vector3i min, out Vector3i max);

            var worldMins = new Vector3(min.x, min.x, min.y);
            var worldMaxs = new Vector3(max.x, max.x, max.y);

            var config = LoadConfiguration();
            simulation.Reset(worldMins, worldMaxs, config);

            Logging.Out("Initialized Simulation World, World Size: {0}, {1}; Agents: {2}", worldMins, worldMaxs, config.MaxAgents);
        }

        static void LoadMapData()
        {
            var simulation = Simulation.Instance;

            // Get the path to the world directory.
            string worldFolder = PathAbstractions.WorldsSearchPaths.GetLocation(GamePrefs.GetString(EnumGamePrefs.GameWorld)).FullPath;
            Logging.Out("World Folder: {0}", worldFolder);

            Logging.Out("Loading Map Data...");
            var loaded = false;
            var elapsed = Utils.Measure(() =>
            {
                loaded = simulation.LoadMapData(worldFolder);
            });

            if (loaded)
                Logging.Out("Map Data Loaded in {0}ms", elapsed.TotalMilliseconds);
            else
                Logging.Err("Failed to load map data");
        }

        static void GameStartDone()
        {
            InitializeSimWorld();

            LoadMapData();

            var simulation = Simulation.Instance;

            Logging.Out("Spinning up simulation...");
            for (int i = 0; i < 5000; i++)
            {
                simulation.Tick();
            }

            Logging.Out("done, starting simulation.");
            simulation.Start();
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
                simulation.UpdatePlayer(entityId, VectorUtils.ToSim(pos));
            }
        }

        static void UpdateActiveAgents()
        {
            var world = GameManager.Instance.World;

            var simulation = Simulation.Instance;

            foreach (var kv in simulation.Active)
            {
                var agent = kv.Value;
                if (agent.EntityId == -1)
                    continue;

                var entity = world.GetEntity(agent.EntityId);
                if (entity == null)
                {
                    Logging.Out("Entity not found: {0}", agent.EntityId);

                    // Mark as dead so it will be sweeped.
                    agent.CurrentState = Agent.State.Dead;
                    agent.ResetSpawnData();

                    continue;
                }

                if (!entity.IsAlive())
                {
                    Logging.Out("Entity dead: {0}", agent.EntityId);

                    // Mark as dead so it will be sweeped.
                    agent.CurrentState = Agent.State.Dead;
                    agent.ResetSpawnData();

                    continue;
                }

                var newPos = entity.GetPosition();
                agent.Position = VectorUtils.ToSim(newPos);
                agent.Position.Validate();
            }
        }

        static void UpdateSimulation()
        {
            var now = DateTime.Now;
            var diff = now - lastUpdate;

            lastUpdate = now;

            var simulation = Simulation.Instance;
            simulation.Update((float)diff.TotalSeconds);
        }

        static void GameUpdate()
        {
            var world = GameManager.Instance.World;
            if (world == null)
                return;

            var simulation = Simulation.Instance;
            if (simulation == null)
                return;

            var isPaused = GameManager.Instance.IsPaused();
            simulation.SetPaused(isPaused);

            if (isPaused)
            {
                return;
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
            var simulation = Simulation.Instance;

            var entityId = GetPlayerEntityId(_cInfo);

            simulation.RemovePlayer(entityId);
        }

        static void EntityKilled(Entity _entity, Entity _source)
        {
            var simulation = Simulation.Instance;

            var entityId = _entity.entityId;

            simulation.EntityKilled(entityId);
        }

        internal static void NotifyNoise(Entity instigator, UnityEngine.Vector3 position, string clipName, float volumeScale)
        {
            if (!AIDirectorData.FindNoise(clipName, out AIDirectorData.Noise noise) || instigator is EntityEnemy)
            {
                return;
            }

            if (instigator != null)
            {
                if (instigator.IsIgnoredByAI())
                {
                    return;
                }
            }

            // Log all variables from noise.
            Logging.Debug("Noise: {0}, Volume: {1}, Duration: {2}, MuffledWhenCrouched: {3}, HeatMapStrength: {4}, HeatMapWorldTimeToLive: {5}",
                               clipName,
                               noise.volume,
                               noise.duration,
                               noise.muffledWhenCrouched,
                               noise.heatMapStrength,
                               noise.heatMapWorldTimeToLive);

            var simulation = Simulation.Instance;

            var sndInfo = SoundInfo.GetSoundInfo(clipName);
            if (sndInfo == null)
            {
                Logging.Debug("Not SoundInfo for: {0}", clipName);
                return;
            }

            simulation.AddNoiseEvent(VectorUtils.ToSim(position), sndInfo.Radius, sndInfo.DecayRate);
        }
    }
}
