using System.Drawing;

namespace WalkerSim
{
    internal class MapData
    {
        public class MapInfo
        {
            public string Name;
            public string Description;
            public string Modes;
            public int HeightMapWidth;
            public int HeightMapHeight;
        }

        private Roads _roads;
        private MapInfo _info;

        public Roads Roads
        {
            get
            {
                return _roads;
            }
        }

        public MapInfo Info
        {
            get
            {
                return _info;
            }
        }

        private static MapInfo ParseInfo(string path)
        {
            var fileData = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
            if (fileData == null)
            {
                return null;
            }

            var info = new MapInfo();

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

            return info;
        }

        private static Roads LoadRoadSplat(string folderPath)
        {
            var splatPath = System.IO.Path.Combine(folderPath, "splat3_half.png");
            if (!System.IO.File.Exists(splatPath))
            {
                splatPath = System.IO.Path.Combine(folderPath, "splat3_processed.png");
            }
            if (!System.IO.File.Exists(splatPath))
            {
                splatPath = System.IO.Path.Combine(folderPath, "splat3.png");
            }
            if (!System.IO.File.Exists(splatPath))
            {
                return null;
            }

            var img = Image.FromFile(splatPath);
            ImageUtils.RemoveTransparency((Bitmap)img);

            return Roads.LoadFromBitmap((Bitmap)img);
        }

        public static MapData LoadFromFolder(string folderPath)
        {
            // Parse map_info.xml
            var mapInfoPath = System.IO.Path.Combine(folderPath, "map_info.xml");
            var info = ParseInfo(mapInfoPath);
            if (info == null)
            {
                return null;
            }

            var roads = LoadRoadSplat(folderPath);
            if (roads == null)
            {
                return null;
            }

            var res = new MapData();

            res._info = info;
            res._roads = roads;

            return res;
        }

        public static MapData Load()
        {
            var mapData = LoadFromFolder("Data/Worlds/RandomGen/Preview");

            return mapData;
        }
    }
}
