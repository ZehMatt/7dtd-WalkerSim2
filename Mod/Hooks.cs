using HarmonyLib;

namespace WalkerSim
{
    [HarmonyPatch(typeof(AIDirectorWanderingHordeComponent))]
    [HarmonyPatch("StartSpawning")]
    class HordeSpawnHook
    {
        static bool Prefix(AIDirectorWanderingHordeComponent __instance, AIWanderingHordeSpawner.SpawnType _spawnType)
        {
            Logging.Out("Preventing wandering horde spawn.");

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

    static class Hooks
    {
        public static void Init()
        {
            var harmony = new Harmony("WalkerSim.Hooks");
            harmony.PatchAll();
        }
    }
}
