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
            Logging.CondInfo(logEvents, () => $"Noise: {clipName}, " +
                $" Volume: {noise.volume}, " +
                $"Duration: {noise.duration}, " +
                $"MuffledWhenCrouched: {noise.muffledWhenCrouched}, " +
                $"HeatMapStrength: {noise.heatMapStrength}, " +
                $"HeatMapWorldTimeToLive: {noise.heatMapWorldTimeToLive}, " +
                $"volumeScale: {volumeScale}, " +
                $"Travel Distance: {distance}, " +
                $"Scaled Travel Distance: {distanceScaled}");

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

            Logging.DbgInfo("Enclosed: {0}, IndoorBase: {1}, EnclosureMod: {2}, FinalScale: {3}",
                amountEnclosed, indoorBaseScale, enclosureMod, finalScale);

            Logging.DbgInfo("Adjusted Sound Distance Scale: {0} to {1}", distancePreScaled, distanceScaled);

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
                    // Beyond the maximum distance the sound can be heard.
                    continue;
                }

                // Closer enemies pinpoint the sound more accurately; farther ones get a wider spread.
                var distRatio = entDist / distance; // 0.0 at source, 1.0 at max hearing range
                var spread = Math.Min(distance * distRatio * 0.5f, 20f);
                var randomOffset = UnityEngine.Random.insideUnitSphere * spread;
                randomOffset.y = 0f;
                var investigatePos = position + randomOffset;

                // Get the ground position at the investigate position.
                var actualHeight = world.GetHeightAt(investigatePos.x, investigatePos.z);

                investigatePos.y = actualHeight + 1.5f;

                Logging.CondInfo(logEvents, () => $"Alerting enemy {ent.entityId} at {entPos} to noise at {position}, distance: {entDist}.");

                // Have them interested for for 2~ min, assuming 30 ticks a second.
                ent.SetInvestigatePosition(investigatePos, 3600, true);
            }
        }
    }
}
