using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace WalkerSim
{
    internal class MapDrawing
    {
        public static bool IsEnabled { get; set; } = false;
        public static bool IsTemporarilyEnabled { get; set; } = false;

        private static readonly Color32 ColorActive = new Color32(0, 255, 0, 255);
        private static readonly Color32 ColorInactive = new Color32(255, 0, 0, 255);
        private static readonly Color32 ColorEvent = new Color32(200, 128, 128, 128);
        private static readonly Color32 ColorOutterActivation = new Color32(0, 0, 255, 128);
        private static readonly Color32 ColorInnerActivation = new Color32(255, 255, 0, 128);
        private static readonly Color32 ColorActivationZone = new Color32(0, 240, 0, 50);

        internal static void OnClose(XUiC_MapArea inst)
        {
            IsTemporarilyEnabled = false;
        }

        internal static void DrawMapSection(XUiC_MapArea inst,
            int mapStartX,
            int mapStartZ,
            int mapEndX,
            int mapEndZ,
            int textureStartX,
            int textureStartZ,
            int textureEndX,
            int textureEndZ)
        {
            if (!IsEnabled && !IsTemporarilyEnabled)
            {
                return;
            }

            NativeArray<Color32> textureData = inst.mapTexture.GetRawTextureData<Color32>();
            var simulation = Simulation.Instance;

            int textureWidth = 2048; // vanilla 7DTD map texture size

            foreach (var agent in simulation.Agents)
            {
                var worldPos = VectorUtils.ToUnity(agent.Position);

                if (worldPos.x < mapStartX || worldPos.x >= mapEndX ||
                    worldPos.z < mapStartZ || worldPos.z >= mapEndZ)
                {
                    continue;
                }

                var texX = global::Utils.WrapIndex(textureStartX + (int)(worldPos.x - mapStartX), textureWidth);
                var texZ = global::Utils.WrapIndex(textureStartZ + (int)(worldPos.z - mapStartZ), textureWidth);

                // Compute index into texture
                var pixelIndex = texZ * textureWidth + texX;

                var color = agent.CurrentState == Agent.State.Active ? ColorActive : ColorInactive;

                // Overdraw pixel with chosen color (bright red marker here)
                textureData[pixelIndex] = color;

                DrawMarker(textureData, texX, texZ, textureWidth, color);
            }

            var config = simulation.Config;

            foreach (var worldEvent in simulation.Events)
            {
                var eventWorldPos = VectorUtils.ToUnity(worldEvent.Position);

                var baseRadius = worldEvent.Radius;
                var t = (simulation.Ticks % 50) / 50f;
                var eventTexX = global::Utils.WrapIndex(textureStartX + (int)(eventWorldPos.x - mapStartX), textureWidth);
                var eventTexZ = global::Utils.WrapIndex(textureStartZ + (int)(eventWorldPos.z - mapStartZ), textureWidth);

                var ringCount = 4;
                var ringSpacing = 24f;
                var maxOffset = (ringCount - 1) * ringSpacing;
                var total = baseRadius + maxOffset;
                for (int r = 0; r < ringCount; r++)
                {
                    float offset = r * ringSpacing;
                    float animatedRadius = total * t - offset;
                    if (animatedRadius > baseRadius || animatedRadius < 0)
                    {
                        continue;
                    }

                    DrawCircle(textureData, eventTexX, eventTexZ, (int)animatedRadius, textureWidth, ColorEvent);
                }
            }

            // TODO: Expose this from the simulation.
            var activationBorderSize = 8.0f;

            foreach (var ply in simulation.Players)
            {
                var plyWorldPos = VectorUtils.ToUnity(ply.Value.Position);

                var playerTexX = global::Utils.WrapIndex(textureStartX + (int)(plyWorldPos.x - mapStartX), textureWidth);
                var playerTexZ = global::Utils.WrapIndex(textureStartZ + (int)(plyWorldPos.z - mapStartZ), textureWidth);

                // This is some inefficient garbage but it works.
                for (int i = 0; i < activationBorderSize; i++)
                {
                    DrawCircle(textureData, playerTexX, playerTexZ, (int)(config.SpawnActivationRadius - i), textureWidth, ColorActivationZone);
                }

                DrawCircle(textureData, playerTexX, playerTexZ, (int)config.SpawnActivationRadius, textureWidth, ColorOutterActivation);
                DrawCircle(textureData, playerTexX, playerTexZ, (int)(config.SpawnActivationRadius - activationBorderSize), textureWidth, ColorInnerActivation);
            }

            inst.timeToRedrawMap = 0.3f;
        }

        // Somewhat sub-optimal but since we replace the pixel we have to do alpha by hand.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DrawPixel(NativeArray<Color32> textureData, int pixelIndex, Color32 color)
        {
            var oldPixel = textureData[pixelIndex];

            // Normalize alpha to 0-1 range
            float srcAlpha = color.a / 255f;
            float dstAlpha = 1f - srcAlpha;

            // Blend each channel
            byte r = (byte)(color.r * srcAlpha + oldPixel.r * dstAlpha);
            byte g = (byte)(color.g * srcAlpha + oldPixel.g * dstAlpha);
            byte b = (byte)(color.b * srcAlpha + oldPixel.b * dstAlpha);

            // Compute resulting alpha (optional: keep it 255 if texture is opaque)
            byte a = (byte)(color.a + oldPixel.a * dstAlpha);

            textureData[pixelIndex] = new Color32(r, g, b, a);
        }

        private static void DrawMarker(
            NativeArray<Color32> textureData,
            int centerX,
            int centerZ,
            int textureWidth,
            Color32 color)
        {
            void SetPixel(int x, int z)
            {
                if (x < 0 || x >= textureWidth || z < 0 || z >= textureWidth)
                    return;
                int idx = z * textureWidth + x;
                DrawPixel(textureData, idx, color);
            }

            SetPixel(centerX, centerZ);

            SetPixel(centerX + 1, centerZ);
            SetPixel(centerX - 1, centerZ);
            SetPixel(centerX, centerZ + 1);
            SetPixel(centerX, centerZ - 1);
        }

        private static void DrawCircle(
            NativeArray<Color32> textureData,
            int centerX,
            int centerZ,
            int radius,
            int textureWidth,
            Color32 color)
        {
            int rSquared = radius * radius;

            for (int dz = -radius; dz <= radius; dz++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int distSq = dx * dx + dz * dz;

                    // Draw only the "ring" (within ~1 pixel thickness)
                    if (distSq >= rSquared - radius && distSq <= rSquared + radius)
                    {
                        int x = centerX + dx;
                        int z = centerZ + dz;

                        if (x < 0 || x >= textureWidth || z < 0 || z >= textureWidth)
                            continue;

                        int idx = z * textureWidth + x;

                        DrawPixel(textureData, idx, color);
                    }
                }
            }
        }
    }
}
