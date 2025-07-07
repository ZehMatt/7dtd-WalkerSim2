using System;
using System.Collections.Generic;
using System.Xml.Serialization;

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

        [XmlRoot("prefabs")]
        public class PrefabsData
        {
            [XmlElement("decoration")]
            public Decoration[] Decorations;
        }

        public class Decoration
        {
            [XmlAttribute("type")]
            public string Type;

            [XmlAttribute("name")]
            public string Name;

            [XmlIgnore]
            public Vector3 Position;

            [XmlIgnore]
            public Vector3 Bounds = new Vector3(64, 64);

            [XmlAttribute("position")]
            public string PositionString
            {
                get => Position.ToString();
                set
                {
                    Position = Vector3.Parse(value, true);
                }
            }

            [XmlAttribute("rotation")]
            public int Rotation;

            [XmlAttribute("y_is_groundlevel")]
            public bool YIsGroundlevel;
        }

        public Roads Roads { get; private set; }

        public Biomes Biomes { get; private set; }

        public MapInfo Info { get; private set; }

        public PrefabsData Prefabs { get; private set; }

        public Vector3 WorldSize { get; private set; }

        public Vector3 WorldMins { get; private set; }

        public Vector3 WorldMaxs { get; private set; }

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

            GC.Collect();

            return res;
        }

        private static PrefabsData LoadPrefabs(string folderPath)
        {
            PrefabsData res = null;
            var filePath = System.IO.Path.Combine(folderPath, "prefabs.xml");
            var serializer = new XmlSerializer(typeof(PrefabsData));
            try
            {
                using (var reader = new System.IO.StreamReader(filePath))
                {
                    res = (PrefabsData)serializer.Deserialize(reader);
                }
            }
            catch (System.Exception)
            {
            }

            if (res.Decorations == null)
            {
                res.Decorations = new Decoration[0];
            }
            else
            {
                MergePrefabs(res);
            }

            GC.Collect();

            return res;
        }

        private static void MergePrefabs(PrefabsData prefabs)
        {
            if (prefabs.Decorations == null)
            {
                return;
            }

            var newList = new List<Decoration>(prefabs.Decorations);

            bool invalidated = true;
            while (invalidated)
            {
                invalidated = false;

                for (int i = 0; i < newList.Count; i++)
                {
                    Vector3 pos = newList[i].Position;
                    // z is unused, this is 2D.
                    Vector3 bounds = newList[i].Bounds;

                    for (int j = i + 1; j < newList.Count;)
                    {
                        // Check for intersecting bounds.
                        var other = newList[j];
                        Vector3 otherPos = other.Position;
                        Vector3 otherBounds = other.Bounds;

                        // Check if bounds intersect in 2D (ignoring z).
                        bool intersects = System.Math.Abs(pos.X - otherPos.X) < (bounds.X + otherBounds.X) / 2 &&
                                        System.Math.Abs(pos.Y - otherPos.Y) < (bounds.Y + otherBounds.Y) / 2;

                        if (intersects)
                        {
                            // Calculate new merged bounds
                            float minX = System.Math.Min(pos.X - bounds.X / 2, otherPos.X - otherBounds.X / 2);
                            float maxX = System.Math.Max(pos.X + bounds.X / 2, otherPos.X + otherBounds.X / 2);
                            float minY = System.Math.Min(pos.Y - bounds.Y / 2, otherPos.Y - otherBounds.Y / 2);
                            float maxY = System.Math.Max(pos.Y + bounds.Y / 2, otherPos.Y + otherBounds.Y / 2);

                            // Update current decoration's position and bounds
                            newList[i].Position = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, pos.Z);
                            newList[i].Bounds = new Vector3(maxX - minX, maxY - minY, bounds.Z);

                            // Remove the other decoration
                            newList.RemoveAt(j);
                            invalidated = true;
                        }
                        else
                        {
                            j++;
                        }
                    }
                }
            }

            prefabs.Decorations = newList.ToArray();
        }

        private static Vector3 GetWorldSize(string folderPath)
        {
            // The only way to tell at the moment is to use the file size of dtm.raw.
            var dtmFile = System.IO.Path.Combine(folderPath, "dtm.raw");
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

        public static MapData LoadFromFolder(string folderPath)
        {
            // Parse map_info.xml         
            var mapInfoPath = System.IO.Path.Combine(folderPath, "map_info.xml");
            var mapInfo = LoadMapInfo(mapInfoPath);

            var roads = LoadRoadSplat(folderPath);
            var biomes = LoadBiomes(folderPath);
            var prefabs = LoadPrefabs(folderPath);

            var worldSize = GetWorldSize(folderPath);
            var sizeX = worldSize.X / 2;
            var sizeY = worldSize.Y / 2;
            var sizeZ = worldSize.Z;

            var res = new MapData();
            res.Info = mapInfo;
            res.Roads = roads;
            res.Prefabs = prefabs;
            res.Biomes = biomes;
            res.WorldSize = worldSize;
            res.WorldMins = new Vector3(-sizeX, -sizeY, 0);
            res.WorldMaxs = new Vector3(sizeX, sizeY, sizeZ);

            return res;
        }
    }
}
