using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WalkerSim
{
    public class SpawnGroups
    {
        // NOTE: First entry is always empty, so that the first group is at index 1, 0 means nothing.
        List<SpawnGroup> _spawnGroups = new List<SpawnGroup>();

        // Index to _spawnGroups
        byte[,] _spawnMask = null;

        public class SpawnGroup
        {
            [XmlAttribute("Color")]
            public string ColorString { get; set; }

            [XmlAttribute("EntityGroupDay")]
            public string EntityGroupDay { get; set; }

            [XmlAttribute("EntityGroupNight")]
            public string EntityGroupNight { get; set; }

            public Drawing.Color Color { get { return Drawing.Color.FromHtml(ColorString); } }
        }

        [XmlRoot("SpawnGroups")]
        public class SpawnGroupsData
        {
            [XmlElement("SpawnGroup")]
            public List<SpawnGroup> Groups { get; set; }
        }

        public bool Load(string worldFolder, int worldSizeX, int worldSizeY)
        {
            var spawnGroupsFile = Path.Combine(worldFolder, "ws_spawngroups.xml");
            if (!File.Exists(spawnGroupsFile))
            {
                // No spawn groups file, nothing to do.
                return true;
            }

            var spawnMaskFile = Path.Combine(worldFolder, "ws_spawngroupsmask.png");
            if (!File.Exists(spawnMaskFile))
            {
                // No spawn mask file, nothing to do.
                return true;
            }

            var spawnGroups = LoadSpawnGroups(spawnGroupsFile);
            if (spawnGroups == null)
            {
                // Failed to load spawn groups, nothing to do.
                return false;
            }

            var spawnMask = LoadSpawnMask(spawnMaskFile, spawnGroups, worldSizeX, worldSizeY);
            if (spawnMask == null)
            {
                // Failed to load spawn mask, nothing to do.
                return false;
            }

            // Map the spawn groups.
            _spawnGroups.Clear();
            _spawnGroups.Add(null); // Index 0 is reserved for no spawn group.

            foreach (var group in spawnGroups)
            {
                _spawnGroups.Add(group);
            }

            _spawnMask = spawnMask;

            return true;
        }

        private List<SpawnGroup> LoadSpawnGroups(string spawnGroupsFile)
        {
            var res = new List<SpawnGroup>();
            try
            {
                var serializer = new XmlSerializer(typeof(SpawnGroupsData));
                using (var reader = new StreamReader(spawnGroupsFile))
                {
                    var data = (SpawnGroupsData)serializer.Deserialize(reader);
                    if (data == null || data.Groups == null)
                    {
                        return null;
                    }
                    return data.Groups;
                }
            }
            catch
            {
                return null;
            }
        }

        private byte[,] LoadSpawnMask(string maskFile, List<SpawnGroup> spawnGroups, int worldSizeX, int worldSizeY)
        {
            byte[,] spawnMask = new byte[worldSizeX, worldSizeY];

            var colorToIndex = new Dictionary<Drawing.Color, int>();
            for (int i = 0; i < spawnGroups.Count; i++)
            {
                if (spawnGroups[i] != null)
                {
                    // Convert the color string to a Drawing.Color
                    var color = Drawing.Color.FromHtml(spawnGroups[i].ColorString);

                    // NOTE: +1 because index 0 is reserved for no spawn group.
                    colorToIndex[color] = i + 1;
                }
            }

            using (var img = WalkerSim.Drawing.LoadFromFile(maskFile))
            {
                var scaledImg = img;
                if (img.Width != worldSizeX || img.Height != worldSizeY)
                {
                    scaledImg = Drawing.Create(img, worldSizeX, worldSizeY);
                }

                scaledImg.LockPixels();
                Parallel.For(0, worldSizeX * worldSizeY, pixelIndex =>
                {
                    int y = pixelIndex / worldSizeX;
                    int x = pixelIndex % worldSizeX;
                    var pixel = scaledImg.GetPixel(x, y);

                    if (pixel.A == 0)
                    {
                        spawnMask[x, y] = 0; // No spawn group
                    }
                    else
                    {
                        // Find the spawn group by color
                        int index = colorToIndex.TryGetValue(pixel, out index) ? index : 0;
                        spawnMask[x, y] = (byte)index;
                    }
                });

                scaledImg.UnlockPixels();
            }

            return spawnMask;
        }

        // NOTE: x, y must be remapped to the world size.
        public SpawnGroup GetSpawnGroup(int x, int y)
        {
            if (_spawnMask == null || x < 0 || y < 0 || x >= _spawnMask.GetLength(0) || y >= _spawnMask.GetLength(1))
            {
                return null;
            }
            var index = _spawnMask[x, y];
            if (index < 0 || index >= _spawnGroups.Count)
            {
                return null;
            }
            return _spawnGroups[index];
        }
    }
}
