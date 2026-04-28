using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace WalkerSim
{
    internal class MapDrawing
    {
        public static bool IsEnabled { get; set; } = false;
        public static bool IsTemporarilyEnabled { get; set; } = false;
        public static bool IsGraphEnabled { get; set; } = false;
        public static bool IsBiomesEnabled { get; set; } = false;
        public static bool IsCitiesEnabled { get; set; } = false;

        private static readonly Color32 ColorActive = new Color32(0, 255, 0, 255);
        private static readonly Color32 ColorInactive = new Color32(255, 0, 0, 255);
        private static readonly Color32 ColorEvent = new Color32(200, 128, 128, 128);
        private static readonly Color32 ColorOutterActivation = new Color32(0, 0, 255, 128);
        private static readonly Color32 ColorInnerActivation = new Color32(255, 255, 0, 128);
        private static readonly Color32 ColorActivationZone = new Color32(0, 240, 0, 50);
        private static readonly Color32 ColorRoadEdge = new Color32(255, 170, 0, 200);
        private static readonly Color32 ColorRoadNode = new Color32(255, 220, 80, 255);
        private static readonly Color32 ColorRoadBridge = new Color32(255, 64, 255, 255);

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
            if (!IsEnabled && !IsTemporarilyEnabled && !IsGraphEnabled && !IsBiomesEnabled && !IsCitiesEnabled)
            {
                return;
            }

            NativeArray<Color32> textureData = inst.mapTexture.GetRawTextureData<Color32>();
            var simulation = Simulation.Instance;

            int textureWidth = 2048; // vanilla 7DTD map texture size

            // Biomes first so the graph and agent markers render on top.
            if (IsBiomesEnabled)
            {
                DrawBiomes(textureData, simulation,
                    mapStartX, mapStartZ, mapEndX, mapEndZ,
                    textureStartX, textureStartZ, textureWidth);
            }

            if (IsCitiesEnabled)
            {
                DrawCities(textureData, simulation,
                    mapStartX, mapStartZ, mapEndX, mapEndZ,
                    textureStartX, textureStartZ, textureWidth);
            }

            if (IsGraphEnabled)
            {
                DrawRoadGraph(textureData, simulation,
                    mapStartX, mapStartZ, mapEndX, mapEndZ,
                    textureStartX, textureStartZ, textureWidth);
            }

            if (!IsEnabled && !IsTemporarilyEnabled)
            {
                inst.timeToRedrawMap = 0.3f;
                return;
            }

            foreach (var agent in simulation.Agents)
            {
                if (agent.CurrentState == Agent.State.Inactive || agent.CurrentState == Agent.State.Dead || agent.CurrentState == Agent.State.Respawning)
                {
                    continue;
                }

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

                var color = agent.CurrentState == Agent.State.Spawned ? ColorActive : ColorInactive;

                // Overdraw pixel with chosen color (bright red marker here)
                textureData[pixelIndex] = color;

                DrawMarker(textureData, texX, texZ, textureWidth, color);
            }

            var config = simulation.Config;

            foreach (var worldEvent in simulation.Events)
            {
                var eventWorldPos = VectorUtils.ToUnity(worldEvent.Position);

                if (eventWorldPos.x < mapStartX || eventWorldPos.x >= mapEndX ||
                    eventWorldPos.z < mapStartZ || eventWorldPos.z >= mapEndZ)
                {
                    continue;
                }

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

            var activationBorderSize = Simulation.Constants.SpawnBorderSize;

            int numPlayers = 0;
            foreach (var ply in simulation.Players)
            {
                var plyWorldPos = VectorUtils.ToUnity(ply.Value.Position);

                if (plyWorldPos.x < mapStartX || plyWorldPos.x >= mapEndX ||
                    plyWorldPos.z < mapStartZ || plyWorldPos.z >= mapEndZ)
                {
                    continue;
                }

                var playerTexX = global::Utils.WrapIndex(textureStartX + (int)(plyWorldPos.x - mapStartX), textureWidth);
                var playerTexZ = global::Utils.WrapIndex(textureStartZ + (int)(plyWorldPos.z - mapStartZ), textureWidth);

                // This is some inefficient garbage but it works.
                for (int i = 0; i < activationBorderSize; i++)
                {
                    DrawCircle(textureData, playerTexX, playerTexZ, (int)(config.SpawnActivationRadius - i), textureWidth, ColorActivationZone);
                }

                DrawCircle(textureData, playerTexX, playerTexZ, (int)config.SpawnActivationRadius, textureWidth, ColorOutterActivation);
                DrawCircle(textureData, playerTexX, playerTexZ, (int)(config.SpawnActivationRadius - activationBorderSize), textureWidth, ColorInnerActivation);

                numPlayers++;
            }

            inst.timeToRedrawMap = 0.3f;
        }

        // Cached biome type → Color32 lookup. Indexed by (byte)Biomes.Type; all
        // entries with Alpha=0 are treated as "no biome / skip".
        private static Color32[] _biomeColors;

        private static Color32[] GetBiomeColorLookup()
        {
            if (_biomeColors != null)
                return _biomeColors;

            var table = new Color32[256];
            const byte alpha = 110;
            foreach (var bt in Biomes.ValidTypes)
            {
                var c = Biomes.GetColorForType(bt);
                table[(byte)bt] = new Color32(c.R, c.G, c.B, alpha);
            }
            _biomeColors = table;
            return table;
        }

        private static void DrawBiomes(
            NativeArray<Color32> textureData,
            Simulation simulation,
            int mapStartX, int mapStartZ, int mapEndX, int mapEndZ,
            int textureStartX, int textureStartZ,
            int textureWidth)
        {
            var mapData = simulation.MapData;
            if (mapData == null)
                return;

            var biomes = mapData.Biomes;
            if (biomes == null || biomes.Width == 0 || biomes.Height == 0)
                return;

            var worldMins = mapData.WorldMins;
            var worldMaxs = mapData.WorldMaxs;
            float worldRangeX = worldMaxs.X - worldMins.X;
            float worldRangeY = worldMaxs.Y - worldMins.Y;
            if (worldRangeX <= 0f || worldRangeY <= 0f)
                return;

            var biomeMap = biomes.BiomeMap;
            int bWidth = biomes.Width;
            int bHeight = biomes.Height;
            var colorLookup = GetBiomeColorLookup();

            // sim coord = world coord for X; sim Y = -world Z (see VectorUtils).
            // One texture pixel corresponds to one Unity world unit.
            float invRangeX = bWidth / worldRangeX;
            float invRangeY = bHeight / worldRangeY;

            for (int wz = mapStartZ; wz < mapEndZ; wz++)
            {
                float simY = -wz;
                int by = (int)((simY - worldMins.Y) * invRangeY);
                if (by < 0 || by >= bHeight)
                    continue;

                int tzOffset = wz - mapStartZ;
                int tz = global::Utils.WrapIndex(textureStartZ + tzOffset, textureWidth);
                int rowBase = tz * textureWidth;

                for (int wx = mapStartX; wx < mapEndX; wx++)
                {
                    int bx = (int)((wx - worldMins.X) * invRangeX);
                    if (bx < 0 || bx >= bWidth)
                        continue;

                    var bt = biomeMap[bx, by];
                    var color = colorLookup[(byte)bt];
                    if (color.a == 0)
                        continue;

                    int tx = global::Utils.WrapIndex(textureStartX + (wx - mapStartX), textureWidth);
                    DrawPixel(textureData, rowBase + tx, color);
                }
            }
        }

        private static Cities _cachedCitiesRef;
        private static Color32[] _cityColorLookup;

        private static Color32[] GetCityColorLookup(Cities cities)
        {
            if (ReferenceEquals(_cachedCitiesRef, cities) && _cityColorLookup != null)
                return _cityColorLookup;

            int cityCount = cities.CityList.Count;
            var table = new Color32[cityCount + 1];
            const byte alpha = 140;
            for (int i = 0; i < cityCount; i++)
            {
                float hue = ((i * 2654435761u) % 360u) / 360f;
                HsvToRgb(hue, 0.85f, 1.0f, out byte r, out byte g, out byte b);
                table[i + 1] = new Color32(r, g, b, alpha);
            }
            _cityColorLookup = table;
            _cachedCitiesRef = cities;
            return table;
        }

        private static void HsvToRgb(float h, float s, float v, out byte r, out byte g, out byte b)
        {
            float c = v * s;
            float hp = h * 6f;
            float x = c * (1f - System.Math.Abs((hp % 2f) - 1f));
            float r1, g1, b1;
            if (hp < 1f)
            { r1 = c; g1 = x; b1 = 0; }
            else if (hp < 2f)
            { r1 = x; g1 = c; b1 = 0; }
            else if (hp < 3f)
            { r1 = 0; g1 = c; b1 = x; }
            else if (hp < 4f)
            { r1 = 0; g1 = x; b1 = c; }
            else if (hp < 5f)
            { r1 = x; g1 = 0; b1 = c; }
            else
            { r1 = c; g1 = 0; b1 = x; }
            float m = v - c;
            r = (byte)System.Math.Round((r1 + m) * 255f);
            g = (byte)System.Math.Round((g1 + m) * 255f);
            b = (byte)System.Math.Round((b1 + m) * 255f);
        }

        private static void DrawCities(
            NativeArray<Color32> textureData,
            Simulation simulation,
            int mapStartX, int mapStartZ, int mapEndX, int mapEndZ,
            int textureStartX, int textureStartZ,
            int textureWidth)
        {
            var mapData = simulation.MapData;
            if (mapData == null)
                return;

            var cities = mapData.Cities;
            if (cities == null || cities.Width == 0 || cities.Height == 0 || cities.CityIdMap == null)
                return;

            var idMap = cities.CityIdMap;
            int cWidth = cities.Width;
            int cHeight = cities.Height;
            float cellSize = cities.CellSize;
            var cWorldMins = cities.WorldMins;
            int cityCount = cities.CityList.Count;
            var colorLookup = GetCityColorLookup(cities);

            for (int wz = mapStartZ; wz < mapEndZ; wz++)
            {
                float simY = -wz;
                int gy = (int)((simY - cWorldMins.Y) / cellSize);
                if (gy < 0 || gy >= cHeight)
                    continue;

                int tzOffset = wz - mapStartZ;
                int tz = global::Utils.WrapIndex(textureStartZ + tzOffset, textureWidth);
                int rowBase = tz * textureWidth;
                int srcRowBase = gy * cWidth;

                for (int wx = mapStartX; wx < mapEndX; wx++)
                {
                    int gx = (int)((wx - cWorldMins.X) / cellSize);
                    if (gx < 0 || gx >= cWidth)
                        continue;

                    ushort id = idMap[srcRowBase + gx];
                    if (id == 0 || id > cityCount)
                        continue;

                    int tx = global::Utils.WrapIndex(textureStartX + (wx - mapStartX), textureWidth);
                    DrawPixel(textureData, rowBase + tx, colorLookup[id]);
                }
            }
        }

        private static void DrawRoadGraph(
            NativeArray<Color32> textureData,
            Simulation simulation,
            int mapStartX, int mapStartZ, int mapEndX, int mapEndZ,
            int textureStartX, int textureStartZ,
            int textureWidth)
        {
            var mapData = simulation.MapData;
            if (mapData == null)
                return;

            var roads = mapData.Roads;
            if (roads == null)
                return;

            var graph = roads.Graph;
            if (graph == null || graph.Nodes.Length == 0)
                return;

            var worldMins = mapData.WorldMins;
            var worldMaxs = mapData.WorldMaxs;
            float worldRangeX = worldMaxs.X - worldMins.X;
            float worldRangeY = worldMaxs.Y - worldMins.Y;
            if (worldRangeX <= 0f || worldRangeY <= 0f)
                return;

            int bitmapWidth = roads.Width;
            int bitmapHeight = roads.Height;

            // Bitmap pixel → unwrapped texture pixel. We defer wrapping until
            // we actually write to the texture so a single long edge that
            // crosses the 0/textureWidth seam doesn't turn into a full-texture
            // diagonal through WrapIndex jumps.
            void BitmapToTextureRaw(float bx, float by, out int tx, out int tz)
            {
                float simX = worldMins.X + (bx / bitmapWidth) * worldRangeX;
                float simY = worldMins.Y + (by / bitmapHeight) * worldRangeY;
                var unityPos = VectorUtils.ToUnity(new Vector3(simX, simY));
                tx = textureStartX + (int)(unityPos.x - mapStartX);
                tz = textureStartZ + (int)(unityPos.z - mapStartZ);
            }

            bool IsNodeVisible(RoadGraph.RoadNode node)
            {
                float simX = worldMins.X + (node.X / bitmapWidth) * worldRangeX;
                float simY = worldMins.Y + (node.Y / bitmapHeight) * worldRangeY;
                var unityPos = VectorUtils.ToUnity(new Vector3(simX, simY));
                return unityPos.x >= mapStartX && unityPos.x < mapEndX &&
                       unityPos.z >= mapStartZ && unityPos.z < mapEndZ;
            }

            // Edges first, so node markers draw on top.
            var nodes = graph.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                var a = nodes[i];
                bool aVisible = IsNodeVisible(a);
                var conns = a.Connections;
                for (int c = 0; c < conns.Length; c++)
                {
                    int j = conns[c];
                    if (j <= i)
                        continue; // Draw each edge once.

                    var b = nodes[j];
                    if (!aVisible && !IsNodeVisible(b))
                        continue;

                    BitmapToTextureRaw(a.X, a.Y, out int ax, out int az);
                    BitmapToTextureRaw(b.X, b.Y, out int bx, out int bz);
                    DrawTextureLine(textureData, ax, az, bx, bz, textureWidth, ColorRoadEdge);
                }
            }

            // Node markers.
            var bridges = graph.IsBridgeEndpoint;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (!IsNodeVisible(node))
                    continue;

                BitmapToTextureRaw(node.X, node.Y, out int tx, out int tz);
                int wx = global::Utils.WrapIndex(tx, textureWidth);
                int wz = global::Utils.WrapIndex(tz, textureWidth);
                bool isBridge = bridges != null && i < bridges.Length && bridges[i];
                DrawMarker(textureData, wx, wz, textureWidth, isBridge ? ColorRoadBridge : ColorRoadNode);
            }
        }

        private static void DrawTextureLine(
            NativeArray<Color32> textureData,
            int x0, int y0, int x1, int y1,
            int textureWidth,
            Color32 color)
        {
            int dx = System.Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -System.Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                int wx = global::Utils.WrapIndex(x0, textureWidth);
                int wy = global::Utils.WrapIndex(y0, textureWidth);
                int idx = wy * textureWidth + wx;
                DrawPixel(textureData, idx, color);

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
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
