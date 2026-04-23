using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace WalkerSim
{
    /// <summary>
    /// Global prefab database. Holds the complete name → footprint table for every
    /// prefab discoverable across all configured 7DTD installs and mods, regardless
    /// of which world is currently loaded. Initialized once at app/mod startup via
    /// <see cref="Initialize(System.Collections.Generic.IEnumerable{string})"/>; world
    /// loading then queries the singleton through <see cref="LoadDecorationsFromWorld"/>.
    /// </summary>
    public class Prefabs
    {
        // Fallback footprint for prefabs whose real size cannot be resolved.
        private const float DefaultPrefabSize = 20f;

        // Decorations smaller than this in either ground dimension are dropped.
        private const float MinPrefabGroundSize = 6f;

        public enum PrefabKind : byte
        {
            Building = 0,
            NavOnly = 1,
            BiomeOnly = 2,
            DevOnly = 3,
        }

        public readonly struct PrefabInfo
        {
            public readonly float SizeX;
            public readonly float SizeZ;
            public readonly PrefabKind Kind;

            public PrefabInfo(float sizeX, float sizeZ, PrefabKind kind)
            {
                SizeX = sizeX;
                SizeZ = sizeZ;
                Kind = kind;
            }

            public bool IsBuilding => Kind == PrefabKind.Building;
        }

        // Singleton — empty until Initialize is called.
        private static Prefabs _instance = new Prefabs();
        public static Prefabs Database => _instance;

        private readonly Dictionary<string, PrefabInfo> _entries =
            new Dictionary<string, PrefabInfo>(StringComparer.OrdinalIgnoreCase);

        public int Count => _entries.Count;

        public bool TryGetInfo(string name, out PrefabInfo info)
        {
            return _entries.TryGetValue(name, out info);
        }

        /// <summary>
        /// Builds a fresh global prefab database by scanning every search path
        /// for prefab xml files. Replaces any previously initialized database.
        /// First-match-wins on name collisions, so callers should pass paths in
        /// priority order (mod prefabs before vanilla).
        /// </summary>
        public static void Initialize(IEnumerable<string> searchPaths)
        {
            var fresh = new Prefabs();
            fresh.Build(searchPaths);
            _instance = fresh;
        }

        private void Build(IEnumerable<string> searchPaths)
        {
            int filesScanned = 0;
            int buildingCount = 0;
            int navOnlyCount = 0;
            int biomeOnlyCount = 0;
            int devOnlyCount = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var pathList = new List<string>();
            foreach (var p in searchPaths)
            {
                if (string.IsNullOrEmpty(p) || !Directory.Exists(p))
                    continue;
                pathList.Add(p);
            }

            if (pathList.Count == 0)
            {
                Logging.Warn("Prefab database initialized with no search paths — decorations will fall back to a {0}m default footprint.", DefaultPrefabSize);
                return;
            }

            Logging.Info("Building prefab database from {0} search path(s):", pathList.Count);
            using (Logging.Scope())
            {
                foreach (var p in pathList)
                    Logging.Info("{0}", p);
            }

            foreach (var path in pathList)
            {
                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories);
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed enumerating prefab xmls in '{0}': {1}", path, ex.Message);
                    continue;
                }

                foreach (var file in files)
                {
                    filesScanned++;
                    var name = Path.GetFileNameWithoutExtension(file);
                    if (_entries.ContainsKey(name))
                        continue;

                    if (TryReadPrefabInfo(file, out var info))
                    {
                        _entries[name] = info;
                        switch (info.Kind)
                        {
                            case PrefabKind.Building:
                                buildingCount++;
                                break;
                            case PrefabKind.NavOnly:
                                navOnlyCount++;
                                break;
                            case PrefabKind.BiomeOnly:
                                biomeOnlyCount++;
                                break;
                            case PrefabKind.DevOnly:
                                devOnlyCount++;
                                break;
                        }
                    }
                }
            }

            sw.Stop();
            Logging.Info("Prefab database built: {0} entries from {1} files in {2:F2}s ({3} buildings, {4} nav-only, {5} biome-only, {6} dev-only).",
                _entries.Count, filesScanned, sw.Elapsed.TotalSeconds,
                buildingCount, navOnlyCount, biomeOnlyCount, devOnlyCount);
        }

        /// <summary>
        /// Given a 7DTD install root, returns the prefab search folders 7DTD would use
        /// at runtime, in priority order (mod prefabs first, vanilla last). Used by
        /// editor and mod to construct the global database.
        /// </summary>
        public static List<string> SearchPathsForInstall(string installRoot)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(installRoot) || !Directory.Exists(installRoot))
                return result;

            // Mod prefabs first (priority over vanilla on collision).
            var modsRoot = Path.Combine(installRoot, "Mods");
            if (Directory.Exists(modsRoot))
            {
                try
                {
                    foreach (var modDir in Directory.EnumerateDirectories(modsRoot))
                    {
                        var modPrefabs = Path.Combine(modDir, "Prefabs");
                        if (Directory.Exists(modPrefabs))
                            result.Add(modPrefabs);
                    }
                }
                catch { }
            }

            // Vanilla prefabs.
            var dataPrefabs = Path.Combine(installRoot, "Data", "Prefabs");
            if (Directory.Exists(dataPrefabs))
                result.Add(dataPrefabs);

            return result;
        }

        /// <summary>
        /// Given an arbitrary path inside a 7DTD install (e.g. the running mod's assembly
        /// location), walks up to locate the install root — the first ancestor that
        /// contains both a "Data" and a "Mods" subdirectory. Returns null on failure.
        /// </summary>
        public static string GuessInstallRoot(string startPath)
        {
            if (string.IsNullOrEmpty(startPath))
                return null;

            try
            {
                var dir = new DirectoryInfo(startPath);
                while (dir != null)
                {
                    if (Directory.Exists(Path.Combine(dir.FullName, "Data")) &&
                        Directory.Exists(Path.Combine(dir.FullName, "Mods")))
                        return dir.FullName;
                    dir = dir.Parent;
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Loads decorations from &lt;worldFolder&gt;/prefabs.xml and resolves each entry's
        /// ground-plane footprint via the global prefab database. Non-building prefabs
        /// (signs, biome decoration, dev-only) are dropped here so connected-component
        /// city detection only sees real POIs.
        /// </summary>
        public static MapData.PrefabsData LoadDecorationsFromWorld(string worldFolder)
        {
            var res = new MapData.PrefabsData { Decorations = new MapData.Decoration[0] };

            var prefabsXmlPath = Path.Combine(worldFolder, "prefabs.xml");
            if (!File.Exists(prefabsXmlPath))
            {
                Logging.Warn("No prefabs.xml found in '{0}'.", worldFolder);
                return res;
            }

            var database = Database;
            if (database.Count == 0)
            {
                Logging.Warn("Prefab database is empty — call Prefabs.Initialize before loading worlds. Decorations will use the {0}m default footprint.",
                    DefaultPrefabSize);
            }

            int missingSize = 0;
            int droppedSmall = 0;
            int droppedNonBuilding = 0;

            try
            {
                var doc = new XmlDocument();
                doc.Load(prefabsXmlPath);

                var decorations = new List<MapData.Decoration>();
                foreach (XmlNode el in doc.DocumentElement.GetElementsByTagName("decoration"))
                {
                    var dec = new MapData.Decoration();
                    dec.Type = el.Attributes["type"]?.Value;
                    dec.Name = el.Attributes["name"]?.Value;

                    var posAttr = el.Attributes["position"]?.Value;
                    if (posAttr != null)
                        dec.PositionString = posAttr;

                    var rotAttr = el.Attributes["rotation"]?.Value;
                    if (rotAttr != null && int.TryParse(rotAttr, out int rot))
                        dec.Rotation = rot;

                    var yAttr = el.Attributes["y_is_groundlevel"]?.Value;
                    dec.YIsGroundlevel = yAttr != null && bool.TryParse(yAttr, out bool yVal) && yVal;

                    float sx, sz;
                    if (dec.Name != null && database.TryGetInfo(dec.Name, out var info))
                    {
                        if (!info.IsBuilding)
                        {
                            droppedNonBuilding++;
                            continue;
                        }
                        sx = info.SizeX;
                        sz = info.SizeZ;
                    }
                    else
                    {
                        sx = DefaultPrefabSize;
                        sz = DefaultPrefabSize;
                        missingSize++;
                    }

                    if ((dec.Rotation & 1) != 0)
                        (sx, sz) = (sz, sx);

                    if (sx < MinPrefabGroundSize || sz < MinPrefabGroundSize)
                    {
                        droppedSmall++;
                        continue;
                    }

                    dec.Bounds = new Vector3(sx, sz, 0);
                    decorations.Add(dec);
                }

                // Drop POIs that are fully contained within a larger POI.
                // 7DTD worlds place large city-block prefabs AND the individual
                // buildings inside them; both show up as decorations, producing
                // POIs-inside-POIs in the simulation. Keep only the outermost.
                int droppedNested = FilterContainedDecorations(decorations);

                res.Decorations = decorations.ToArray();

                Logging.Info("Resolved {0} decorations from '{1}' ({2} unresolved, {3} dropped as non-building, {4} dropped as too small, {5} dropped as nested).",
                    res.Decorations.Length,
                    prefabsXmlPath,
                    missingSize,
                    droppedNonBuilding,
                    droppedSmall,
                    droppedNested);
            }
            catch (Exception ex)
            {
                Logging.Err("Failed to load prefabs from '{0}'.", prefabsXmlPath);
                Logging.Exception(ex);
                res.Decorations = new MapData.Decoration[0];
            }

            GC.Collect();
            return res;
        }

        /// <summary>
        /// Reads PrefabSize and Zoning out of a prefab xml using a forward-only XmlReader
        /// (much faster than XmlDocument when scanning thousands of files). 7DTD's
        /// coordinate system has Y as vertical height, so WalkerSim's ground bounds come
        /// from components 0 and 2.
        /// </summary>
        // Removes decorations whose AABB footprint is fully contained within
        // the AABB of a larger decoration. Sorts by descending area so each
        // POI is only tested against ones that could potentially contain it.
        // Position is treated as the prefab's center and Bounds as full extent
        // (matching Cities.cs rasterization).
        private static int FilterContainedDecorations(List<MapData.Decoration> decorations)
        {
            int n = decorations.Count;
            if (n < 2)
                return 0;

            // Sort by area descending — largest first.
            decorations.Sort((a, b) =>
            {
                float aa = a.Bounds.X * a.Bounds.Y;
                float ba = b.Bounds.X * b.Bounds.Y;
                return ba.CompareTo(aa);
            });

            // Precompute AABBs.
            var minX = new float[n];
            var maxX = new float[n];
            var minY = new float[n];
            var maxY = new float[n];
            for (int i = 0; i < n; i++)
            {
                var d = decorations[i];
                float hx = d.Bounds.X * 0.5f;
                float hy = d.Bounds.Y * 0.5f;
                minX[i] = d.Position.X - hx;
                maxX[i] = d.Position.X + hx;
                minY[i] = d.Position.Y - hy;
                maxY[i] = d.Position.Y + hy;
            }

            // Mark contained decorations. Compare each POI only against strictly
            // larger ones that have been kept (earlier in the sorted list).
            var drop = new bool[n];
            for (int i = 1; i < n; i++)
            {
                float ix0 = minX[i], ix1 = maxX[i], iy0 = minY[i], iy1 = maxY[i];
                for (int j = 0; j < i; j++)
                {
                    if (drop[j])
                        continue;
                    if (ix0 >= minX[j] && ix1 <= maxX[j] &&
                        iy0 >= minY[j] && iy1 <= maxY[j])
                    {
                        drop[i] = true;
                        break;
                    }
                }
            }

            int dropped = 0;
            for (int i = n - 1; i >= 0; i--)
            {
                if (drop[i])
                {
                    decorations.RemoveAt(i);
                    dropped++;
                }
            }
            return dropped;
        }

        private static bool TryReadPrefabInfo(string xmlPath, out PrefabInfo info)
        {
            info = default;
            try
            {
                var settings = new XmlReaderSettings
                {
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true,
                    DtdProcessing = DtdProcessing.Ignore,
                };

                float sizeX = 0, sizeZ = 0;
                bool sizeFound = false;
                var kind = PrefabKind.Building;

                using (var reader = XmlReader.Create(xmlPath, settings))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element || reader.Name != "property")
                            continue;

                        var propName = reader.GetAttribute("name");
                        if (propName == null)
                            continue;

                        if (propName == "PrefabSize")
                        {
                            var value = reader.GetAttribute("value");
                            if (value == null)
                                continue;
                            var parts = value.Split(',');
                            if (parts.Length != 3)
                                continue;
                            sizeX = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                            sizeZ = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                            sizeFound = true;
                        }
                        else if (propName == "Zoning")
                        {
                            var value = reader.GetAttribute("value");
                            if (value != null)
                                kind = ClassifyZoning(value);
                        }

                        if (sizeFound && kind != PrefabKind.Building)
                            break;
                    }
                }

                if (!sizeFound)
                    return false;

                info = new PrefabInfo(sizeX, sizeZ, kind);
                return true;
            }
            catch { }
            return false;
        }

        private static PrefabKind ClassifyZoning(string zoning)
        {
            bool sawNavOnly = false;
            bool sawBiomeOnly = false;
            bool sawDevOnly = false;
            bool sawBuildingZone = false;

            foreach (var rawToken in zoning.Split(','))
            {
                var token = rawToken.Trim();
                if (token.Length == 0)
                    continue;

                if (string.Equals(token, "NavOnly", StringComparison.OrdinalIgnoreCase))
                    sawNavOnly = true;
                else if (string.Equals(token, "BiomeOnly", StringComparison.OrdinalIgnoreCase))
                    sawBiomeOnly = true;
                else if (string.Equals(token, "DevOnly", StringComparison.OrdinalIgnoreCase))
                    sawDevOnly = true;
                else
                    sawBuildingZone = true;
            }

            if (sawBuildingZone)
                return PrefabKind.Building;
            if (sawNavOnly)
                return PrefabKind.NavOnly;
            if (sawBiomeOnly)
                return PrefabKind.BiomeOnly;
            if (sawDevOnly)
                return PrefabKind.DevOnly;
            return PrefabKind.Building;
        }
    }
}
