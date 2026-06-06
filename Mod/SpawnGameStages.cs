using System.Collections.Generic;
using System.Xml.Linq;

namespace WalkerSim
{
    // The game parses spawning.xml into BiomeSpawnEntityGroupData but discards any
    // mings/maxgs attributes. We re-read the already merged and patched document and
    // keep the game stage range per spawn entry, keyed the same way the game identifies
    // the entry: biome name + the hash of the spawn id (BiomeSpawnEntityGroupData.idHash).
    internal static class SpawnGameStages
    {
        struct Range
        {
            public int Min;
            public int Max;
        }

        static readonly Dictionary<string, Range> _ranges = new Dictionary<string, Range>();

        static string MakeKey(string biomeName, int idHash) => biomeName + ":" + idHash;

        public static int Count => _ranges.Count;

        public static void Clear() => _ranges.Clear();

        public static bool TryGetRange(string biomeName, int idHash, out int min, out int max)
        {
            if (biomeName != null && _ranges.TryGetValue(MakeKey(biomeName, idHash), out var range))
            {
                min = range.Min;
                max = range.Max;
                return true;
            }

            // No range specified for this entry, leave it ungated.
            min = int.MinValue;
            max = int.MaxValue;
            return false;
        }

        // Reads the merged <spawning> document. Only entries that actually specify mings
        // or maxgs are recorded, everything else stays ungated via TryGetRange's defaults.
        public static void Parse(XElement spawningRoot)
        {
            _ranges.Clear();
            if (spawningRoot == null)
            {
                return;
            }

            foreach (var biome in spawningRoot.Elements("biome"))
            {
                var biomeName = biome.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(biomeName))
                {
                    continue;
                }

                foreach (var spawn in biome.Elements("spawn"))
                {
                    var minAttr = spawn.Attribute("mings");
                    var maxAttr = spawn.Attribute("maxgs");
                    if (minAttr == null && maxAttr == null)
                    {
                        continue;
                    }

                    var range = new Range { Min = int.MinValue, Max = int.MaxValue };
                    if (minAttr != null && int.TryParse(minAttr.Value, out var min))
                    {
                        range.Min = min;
                    }
                    if (maxAttr != null && int.TryParse(maxAttr.Value, out var max))
                    {
                        range.Max = max;
                    }

                    var idHash = (spawn.Attribute("id")?.Value ?? "").GetHashCode();
                    _ranges[MakeKey(biomeName, idHash)] = range;
                }
            }
        }
    }
}
