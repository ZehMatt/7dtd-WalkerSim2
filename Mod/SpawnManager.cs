using System.Collections.Generic;
using UnityEngine;

namespace WalkerSim
{
    internal class SpawnManager
    {
        static int _lastClassId = -1;

        static List<PrefabInstance> spawnPIs = new List<PrefabInstance>();

        static Dictionary<long, int> groupsEnabledMap = new Dictionary<long, int>();

        static private bool CanSpawnZombie()
        {
            // Check for maximum count.
            var alive = GameStats.GetInt(EnumGameStats.EnemyCount);

            // We only allow half of the max count to be spawned to give sleepers some room.
            // TODO: Make this a configuration.
            var maxAllowed = GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedZombies) / 2;

            if (alive >= maxAllowed)
            {
                Logging.Debug("Max zombies reached, alive: {0}, max: {1}", alive, maxAllowed);
                return false;
            }

            return true;
        }

        static private bool CanSpawnAtPosition(UnityEngine.Vector3 position)
        {
            var world = GameManager.Instance.World;
            if (world == null)
            {
                return false;
            }

            if (!world.CanMobsSpawnAtPos(position))
            {
                Logging.Debug("CanMobsSpawnAtPos returned false for position {0}", position);
                return false;
            }

            if (world.isPositionInRangeOfBedrolls(position))
            {
                Logging.Debug("Position {0} is near a bedroll", position);
                return false;
            }

            return true;
        }

        static private int GetEntityClassId(Chunk chunk, UnityEngine.Vector3 worldPos)
        {
            var world = GameManager.Instance.World;
            int worldX = Mathf.FloorToInt(worldPos.x);
            int worldZ = Mathf.FloorToInt(worldPos.z);

            if (world == null)
            {
                return -1;
            }

            var biomeId = chunk.GetBiomeId(
                World.toBlockXZ(worldX),
                World.toBlockXZ(worldZ)
            );

            var biomeData = world.Biomes.GetBiome(biomeId);

            if (BiomeSpawningClass.list.TryGetValue(biomeData.m_sBiomeName, out BiomeSpawnEntityGroupList biomeList))
            {
                EDaytime eDaytime = world.IsDaytime() ? EDaytime.Day : EDaytime.Night;
                GameRandom gameRandom = world.GetGameRandom();

                var maxAttempts = System.Math.Min(biomeList.list.Count, 10);
                var lastPick = -1;

                int groupsEnabledFlags = 0;

                Logging.Debug("Are POIs checked for chunk? {0}", groupsEnabledMap.ContainsKey(chunk.Key));

                // Only once per each chunk, check the tags of the POIs around that chunk
                if (!groupsEnabledMap.ContainsKey(chunk.Key))
                {
                    FastTags<TagGroup.Poi> fastTags = FastTags<TagGroup.Poi>.none;
                    Logging.Debug("About to get POIs");
                    world.GetPOIsAtXZ(worldX + 16, worldX + 80 - 16, worldZ + 16, worldZ + 80 - 16, spawnPIs);
                    Logging.Debug("Got {0} POIs", spawnPIs.Count);
                    for (int j = 0; j < spawnPIs.Count; j++)
                    {
                        // build list of tags for POIs around chunk
                        PrefabInstance prefabInstance = spawnPIs[j];
                        fastTags |= prefabInstance.prefab.Tags;
                    }
                    bool isEmpty = fastTags.IsEmpty;
                    for (int k = 0; k < biomeList.list.Count; k++)
                    {
                        // build bit mask for enabling groups. If the chunk tag list contains a value in the noTags set in the spawn group, disable it. If the chunk tag list has any tags, make sure the chunk has them too
                        BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeList.list[k];
                        if ((biomeSpawnEntityGroupData.POITags.IsEmpty || biomeSpawnEntityGroupData.POITags.Test_AnySet(fastTags)) && (isEmpty || biomeSpawnEntityGroupData.noPOITags.IsEmpty || !biomeSpawnEntityGroupData.noPOITags.Test_AnySet(fastTags)))
                        {
                            groupsEnabledFlags |= 1 << k;
                        }
                    }
                    groupsEnabledMap.Add(chunk.Key, groupsEnabledFlags);
                }

                groupsEnabledMap.TryGetValue(chunk.Key, out groupsEnabledFlags);
                Logging.Debug("ChunkFlags generated: {0}", groupsEnabledFlags);

                for (int i = 0; i < maxAttempts; i++)
                {
                    var randomPick = gameRandom.RandomRange(0, biomeList.list.Count);
                    if (randomPick == lastPick)
                    {
                        i--;
                        continue;
                    }

                    lastPick = randomPick;
                    var group = biomeList.list[randomPick];

                    // check the group enabled bit mask against the current pick
                    if ((groupsEnabledFlags & (1 << randomPick)) == 0)
                    {
                        Logging.Debug("Group Disabled: {0}", group.entityGroupName);
                        continue;
                    }

                    if (group.daytime != eDaytime && group.daytime != EDaytime.Any)
                    {
                        continue;
                    }

                    if (!EntityGroups.IsEnemyGroup(group.entityGroupName))
                    {
                        continue;
                    }

                    if (!EntityGroups.list.ContainsKey(group.entityGroupName))
                    {
                        Logging.Err("Entity group not found: {0}", group.entityGroupName);
                        continue;
                    }

                    int lastClassId = _lastClassId;

                    int entityClassId = EntityGroups.GetRandomFromGroup(group.entityGroupName, ref lastClassId, gameRandom);
                    if (entityClassId != -1 && entityClassId != 0)
                    {
                        Logging.Debug("Selected entity class id {0} : {1}", entityClassId, group.entityGroupName);
                        return entityClassId;
                    }
                }
            }

            return -1;
        }

        static public int SpawnAgent(Simulation simulation, Agent agent)
        {
            var world = GameManager.Instance.World;
            if (world == null)
            {
                return -1;
            }

            if (!CanSpawnZombie())
            {
                return -1;
            }

            var worldPos = VectorUtils.ToUnity(agent.Position);

            // We leave y position to be adjusted by the terrain.
            worldPos.y = 0;

            var chunkPosX = World.toChunkXZ(Mathf.FloorToInt(worldPos.x));
            var chunkPosZ = World.toChunkXZ(Mathf.FloorToInt(worldPos.z));

            var chunk = world.GetChunkSync(chunkPosX, chunkPosZ) as Chunk;
            if (chunk == null)
            {
                Logging.DebugErr("Failed to spawn agent, chunk not loaded at {0}, {1}", chunkPosX, chunkPosZ);
                return -1;
            }

            var terrainHeight = world.GetTerrainHeight(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.z)) + 1;
            Logging.Debug("Terrain height at {0}, {1} is {2}", worldPos.x, worldPos.z, terrainHeight);

            // Adjust position height.
            worldPos.y = terrainHeight;

            if (!CanSpawnAtPosition(worldPos))
            {
                Logging.Debug("Failed to spawn agent, position not suitable at {0}", worldPos);
                return -1;
            }

            if (simulation.Config.Debug.LogSpawnDespawn)
            {
                Logging.Out("Spawning agent at {0}", worldPos);
            }

            // Use previously assigned entity class id.
            int entityClassId = agent.EntityClassId;
            if (entityClassId == -1 || entityClassId == 0)
            {
                entityClassId = GetEntityClassId(chunk, worldPos);
                if (entityClassId == -1 || entityClassId == 0)
                {
                    // Fallback to random.
                    int lastClassId = -1;
                    entityClassId = EntityGroups.GetRandomFromGroup("ZombiesAll", ref lastClassId);

                    if (entityClassId == -1 || entityClassId == 0)
                    {
                        Logging.Err("Failed to get entity class id from ZombiesAll.");
                        return -1;
                    }
                }
            }
            else
            {
                Logging.Debug("Using previous entity class id: {0}", entityClassId);
            }

            var rot = VectorUtils.ToUnity(agent.Velocity);
            rot.y = 0;
            rot.Normalize();

            var spawnedAgent = EntityFactory.CreateEntity(entityClassId, worldPos, rot) as EntityAlive;
            if (spawnedAgent == null)
            {
                Logging.DebugErr("Unable to create zombie entity!, Class Id: {0}, Pos: {1}", entityClassId, worldPos);
                return -1;
            }

            // Only update last class id if we successfully spawned the agent.
            _lastClassId = entityClassId;

            spawnedAgent.bIsChunkObserver = true;
            spawnedAgent.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            spawnedAgent.moveDirection = rot;

            // Because some Mods use the entitygroups.xml to do normal NPCs, we have to check this first.
            if (spawnedAgent is EntityEnemy)
            {
                var enemy = spawnedAgent as EntityEnemy;
                enemy.IsHordeZombie = true;
            }

            if (spawnedAgent is EntityZombie spawnedZombie)
            {
                spawnedZombie.IsHordeZombie = true;
            }

            if (agent.Health != -1)
            {
                Logging.Debug("Using previous health: {0}", agent.Health);
                spawnedAgent.Health = agent.Health;
            }

            var destPos = worldPos + (rot * 80);
            spawnedAgent.SetInvestigatePosition(destPos, 6000, false);

            world.SpawnEntityInWorld(spawnedAgent);

            // Update the agent data.
            agent.EntityId = spawnedAgent.entityId;
            agent.EntityClassId = entityClassId;
            agent.CurrentState = Agent.State.Active;
            agent.Health = spawnedAgent.Health;

            if (simulation.Config.Debug.LogSpawnDespawn)
            {
                Logging.Out("Agent spawned at {0}, entity id {1}, class id {2}", worldPos, spawnedAgent.entityId, entityClassId);
            }

            return spawnedAgent.entityId;
        }

        static public bool DespawnAgent(Simulation simulation, Agent agent)
        {
            var world = GameManager.Instance.World;
            if (world == null)
            {
                return false;
            }

            var entity = world.GetEntity(agent.EntityId) as EntityAlive;
            if (entity == null)
            {
                Logging.Warn("Entity not found: {0}, unable to despawn.", agent.EntityId);
                return false;
            }

            // Retain current state.
            agent.Health = entity.Health;

            agent.Velocity = VectorUtils.ToSim(entity.moveDirection);
            agent.Velocity.Validate();

            agent.Position = VectorUtils.ToSim(entity.position);
            agent.Position.Validate();

            world.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Despawned);

            if (simulation.Config.Debug.LogSpawnDespawn)
            {
                Logging.Out("Agent despawned, entity id: {0}", agent.EntityId);
            }

            return true;
        }
    }
}
