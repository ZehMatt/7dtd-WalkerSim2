using System.Collections.Generic;
using UnityEngine;

namespace WalkerSim
{
    internal class SpawnManager
    {

        static Dictionary<long, List<List<SEntityClassAndProb>>> _spawnDataNight = new Dictionary<long, List<List<SEntityClassAndProb>>>();
        static Dictionary<long, List<List<SEntityClassAndProb>>> _spawnDataDay = new Dictionary<long, List<List<SEntityClassAndProb>>>();
        static List<SEntityClassAndProb> _spawnGeneric;

        static Dictionary<int, int> _classIdCounter = new Dictionary<int, int>();
        const int MaxSpawnRetryAttempts = 20;

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

        static private bool CanSpawnAtPosition(UnityEngine.Vector3 position, bool rainMode)
        {
            var world = GameManager.Instance.World;
            if (world == null)
            {
                return false;
            }

            if (!rainMode)
            {
                if (!world.CanMobsSpawnAtPos(position))
                {
                    Logging.DbgInfo("CanMobsSpawnAtPos returned false for position {0}", position);
                    return false;
                }
            }

            if (world.isPositionInRangeOfBedrolls(position))
            {
                Logging.DbgInfo("Position {0} is near a bedroll", position);
                return false;
            }

            return true;
        }

        static List<List<SEntityClassAndProb>> GetBiomeEntityClasses(long chunkKey)
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

        static private int PerformSelectionSubGroup(Simulation simulation, List<SEntityClassAndProb> spawnList, int maxRetries, bool allowDuplicates = false)
        {
            var rand = simulation.PRNG;
            var config = simulation.Config;

            var selectedClassId = 0;
            var maxRetryAttempts = System.Math.Min(maxRetries, spawnList.Count);

            // Calculate the total probability.
            float maxTotalProb = 0;
            for (int i = 0; i < spawnList.Count; i++)
            {
                var entry = spawnList[i];
                var prob = GetEntityClassProbability(entry.prob, entry.entityClassId);

                maxTotalProb += prob;
            }

            // Attempt to pick a non-duplicate class.
            for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
            {
                var totalProb = maxTotalProb;

                // Select a random class id, it also attempts to avoid spawning duplicates.
                var randomValue = rand.NextSingle() * maxTotalProb;

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

                if (selectedClassId == 0)
                {
                    // Some groups have "none" with high probability, retry if we hit that.
                    continue;
                    //return 0;
                }

                // If we already have the same class id, retry selection.
                var existingCount = GetSpawnedClassIdCount(selectedClassId);
                if (existingCount > 0)
                {
                    if (!allowDuplicates)
                    {
                        // Try again, this is a duplicate and we are not allowing duplicates in this attempt.
                        selectedClassId = -1;
                        continue;
                    }

                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Selected entity class {0} ({1}) already exists, instances: {2}, retrying...",
                        GetEntityClassName(selectedClassId),
                        selectedClassId,
                        existingCount);

                    continue;
                }
                else
                {
                    // Found something.

                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Selected entity class {0} ({1}) from {2} attempts",
                        GetEntityClassName(selectedClassId),
                        selectedClassId,
                        attempt + 1);

                    return selectedClassId;
                }
            }

            if (selectedClassId == -1)
            {
                Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                    "Failed to select a non-duplicate entity class after {0} attempts.",
                    maxRetryAttempts,
                    allowDuplicates);
            }

            return selectedClassId;
        }

        static private int PerformSelection(Simulation simulation, List<List<SEntityClassAndProb>> biomeList)
        {
            var rand = simulation.PRNG;
            var config = simulation.Config;

            if (biomeList.Count == 0)
            {
                Logging.CondWrn(config.LoggingOpts.EntityClassSelection, "Biome list is empty, no entity classes to select from.");
                return -1;
            }

            // Randomize the order of the spawn groups to avoid always selecting from the same group first.
            rand.ShuffleList(biomeList);

            var selectedClassId = 0;

            // First attempt, no duplicates allowed.
            for (var subIndex = 0; subIndex < biomeList.Count; subIndex++)
            {
                var spawnList = biomeList[subIndex];

                selectedClassId = PerformSelectionSubGroup(simulation, spawnList, MaxSpawnRetryAttempts, false);
                if (selectedClassId == 0)
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Selected 'none' entity class from group {0}, trying next group if available. Groups in biome list: {1}",
                        subIndex, biomeList.Count);

                    continue;
                }
                else if (selectedClassId == -1)
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Failed to select an entity class from group {0} with no duplicates, trying next group if available. Groups in biome list: {1}",
                        subIndex, biomeList.Count);

                    continue;
                }

                // Found a valid class id.
                break;
            }

            // Second attempt, allow duplicates.
            if (selectedClassId == -1)
            {
                Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                    "Failed to select an entity class with no duplicates, retrying with duplicates allowed. Groups in biome list: {0}",
                    biomeList.Count);

                // Reduce the amount of retry attempts, we already tried before with MaxSpawnRetryAttempts so we can be more lenient now.
                var maxRetries = System.Math.Max(MaxSpawnRetryAttempts / biomeList.Count, 5);

                for (var subIndex = 0; subIndex < biomeList.Count; subIndex++)
                {
                    var spawnList = biomeList[subIndex];

                    selectedClassId = PerformSelectionSubGroup(simulation, spawnList, maxRetries, true);
                    if (selectedClassId != 0 && selectedClassId != -1)
                    {
                        return selectedClassId;
                    }
                }
            }

            if (selectedClassId == 0)
            {
                if (_spawnGeneric.Count > 0)
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "Using fallback to generic ZombiesAll group for 'none' selection. Groups in biome list: {0}",
                        biomeList.Count);

                    return PerformSelectionSubGroup(simulation, _spawnGeneric, MaxSpawnRetryAttempts);
                }

                Logging.Err("Selected 'none' entity class, 'ZombiesAll' doesn't exist or is empty, no fallback possible.");
                return -1;
            }
            else if (selectedClassId == -1)
            {
                // We should never end up here.
                Logging.Err("Failed to select an entity class {0}, no valid classes found in biome list. Groups in biome list: {1}",
                    selectedClassId, biomeList.Count);
            }

            return selectedClassId;
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

            return PerformSelectionSubGroup(simulation, entityGroupData, MaxSpawnRetryAttempts);
        }

        static private bool IsEntityClassAllowed(int entityClassId)
        {
            var entityClassInfo = EntityClass.GetEntityClass(entityClassId);
            if (entityClassInfo.entityFlags.HasFlag(EntityFlags.Zombie))
            {
                // Zombies are always allowed.
                return true;
            }

            if (entityClassInfo.entityFlags.HasFlag(EntityFlags.Animal) && entityClassInfo.bIsEnemyEntity)
            {
                // Hostile animals are allowed.
                return true;
            }

            return false;
        }

        static private int GetEntityClassId(Simulation simulation, Chunk chunk, UnityEngine.Vector3 worldPos)
        {
            var world = GameManager.Instance.World;
            if (world == null)
            {
                return -1;
            }

            var config = simulation.Config;

            if (_spawnGeneric == null)
            {
                if (EntityGroups.list.TryGetValue("ZombiesAll", out var zombiesAll))
                {
                    // Remove 0 "none" entries if any.
                    _spawnGeneric = new List<SEntityClassAndProb>(zombiesAll);
                    _spawnGeneric.RemoveAll(entry => entry.entityClassId == 0);
                }
                else
                {
                    // Empty list.
                    _spawnGeneric = new List<SEntityClassAndProb>();
                }
            }

            List<List<SEntityClassAndProb>> spawnList = GetBiomeEntityClasses(chunk.Key);
            if (spawnList == null)
            {
                Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                    "No cached entity classes for chunk: {0}, building cache...",
                    chunk.Key);

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
                        "  Biome data is null for id {0}",
                        biomeId);
                    return -1;
                }

                if (!BiomeSpawningClass.list.TryGetValue(biomeData.m_sBiomeName, out BiomeSpawnEntityGroupList biomeList))
                {
                    Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                        "  Biome data not found for id {0}",
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
                        "  Got {0} POIs for ({1}, {2}) ({3}, {4})",
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
                        "  ChunkFlags generated: {0}",
                        groupsEnabledFlags);
                }

                // Extract possible groups from the bit mask.
                List<List<SEntityClassAndProb>> spawnDataNight = new List<List<SEntityClassAndProb>>();
                List<List<SEntityClassAndProb>> spawnDataDay = new List<List<SEntityClassAndProb>>();

                for (int i = 0; i < biomeList.list.Count; i++)
                {
                    var group = biomeList.list[i];

                    // Gather all possible groups.
                    var entityClassesDay = new List<SEntityClassAndProb>();
                    var entityClassesNight = new List<SEntityClassAndProb>();

                    if ((groupsEnabledFlags & (1 << i)) == 0)
                    {
                        // This group is not enabled for this chunk.
                        continue;
                    }

                    if (EntityGroups.list.TryGetValue(group.entityGroupName, out var data))
                    {
                        foreach (var entry in data)
                        {
                            if (entry.entityClassId != 0 && !IsEntityClassAllowed(entry.entityClassId))
                            {
                                Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                                    "  Ignoring entity class {0}:{1}, entity not handled.",
                                    GetEntityClassName(entry.entityClassId),
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

                    // De-duplicate the spawn lists with identical entity class ids, select highest probability.
                    DeduplicateSpawnList(entityClassesDay);
                    DeduplicateSpawnList(entityClassesNight);

                    // Sort by probability.
                    entityClassesDay.Sort((a, b) => b.prob.CompareTo(a.prob));
                    entityClassesNight.Sort((a, b) => b.prob.CompareTo(a.prob));

                    spawnDataDay.Add(entityClassesDay);
                    spawnDataNight.Add(entityClassesNight);
                }

                Logging.CondInfo(config.LoggingOpts.EntityClassSelection,
                    "  Spawn groups for chunk {0} (Day: {1}, Night: {2}), Biome: {3}",
                    chunk.Key,
                    spawnDataNight.Count,
                    spawnDataDay.Count,
                    biomeData.m_sBiomeName);

                // Remove empty groups to avoid unnecessary selection attempts.
                spawnDataNight.RemoveAll(group => group.Count == 0);
                spawnDataDay.RemoveAll(group => group.Count == 0);

                _spawnDataNight.Add(chunk.Key, spawnDataNight);
                _spawnDataDay.Add(chunk.Key, spawnDataDay);

                // Log out all sub groups.
                if (config.LoggingOpts.EntityClassSelection)
                {
                    Logging.Info("  Spawn groups Day:", chunk.Key);
                    foreach (var group in spawnDataDay)
                    {
                        Logging.Info("  >");
                        foreach (var entry in group)
                        {
                            Logging.Info("    Entity Class {0}:{1}, Probability: {2}",
                                GetEntityClassName(entry.entityClassId), entry.entityClassId, entry.prob);
                        }
                    }

                    Logging.Info("  Spawn groups Night:", chunk.Key);
                    foreach (var group in spawnDataNight)
                    {
                        Logging.Info("  >");
                        foreach (var entry in group)
                        {
                            Logging.Info("    Entity Class {0}:{1}, Probability: {2}",
                                GetEntityClassName(entry.entityClassId), entry.entityClassId, entry.prob);
                        }
                    }
                }

                spawnList = world.IsDaytime() ? spawnDataDay : spawnDataNight;
            }

            int classId = PerformSelection(simulation, spawnList);
            if (classId == 0 || classId == -1)
            {
                // Selection failed.
                return -1;
            }

            return classId;
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

            var blockPos = World.worldToBlockPos(worldPos);
            var terrainOffset = world.GetTerrainOffset(0, blockPos);

            worldPos.y += terrainOffset;

            if (!CanSpawnAtPosition(worldPos, spawnData.ZombieRain))
            {
                Logging.CondInfo(config.LoggingOpts.Spawns, "Failed to spawn agent {0}, position not suitable at {1}", agent.Index, worldPos);
                return -1;
            }

            // Prevent them from spawning inside blocks.
            worldPos.y += 1;

            // Easter egg.
            if (spawnData.ZombieRain)
            {
                worldPos.y = 150;
            }

            Logging.CondInfo(config.LoggingOpts.Spawns, "Spawning agent {0} at {1}", agent.Index, worldPos);

            // Use previously assigned entity class id.
            int entityClassId = agent.EntityClassId;

            var remainingLifeTime = agent.TimeToDie > world.worldTime ? agent.TimeToDie - world.worldTime : 0;
            if (remainingLifeTime <= 500 /* 30 in-game minutes */)
            {
                // Don't reuse if they are close to death or past the lifetime.
                entityClassId = -1;

                // Reset the previous state.
                agent.TimeToDie = ulong.MaxValue;
                agent.Health = -1;
            }

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

            // Keep maximum assigned lifetime.
            if (agent.TimeToDie != ulong.MaxValue)
            {
                if (spawnedAgent is EntityHuman spawnedHuman)
                {
                    spawnedHuman.timeToDie = agent.TimeToDie;
                }
                else if (spawnedAgent is EntityZombieDog spawnedDog)
                {
                    spawnedDog.timeToDie = agent.TimeToDie;
                }

                Logging.CondInfo(config.LoggingOpts.Spawns,
                    "Using previous time to die: {0}, remaining: {1}",
                    agent.TimeToDie,
                    remainingLifeTime);
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

            if (spawnData.ZombieRain)
            {
                spawnedAgent.emodel.DoRagdoll(5.0f, EnumBodyPartHit.None, UnityEngine.Vector3.zero, UnityEngine.Vector3.zero, false);
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

            if (entity is EntityHuman humanEntity)
            {
                agent.TimeToDie = humanEntity.timeToDie;
            }
            else if (entity is EntityZombieDog dogEntity)
            {
                agent.TimeToDie = dogEntity.timeToDie;
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
