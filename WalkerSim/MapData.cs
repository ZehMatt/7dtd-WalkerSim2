using System;

namespace WalkerSim
{
    public class MapData
    {
        public class MapInfo
        {
            public string Name;
            public string Description;
            public string Modes;
            public int HeightMapWidth;
            public int HeightMapHeight;
        }

        public class PrefabsData
        {
            public Decoration[] Decorations;
        }

        public class Decoration
        {
            public string Type;
            public string Name;
            public Vector3 Position;
            public Vector3 Bounds = new Vector3(64, 64);

            public string PositionString
            {
                get { return Position.ToString(); }
                set { Position = Vector3.Parse(value, true); }
            }

            public int Rotation;
            public bool YIsGroundlevel;
        }

        public Roads Roads { get; private set; }

        public Biomes Biomes { get; private set; }

        public MapInfo Info { get; private set; }

        public PrefabsData Prefabs { get; private set; }

        public Vector3 WorldSize { get; private set; }

        public Vector3 WorldMins { get; private set; }

        public Vector3 WorldMaxs { get; private set; }

        public SpawnGroups SpawnGroups { get; private set; }

        public Cities Cities { get; private set; }

        private static MapInfo LoadMapInfo(string path)
        {
            var info = new MapInfo();

            try
            {
                if (System.IO.File.Exists(path))
                {
                    var fileData = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
                    if (fileData == null)
                    {
                        return null;
                    }

                    var doc = new System.Xml.XmlDocument();
                    doc.LoadXml(fileData);

                    var root = doc.DocumentElement;
                    if (root.Name != "MapInfo")
                    {
                        return null;
                    }

                    // Read <propert name="X" value="Y" />
                    foreach (System.Xml.XmlNode node in root.ChildNodes)
                    {
                        if (node.Name == "property")
                        {
                            var name = node.Attributes.GetNamedItem("name").Value;
                            var value = node.Attributes.GetNamedItem("value").Value;

                            if (name == "Name")
                            {
                                info.Name = value;
                            }
                            else if (name == "Description")
                            {
                                info.Description = value;
                            }
                            else if (name == "Modes")
                            {
                                info.Modes = value;
                            }
                            else if (name == "HeightMapSize")
                            {
                                var split = value.Split(',');
                                if (split.Length != 2)
                                {
                                    return null;
                                }

                                info.HeightMapWidth = int.Parse(split[0]);
                                info.HeightMapHeight = int.Parse(split[1]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Err("Failed to load map info from '{0}'.", path);
                Logging.Exception(ex);
                return null;
            }

            GC.Collect();

            return info;
        }

        private static Roads LoadRoadSplat(string folderPath)
        {
            Roads res = null;
            var splatPath = string.Empty;
            try
            {
                splatPath = System.IO.Path.Combine(folderPath, "splat3_half.png");
                if (!System.IO.File.Exists(splatPath))
                {
                    splatPath = System.IO.Path.Combine(folderPath, "splat3_processed.png");
                }
                if (!System.IO.File.Exists(splatPath))
                {
                    splatPath = System.IO.Path.Combine(folderPath, "splat3.png");
                }

                if (System.IO.File.Exists(splatPath))
                {
                    res = Roads.LoadFromFile(splatPath);
                }
            }
            catch (Exception ex)
            {
                Logging.Err("Failed to load roads from '{0}'.", splatPath);
                Logging.Exception(ex);
            }

            if (res == null)
            {
                res = new Roads();
            }

            GC.Collect();

            return res;
        }

        private static Biomes LoadBiomes(string folderPath)
        {
            Biomes res = null;

            var biomePath = System.IO.Path.Combine(folderPath, "biomes.png");
            if (System.IO.File.Exists(biomePath))
            {
                res = Biomes.LoadFromFile(biomePath);
            }
            if (res == null)
            {
                res = new Biomes();
            }

            GC.Collect();

            return res;
        }

        private static Vector3 GetWorldSize(string folderPath)
        {
            // The only way to tell at the moment is to use the file size of dtm.raw.
            var dtmFile = System.IO.Path.Combine(folderPath, "dtm.raw");
            if (!System.IO.File.Exists(dtmFile))
            {
                Logging.Err("DTM file '{0}' does not exist.", dtmFile);
                return Vector3.Zero;
            }

            try
            {
                var fileInfo = new System.IO.FileInfo(dtmFile);
                var fileSize = fileInfo.Length;
                var worldSize = System.Math.Sqrt(fileSize / 2);
                return new Vector3((float)worldSize, (float)worldSize, 256);
            }
            catch (Exception ex)
            {
                Logging.Err("Failed to obtain file size of '{0}'.", dtmFile);
                Logging.Exception(ex);
                return Vector3.Zero;
            }
        }

        public static SpawnGroups LoadSpawnGroups(string folderPath, Vector3 worldSize)
        {
            var spawnGroups = new SpawnGroups();

            spawnGroups.Load(folderPath, (int)worldSize.X, (int)worldSize.Y);

            return spawnGroups;
        }

        public static MapData LoadFromFolder(string folderPath)
        {
            Logging.Info("Loading map data from folder '{0}'...", folderPath);

            var timeWatch = new System.Diagnostics.Stopwatch();
            timeWatch.Start();

            var res = new MapData();

            using (Logging.Scope())
            {

                var mapInfoPath = System.IO.Path.Combine(folderPath, "map_info.xml");
                var mapInfo = LoadMapInfo(mapInfoPath);

                var roads = LoadRoadSplat(folderPath);
                var biomes = LoadBiomes(folderPath);
                var prefabs = WalkerSim.Prefabs.LoadDecorationsFromWorld(folderPath);

                var worldSize = GetWorldSize(folderPath);
                var sizeX = worldSize.X / 2;
                var sizeY = worldSize.Y / 2;
                var sizeZ = worldSize.Z;

                var spawnGroups = LoadSpawnGroups(folderPath, worldSize);

                var worldMins = new Vector3(-sizeX, -sizeY, 0);
                var worldMaxs = new Vector3(sizeX, sizeY, sizeZ);

                // Generate city regions by rasterizing POIs onto a grid and running
                // connected-component labelling. Produces non-rectangular shapes.
                var cities = Cities.GenerateFromPOIs(prefabs.Decorations, worldMins, worldMaxs);

                res.Info = mapInfo;
                res.Roads = roads;
                res.Prefabs = prefabs;
                res.Biomes = biomes;
                res.WorldSize = worldSize;
                res.WorldMins = worldMins;
                res.WorldMaxs = worldMaxs;
                res.SpawnGroups = spawnGroups;
                res.Cities = cities;
            }

            timeWatch.Stop();
            var elapsed = timeWatch.Elapsed.TotalSeconds;

            Logging.Info("Finished loading map data in {0}s.", elapsed);

            return res;
        }
    }
}
