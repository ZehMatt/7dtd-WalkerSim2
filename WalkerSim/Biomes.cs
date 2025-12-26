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
            Underwater = 19,
        }

        private static readonly Dictionary<Drawing.Color, Type> _colorMapping = new Dictionary<Drawing.Color, Type>
        {
            { WalkerSim.Drawing.Color.FromHtml("#FFFFFF"), Type.Snow },
            { WalkerSim.Drawing.Color.FromHtml("#004000"), Type.PineForest },
            { WalkerSim.Drawing.Color.FromHtml("#FFE477"), Type.Desert },
            { WalkerSim.Drawing.Color.FromHtml("#ffa800"), Type.Wasteland },
            { WalkerSim.Drawing.Color.FromHtml("#BA00FF"), Type.BurntForest },
            { WalkerSim.Drawing.Color.FromHtml("#001234"), Type.Underwater }
        };

        public Type[,] BiomeMap { get; private set; } = new Type[0, 0];

        public class BiomeRegion
        {
            public Type Type { get; set; }
            public List<(int X, int Y)> Points { get; set; } = new List<(int X, int Y)>();

            // TODO: Trace along the edges to create a shape for more efficient lookups.
        }

        public List<BiomeRegion> Regions { get; private set; } = new List<BiomeRegion>();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public string Name { get; private set; } = string.Empty;

        // For reasons the biome.png size is inconsistent, stick to one.
        const int ScaledWidth = 1024;
        const int ScaledHeight = 1024;

        public static Biomes LoadFromFile(string filePath)
        {
            using (var img = WalkerSim.Drawing.LoadFromFile(filePath))
            {
                img.RemoveTransparency();

                return Biomes.LoadFromBitmap(img, filePath);
            }
        }

        public static Type GetTypeFromColor(Drawing.Color color)
        {
            if (_colorMapping.TryGetValue(color, out var biomeType))
            {
                return biomeType;
            }
            return Type.Invalid; // Return Invalid if the color is not mapped
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
                case Type.Underwater:
                    return WalkerSim.Drawing.Color.FromHtml("#001234");
                default:
                    return WalkerSim.Drawing.Color.FromHtml("#000000"); // Invalid or unknown biome
            }
        }

        public static Biomes LoadFromBitmap(WalkerSim.Drawing.IBitmap img, string filePath)
        {
            var scaled = Drawing.Create(img, ScaledWidth, ScaledHeight);

            var height = scaled.Height;
            var width = scaled.Width;

            var data = new Type[width, height];

            scaled.LockPixels();
            Parallel.For(0, height * width, y =>
            {
                var x = y % width;
                y /= width;

                var pixel = scaled.GetPixel(x, y);
                var mappedType = Type.Invalid;
                if (!_colorMapping.TryGetValue(pixel, out mappedType))
                {
                    mappedType = Type.Invalid;
                }
                data[x, y] = mappedType;
            });
            scaled.UnlockPixels();

            var biomes = new Biomes
            {
                Width = width,
                Height = height,
                BiomeMap = data,
                Name = filePath,
                Regions = ExtractRegions(data, width, height)
            };

            return biomes;
        }

        private static List<BiomeRegion> ExtractRegions(Type[,] data, int width, int height)
        {
            bool[,] visited = new bool[width, height];
            List<BiomeRegion> regions = new List<BiomeRegion>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[x, y] || data[x, y] == Type.Invalid)
                        continue;

                    Type currentType = data[x, y];
                    BiomeRegion region = new BiomeRegion { Type = currentType };
                    Queue<(int X, int Y)> queue = new Queue<(int X, int Y)>();
                    queue.Enqueue((x, y));
                    visited[x, y] = true;

                    while (queue.Count > 0)
                    {
                        (int cx, int cy) = queue.Dequeue();
                        region.Points.Add((cx, cy));

                        // Check 4-connected neighbors.
                        (int dx, int dy)[] directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };
                        foreach (var (dx, dy) in directions)
                        {
                            int nx = cx + dx;
                            int ny = cy + dy;
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height &&
                                !visited[nx, ny] && data[nx, ny] == currentType)
                            {
                                visited[nx, ny] = true;
                                queue.Enqueue((nx, ny));
                            }
                        }
                    }

                    regions.Add(region);
                }
            }

            return regions;
        }

        public Type GetBiomeType(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return Type.Invalid; // Out of bounds
            }
            return BiomeMap[x, y];
        }
    }
}
