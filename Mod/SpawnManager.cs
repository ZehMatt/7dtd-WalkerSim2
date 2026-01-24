using System.Collections.Generic;
using UnityEngine;

namespace WalkerSim
{
    internal class SpawnManager
    {
        static Dictionary<long, List<SEntityClassAndProb>> _spawnDataNight = new Dictionary<long, List<SEntityClassAndProb>>();
        static Dictionary<long, List<SEntityClassAndProb>> _spawnDataDay = new Dictionary<long, List<SEntityClassAndProb>>();
        static Dictionary<int, int> _classIdCounter = new Dictionary<int, int>();
        const int MaxSpawnRetryAttempts = 10;

        static private bool CanSpawnZombie(Simulation simulation)
        {
            // Check for maximum count, this is ordinarily checked before spawning but to be sure.
            var alive = simulation.ActiveCount;
            var maxAllowed = simulation.MaxAllowedAliveAgents;

            if (alive >= maxAllowed)
            {
                Logging.DbgInfo("Max zombies reached, alive: {0}, max: {1}", alive, maxAllowed);
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
                if (count > 0)
                {
                    _classIdCounter[classId] = count - 1;
                }
                else
                {
                    Logging.DbgErr("Decrementing class id {0} count below 0, entity {1}, bad counting, this is a bug.", classId, entityId);
                }
            }
        }

        static float GetEntityClassProbability(float prob, int entityClassId)
        {
            var classIdCount = GetSpawnedClassIdCount(entityClassId);

            if (classIdCount > 0)
            {
                var penalty = 1.75f + (float)System.Math.Pow(classIdCount, 7.5);
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
                Logging.DbgInfo("CanMobsSpawnAtPos returned false for position {0}", position);
                return false;
            }

            if (world.isPositionInRangeOfBedrolls(position))
            {
                Logging.DbgInfo("Position {0} is near a bedroll", position);
                return false;
            }

            return true;
        }

        static List<SEntityClassAndProb> GetBiomeEntityClasses(long chunkKey)
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

        static string GetEntityClassName(int classId)
        {
            var classInfo = EntityClass.GetEntityClass(classId);
            if (classInfo != null)
            {
                return classInfo.entityClassName;
            }
            return string.Empty;
        }

        static void DeduplicateSpawnList(List<SEntityClassAndProb> list)
        {
            list.Sort((a, b) => a.entityClassId.CompareTo(b.entityClassId));

            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (entry.entityClassId == list[j].entityClassId)
                    {
                        entry.prob = System.Math.Max(entry.prob, list[j].prob);

                        Logging.DbgInfo("Deduplicating entity class {0} ({1}), keeping highest probability {2}",
                            GetEntityClassName(entry.entityClassId), entry.entityClassId, entry.prob);

                        list.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        static private int GetEntityClassIdFromMask(Simulation simulation, Chunk chunk, UnityEngine.Vector3 worldPos)
        {
            var world = GameManager.Instance.World;
            if (world == null)
            {
                return -1;
            }

            var mapData = simulation.MapData;
            if (mapData == null)
            {
                return -1;
            }

            var spawnGroups = simulation.MapData.SpawnGroups;
            if (spawnGroups == null)
            {
                return -1;
            }

            // Remap the position
            var pos = VectorUtils.ToSim(worldPos);
            var worldMins = mapData.WorldMins;
            var worldMaxs = mapData.WorldMaxs;
            var worldSize = mapData.WorldSize;
            var x = (int)MathEx.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, worldSize.X);
            var y = (int)worldSize.Y - (int)MathEx.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, worldSize.Y);

            // Get the spawn group mask for this position.
            var spawnGroup = spawnGroups.GetSpawnGroup(x, y);
            if (spawnGroup == null)
            {
                return -1;
            }

            var groupName = world.IsDaytime() ? spawnGroup.EntityGroupDay : spawnGroup.EntityGroupNight;
            if (string.IsNullOrEmpty(groupName))
            {
                Logging.CondInfo(simulation.Config.LoggingOpts.EntityClassSelection,
                    "No entity group found for position {0}, spawn group: {1}",
                    worldPos, spawnGroup);
                return -1;
            }

            if (!EntityGroups.list.TryGetValue(groupName, out var entityGroupData))
            {
                Logging.CondInfo(simulation.Config.LoggingOpts.EntityClassSelection,
                    "Entity group not found: {0} for position {1}, spawn group: {2}",
                    groupName, worldPos, spawnGroup);
                return -1;
            }

            return PerformSelection(simulation, entityGroupData);
        }

        static private int GetEntityClassId(Simulation simulation, Chunk chunk, UnityEngine.Vector3 worldPos)
        {
            var world = GameManager.Instance.World;
            if (world == null)
            {
                return -1;
            }

            var config = simulation.Config;

            List<SEntityClassAndProb> spawnList = GetBiomeEntityClasses(chunk.Key);
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
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Biome data is null for id {0}",
                        biomeId);
                    return -1;
                }

                if (!BiomeSpawningClass.list.TryGetValue(biomeData.m_sBiomeName, out BiomeSpawnEntityGroupList biomeList))
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Biome data not found for id {0}",
                        biomeId);
                    return -1;
                }

                // Get bit mask for enabled groups, we cache this to avoid doing it multiple times.
                int groupsEnabledFlags = 0;
                {
                    FastTags<TagGroup.Poi> fastTags = FastTags<TagGroup.Poi>.none;
                    List<PrefabInstance> spawnPIs = new List<PrefabInstance>();

                    var xMin = worldX + 16;
                    var xMax = worldX + 80 - 16;
                    var zMin = worldZ + 16;
                    var zMax = worldZ + 80 - 16;
                    world.GetPOIsAtXZ(xMin, xMax, zMin, zMax, spawnPIs);

                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Got {0} POIs for ({1}, {2}) ({3}, {4})",
                        spawnPIs.Count,
                        xMin,
                        xMax,
                        zMin,
                        zMax);

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

                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "ChunkFlags generated: {0}",
                        groupsEnabledFlags);
                }

                // Gather all possible groups.
                var entityClassesDay = new List<SEntityClassAndProb>();
                var entityClassesNight = new List<SEntityClassAndProb>();

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

                            var entityClassInfo = EntityClass.GetEntityClass(entry.entityClassId);
                            if (!entityClassInfo.entityFlags.HasFlag(EntityFlags.Zombie))
                            {
                                // NOTE: This is to have better compatibility with mods that mess around with NPC's.
                                Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                                    "Ignoring entity class {0}:{1}, entity is not a zombie",
                                    entityClassInfo.entityClassName,
                                    entry.entityClassId);
                                continue;
                            }

                            if (group.daytime == EDaytime.Day || group.daytime == EDaytime.Any)
                            {
                                entityClassesDay.Add(new SEntityClassAndProb
                                {
                                    entityClassId = entry.entityClassId,
                                    prob = entry.prob
                                });
                            }

                            if (group.daytime == EDaytime.Night || group.daytime == EDaytime.Any)
                            {
                                entityClassesNight.Add(new SEntityClassAndProb
                                {
                                    entityClassId = entry.entityClassId,
                                    prob = entry.prob
                                });
                            }
                        }
                    }
                }


                // De-duplicate the spawn lists with identical entity class ids, select highest probability.
                DeduplicateSpawnList(entityClassesDay);
                DeduplicateSpawnList(entityClassesNight);

                // Sort by probability.
                entityClassesDay.Sort((a, b) => b.prob.CompareTo(a.prob));
                entityClassesNight.Sort((a, b) => b.prob.CompareTo(a.prob));

                // Cache the spawn data for this chunk.
                _spawnDataDay[chunk.Key] = entityClassesDay;
                _spawnDataNight[chunk.Key] = entityClassesNight;

                Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                    "Spawn list for chunk {0} (Day: {1}, Night: {2}), Biome: {3}",
                    chunk.Key,
                    entityClassesDay.Count,
                    entityClassesNight.Count,
                    biomeData.m_sBiomeName);

                // Day.
                Logging.CondInfo(config.LoggingOpts.EntityClassSelection, "  Daytime:");
                foreach (var entry in entityClassesDay)
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "    Class: {0} ({1}), Probability: {2}",
                        GetEntityClassName(entry.entityClassId),
                        entry.entityClassId,
                        entry.prob);
                }

                // Night.
                Logging.CondInfo(config.LoggingOpts.EntityClassSelection, "  Nighttime:");
                foreach (var entry in entityClassesNight)
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "    Class: {0} ({1}), Probability: {2}",
                        GetEntityClassName(entry.entityClassId),
                        entry.entityClassId,
                        entry.prob);
                }

                spawnList = world.IsDaytime() ? entityClassesDay : entityClassesNight;
            }

            return PerformSelection(simulation, spawnList);
        }

        static private int PerformSelection(Simulation simulation, List<SEntityClassAndProb> spawnList)
        {
            var rand = simulation.PRNG;
            var config = simulation.Config;

            // Calculate the total probability.
            float totalProb = 0;
            for (int i = 0; i < spawnList.Count; i++)
            {
                var entry = spawnList[i];
                var prob = GetEntityClassProbability(entry.prob, entry.entityClassId);

                totalProb += prob;
            }

            // Select a random class id, it also attempts to avoid spawning duplicates.
            var selectedClassId = -1;
            var maxRetryAttempts = System.Math.Min(MaxSpawnRetryAttempts, spawnList.Count);
            for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
            {
                var randomValue = rand.NextSingle() * totalProb;

                for (int i = 0; i < spawnList.Count; i++)
                {
                    var entry = spawnList[i];
                    var prob = GetEntityClassProbability(entry.prob, entry.entityClassId);

                    randomValue -= prob;

                    if (randomValue <= 0)
                    {
                        selectedClassId = entry.entityClassId;
                        break;
                    }
                }

                var existingCount = GetSpawnedClassIdCount(selectedClassId);
                if (existingCount >= 1)
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Selected entity class {0} ({1}) already exists, instances: {2}, retrying...",
                        GetEntityClassName(selectedClassId),
                        selectedClassId,
                        existingCount);
                }
                else
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Selected entity class {0} ({1}) from {2} attempts",
                        GetEntityClassName(selectedClassId),
                        selectedClassId,
                        attempt + 1);
                    break;
                }
            }

            return selectedClassId;
        }

        static public int SpawnAgent(Simulation simulation, Simulation.SpawnData spawnData)
        {
            var agent = spawnData.Agent;
            var config = simulation.Config;

            var world = GameManager.Instance.World;
            if (world == null)
            {
                return -1;
            }

            if (!CanSpawnZombie(simulation))
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
                Logging.DbgErr("Failed to spawn agent {0}, chunk not loaded at {1}, {2}", agent.Index, chunkPosX, chunkPosZ);
                return -1;
            }

            var terrainHeight = world.GetTerrainHeight(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.z)) + 1;
            Logging.CondInfo(config.LoggingOpts.Spawns,
                "Terrain height at {0}, {1} is {2}",
                worldPos.x,
                worldPos.z,
                terrainHeight);

            // Adjust position height.
            worldPos.y = terrainHeight;

            if (!CanSpawnAtPosition(worldPos))
            {
                Logging.CondInfo(config.LoggingOpts.Spawns, "Failed to spawn agent {0}, position not suitable at {1}", agent.Index, worldPos);
                return -1;
            }

            Logging.CondInfo(config.LoggingOpts.Spawns, "Spawning agent {0} at {1}", agent.Index, worldPos);

            // Use previously assigned entity class id.
            int entityClassId = agent.EntityClassId;
            if (entityClassId == -1 || entityClassId == 0)
            {
                // Try first from mask.
                entityClassId = GetEntityClassIdFromMask(simulation, chunk, worldPos);
                if (entityClassId == -1)
                {
                    // Select from biome.
                    entityClassId = GetEntityClassId(simulation, chunk, worldPos);
                }
                else
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Selected entity class id {0} from mask for agent {1} at position {2}",
                        entityClassId,
                        agent.Index,
                        worldPos);
                }

                if (entityClassId == -1 || entityClassId == 0)
                {
                    if (entityClassId == -1)
                    {
                        Logging.CondErr(config.LoggingOpts.Spawns,
                            "Failed to get entity class id for chunk {0} at position {1}", chunk.Key, worldPos);
                    }

                    // 0 signals to not spawn, this is on purpose to change the weights, comes from entitygroups.xml.
                    return entityClassId;
                }
            }
            else
            {
                Logging.CondInfo(config.LoggingOpts.Spawns,
                    "Using previous entity class id: {0}",
                    entityClassId);
            }

            var rot = VectorUtils.ToUnity(agent.Velocity);
            rot.y = 0;
            rot.Normalize();

            var spawnedAgent = EntityFactory.CreateEntity(entityClassId, worldPos, rot) as EntityAlive;
            if (spawnedAgent == null)
            {
                Logging.CondErr(config.LoggingOpts.Spawns,
                    "Unable to create zombie entity!, Class Id: {0}, Pos: {1}",
                    entityClassId,
                    worldPos);
                return -1;
            }

            // Update attributes.
            spawnedAgent.bIsChunkObserver = true;
            spawnedAgent.SetSpawnerSource(EnumSpawnerSource.Biome);
            spawnedAgent.ticksNoPlayerAdjacent = 0;
            spawnedAgent.bMovementRunning = false;
            spawnedAgent.moveSpeed = 1.0f;

            // Spawn.
            world.SpawnEntityInWorld(spawnedAgent);

            // Because some Mods use the entitygroups.xml to do normal NPCs, we have to check this first.
            if (spawnedAgent is EntityZombie spawnedZombie)
            {
                spawnedZombie.IsHordeZombie = true;
            }

            if (agent.TimeToDie != ulong.MaxValue)
            {
                if (spawnedAgent is EntityHuman spawnedHuman)
                {
                    spawnedHuman.timeToDie = agent.TimeToDie;
                }
            }

            if (agent.Health != -1)
            {
                Logging.CondInfo(config.LoggingOpts.Spawns,
                    "Using previous health: {0}",
                    agent.Health);
                spawnedAgent.Health = agent.Health;
            }

            // Post spawn behavior.
            var isAlerted = spawnData.SubState == Agent.SubState.Alerted;
            if (spawnData.PostSpawnBehavior == Config.PostSpawnBehavior.Wander || isAlerted)
            {
                var destPos = isAlerted ? VectorUtils.ToUnity(spawnData.AlertPosition) : worldPos + (rot * 80);

                // Adjust position by getting terrain height at the destination, they might dig if the destination is
                // below the terrain.
                var targetTerrainHeight = world.GetTerrainHeight(Mathf.FloorToInt(destPos.x), Mathf.FloorToInt(destPos.z));
                var combinedTargetHeight = targetTerrainHeight + 1.5f;

                destPos.y = combinedTargetHeight;

                spawnedAgent.SetInvestigatePosition(destPos, 6000, isAlerted);
                Logging.CondInfo(config.LoggingOpts.Spawns,
                    "Spawned agent {0}, entity id: {1} wandering to {2}, alert: {3}",
                    agent.Index,
                    spawnedAgent.entityId,
                    destPos,
                    isAlerted
                    );
            }
            else if (spawnData.PostSpawnBehavior == Config.PostSpawnBehavior.ChaseActivator)
            {
                var activator = world.GetEntity(spawnData.ActivatorEntityId) as EntityAlive;
                if (activator != null)
                {
                    spawnedAgent.SetAttackTarget(activator, 6000);
                    Logging.CondInfo(config.LoggingOpts.Spawns,
                        "Spawned agent {0}, entity id: {1} chasing activator {2}",
                        agent.Index,
                        spawnedAgent.entityId,
                        spawnData.ActivatorEntityId);
                }
                else
                {
                    Logging.Warn("This should never happen, zombie spawned with no activator.");
                }

            }
            else if (spawnData.PostSpawnBehavior == Config.PostSpawnBehavior.Nothing)
            {
                // Do nothing.
                Logging.CondInfo(config.LoggingOpts.Spawns,
                    "No PostSpawn action for agent {0}, entity id {1}",
                    agent.Index,
                    spawnedAgent.entityId);
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

            Logging.CondInfo(config.LoggingOpts.Spawns,
                "Agent {0} spawned at {1}, entity id {2}, class id {3}",
                agent.Index,
                worldPos,
                spawnedAgent.entityId,
                entityClassId);

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

            if (agent.CurrentState != Agent.State.Active)
            {
                Logging.DbgInfo("Trying to despawn agent {0} that is not active, state: {1}.", agent.Index, agent.CurrentState);
                return false;
            }

            var entity = world.GetEntity(agent.EntityId) as EntityAlive;
            if (entity == null)
            {
                Logging.Warn("Entity not found {0} for agent {1}, unable to despawn.", agent.EntityId, agent.Index);
                return false;
            }

            DecrementSpawnedClassIdCount(entity.entityClass, entity.entityId);

            // Retain current state.
            agent.Health = entity.Health;

            var humanEntity = entity as EntityHuman;
            if (humanEntity != null)
            {
                agent.TimeToDie = humanEntity.timeToDie;
            }
            else
            {
                agent.TimeToDie = ulong.MaxValue;
            }

            agent.Velocity = VectorUtils.ToSim(entity.moveDirection);
            agent.Velocity.Validate();

            agent.Position = VectorUtils.ToSim(entity.position);
            agent.Position.Validate();

            world.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Despawned);

            var config = simulation.Config;
            Logging.CondInfo(config.LoggingOpts.Despawns, "Agent despawned, entity id: {0}", agent.EntityId);

            return true;
        }

        private static bool IsEntityDead(Entity entity, int entityId)
        {
            if (entity == null)
            {
                Logging.DbgInfo("Entity not found: {0}, can't update state.", entityId);
                return true;
            }

            if (!entity.IsAlive())
            {
                Logging.DbgInfo("Entity is dead: {0}", entityId);
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
                    Logging.DbgInfo("Agent {0} has no entity id, skipping. Agent state: {1}", agent.Index, agent.CurrentState);
                    continue;
                }

                if (agent.CurrentState != Agent.State.Active)
                {
                    Logging.DbgInfo("Agent {0} is not active, skipping. Agent state: {1}", agent.Index, agent.CurrentState);
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
