using HarmonyLib;

namespace WalkerSim
{
    [HarmonyPatch(typeof(AIDirectorWanderingHordeComponent))]
    [HarmonyPatch("StartSpawning")]
    class HordeSpawnHook
    {
        static bool Prefix(AIDirectorWanderingHordeComponent __instance, AIWanderingHordeSpawner.SpawnType _spawnType)
        {
#if false
            Logging.DbgInfo("Preventing wandering horde spawn.");
#endif

            // Prevent it from running each frame.
            __instance.SetNextTime(_spawnType, ulong.MaxValue);

            // Prevent hordes from spawning.
            return false;
        }
    }

    [HarmonyPatch(typeof(SpawnManagerBiomes))]
    [HarmonyPatch("SpawnUpdate")]
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

    [HarmonyPatch(typeof(AIDirector))]
    [HarmonyPatch("NotifyNoise")]
    class NotifyNoiseHook
    {
        static void Prefix(Entity instigator, UnityEngine.Vector3 position, string clipName, float volumeScale)
        {
            WalkerSimMod.NotifyNoise(instigator, position, clipName, volumeScale);
        }
    }

    [HarmonyPatch(typeof(XUiC_MapArea))]
    [HarmonyPatch("updateMapSection")]
    class MapAreaDrawHook
    {
        // public void updateMapSection(int mapStartX, int mapStartZ, int mapEndX, int mapEndZ, int drawnMapStartX, int drawnMapStartZ, int drawnMapEndX, int drawnMapEndZ)
        static void Postfix(XUiC_MapArea __instance, int mapStartX, int mapStartZ, int mapEndX, int mapEndZ, int drawnMapStartX, int drawnMapStartZ, int drawnMapEndX, int drawnMapEndZ)
        {
            MapDrawing.DrawMapSection(__instance, mapStartX, mapStartZ, mapEndX, mapEndZ, drawnMapStartX, drawnMapStartZ, drawnMapEndX, drawnMapEndZ);
        }
    }

    [HarmonyPatch(typeof(XUiC_MapArea))]
    [HarmonyPatch("OnClose")]
    class MapAreaCloseHook
    {
        // public void updateMapSection(int mapStartX, int mapStartZ, int mapEndX, int mapEndZ, int drawnMapStartX, int drawnMapStartZ, int drawnMapEndX, int drawnMapEndZ)
        static void Postfix(XUiC_MapArea __instance)
        {
            MapDrawing.OnClose(__instance);
        }
    }

    [HarmonyPatch(typeof(EntityHuman))]
    [HarmonyPatch("GetMoveSpeed")]
    class EntityHumanGetMoveSpeedHook
    {
        static void Postfix(EntityHuman __instance, ref float __result)
        {
            WalkerSimMod.GetZombieWanderingSpeed(__instance, ref __result);
        }
    }

    [HarmonyPatch(typeof(EntityHuman))]
    [HarmonyPatch("GetMoveSpeedAggro")]
    class EntityHumanGetMoveSpeedAggroHook
    {
        static void Postfix(EntityHuman __instance, ref float __result)
        {
            WalkerSimMod.GetZombieWanderingSpeed(__instance, ref __result);
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
