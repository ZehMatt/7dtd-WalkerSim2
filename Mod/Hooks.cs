using HarmonyLib;
using UnityEngine;

namespace WalkerSim
{
    [HarmonyPatch(typeof(AIDirectorWanderingHordeComponent), nameof(AIDirectorWanderingHordeComponent.StartSpawning))]
    class HordeSpawnHook
    {
        static bool Prefix(AIDirectorWanderingHordeComponent __instance, AIWanderingHordeSpawner.SpawnType _spawnType)
        {
#if false
            Logging.DbgInfo("Preventing wandering horde spawn.");
#endif
            if (_spawnType == AIWanderingHordeSpawner.SpawnType.Bandits)
            {
                // Allow bandit spawns, not used in vanilla, but some mods might have custom spawners.
                return true;
            }

            // Prevent it from running each frame.
            __instance.SetNextTime(_spawnType, ulong.MaxValue);

            // Prevent hordes from spawning.
            return false;
        }
    }

    [HarmonyPatch(typeof(SpawnManagerBiomes), nameof(SpawnManagerBiomes.SpawnUpdate))]
    class BiomeSpawnerHook
    {
        static void Prefix(string _spawnerName, ref bool _isSpawnEnemy, ChunkAreaBiomeSpawnData _spawnData)
        {
            if (!_isSpawnEnemy)
                return;

            // Logging.Out("Preventing biome spawn.");
            _isSpawnEnemy = false;
        }
    }

    [HarmonyPatch(typeof(AIDirector), nameof(AIDirector.NotifyNoise))]
    class NotifyNoiseHook
    {
        static void Prefix(Entity instigator, UnityEngine.Vector3 position, string clipName, float volumeScale)
        {
            Sound.NotifyNoise(instigator, position, clipName, volumeScale);
        }
    }

    [HarmonyPatch(typeof(XUiC_MapArea), nameof(XUiC_MapArea.updateMapSection))]
    class MapAreaDrawHook
    {
        // public void updateMapSection(int mapStartX, int mapStartZ, int mapEndX, int mapEndZ, int drawnMapStartX, int drawnMapStartZ, int drawnMapEndX, int drawnMapEndZ)
        static void Postfix(XUiC_MapArea __instance, int mapStartX, int mapStartZ, int mapEndX, int mapEndZ, int drawnMapStartX, int drawnMapStartZ, int drawnMapEndX, int drawnMapEndZ)
        {
            MapDrawing.DrawMapSection(__instance, mapStartX, mapStartZ, mapEndX, mapEndZ, drawnMapStartX, drawnMapStartZ, drawnMapEndX, drawnMapEndZ);
        }
    }

    [HarmonyPatch(typeof(XUiC_MapArea), nameof(XUiC_MapArea.OnClose))]
    class MapAreaCloseHook
    {
        // public void updateMapSection(int mapStartX, int mapStartZ, int mapEndX, int mapEndZ, int drawnMapStartX, int drawnMapStartZ, int drawnMapEndX, int drawnMapEndZ)
        static void Postfix(XUiC_MapArea __instance)
        {
            MapDrawing.OnClose(__instance);
        }
    }

    [HarmonyPatch(typeof(EntityHuman), nameof(EntityHuman.GetMoveSpeed))]
    class EntityHumanGetMoveSpeedHook
    {
        static void Postfix(EntityHuman __instance, ref float __result)
        {
            WalkerSimMod.GetZombieWanderingSpeed(__instance, ref __result);
        }
    }

    [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.GetMoveSpeed))]
    class EntityAliveGetMoveSpeedHook
    {
        static void Postfix(EntityHuman __instance, ref float __result)
        {
            WalkerSimMod.GetZombieWanderingSpeed(__instance, ref __result);
        }
    }

    [HarmonyPatch(typeof(EntityHuman), nameof(EntityHuman.GetMoveSpeedAggro))]
    class EntityHumanGetMoveSpeedAggroHook
    {
        static void Postfix(EntityHuman __instance, ref float __result)
        {
            WalkerSimMod.GetZombieWanderingSpeed(__instance, ref __result);
        }
    }

    [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.GetMoveSpeedAggro))]
    class EntityAliveGetMoveSpeedAggroHook
    {
        static void Postfix(EntityHuman __instance, ref float __result)
        {
            WalkerSimMod.GetZombieWanderingSpeed(__instance, ref __result);
        }
    }

    [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.GetMoveSpeedPanic))]
    class EntityAliveGetMoveSpeedPanic
    {
        static void Postfix(EntityHuman __instance, ref float __result)
        {
            WalkerSimMod.GetZombieWanderingSpeed(__instance, ref __result);
        }
    }

    [HarmonyPatch(typeof(EntityPlayer), nameof(EntityPlayer.DetectUsScale))]
    class EntityPlayerDetectUsScaleHook
    {
        static bool Prefix(EntityPlayer __instance, ref float __result, EntityAlive _entity)
        {
            if (__instance.prefab != null &&
                __instance.prefab.prefab.DifficultyTier >= 1 &&
                Time.time - __instance.prefabTimeIn > 60f &&
                /* _entity.GetSpawnerSource() == EnumSpawnerSource.Biome && */ // This is stupid, we want smell to work for all zombies.
                _entity is EntityEnemy)
            {
                __result = 0.3f;
                return false;
            }
            __result = 1f;
            return false;
        }
    }


    static class Hooks
    {
        public static void Init()
        {
            var harmony = new Harmony("WalkerSim.Hooks");
            harmony.PatchAll();
        }
    }
}
