using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WalkerSim
{
    public class Biomes
    {
        public enum Type : byte
        {
            Invalid = 0,
            Snow = 1,
            PineForest = 3,
            Desert = 5,
            // Water = 6,
            // Radiated = 7,
            Wasteland = 8,
            BurntForest = 9,
            // CaveFloor = 13,
            // CaveCeiling = 14,
            // Underwater = 19,
        }

        private static readonly Dictionary<Drawing.Color, Type> _colorMapping = new Dictionary<Drawing.Color, Type>
        {
            { WalkerSim.Drawing.Color.FromHtml("#FFFFFF"), Type.Snow },
            { WalkerSim.Drawing.Color.FromHtml("#004000"), Type.PineForest },
            { WalkerSim.Drawing.Color.FromHtml("#FFE477"), Type.Desert },
            { WalkerSim.Drawing.Color.FromHtml("#ffa800"), Type.Wasteland },
            { WalkerSim.Drawing.Color.FromHtml("#BA00FF"), Type.BurntForest },
        };

        // All valid biome types for iteration.
        public static readonly Type[] ValidTypes = new Type[]
        {
            Type.Snow, Type.PineForest, Type.Desert, Type.Wasteland, Type.BurntForest
        };

        // Full-resolution biome map for rendering.
        public Type[,] BiomeMap { get; private set; } = new Type[0, 0];

        public int Width { get; private set; }
        public int Height { get; private set; }
        public string Name { get; private set; } = string.Empty;

        // Signed distance fields per biome type at reduced resolution.
        // Positive = inside biome, negative = outside.
        private Dictionary<Type, float[]> _sdfFields = new Dictionary<Type, float[]>();

        public int SDFWidth { get; private set; }
        public int SDFHeight { get; private set; }

        const int MaxScaledSize = 2048;
        const int SDFSize = 256;

        public static Biomes LoadFromFile(string filePath)
        {
            using (var img = WalkerSim.Drawing.LoadFromFile(filePath))
            {
                return Biomes.LoadFromBitmap(img, filePath);
            }
        }

        public static Type GetTypeFromColor(Drawing.Color color)
        {
            if (_colorMapping.TryGetValue(color, out var biomeType))
            {
                return biomeType;
            }
            return Type.Invalid;
        }

        public static Drawing.Color GetColorForType(Type biomeType)
        {
            switch (biomeType)
            {
                case Type.Snow:
                    return WalkerSim.Drawing.Color.FromHtml("#FFFFFF");
                case Type.PineForest:
                    return WalkerSim.Drawing.Color.FromHtml("#004000");
                case Type.Desert:
                    return WalkerSim.Drawing.Color.FromHtml("#FFE477");
                case Type.Wasteland:
                    return WalkerSim.Drawing.Color.FromHtml("#ffa800");
                case Type.BurntForest:
                    return WalkerSim.Drawing.Color.FromHtml("#BA00FF");
                default:
                    return WalkerSim.Drawing.Color.FromHtml("#000000");
            }
        }

        public static Biomes LoadFromBitmap(WalkerSim.Drawing.IBitmap img, string filePath)
        {
            Logging.Info("Loading biomes...");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();

            Biomes biomes;

            using (Logging.Scope())
            {

                var scaled = (img.Width <= MaxScaledSize && img.Height <= MaxScaledSize)
                    ? img
                    : Drawing.Create(img, MaxScaledSize, MaxScaledSize);

                var height = scaled.Height;
                var width = scaled.Width;

                var data = new Type[width, height];

                scaled.LockPixels();
                Parallel.For(0, height * width, y =>
                {
                    var x = y % width;
                    y /= width;

                    var pixel = scaled.GetPixel(x, y);
                    var key = new Drawing.Color(pixel.R, pixel.G, pixel.B);
                    if (!_colorMapping.TryGetValue(key, out var mappedType))
                    {
                        mappedType = Type.Invalid;
                    }
                    data[x, y] = mappedType;
                });
                scaled.UnlockPixels();

                biomes = new Biomes
                {
                    Width = width,
                    Height = height,
                    BiomeMap = data,
                    Name = filePath,
                };

                biomes.BuildSDFs();
            }

            sw.Stop();
            var elapsed = sw.Elapsed;

            Logging.Info("Loaded biomes in {0}s", elapsed.TotalSeconds);

            return biomes;
        }

        public Type GetBiomeType(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return Type.Invalid;
            }
            return BiomeMap[x, y];
        }

        /// <summary>
        /// Sample the signed distance field for a biome type.
        /// Coordinates are in biome-map pixel space [0..Width, 0..Height].
        /// Returns positive inside the biome, negative outside.
        /// </summary>
        public float SampleSDF(Type biome, float bx, float by)
        {
            if (!_sdfFields.TryGetValue(biome, out var sdf))
                return -1e6f;

            // Map from biome-map coords to SDF coords.
            float sx = (bx / Width) * (SDFWidth - 1);
            float sy = (by / Height) * (SDFHeight - 1);

            // Bilinear interpolation.
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

            float fx = sx - (int)sx;
            float fy = sy - (int)sy;

            float v00 = sdf[y0 * SDFWidth + x0];
            float v10 = sdf[y0 * SDFWidth + x1];
            float v01 = sdf[y1 * SDFWidth + x0];
            float v11 = sdf[y1 * SDFWidth + x1];

            float top = v00 + (v10 - v00) * fx;
            float bot = v01 + (v11 - v01) * fx;
            return top + (bot - top) * fy;
        }

        /// <summary>
        /// Sample the SDF gradient for a biome type using central differences.
        /// Returns a vector pointing from outside toward inside (toward the biome interior).
        /// Coordinates are in biome-map pixel space. Z is always 0.
        /// </summary>
        public Vector3 SampleSDFGradient(Type biome, float bx, float by)
        {
            // Step size in biome-map pixels (one SDF cell).
            float step = (float)Width / SDFWidth;
            float dx = SampleSDF(biome, bx + step, by) - SampleSDF(biome, bx - step, by);
            float dy = SampleSDF(biome, bx, by + step) - SampleSDF(biome, bx, by - step);
            return new Vector3(dx, dy, 0f);
        }

        #region SDF Construction

        private void BuildSDFs()
        {
            Logging.Info("Building SDFs for biomes...");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();

            using (Logging.Scope())
            {
                SDFWidth = SDFSize;
                SDFHeight = SDFSize;

                // Downsample BiomeMap to SDF resolution.
                var downsampled = new Type[SDFWidth * SDFHeight];
                float scaleX = (float)Width / SDFWidth;
                float scaleY = (float)Height / SDFHeight;

                for (int y = 0; y < SDFHeight; y++)
                {
                    for (int x = 0; x < SDFWidth; x++)
                    {
                        // Sample center of corresponding BiomeMap region.
                        int srcX = Math.Min((int)(x * scaleX + scaleX * 0.5f), Width - 1);
                        int srcY = Math.Min((int)(y * scaleY + scaleY * 0.5f), Height - 1);
                        downsampled[y * SDFWidth + x] = BiomeMap[srcX, srcY];
                    }
                }

                // Build SDF for each valid biome type in parallel.
                var results = new Dictionary<Type, float[]>();
                foreach (var biomeType in ValidTypes)
                {
                    results[biomeType] = null;
                }

                Parallel.ForEach(ValidTypes, biomeType =>
                {
                    var sdf = ComputeSDF(downsampled, SDFWidth, SDFHeight, biomeType);
                    lock (results)
                    {
                        results[biomeType] = sdf;
                    }
                });

                _sdfFields = new Dictionary<Type, float[]>(results);
            }

            sw.Stop();
            var elapsed = sw.Elapsed;

            Logging.Info("Built SDFs in {0}s", elapsed.TotalSeconds);
        }

        private static float[] ComputeSDF(Type[] map, int w, int h, Type biomeType)
        {
            int n = w * h;

            // Distance to nearest outside pixel (for cells inside the biome).
            var insideDist = new float[n];
            // Distance to nearest inside pixel (for cells outside the biome).
            var outsideDist = new float[n];

            const float INF = 1e10f;

            for (int i = 0; i < n; i++)
            {
                bool inside = map[i] == biomeType;
                insideDist[i] = inside ? INF : 0f;
                outsideDist[i] = inside ? 0f : INF;
            }

            // 2D squared Euclidean distance transform (Felzenszwalb & Huttenlocher).
            EDT2D(insideDist, w, h);
            EDT2D(outsideDist, w, h);

            // Combine into signed distance field.
            var sdf = new float[n];
            for (int i = 0; i < n; i++)
            {
                sdf[i] = (float)(Math.Sqrt(insideDist[i]) - Math.Sqrt(outsideDist[i]));
            }
            return sdf;
        }

        /// <summary>
        /// In-place 2D squared Euclidean distance transform.
        /// Uses separable 1D transforms on columns then rows.
        /// </summary>
        private static void EDT2D(float[] grid, int w, int h)
        {
            // Transform columns.
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

            // Transform rows.
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

        /// <summary>
        /// 1D squared Euclidean distance transform (Felzenszwalb & Huttenlocher).
        /// Input f[i] = 0 for seed cells, large value for non-seed cells.
        /// Output d[i] = min_j { (i-j)^2 + f[j] }.
        /// </summary>
        private static void EDT1D(float[] f, float[] d, int n)
        {
            if (n == 0)
                return;

            var v = new int[n];     // Locations of parabolas in lower envelope.
            var z = new float[n + 1]; // Boundaries between parabolas.
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

        #endregion
    }
}
