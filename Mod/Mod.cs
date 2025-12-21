using System;

namespace WalkerSim
{
    public class WalkerSimMod : IModApi
    {
        static DateTime _lastUpdate = DateTime.Now;

        void IModApi.InitMod(Mod _modInstance)
        {
            // Set the image loader to Unity.
            Drawing.Loader = new Unity.Drawing.UnityImageLoader();

            // Set up logging.
            Logging.AddSink(new LogFileSink("WalkerSim"));
            Logging.AddSink(new LogGameConsoleSink());

            Hooks.Init();

            // Register for events.
            ModEvents.GameAwake.RegisterHandler(GameAwake);
            ModEvents.GameStartDone.RegisterHandler(GameStartDone);
            ModEvents.GameUpdate.RegisterHandler(GameUpdate);
            ModEvents.GameShutdown.RegisterHandler(GameShutdown);
            ModEvents.WorldShuttingDown.RegisterHandler(WorldShuttingdown);
            ModEvents.EntityKilled.RegisterHandler(EntityKilled);

            ModEvents.PlayerSpawnedInWorld.RegisterHandler(PlayerSpawnedInWorld);
            ModEvents.PlayerDisconnected.RegisterHandler(PlayerDisconnected);

            Simulation.Instance.SetAgentSpawnHandler(SpawnManager.SpawnAgent);
            Simulation.Instance.SetAgentDespawnHandler(SpawnManager.DespawnAgent);

            Logging.Out($"WalkerSim v{BuildInfo.Version} initialized.");
        }

        static void GameAwake(ref ModEvents.SGameAwakeData data)
        {
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

            Logging.Warn("No config found, using defaults.");
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


            {
                var maxSpawnedSetting = simulation.Config.MaxSpawnedZombies;
                var gameMaxSpawnedAlive = GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedZombies);
                var maxAliveAllowed = 0;

                if (maxSpawnedSetting.EndsWith("%"))
                {
                    // Perecentage of gameMaxSpawnedAlive
                    if (int.TryParse(maxSpawnedSetting.TrimEnd('%'), out var perc))
                    {
                        perc = Math.Min(Math.Max(perc, 1), 200);
                        maxAliveAllowed = gameMaxSpawnedAlive * perc / 100;
                    }
                    else
                    {
                        Logging.Err("Invalid MaxSpawnedZombies percentage setting: {0}, using 75% of game setting.", maxSpawnedSetting);
                        maxAliveAllowed = gameMaxSpawnedAlive * 75 / 100;
                    }
                }
                else
                {
                    if (int.TryParse(maxSpawnedSetting, out var abs))
                    {
                        maxAliveAllowed = Math.Min(Math.Max(abs, 1), 200);
                    }
                    else
                    {
                        Logging.Err("Invalid MaxSpawnedZombies absolute setting: {0}, using 75% of game setting.", maxSpawnedSetting);
                        maxAliveAllowed = gameMaxSpawnedAlive * 75 / 100;
                    }
                }

                simulation.SetMaxAllowedAliveAgents(maxAliveAllowed);

                Logging.Out("Max Allowed Alive Agents: {0}", maxAliveAllowed);
            }
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
                Logging.Warn("Configuration on disk is different than the configuration in the saved state. In order to apply the changes the simulation must be restarted, this can be done using 'walkersim restart' in the console.");
            }
        }

        static (Vector3, Vector3) GetActualWorldSize()
        {
            var world = GameManager.Instance.World;
            world.GetWorldExtent(out Vector3i min, out Vector3i max);

            // Construct the corrected min and max vectors
            var worldMins = VectorUtils.ToSim(min);
            var worldMaxs = VectorUtils.ToSim(max);

            // NOTE: Because the conversion might rotate the axis we have to adjust for
            // the correct min and max values.
            var actualMinX = Math.Min(worldMins.X, worldMaxs.X);
            var actualMinY = Math.Min(worldMins.Y, worldMaxs.Y);
            var actualMinZ = Math.Min(worldMins.Z, worldMaxs.Z);
            var actualMaxX = Math.Max(worldMins.X, worldMaxs.X);
            var actualMaxY = Math.Max(worldMins.Y, worldMaxs.Y);
            var actualMaxZ = Math.Max(worldMins.Z, worldMaxs.Z);

            worldMins = new Vector3(actualMinX, actualMinY, actualMinZ);
            worldMaxs = new Vector3(actualMaxX, actualMaxY, actualMaxZ);

            return (worldMins, worldMaxs);
        }

        static void InitializeSimulation()
        {
            var simulation = Simulation.Instance;

            string worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
            string worldFolder = PathAbstractions.WorldsSearchPaths.GetLocation(worldName).FullPath;

            // Load the map data
            {
                Logging.Out("World Folder: {0}", worldFolder);

                Logging.Out("Loading Map Data...");
                var loaded = false;
                var elapsed = Utils.Measure(() =>
                {
                    loaded = simulation.LoadMapData(worldFolder, worldName);
                });

                if (loaded)
                    Logging.Out("Map Data Loaded in {0}.", elapsed);
                else
                    Logging.Err("Failed to load map data");
            }

            var (worldMins, worldMaxs) = GetActualWorldSize();

            // Set to actual world size.
            {
                Logging.Out("Actual World Size: {0}, {1}", worldMins, worldMaxs);

                simulation.SetWorldSize(worldMins, worldMaxs);
            }

            // Load or create the state.
            {
                var simFile = GetSimulationSaveFile();
                var resetSim = false;

                if (System.IO.File.Exists(simFile) && simulation.Load(simFile))
                {
                    Logging.Out("Using existing simulation from: {0}", simFile);
                    CompareConfig();

                    // See if world size matches.
                    if (simulation.WorldMins != worldMins || simulation.WorldMaxs != worldMaxs)
                    {
                        Logging.Warn("World size in simulation does not match the actual world size, resetting simulation." +
                            "This should never happen, make sure the save folder does not contain a 'walkersim.bin' save file from another game.");
                        resetSim = true;
                    }

                    if (simulation.WorldName != worldName)
                    {
                        Logging.Warn("World name in simulation does not match the actual world name, resetting simulation." +
                            "This should never happen, make sure the save folder does not contain a 'walkersim.bin' save file from another game.");
                        resetSim = true;
                    }
                }
                else
                {
                    Logging.Out("No previous simulation found, starting new.");
                    resetSim = true;
                }

                if (resetSim)
                {
                    ResetSimulation();
                }

                simulation.EnableAutoSave(simFile, 60.0f);
            }

            Logging.Out("Initialized Simulation World, World Size: {0}, {1}; Agents: {2}",
                simulation.WorldMins,
                simulation.WorldMaxs,
                simulation.Agents.Count);

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

        static void GameStartDone(ref ModEvents.SGameStartDoneData data)
        {
            Logging.Info("GameStartDone, setting up simulation...");

            if (!IsHost())
            {
                // If we are just a client we also avoid using hooks to not mess with EAC.
                Logging.Out("WalkerSim disabled, not host.");
                return;
            }

            // Ensure we have a cache only from this session.
            SpawnManager.ClearCache();

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

        static void UpdateSimulation()
        {
            var now = DateTime.Now;
            var diff = now - _lastUpdate;

            _lastUpdate = now;

            var simulation = Simulation.Instance;
            simulation.GameUpdate((float)diff.TotalSeconds);
        }

        static void GameUpdate(ref ModEvents.SGameUpdateData data)
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

            // Update state.
            simulation.SetEnableAgentSpawn(GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode));
            simulation.SetIsBloodmoon(world.isEventBloodMoon);
            simulation.SetIsDayTime(world.IsDaytime());
            simulation.SetGamePaused(GameManager.Instance.IsPaused());

            try
            {
                UpdatePlayerPositions();
                SpawnManager.UpdateActiveAgents();
                UpdateSimulation();
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
            }
        }

        static void ShutdownSimulation()
        {
            var simulation = Simulation.Instance;
            simulation.Stop();
            simulation.AutoSave();
        }

        static void GameShutdown(ref ModEvents.SGameShutdownData data)
        {
            Logging.Info("GameShutdown, stopping simulation...");

            ShutdownSimulation();
        }

        static void WorldShuttingdown(ref ModEvents.SWorldShuttingDownData data)
        {
            Logging.Info("WorldShuttingdown, stopping simulation...");

            ShutdownSimulation();
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

        static void PlayerSpawnedInWorld(ref ModEvents.SPlayerSpawnedInWorldData data)
        {
            if (!IsHost())
            {
                return;
            }

            var simulation = Simulation.Instance;

            Logging.DbgInfo("Player Spawn: {0}", data.RespawnType);
            Logging.DbgInfo("Spawn Position: {0}", data.Position);

            int spawnDelay = 0;
            switch (data.RespawnType)
            {
                case RespawnType.JoinMultiplayer:
                case RespawnType.EnterMultiplayer:
                case RespawnType.LoadedGame:
                case RespawnType.Died:
                    spawnDelay = 30;
                    break;
                case RespawnType.NewGame:
                    spawnDelay = simulation.Config.SpawnProtectionTime;
                    break;
            }

            switch (data.RespawnType)
            {
                case RespawnType.JoinMultiplayer:
                case RespawnType.EnterMultiplayer:
                case RespawnType.NewGame:
                case RespawnType.LoadedGame:
                    var entityId = GetPlayerEntityId(data.ClientInfo);
                    simulation.AddPlayer(entityId, VectorUtils.ToSim(data.Position), spawnDelay);
                    break;
                case RespawnType.Died:
                    simulation.NotifyPlayerSpawned(GetPlayerEntityId(data.ClientInfo), spawnDelay);
                    break;
            }
        }

        static void PlayerDisconnected(ref ModEvents.SPlayerDisconnectedData data)
        {
            if (!IsHost())
            {
                return;
            }

            var simulation = Simulation.Instance;

            var entityId = GetPlayerEntityId(data.ClientInfo);

            simulation.RemovePlayer(entityId);
        }

        static void EntityKilled(ref ModEvents.SEntityKilledData data)
        {
            if (!IsHost())
            {
                return;
            }

            var killedEntity = data.KilledEntitiy;

            var simulation = Simulation.Instance;
            var entityId = killedEntity.entityId;

            simulation.EntityKilled(entityId);
        }

        internal static void NotifyNoise(Entity instigator, UnityEngine.Vector3 position, string clipName, float volumeScale)
        {
            if (!IsHost())
            {
                return;
            }

            var simulation = Simulation.Instance;
            if (simulation == null)
            {
                return;
            }

            var config = simulation.Config;
            if (config == null)
            {
                return;
            }

            var logEvents = config.LoggingOpts.Events;

            if (!AIDirectorData.FindNoise(clipName, out AIDirectorData.Noise noise))
            {
                return;
            }

            var distance = noise.volume * volumeScale * 3.3f;
            var normalizedHeatmapStrength = Math.Min(noise.heatMapStrength, 1.0f);
            var distanceScaled = distance * normalizedHeatmapStrength;
            var eventDuration = noise.heatMapWorldTimeToLive / 60;

            // Log all variables from noise.
            Logging.CondInfo(logEvents, "Noise: {0}, " +
                " Volume: {1}, " +
                "Duration: {2}, " +
                "MuffledWhenCrouched: {3}, " +
                "HeatMapStrength: {4}, " +
                "HeatMapWorldTimeToLive: {5}, " +
                "volumeScale: {6}, " +
                "Travel Distance: {7}, " +
                "Scaled Travel Distance: {8}",
                               clipName,
                               noise.volume,
                               noise.duration,
                               noise.muffledWhenCrouched,
                               noise.heatMapStrength,
                               noise.heatMapWorldTimeToLive,
                               volumeScale,
                               distance,
                               distanceScaled);

            if (noise.heatMapStrength == 0.0f)
            {
                return;
            }

            if (instigator != null && instigator.IsIgnoredByAI())
            {
                return;
            }

            simulation.AddSoundEvent(VectorUtils.ToSim(position), distanceScaled, eventDuration);

            if (simulation.Config.EnhancedSoundAwareness)
            {
                NotifyNearbyEnemies(simulation, instigator, position, distanceScaled);
            }
        }

        internal static void NotifyNearbyEnemies(Simulation simulation, Entity instigator, UnityEngine.Vector3 position, float distance)
        {
            if (!IsHost())
            {
                return;
            }

            if (instigator is EntityEnemy)
            {
                return;
            }

            var world = GameManager.Instance.World;
            var logEvents = simulation.Config.LoggingOpts.Events;

            // Notify all nearby enemies in idle state.
            foreach (var active in Simulation.Instance.Active)
            {
                var ent = world.GetEntity(active.Key) as EntityEnemy;
                if (ent == null || ent.IsDead())
                {
                    continue;
                }

                if (ent.attackTarget != null || ent.IsAlert)
                {
                    continue;
                }

                var entPos = ent.GetPosition();
                var entDist = UnityEngine.Vector3.Distance(entPos, position);
                if (entDist > distance)
                {
                    continue;
                }

                Logging.CondInfo(logEvents, "Alerting enemy {0} at {1} to noise at {2}, distance: {3}.",
                             ent.entityId, entPos, position, entDist);

                // Prevent them from digging.
                var heightOffset = new UnityEngine.Vector3(0, 1.5f, 0);

                // Have them interested for for 2~ min, assuming 30 ticks a second.
                ent.SetInvestigatePosition(position + heightOffset, 3600, true);
            }
        }

        internal static void GetZombieWanderingSpeed(EntityHuman entity, ref float speed)
        {
            var simulation = Simulation.Instance;
            if (simulation == null || simulation.Config == null)
            {
                return;
            }

            if (!simulation.Active.ContainsKey(entity.entityId))
            {
                // Apply this only to zombies spawned by the simulation.
                Logging.Info("Entity {0} is not managed by the simulation, skipping wandering speed override.", entity.entityId);

                return;
            }

            if (entity.IsAlert)
            {
                Logging.Info("Entity {0} is alert, skipping wandering speed override.", entity.entityId);
                return;
            }

            if (entity.GetAttackTarget() != null)
            {
                Logging.Info("Entity {0} has an attack target, skipping wandering speed override.", entity.entityId);
                return;
            }


            var wanderSpeed = simulation.GetPostSpawnWanderSpeed(entity.entityId);
            if (wanderSpeed == Config.WanderingSpeed.NoOverride)
            {
                return;
            }

            //Logging.Info("Selected wandering speed override {0} for entity {1}.", wanderSpeed, entity.entityId);

            int walkSpeedSetting = (int)wanderSpeed - 1;

            float moveSpeed = EntityHuman.moveSpeeds[walkSpeedSetting];
            if (entity.moveSpeedRagePer > 1f)
            {
                moveSpeed = EntityHuman.moveSuperRageSpeeds[walkSpeedSetting];
            }
            else if (entity.moveSpeedRagePer > 0f)
            {
                float num2 = EntityHuman.moveRageSpeeds[walkSpeedSetting];
                moveSpeed = moveSpeed * (1f - entity.moveSpeedRagePer) + num2 * entity.moveSpeedRagePer;
            }

            if (moveSpeed < 1f)
            {
                moveSpeed = entity.moveSpeedAggro * (1f - moveSpeed) + entity.moveSpeed * moveSpeed;
            }
            else
            {
                moveSpeed = entity.moveSpeedAggroMax * moveSpeed;
            }

            moveSpeed *= entity.moveSpeedPatternScale;

            var newSpeed = EffectManager.GetValue(PassiveEffects.RunSpeed, null, moveSpeed, entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);

            //Logging.Info("Overriding wandering speed for entity {0} from {1} to {2}.", entity.entityId, speed, newSpeed);

            speed = newSpeed;
        }
    }
}
