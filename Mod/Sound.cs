using System;

namespace WalkerSim
{
    internal static class Sound
    {
        // Equivalent to EntityAlive.GetAmountEnclosed but free-standing.
        private static float GetAmountEnclosed(UnityEngine.Vector3 position)
        {
            var world = GameManager.Instance.World;

            position.y += 0.5f;
            Vector3i vector3i = World.worldToBlockPos(position);
            if (vector3i.y < 0xFF)
            {
                IChunk chunkFromWorldPos = world.GetChunkFromWorldPos(vector3i);
                if (chunkFromWorldPos != null)
                {
                    float num = (float)chunkFromWorldPos.GetLight(vector3i.x, vector3i.y, vector3i.z, Chunk.LIGHT_TYPE.SUN);
                    float num2 = (float)chunkFromWorldPos.GetLight(vector3i.x, vector3i.y + 1, vector3i.z, Chunk.LIGHT_TYPE.SUN);
                    float num3 = Math.Max(num, num2) / 15f;
                    return 1f - num3;
                }
            }

            return 1f;
        }

        internal static void NotifyNoise(Entity instigator, UnityEngine.Vector3 position, string clipName, float volumeScale)
        {
            if (!Game.IsHost())
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

            var distance = (noise.volume * volumeScale * 3.0f) * config.SoundDistanceScale;
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

            if (instigator != null)
            {
                if (instigator.IsIgnoredByAI())
                    return;
            }

            // Any values higher than 0 is considered indoors, returns [0.0, 1.0]
            var amountEnclosed = GetAmountEnclosed(position);

            var isIndoors = amountEnclosed > 0.0f;
            var indoorBaseScale = isIndoors ? 0.3f : 1.0f; // 70% reduction minimum indoors

            var enclosureMod = isIndoors
                ? MathEx.Lerp(1.0f, 0.7f, amountEnclosed)
                : 1.0f;

            var finalScale = indoorBaseScale * enclosureMod;

            var distancePreScaled = distanceScaled;
            distanceScaled *= finalScale;
            eventDuration = (ulong)(eventDuration * finalScale);

            Logging.Info("Enclosed: {0}, IndoorBase: {1}, EnclosureMod: {2}, FinalScale: {3}",
                amountEnclosed, indoorBaseScale, enclosureMod, finalScale);

            Logging.Info("Adjusted Sound Distance Scale: {0} to {1}", distancePreScaled, distanceScaled);

            simulation.AddSoundEvent(VectorUtils.ToSim(position), distanceScaled, eventDuration);

            if (simulation.Config.EnhancedSoundAwareness)
            {
                NotifyNearbyEnemies(simulation, instigator, position, distanceScaled);
            }
        }

        internal static void NotifyNearbyEnemies(Simulation simulation, Entity instigator, UnityEngine.Vector3 position, float distance)
        {
            if (!Game.IsHost())
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
    }
}
