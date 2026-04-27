using System;
using System.Collections.Generic;
using System.Linq;

namespace WalkerSim
{
    public class Cities
    {
        public class City
        {
            // 1-based id matching the component id used in CityIdMap. 0 is reserved for "no city".
            public int Id { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 Bounds { get; set; }
            public int CellCount { get; set; }
            public List<MapData.Decoration> POIs { get; set; }
            public Biomes.Type Biome { get; set; } = Biomes.Type.Invalid;

            public City()
            {
                POIs = new List<MapData.Decoration>();
            }

            public float MinX => Position.X - Bounds.X / 2;
            public float MaxX => Position.X + Bounds.X / 2;
            public float MinY => Position.Y - Bounds.Y / 2;
            public float MaxY => Position.Y + Bounds.Y / 2;
        }

        public List<City> CityList { get; private set; }
        public float[] CityAreaWeights { get; private set; }
        public float TotalAreaWeight { get; private set; }

        // City-id raster. 0 = not a city, otherwise 1-based city id.
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ushort[] CityIdMap { get; private set; }

        // World bounds captured at build time so GetCityAt can remap without needing State.
        public Vector3 WorldMins { get; private set; }
        public Vector3 WorldMaxs { get; private set; }
        public float CellSize { get; private set; }

        // Signed distance field for "inside any city": positive inside, negative outside.
        public int SDFWidth { get; private set; }
        public int SDFHeight { get; private set; }
        private float[] _sdf;

        public Cities()
        {
            CityList = new List<City>();
            CityAreaWeights = new float[0];
            TotalAreaWeight = 0;
            CityIdMap = new ushort[0];
            _sdf = new float[0];
        }

        // Target world meters per grid cell. Smaller cells separate buildings better
        // but cost more memory; larger cells over-merge. 16m is a reasonable balance
        // given typical POI footprints of 20-80m.
        private const float TargetCellSize = 16f;
        private const int MaxGridDim = 1024;
        private const int SDFSize = 256;

        // Maximum size (in cells) of an enclosed empty region that gets patched into
        // its surrounding city. Anything larger is left as a real hole — courtyards,
        // plazas, the empty middle of the spiral. At 16m cells this is ~8000 m²,
        // about a 90×90m square.
        private const int MaxHoleCellsToFill = 32;

        /// <summary>
        /// Generates city regions by rasterizing POIs onto a grid, running connected-component
        /// labelling, then filtering components by POI count. Each city is an arbitrary shape
        /// defined by the cells it occupies (no longer a rectangle).
        /// </summary>
        public static Cities GenerateFromPOIs(
            MapData.Decoration[] pois,
            Vector3 worldMins,
            Vector3 worldMaxs,
            float clusterDistance = 32f,
            int minPOIsPerCity = 12,
            int minCellsPerCity = 16,
            float minPoiCoverage = 0.45f)
        {
            var cities = new Cities();
            cities.WorldMins = worldMins;
            cities.WorldMaxs = worldMaxs;

            float worldW = worldMaxs.X - worldMins.X;
            float worldH = worldMaxs.Y - worldMins.Y;
            if (worldW <= 0 || worldH <= 0 || pois == null || pois.Length == 0)
            {
                return cities;
            }

            // Pick a cell size that keeps the grid within MaxGridDim on the longest axis.
            float cellSize = Math.Max(TargetCellSize, Math.Max(worldW, worldH) / MaxGridDim);
            int width = (int)Math.Ceiling(worldW / cellSize);
            int height = (int)Math.Ceiling(worldH / cellSize);

            cities.CellSize = cellSize;
            cities.Width = width;
            cities.Height = height;

            // Two rasterizations: `occupancy` is dilated and used for flood-fill so
            // nearby POIs merge into one component, while `poiOccupancy` marks only
            // the actual POI footprints. Comparing the two per-component produces a
            // "coverage ratio" that cleanly separates packed city blocks (high
            // coverage) from POIs strung along a road (low coverage — the component
            // is mostly dilation halo).
            var occupancy = new byte[width * height];
            var poiOccupancy = new byte[width * height];
            int dilation = (int)Math.Ceiling((clusterDistance * 0.5f) / cellSize);

            foreach (var poi in pois)
            {
                float minX = poi.Position.X - poi.Bounds.X * 0.5f - worldMins.X;
                float maxX = poi.Position.X + poi.Bounds.X * 0.5f - worldMins.X;
                float minY = poi.Position.Y - poi.Bounds.Y * 0.5f - worldMins.Y;
                float maxY = poi.Position.Y + poi.Bounds.Y * 0.5f - worldMins.Y;

                int px0 = (int)Math.Floor(minX / cellSize);
                int px1 = (int)Math.Floor(maxX / cellSize);
                int py0 = (int)Math.Floor(minY / cellSize);
                int py1 = (int)Math.Floor(maxY / cellSize);

                int gx0 = px0 - dilation;
                int gx1 = px1 + dilation;
                int gy0 = py0 - dilation;
                int gy1 = py1 + dilation;

                if (gx0 < 0)
                    gx0 = 0;
                if (gy0 < 0)
                    gy0 = 0;
                if (gx1 >= width)
                    gx1 = width - 1;
                if (gy1 >= height)
                    gy1 = height - 1;

                if (px0 < 0)
                    px0 = 0;
                if (py0 < 0)
                    py0 = 0;
                if (px1 >= width)
                    px1 = width - 1;
                if (py1 >= height)
                    py1 = height - 1;

                for (int y = gy0; y <= gy1; y++)
                {
                    int row = y * width;
                    for (int x = gx0; x <= gx1; x++)
                    {
                        occupancy[row + x] = 1;
                    }
                }

                for (int y = py0; y <= py1; y++)
                {
                    int row = y * width;
                    for (int x = px0; x <= px1; x++)
                    {
                        poiOccupancy[row + x] = 1;
                    }
                }
            }

            // Connected-component labelling (4-connected flood fill).
            var idMap = new ushort[width * height];
            var componentCells = new List<int>();  // cell count per id
            var componentPoiCells = new List<int>(); // cells that contain real POI footprint
            var componentMinX = new List<int>();
            var componentMaxX = new List<int>();
            var componentMinY = new List<int>();
            var componentMaxY = new List<int>();
            // Placeholders for id 0 (unused).
            componentCells.Add(0);
            componentPoiCells.Add(0);
            componentMinX.Add(0);
            componentMaxX.Add(0);
            componentMinY.Add(0);
            componentMaxY.Add(0);

            var queue = new Queue<int>();
            ushort nextId = 1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    if (occupancy[idx] == 0 || idMap[idx] != 0)
                        continue;

                    ushort id = nextId++;
                    idMap[idx] = id;
                    queue.Enqueue(idx);

                    int count = 0;
                    int poiCount = 0;
                    int minCX = x, maxCX = x, minCY = y, maxCY = y;

                    while (queue.Count > 0)
                    {
                        int cur = queue.Dequeue();
                        count++;
                        if (poiOccupancy[cur] != 0)
                            poiCount++;
                        int cx = cur % width;
                        int cy = cur / width;
                        if (cx < minCX)
                            minCX = cx;
                        if (cx > maxCX)
                            maxCX = cx;
                        if (cy < minCY)
                            minCY = cy;
                        if (cy > maxCY)
                            maxCY = cy;

                        // 4-connected neighbors.
                        if (cx > 0)
                        {
                            int n = cur - 1;
                            if (occupancy[n] != 0 && idMap[n] == 0)
                            {
                                idMap[n] = id;
                                queue.Enqueue(n);
                            }
                        }
                        if (cx < width - 1)
                        {
                            int n = cur + 1;
                            if (occupancy[n] != 0 && idMap[n] == 0)
                            {
                                idMap[n] = id;
                                queue.Enqueue(n);
                            }
                        }
                        if (cy > 0)
                        {
                            int n = cur - width;
                            if (occupancy[n] != 0 && idMap[n] == 0)
                            {
                                idMap[n] = id;
                                queue.Enqueue(n);
                            }
                        }
                        if (cy < height - 1)
                        {
                            int n = cur + width;
                            if (occupancy[n] != 0 && idMap[n] == 0)
                            {
                                idMap[n] = id;
                                queue.Enqueue(n);
                            }
                        }
                    }

                    componentCells.Add(count);
                    componentPoiCells.Add(poiCount);
                    componentMinX.Add(minCX);
                    componentMaxX.Add(maxCX);
                    componentMinY.Add(minCY);
                    componentMaxY.Add(maxCY);
                }
            }

            int rawComponentCount = nextId - 1;

            // Assign each POI to the component its center lands on.
            var poisByComponent = new Dictionary<ushort, List<MapData.Decoration>>();
            foreach (var poi in pois)
            {
                int gx = (int)Math.Floor((poi.Position.X - worldMins.X) / cellSize);
                int gy = (int)Math.Floor((poi.Position.Y - worldMins.Y) / cellSize);
                if (gx < 0 || gx >= width || gy < 0 || gy >= height)
                    continue;

                ushort id = idMap[gy * width + gx];
                if (id == 0)
                    continue;

                if (!poisByComponent.TryGetValue(id, out var list))
                {
                    list = new List<MapData.Decoration>();
                    poisByComponent[id] = list;
                }
                list.Add(poi);
            }

            // Build City objects for components that meet thresholds. Renumber surviving
            // components into a contiguous 1-based range and relabel the id map accordingly.
            int rejectedByCoverage = 0;
            var idRemap = new ushort[rawComponentCount + 1];
            for (int rawId = 1; rawId <= rawComponentCount; rawId++)
            {
                int cells = componentCells[rawId];
                if (cells < minCellsPerCity)
                    continue;

                if (!poisByComponent.TryGetValue((ushort)rawId, out var componentPois) ||
                    componentPois.Count < minPOIsPerCity)
                    continue;

                // Coverage ratio: fraction of the dilated component that is actually
                // covered by POI footprints. Strings of POIs along a road have big
                // dilation halos and low coverage; packed city blocks have high
                // coverage because the POI rectangles tile or overlap.
                float coverage = (float)componentPoiCells[rawId] / cells;
                if (coverage < minPoiCoverage)
                {
                    rejectedByCoverage++;
                    continue;
                }

                int cellMinX = componentMinX[rawId];
                int cellMaxX = componentMaxX[rawId];
                int cellMinY = componentMinY[rawId];
                int cellMaxY = componentMaxY[rawId];

                float cityMinX = worldMins.X + cellMinX * cellSize;
                float cityMaxX = worldMins.X + (cellMaxX + 1) * cellSize;
                float cityMinY = worldMins.Y + cellMinY * cellSize;
                float cityMaxY = worldMins.Y + (cellMaxY + 1) * cellSize;

                var city = new City
                {
                    Id = cities.CityList.Count + 1,
                    Position = new Vector3((cityMinX + cityMaxX) * 0.5f, (cityMinY + cityMaxY) * 0.5f, 0),
                    Bounds = new Vector3(cityMaxX - cityMinX, cityMaxY - cityMinY, 0),
                    CellCount = cells,
                };
                city.POIs.AddRange(componentPois);
                cities.CityList.Add(city);

                idRemap[rawId] = (ushort)city.Id;
            }

            // Apply remap (drops cells belonging to filtered-out components).
            for (int i = 0; i < idMap.Length; i++)
            {
                ushort id = idMap[i];
                if (id != 0)
                    idMap[i] = idRemap[id];
            }

            cities.CityIdMap = idMap;

            // Patch up small enclosed gaps inside city footprints — single-cell holes
            // and tiny courtyards between buildings should be considered part of the
            // surrounding city, not exterior space. Larger enclosed regions (real
            // courtyards, plazas, the empty middle of the spiral) are left alone.
            int holeCellsFilled = FillEnclosedHoles(cities, MaxHoleCellsToFill);

            var poisInCities = cities.CityList.Sum(c => c.POIs.Count);
            Logging.Info("Generated {0} cities from {1} POIs ({2} POIs in cities, {3} isolated, {4} components rejected by coverage, {5} hole cells filled).",
                cities.CityList.Count,
                pois.Length,
                poisInCities,
                pois.Length - poisInCities,
                rejectedByCoverage,
                holeCellsFilled);

            cities.BuildSDF();
            cities.ComputeAreaWeights();

            return cities;
        }

        /// <summary>
        /// Gets the city containing the given world position, or null if not in any city.
        /// O(1) grid lookup — correctly handles non-rectangular city shapes.
        /// </summary>
        public City GetCityAt(Vector3 position)
        {
            if (Width == 0 || Height == 0)
                return null;

            int gx = (int)Math.Floor((position.X - WorldMins.X) / CellSize);
            int gy = (int)Math.Floor((position.Y - WorldMins.Y) / CellSize);
            if (gx < 0 || gx >= Width || gy < 0 || gy >= Height)
                return null;

            ushort id = CityIdMap[gy * Width + gx];
            if (id == 0 || id > CityList.Count)
                return null;

            return CityList[id - 1];
        }

        public void AssignBiomes(Biomes biomes, Vector3 worldMins, Vector3 worldMaxs)
        {
            if (biomes == null || biomes.Width == 0 || biomes.Height == 0)
                return;

            float worldRangeX = worldMaxs.X - worldMins.X;
            float worldRangeY = worldMaxs.Y - worldMins.Y;
            if (worldRangeX <= 0 || worldRangeY <= 0)
                return;

            for (int i = 0; i < CityList.Count; i++)
            {
                var c = CityList[i];
                int bx = (int)((c.Position.X - worldMins.X) / worldRangeX * biomes.Width);
                int by = (int)((c.Position.Y - worldMins.Y) / worldRangeY * biomes.Height);
                c.Biome = biomes.GetBiomeType(bx, by);
            }
        }

        /// <summary>
        /// Sample the signed distance field. Coordinates are in city-grid cell space [0..Width, 0..Height].
        /// Returns positive inside any city, negative outside.
        /// </summary>
        public float SampleSDF(float bx, float by)
        {
            if (_sdf.Length == 0)
                return -1e6f;

            float sx = (bx / Width) * (SDFWidth - 1);
            float sy = (by / Height) * (SDFHeight - 1);

            int x0 = (int)sx;
            int y0 = (int)sy;
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            if (x0 < 0)
                x0 = 0;
            if (y0 < 0)
                y0 = 0;
            if (x1 >= SDFWidth)
                x1 = SDFWidth - 1;
            if (y1 >= SDFHeight)
                y1 = SDFHeight - 1;
            if (x0 >= SDFWidth)
                x0 = SDFWidth - 1;
            if (y0 >= SDFHeight)
                y0 = SDFHeight - 1;

            float fx = sx - (int)sx;
            float fy = sy - (int)sy;
            if (fx < 0)
                fx = 0;
            if (fy < 0)
                fy = 0;

            float v00 = _sdf[y0 * SDFWidth + x0];
            float v10 = _sdf[y0 * SDFWidth + x1];
            float v01 = _sdf[y1 * SDFWidth + x0];
            float v11 = _sdf[y1 * SDFWidth + x1];

            float top = v00 + (v10 - v00) * fx;
            float bot = v01 + (v11 - v01) * fx;
            return top + (bot - top) * fy;
        }

        /// <summary>
        /// Sample the SDF gradient using central differences. The returned vector points from
        /// outside toward the nearest inside cell. Z is always 0.
        /// </summary>
        public Vector3 SampleSDFGradient(float bx, float by)
        {
            if (_sdf.Length == 0)
                return Vector3.Zero;

            float step = (float)Width / SDFWidth;
            float dx = SampleSDF(bx + step, by) - SampleSDF(bx - step, by);
            float dy = SampleSDF(bx, by + step) - SampleSDF(bx, by - step);
            return new Vector3(dx, dy, 0f);
        }

        public void ComputeAreaWeights()
        {
            int count = CityList.Count;
            CityAreaWeights = new float[count];
            TotalAreaWeight = 0;

            float cellArea = CellSize * CellSize;
            for (int i = 0; i < count; i++)
            {
                // Use actual cell count * cell area, not the AABB area, so spiral-shaped
                // cities don't get inflated weights from their empty bounding box.
                float area = CityList[i].CellCount * cellArea;
                CityAreaWeights[i] = area;
                TotalAreaWeight += area;
            }
        }

        /// <summary>
        /// Builds a single signed distance field from the city-id map. Downsamples to SDFSize
        /// then runs Felzenszwalb &amp; Huttenlocher's 2D Euclidean distance transform on both
        /// the inside and outside pixels, combining the two into a signed field.
        /// </summary>
        private void BuildSDF()
        {
            if (Width == 0 || Height == 0)
            {
                SDFWidth = 0;
                SDFHeight = 0;
                _sdf = new float[0];
                return;
            }

            SDFWidth = Math.Min(SDFSize, Width);
            SDFHeight = Math.Min(SDFSize, Height);

            var downsampled = new bool[SDFWidth * SDFHeight];
            float scaleX = (float)Width / SDFWidth;
            float scaleY = (float)Height / SDFHeight;

            for (int y = 0; y < SDFHeight; y++)
            {
                for (int x = 0; x < SDFWidth; x++)
                {
                    int srcX = Math.Min((int)(x * scaleX + scaleX * 0.5f), Width - 1);
                    int srcY = Math.Min((int)(y * scaleY + scaleY * 0.5f), Height - 1);
                    downsampled[y * SDFWidth + x] = CityIdMap[srcY * Width + srcX] != 0;
                }
            }

            _sdf = ComputeSDF(downsampled, SDFWidth, SDFHeight);
        }

        /// <summary>
        /// Patches small enclosed empty regions inside city footprints. An "enclosed"
        /// region is one that empty-cell flood fill from the grid boundary cannot reach.
        /// Each such region is BFSed to find its surrounding city ids; if the region is
        /// at most <paramref name="maxHoleCells"/> cells the cells are reassigned to the
        /// dominant bordering city. Larger enclosed regions (real courtyards, plazas)
        /// are left as holes.
        /// </summary>
        private static int FillEnclosedHoles(Cities cities, int maxHoleCells)
        {
            var idMap = cities.CityIdMap;
            int width = cities.Width;
            int height = cities.Height;
            int total = width * height;
            if (total == 0 || cities.CityList.Count == 0)
                return 0;

            // Phase 1: mark every empty cell reachable from any grid boundary cell as
            // "exterior" via BFS. Anything left empty after this is an enclosed hole.
            var exterior = new bool[total];
            var queue = new Queue<int>();

            for (int x = 0; x < width; x++)
            {
                SeedExterior(idMap, exterior, queue, x);
                SeedExterior(idMap, exterior, queue, (height - 1) * width + x);
            }
            for (int y = 0; y < height; y++)
            {
                SeedExterior(idMap, exterior, queue, y * width);
                SeedExterior(idMap, exterior, queue, y * width + width - 1);
            }

            while (queue.Count > 0)
            {
                int cur = queue.Dequeue();
                int cx = cur % width;
                int cy = cur / width;
                if (cx > 0)
                    SeedExterior(idMap, exterior, queue, cur - 1);
                if (cx < width - 1)
                    SeedExterior(idMap, exterior, queue, cur + 1);
                if (cy > 0)
                    SeedExterior(idMap, exterior, queue, cur - width);
                if (cy < height - 1)
                    SeedExterior(idMap, exterior, queue, cur + width);
            }

            // Phase 2: walk hole components, find dominant bordering city, fill if small.
            var visited = new bool[total];
            var holeCells = new List<int>();
            var neighborCounts = new Dictionary<ushort, int>();
            var hq = new Queue<int>();
            var addedPerCity = new Dictionary<ushort, int>();
            int totalFilled = 0;

            for (int seed = 0; seed < total; seed++)
            {
                if (idMap[seed] != 0 || exterior[seed] || visited[seed])
                    continue;

                holeCells.Clear();
                neighborCounts.Clear();
                hq.Clear();
                hq.Enqueue(seed);
                visited[seed] = true;

                while (hq.Count > 0)
                {
                    int cur = hq.Dequeue();
                    holeCells.Add(cur);
                    int cx = cur % width;
                    int cy = cur / width;

                    if (cx > 0)
                        VisitHoleNeighbor(cur - 1, idMap, visited, hq, neighborCounts);
                    if (cx < width - 1)
                        VisitHoleNeighbor(cur + 1, idMap, visited, hq, neighborCounts);
                    if (cy > 0)
                        VisitHoleNeighbor(cur - width, idMap, visited, hq, neighborCounts);
                    if (cy < height - 1)
                        VisitHoleNeighbor(cur + width, idMap, visited, hq, neighborCounts);
                }

                if (holeCells.Count > maxHoleCells || neighborCounts.Count == 0)
                    continue;

                ushort dominantId = 0;
                int dominantCount = 0;
                foreach (var kv in neighborCounts)
                {
                    if (kv.Value > dominantCount)
                    {
                        dominantCount = kv.Value;
                        dominantId = kv.Key;
                    }
                }

                foreach (var c in holeCells)
                    idMap[c] = dominantId;
                totalFilled += holeCells.Count;

                if (addedPerCity.TryGetValue(dominantId, out int existing))
                    addedPerCity[dominantId] = existing + holeCells.Count;
                else
                    addedPerCity[dominantId] = holeCells.Count;
            }

            // Update per-city cell counts so area weights and sdf scaling stay accurate.
            foreach (var kv in addedPerCity)
            {
                int idx = kv.Key - 1;
                if (idx >= 0 && idx < cities.CityList.Count)
                    cities.CityList[idx].CellCount += kv.Value;
            }

            return totalFilled;
        }

        private static void SeedExterior(ushort[] idMap, bool[] exterior, Queue<int> queue, int idx)
        {
            if (idMap[idx] == 0 && !exterior[idx])
            {
                exterior[idx] = true;
                queue.Enqueue(idx);
            }
        }

        private static void VisitHoleNeighbor(int idx, ushort[] idMap, bool[] visited, Queue<int> hq, Dictionary<ushort, int> neighborCounts)
        {
            ushort id = idMap[idx];
            if (id == 0)
            {
                if (!visited[idx])
                {
                    visited[idx] = true;
                    hq.Enqueue(idx);
                }
            }
            else
            {
                if (neighborCounts.TryGetValue(id, out int existing))
                    neighborCounts[id] = existing + 1;
                else
                    neighborCounts[id] = 1;
            }
        }

        private static float[] ComputeSDF(bool[] inside, int w, int h)
        {
            int n = w * h;
            var insideDist = new float[n];
            var outsideDist = new float[n];
            const float INF = 1e10f;

            for (int i = 0; i < n; i++)
            {
                if (inside[i])
                {
                    insideDist[i] = INF;
                    outsideDist[i] = 0f;
                }
                else
                {
                    insideDist[i] = 0f;
                    outsideDist[i] = INF;
                }
            }

            EDT2D(insideDist, w, h);
            EDT2D(outsideDist, w, h);

            var sdf = new float[n];
            for (int i = 0; i < n; i++)
            {
                sdf[i] = (float)(Math.Sqrt(insideDist[i]) - Math.Sqrt(outsideDist[i]));
            }
            return sdf;
        }

        private static void EDT2D(float[] grid, int w, int h)
        {
            var col = new float[h];
            var colOut = new float[h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                    col[y] = grid[y * w + x];

                EDT1D(col, colOut, h);

                for (int y = 0; y < h; y++)
                    grid[y * w + x] = colOut[y];
            }

            var row = new float[w];
            var rowOut = new float[w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                    row[x] = grid[y * w + x];

                EDT1D(row, rowOut, w);

                for (int x = 0; x < w; x++)
                    grid[y * w + x] = rowOut[x];
            }
        }

        private static void EDT1D(float[] f, float[] d, int n)
        {
            if (n == 0)
                return;

            var v = new int[n];
            var z = new float[n + 1];
            int k = 0;
            v[0] = 0;
            z[0] = float.NegativeInfinity;
            z[1] = float.PositiveInfinity;

            for (int q = 1; q < n; q++)
            {
                float fq = f[q] + (float)q * q;
                float fvk = f[v[k]] + (float)v[k] * v[k];
                float s = (fq - fvk) / (2f * q - 2f * v[k]);

                while (s <= z[k])
                {
                    k--;
                    fvk = f[v[k]] + (float)v[k] * v[k];
                    s = (fq - fvk) / (2f * q - 2f * v[k]);
                }

                k++;
                v[k] = q;
                z[k] = s;
                z[k + 1] = float.PositiveInfinity;
            }

            k = 0;
            for (int q = 0; q < n; q++)
            {
                while (z[k + 1] < q)
                    k++;
                float diff = q - v[k];
                d[q] = diff * diff + f[v[k]];
            }
        }
    }
}
