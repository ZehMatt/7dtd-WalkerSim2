using System.Collections.Generic;
using UnityEngine;

namespace WalkerSim
{
    internal class SpawnManager
    {
        struct BiomeEntitySpawnData
        {
            public int entityClassId;
            public string className;
            public float prob;
        }

        static Dictionary<long, List<BiomeEntitySpawnData>> _spawnDataNight = new Dictionary<long, List<BiomeEntitySpawnData>>();
        static Dictionary<long, List<BiomeEntitySpawnData>> _spawnDataDay = new Dictionary<long, List<BiomeEntitySpawnData>>();
        static Dictionary<int, int> _classIdCounter = new Dictionary<int, int>();

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

        static int GetSpawnedClassIdCount(int classId)
        {
            if (_classIdCounter.TryGetValue(classId, out var count))
            {
                return count;
            }
            return 0;
        }

        static void IncrementSpawnedClassIdCount(int classId)
        {
            if (_classIdCounter.TryGetValue(classId, out var count))
            {
                _classIdCounter[classId] = count + 1;
            }
            else
            {
                _classIdCounter[classId] = 1;
            }
        }

        static void DecrementSpawnedClassIdCount(int classId, int entityId)
        {
            if (_classIdCounter.TryGetValue(classId, out var count))
            {
                if (count > 1)
                {
                    _classIdCounter[classId] = count - 1;
                }
                else
                {
                    Logging.DebugWarn("Decrementing class id {0} count below 0, entity {1}, bad counting, this is a bug.", classId, entityId);
                }
            }
        }

        static float GetEntityClassProbability(BiomeEntitySpawnData entry)
        {
            var prob = entry.prob;
            var classIdCount = GetSpawnedClassIdCount(entry.entityClassId);

            if (classIdCount > 0)
            {
                var penalty = 0.35f + (float)System.Math.Pow(classIdCount, 3.5);
                prob /= penalty;
            }

            return prob;
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

        static List<BiomeEntitySpawnData> GetBiomeEntityClasses(long chunkKey)
        {
            var world = GameManager.Instance.World;
            if (world.IsDaytime())
            {
                if (_spawnDataDay.TryGetValue(chunkKey, out var spawnList))
                {
                    return spawnList;
                }
            }
            else
            {
                if (_spawnDataNight.TryGetValue(chunkKey, out var spawnList))
                {
                    return spawnList;
                }
            }
            return null;
        }

        static void DeduplicateSpawnList(List<BiomeEntitySpawnData> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (entry.entityClassId == list[j].entityClassId)
                    {
                        entry.prob = System.Math.Max(entry.prob, list[j].prob);
                        list.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        static private int GetEntityClassId(Chunk chunk, UnityEngine.Vector3 worldPos)
        {
            var world = GameManager.Instance.World;

            if (world == null)
            {
                return -1;
            }

            List<BiomeEntitySpawnData> spawnList = GetBiomeEntityClasses(chunk.Key);
            if (spawnList == null)
            {
                int worldX = Mathf.FloorToInt(worldPos.x);
                int worldZ = Mathf.FloorToInt(worldPos.z);

                var biomeId = chunk.GetBiomeId(
                    World.toBlockXZ(worldX),
                    World.toBlockXZ(worldZ)
                );

                var biomeData = world.Biomes.GetBiome(biomeId);
                if (biomeData == null)
                {
                    Logging.Debug("Biome data is null for id {0}", biomeId);
                    return -1;
                }

                if (!BiomeSpawningClass.list.TryGetValue(biomeData.m_sBiomeName, out BiomeSpawnEntityGroupList biomeList))
                {
                    Logging.Debug("Biome data not found for id {0}", biomeId);
                    return -1;
                }

                // Get bit mask for enabled groups, we cache this to avoid doing it multiple times.
                int groupsEnabledFlags = 0;
                {
                    FastTags<TagGroup.Poi> fastTags = FastTags<TagGroup.Poi>.none;
                    Logging.Debug("About to get POIs");

                    List<PrefabInstance> spawnPIs = new List<PrefabInstance>();
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

                    Logging.Debug("ChunkFlags generated: {0}", groupsEnabledFlags);
                }

                // Gather all possible groups.
                var entityClassesDay = new List<BiomeEntitySpawnData>();
                var entityClassesNight = new List<BiomeEntitySpawnData>();

                // Extract possible groups from the bit mask.
                for (int i = 0; i < biomeList.list.Count; i++)
                {
                    var group = biomeList.list[i];

                    if ((groupsEnabledFlags & (1 << i)) == 0)
                    {
                        // This group is not enabled for this chunk.
                        continue;
                    }
                    if (EntityGroups.list.TryGetValue(group.entityGroupName, out var data))
                    {
                        foreach (var entry in data)
                        {
                            if (entry.entityClassId == 0)
                            {
                                // Ignore this case, not relevant in this situation.
                                continue;
                            }

                            if (group.daytime == EDaytime.Day || group.daytime == EDaytime.Any)
                            {
                                entityClassesDay.Add(new BiomeEntitySpawnData
                                {
                                    entityClassId = entry.entityClassId,
                                    className = EntityClass.GetEntityClassName(entry.entityClassId),
                                    prob = entry.prob
                                });
                            }

                            if (group.daytime == EDaytime.Night || group.daytime == EDaytime.Any)
                            {
                                entityClassesNight.Add(new BiomeEntitySpawnData
                                {
                                    entityClassId = entry.entityClassId,
                                    className = EntityClass.GetEntityClassName(entry.entityClassId),
                                    prob = entry.prob
                                });
                            }
                        }
                    }
                }

                // Sort the spawn lists by entity class id.
                entityClassesDay.Sort((a, b) => a.entityClassId.CompareTo(b.entityClassId));
                entityClassesNight.Sort((a, b) => a.entityClassId.CompareTo(b.entityClassId));

                // De-duplicate the spawn lists with identical entity class ids, select highest probability.
                DeduplicateSpawnList(entityClassesDay);
                DeduplicateSpawnList(entityClassesNight);

                // Cache the spawn data for this chunk.
                _spawnDataDay[chunk.Key] = entityClassesDay;
                _spawnDataNight[chunk.Key] = entityClassesNight;

                Logging.Debug("Spawn list for chunk {0} (Day: {1}, Night: {2}), Biome: {3}",
                    chunk.Key,
                    entityClassesDay.Count,
                    entityClassesNight.Count,
                    biomeData.m_sBiomeName);

                // Day.
                Logging.Debug("  Daytime >");
                foreach (var entry in entityClassesDay)
                {
                    Logging.Debug("    Class: {0} ({1}), Probability: {2}", entry.className, entry.entityClassId, entry.prob);
                }

                // Night.
                Logging.Debug("  Nighttime >");
                foreach (var entry in entityClassesNight)
                {
                    Logging.Debug("    Class: {0} ({1}), Probability: {2}", entry.className, entry.entityClassId, entry.prob);
                }

                spawnList = world.IsDaytime() ? entityClassesDay : entityClassesNight;
            }

            // Calculate the total probability.
            float totalProb = 0;
            for (int i = 0; i < spawnList.Count; i++)
            {
                var entry = spawnList[i];
                var prob = GetEntityClassProbability(entry);

                totalProb += prob;
            }

            // Select a random class id, it also attempts to avoid spawning duplicates.
            var selectedClassId = -1;
            var selectedClassName = string.Empty;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                var rand = Simulation.Instance.PRNG;
                var randomValue = rand.NextSingle() * totalProb;

                for (int i = 0; i < spawnList.Count; i++)
                {
                    var entry = spawnList[i];
                    var prob = GetEntityClassProbability(entry);

                    randomValue -= prob;

                    if (randomValue <= 0)
                    {
                        selectedClassId = entry.entityClassId;
                        selectedClassName = entry.className;
                        break;
                    }
                }

                if (GetSpawnedClassIdCount(selectedClassId) > 0)
                {
                    Logging.Debug("Selected entity class {0} ({1}) from {2} attempts", selectedClassName, selectedClassId, attempt + 1);
                    break;
                }
                else
                {
                    Logging.Debug("Selected entity class {0} ({1}) is a duplicate, retrying...", selectedClassName, selectedClassId);
                }
            }

            return selectedClassId;
        }

        static public int SpawnAgent(Simulation simulation, Simulation.SpawnData spawnData)
        {
            var agent = spawnData.Agent;

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
                    if (entityClassId == -1)
                    {
                        Logging.Err("Failed to get entity class id for chunk {0} at position {1}", chunk.Key, worldPos);
                    }

                    // 0 signals to not spawn, this is on purpose to change the weights, comes from entitygroups.xml.
                    return entityClassId;
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

            // Spawn.
            world.SpawnEntityInWorld(spawnedAgent);

            // Update attributes.
            spawnedAgent.bIsChunkObserver = true;
            spawnedAgent.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            spawnedAgent.ticksNoPlayerAdjacent = 130;

            // Because some Mods use the entitygroups.xml to do normal NPCs, we have to check this first.
            if (spawnedAgent is EntityZombie spawnedZombie)
            {
                spawnedZombie.IsHordeZombie = true;
            }

            if (agent.Health != -1)
            {
                Logging.Debug("Using previous health: {0}", agent.Health);
                spawnedAgent.Health = agent.Health;
            }

            // Post spawn behavior.
            if (spawnData.PostSpawnBehavior == Config.PostSpawnBehavior.Wander)
            {
                var destPos = worldPos + (rot * 80);

                // Adjust position by getting terrain height at the destination, they might dig if the destination is
                // below the terrain.
                destPos.y = world.GetTerrainHeight(Mathf.FloorToInt(destPos.x), Mathf.FloorToInt(destPos.z)) + 1;

                spawnedAgent.SetInvestigatePosition(destPos, 6000, false);
                Logging.Debug("Spawned agent wandering to {0}", destPos);
            }
            else if (spawnData.PostSpawnBehavior == Config.PostSpawnBehavior.ChaseActivator)
            {
                var activator = world.GetEntity(spawnData.ActivatorEntityId) as EntityAlive;
                if (activator != null)
                {
                    spawnedAgent.SetAttackTarget(activator, 6000);
                    Logging.Debug("Spawned agent chasing activator {0}", spawnData.ActivatorEntityId);
                }
                else
                {
                    Logging.Warn("This should never happen, zombie spawned with no activator.");
                }

            }
            else if (spawnData.PostSpawnBehavior == Config.PostSpawnBehavior.Nothing)
            {
                // Do nothing.
                Logging.Info("No PostSpawn action for agent {0}", spawnedAgent.entityId);
            }
            else
            {
                Logging.Err("Unknown post spawn behavior: {0}", spawnData.PostSpawnBehavior);
            }

            // Update the agent data.
            agent.EntityId = spawnedAgent.entityId;
            agent.EntityClassId = entityClassId;
            agent.CurrentState = Agent.State.Active;
            agent.Health = spawnedAgent.Health;

            if (simulation.Config.Debug.LogSpawnDespawn)
            {
                Logging.Out("Agent spawned at {0}, entity id {1}, class id {2}", worldPos, spawnedAgent.entityId, entityClassId);
            }

            IncrementSpawnedClassIdCount(entityClassId);

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

            DecrementSpawnedClassIdCount(entity.entityClass, entity.entityId);

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

        private static bool IsEntityDead(Entity entity, int entityId)
        {
            if (entity == null)
            {
                Logging.Debug("Entity not found: {0}, can't update state.", entityId);
                return true;
            }

            if (!entity.IsAlive())
            {
                Logging.Debug("Entity is dead: {0}", entityId);
                return true;
            }

            return false;
        }

        public static void UpdateActiveAgents()
        {
            var world = GameManager.Instance.World;

            var simulation = Simulation.Instance;

            // Don't allocate unless there are actual dead agents.
            List<Agent> deadAgents = null;

            foreach (var kv in simulation.Active)
            {
                var agent = kv.Value;
                if (agent.EntityId == -1)
                {
                    Logging.Debug("Agent has no entity id, skipping. Agent state: {0}", agent.CurrentState);
                    continue;
                }

                var entity = world.GetEntity(agent.EntityId);
                if (IsEntityDead(entity, agent.EntityId))
                {
                    // Mark as dead so it will be sweeped.
                    if (deadAgents == null)
                    {
                        deadAgents = new List<Agent>();
                    }
                    deadAgents.Add(agent);
                }
                else
                {
                    // Update position.
                    var newPos = entity.GetPosition();
                    agent.Position = VectorUtils.ToSim(newPos);
                    agent.Position.Validate();
                }
            }

            // Remove dead agents.
            if (deadAgents != null)
            {
                foreach (var agent in deadAgents)
                {
                    DecrementSpawnedClassIdCount(agent.EntityClassId, agent.EntityId);

                    simulation.MarkAgentDead(agent);
                }
            }
        }

        public static void ClearCache()
        {
            _spawnDataDay.Clear();
            _spawnDataNight.Clear();
            _classIdCounter.Clear();
        }
    }
}
