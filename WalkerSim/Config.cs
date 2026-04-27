using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace WalkerSim
{
    public class Config
    {
        public enum WanderingSpeed
        {
            NoOverride = 0,
            Walk,
            Jog,
            Run,
            Sprint,
            Nightmare,
        }

        public enum PostSpawnBehavior
        {
            Wander = 0,
            ChaseActivator,
            Nothing,
        }

        public enum MapEdgeBehavior
        {
            Warp = 0,
            Bounce,
            Clamp,
        }

        public enum WorldLocation
        {
            None = 0,
            RandomBorderLocation,
            RandomLocation,
            RandomPOI,
            RandomCity,
            Mixed,
        }

        public enum MovementProcessorType
        {
            Invalid = 0,
            FlockAnyGroup,
            AlignAnyGroup,
            AvoidAnyGroup,
            FlockSameGroup,
            AlignSameGroup,
            AvoidSameGroup,
            FlockOtherGroup,
            AlignOtherGroup,
            AvoidOtherGroup,
            Wind,
            WindInverted,
            StickToRoads,
            AvoidRoads,
            StickToPOIs,
            AvoidPOIs,
            WorldEvents,
            PreferCities,
            AvoidCities,
            CityVisitor,
            StickToBiome,
            AvoidBiome,
        }

        public class MovementProcessor
        {
            public MovementProcessorType Type;
            public float Distance = 0.0f;
            public float Power = 0.0f;
            public float Param1 = 0.0f;
            public float Param2 = 0.0f;
        }

        public class MovementProcessorGroup
        {
            public string Name = "";
            public float Weight = 1.0f;
            public float SpeedScale = 1.0f;
            public PostSpawnBehavior PostSpawnBehavior = PostSpawnBehavior.Wander;
            public WanderingSpeed PostSpawnWanderSpeed = WanderingSpeed.Walk;
            public MapEdgeBehavior MapEdgeBehavior = MapEdgeBehavior.Warp;
            public string Color = "";
            public List<MovementProcessor> Entries = new List<MovementProcessor>();
        }

        public class LoggingOptions
        {
            public bool General = true;
            public bool Spawns = false;
            public bool Despawns = false;
            public bool EntityClassSelection = false;
            public bool Events = false;
        }

        public const int CurrentVersion = 2;

        public int Version = CurrentVersion;
        public LoggingOptions LoggingOpts;
        public int RandomSeed = 1337;
        public int PopulationDensity = 300;
        public int SpawnActivationRadius = 96;
        public bool StartAgentsGrouped = true;
        public bool EnhancedSoundAwareness = true;
        public float SoundDistanceScale = 1.0f;
        public int GroupSize = 200;
        public WorldLocation StartPosition = WorldLocation.RandomLocation;
        public WorldLocation RespawnPosition = WorldLocation.None;
        public bool PauseDuringBloodmoon = true;
        public uint SpawnProtectionTime = 300;
        public bool InfiniteZombieLifetime = false;
        public string MaxSpawnedZombies = "75%";
        public float PopulationStartPercent = 100.0f;
        public int FullPopulationAtDay = 1;
        public List<MovementProcessorGroup> Processors;

        private static void SanitizeConfig(Config config)
        {
            if (config.PopulationDensity < Simulation.Limits.MinDensity ||
                config.PopulationDensity > Simulation.Limits.MaxDensity)
            {
                Logging.Warn("Invalid value for PopulationDensity (Min: {0}, Max: {1}), clamping.",
                    Simulation.Limits.MinDensity,
                    Simulation.Limits.MaxDensity);

                config.PopulationDensity = MathEx.Clamp(config.PopulationDensity,
                    Simulation.Limits.MinDensity,
                    Simulation.Limits.MaxDensity);
            }

            if (config.LoggingOpts == null)
            {
                config.LoggingOpts = new LoggingOptions();
            }

            if (config.Processors == null)
            {
                config.Processors = new List<MovementProcessorGroup>();
            }

            foreach (var proc in config.Processors)
            {
                if (string.IsNullOrEmpty(proc.Color))
                    proc.Color = "#FF00FF";

                foreach (var entry in proc.Entries)
                {
                    if (entry.Type == MovementProcessorType.CityVisitor &&
                        entry.Param1 == 0f && entry.Param2 == 0f)
                    {
                        entry.Param1 = 20f;
                        entry.Param2 = 35f;
                    }
                }
            }
        }

        public static Config LoadFromFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                return LoadFromStream(reader);
            }
        }

        public static Config LoadFromText(string text)
        {
            using (var reader = new StringReader(text))
            {
                return LoadFromStream(reader);
            }
        }

        public static Config LoadFromStream(TextReader reader)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(reader);

                var nsMgr = new XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("ws", "http://zeh.matt/WalkerSim");

                var root = doc.DocumentElement;
                var config = new Config();

                config.Version = ReadAttrInt(root, "Version", 0);

                // Logging
                var loggingNode = root.SelectSingleNode("ws:Logging", nsMgr);
                if (loggingNode != null)
                {
                    config.LoggingOpts = new LoggingOptions();
                    config.LoggingOpts.General = ReadBool(loggingNode, "ws:General", nsMgr, true);
                    config.LoggingOpts.Spawns = ReadBool(loggingNode, "ws:Spawns", nsMgr, false);
                    config.LoggingOpts.Despawns = ReadBool(loggingNode, "ws:Despawns", nsMgr, false);
                    config.LoggingOpts.EntityClassSelection = ReadBool(loggingNode, "ws:EntityClassSelection", nsMgr, false);
                    config.LoggingOpts.Events = ReadBool(loggingNode, "ws:Events", nsMgr, false);
                }

                config.RandomSeed = ReadInt(root, "ws:RandomSeed", nsMgr, 1337);
                config.PopulationDensity = ReadInt(root, "ws:PopulationDensity", nsMgr, 300);
                config.SpawnActivationRadius = ReadInt(root, "ws:SpawnActivationRadius", nsMgr, 96);
                config.StartAgentsGrouped = ReadBool(root, "ws:StartAgentsGrouped", nsMgr, true);
                config.EnhancedSoundAwareness = ReadBool(root, "ws:EnhancedSoundAwareness", nsMgr, true);
                config.SoundDistanceScale = ReadFloat(root, "ws:SoundDistanceScale", nsMgr, 1.0f);
                config.GroupSize = ReadInt(root, "ws:GroupSize", nsMgr, 200);
                config.StartPosition = ReadEnum(root, "ws:AgentStartPosition", nsMgr, WorldLocation.RandomLocation);
                config.RespawnPosition = ReadEnum(root, "ws:AgentRespawnPosition", nsMgr, WorldLocation.None);
                config.PauseDuringBloodmoon = ReadBool(root, "ws:PauseDuringBloodmoon", nsMgr, true);
                config.SpawnProtectionTime = ReadUInt(root, "ws:SpawnProtectionTime", nsMgr, 300);
                config.InfiniteZombieLifetime = ReadBool(root, "ws:InfiniteZombieLifetime", nsMgr, false);
                config.MaxSpawnedZombies = ReadString(root, "ws:MaxSpawnedZombies", nsMgr, "75%");
                config.PopulationStartPercent = ReadFloat(root, "ws:PopulationStartPercent", nsMgr, 100.0f);
                config.FullPopulationAtDay = ReadInt(root, "ws:FullPopulationAtDay", nsMgr,
                    ReadInt(root, "ws:PopulationFullDay", nsMgr, 1));

                // Systems
                var processorsNode = root.SelectSingleNode("ws:Systems", nsMgr);
                if (processorsNode != null)
                {
                    config.Processors = new List<MovementProcessorGroup>();
                    foreach (XmlNode groupNode in processorsNode.SelectNodes("ws:System", nsMgr))
                    {
                        var group = new MovementProcessorGroup();
                        group.Name = ReadAttrString(groupNode, "Name", "");
                        group.Weight = ReadAttrFloat(groupNode, "Weight", 1.0f);
                        group.SpeedScale = ReadAttrFloat(groupNode, "SpeedScale", 1.0f);
                        group.PostSpawnBehavior = ReadAttrEnum(groupNode, "PostSpawnBehavior", PostSpawnBehavior.Wander);
                        group.PostSpawnWanderSpeed = ReadAttrEnum(groupNode, "PostSpawnWanderSpeed", WanderingSpeed.Walk);
                        group.MapEdgeBehavior = ReadAttrEnum(groupNode, "MapEdgeBehavior", MapEdgeBehavior.Warp);
                        group.Color = ReadAttrString(groupNode, "Color", "");

                        group.Entries = new List<MovementProcessor>();
                        foreach (XmlNode procNode in groupNode.SelectNodes("ws:Processor", nsMgr))
                        {
                            var proc = new MovementProcessor();
                            proc.Type = ReadAttrEnum(procNode, "Type", MovementProcessorType.Invalid);
                            proc.Distance = ReadAttrFloat(procNode, "Distance", 0.0f);
                            proc.Power = ReadAttrFloat(procNode, "Power", 0.0f);
                            proc.Param1 = ReadAttrFloat(procNode, "Param1", 0.0f);
                            proc.Param2 = ReadAttrFloat(procNode, "Param2", 0.0f);
                            group.Entries.Add(proc);
                        }

                        config.Processors.Add(group);
                    }
                }

                SanitizeConfig(config);
                return config;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
                return null;
            }
        }

        public static Config GetDefault()
        {
            var conf = new Config()
            {
                LoggingOpts = new LoggingOptions
                {
                    General = true,
                    Spawns = false,
                    Despawns = false,
                    EntityClassSelection = false,
                    Events = false,
                },
                RandomSeed = 1337,
                PopulationDensity = 300,
                SpawnActivationRadius = 96,
                GroupSize = 32,
                StartPosition = WorldLocation.RandomLocation,
                RespawnPosition = WorldLocation.RandomBorderLocation,
                StartAgentsGrouped = true,
                EnhancedSoundAwareness = true,
                SoundDistanceScale = 1.0f,
                PauseDuringBloodmoon = true,
                SpawnProtectionTime = 300,
                InfiniteZombieLifetime = false,
                MaxSpawnedZombies = "75%",
                PopulationStartPercent = 100.0f,
                FullPopulationAtDay = 1,
                Processors = new List<MovementProcessorGroup>
                {
                    new MovementProcessorGroup {
                        Weight = 1.0f,
                        SpeedScale = 1.0f,
                        Entries = new List<MovementProcessor> {
                            new MovementProcessor()
                            {
                                Type = MovementProcessorType.FlockAnyGroup,
                                Distance = 50f,
                                Power = 0.0001f,
                            },
                            new MovementProcessor()
                            {
                                Type = MovementProcessorType.AlignAnyGroup,
                                Distance = 50f,
                                Power = 0.0001f,
                            },
                            new MovementProcessor()
                            {
                                Type = MovementProcessorType.AvoidAnyGroup,
                                Distance = 30f,
                                Power = 0.0002f,
                            },
                            new MovementProcessor()
                            {
                                Type = MovementProcessorType.Wind,
                                Distance = 0f,
                                Power = 0.001f,
                            },
                            new MovementProcessor()
                            {
                                Type = MovementProcessorType.StickToRoads,
                                Distance = 0f,
                                Power = 0.0025f,
                            },
                            new MovementProcessor()
                            {
                                Type = MovementProcessorType.WorldEvents,
                                Distance = 0f,
                                Power = 0.0050f,
                            },
                        }
                    },
                },
            };
            return conf;
        }

        public bool Compare(Config other)
        {
            // Not the most efficient way to compare but it avoids having to update logic for all changes.
            string thisXml;
            using (var writer = new StringWriter())
            {
                Export(writer);
                thisXml = writer.ToString();
            }

            string otherXml;
            using (var writer = new StringWriter())
            {
                other.Export(writer);
                otherXml = writer.ToString();
            }

            return thisXml == otherXml;
        }

        public void Export(TextWriter writer)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = false,
            };

            using (var xw = XmlWriter.Create(writer, settings))
            {
                xw.WriteStartDocument();
                xw.WriteStartElement("WalkerSim", "http://zeh.matt/WalkerSim");
                xw.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                xw.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                xw.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance",
                    "http://zeh.matt/WalkerSim WalkerSimSchema.xsd");
                xw.WriteAttributeString("Version", XmlConvert.ToString(CurrentVersion));

                // Logging
                if (LoggingOpts != null)
                {
                    xw.WriteStartElement("Logging");
                    WriteElement(xw, "General", XmlConvert.ToString(LoggingOpts.General));
                    WriteElement(xw, "Spawns", XmlConvert.ToString(LoggingOpts.Spawns));
                    WriteElement(xw, "Despawns", XmlConvert.ToString(LoggingOpts.Despawns));
                    WriteElement(xw, "EntityClassSelection", XmlConvert.ToString(LoggingOpts.EntityClassSelection));
                    WriteElement(xw, "Events", XmlConvert.ToString(LoggingOpts.Events));
                    xw.WriteEndElement();
                }

                WriteElement(xw, "RandomSeed", XmlConvert.ToString(RandomSeed));
                WriteElement(xw, "PopulationDensity", XmlConvert.ToString(PopulationDensity));
                WriteElement(xw, "SpawnActivationRadius", XmlConvert.ToString(SpawnActivationRadius));
                WriteElement(xw, "StartAgentsGrouped", XmlConvert.ToString(StartAgentsGrouped));
                WriteElement(xw, "EnhancedSoundAwareness", XmlConvert.ToString(EnhancedSoundAwareness));
                WriteElement(xw, "SoundDistanceScale", XmlConvert.ToString(SoundDistanceScale));
                WriteElement(xw, "GroupSize", XmlConvert.ToString(GroupSize));
                WriteElement(xw, "AgentStartPosition", StartPosition.ToString());
                WriteElement(xw, "AgentRespawnPosition", RespawnPosition.ToString());
                WriteElement(xw, "PauseDuringBloodmoon", XmlConvert.ToString(PauseDuringBloodmoon));
                WriteElement(xw, "SpawnProtectionTime", XmlConvert.ToString(SpawnProtectionTime));
                WriteElement(xw, "InfiniteZombieLifetime", XmlConvert.ToString(InfiniteZombieLifetime));
                WriteElement(xw, "MaxSpawnedZombies", MaxSpawnedZombies ?? "75%");
                WriteElement(xw, "PopulationStartPercent", XmlConvert.ToString(PopulationStartPercent));
                WriteElement(xw, "FullPopulationAtDay", XmlConvert.ToString(FullPopulationAtDay));

                // Systems
                if (Processors != null && Processors.Count > 0)
                {
                    xw.WriteStartElement("Systems");
                    foreach (var group in Processors)
                    {
                        xw.WriteStartElement("System");
                        if (!string.IsNullOrEmpty(group.Name))
                            xw.WriteAttributeString("Name", group.Name);
                        xw.WriteAttributeString("Weight", XmlConvert.ToString(group.Weight));
                        xw.WriteAttributeString("SpeedScale", XmlConvert.ToString(group.SpeedScale));
                        xw.WriteAttributeString("PostSpawnBehavior", group.PostSpawnBehavior.ToString());
                        xw.WriteAttributeString("PostSpawnWanderSpeed", group.PostSpawnWanderSpeed.ToString());
                        xw.WriteAttributeString("MapEdgeBehavior", group.MapEdgeBehavior.ToString());
                        if (!string.IsNullOrEmpty(group.Color))
                            xw.WriteAttributeString("Color", group.Color);

                        foreach (var proc in group.Entries)
                        {
                            xw.WriteStartElement("Processor");
                            xw.WriteAttributeString("Type", proc.Type.ToString());
                            xw.WriteAttributeString("Distance", XmlConvert.ToString(proc.Distance));
                            xw.WriteAttributeString("Power", XmlConvert.ToString(proc.Power));
                            if (proc.Param1 != 0f)
                                xw.WriteAttributeString("Param1", XmlConvert.ToString(proc.Param1));
                            if (proc.Param2 != 0f)
                                xw.WriteAttributeString("Param2", XmlConvert.ToString(proc.Param2));
                            xw.WriteEndElement();
                        }

                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                }

                xw.WriteEndElement();
                xw.WriteEndDocument();
            }
        }

        private static void WriteElement(XmlWriter xw, string name, string value)
        {
            xw.WriteStartElement(name);
            xw.WriteString(value);
            xw.WriteEndElement();
        }

        #region XML Parsing Helpers

        private static string ReadString(XmlNode parent, string xpath, XmlNamespaceManager nsMgr, string defaultValue)
        {
            var node = parent.SelectSingleNode(xpath, nsMgr);
            return node != null ? node.InnerText : defaultValue;
        }

        private static int ReadInt(XmlNode parent, string xpath, XmlNamespaceManager nsMgr, int defaultValue)
        {
            var node = parent.SelectSingleNode(xpath, nsMgr);
            if (node != null && int.TryParse(node.InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                return result;
            return defaultValue;
        }

        private static uint ReadUInt(XmlNode parent, string xpath, XmlNamespaceManager nsMgr, uint defaultValue)
        {
            var node = parent.SelectSingleNode(xpath, nsMgr);
            if (node != null && uint.TryParse(node.InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint result))
                return result;
            return defaultValue;
        }

        private static float ReadFloat(XmlNode parent, string xpath, XmlNamespaceManager nsMgr, float defaultValue)
        {
            var node = parent.SelectSingleNode(xpath, nsMgr);
            if (node != null && float.TryParse(node.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                return result;
            return defaultValue;
        }

        private static bool ReadBool(XmlNode parent, string xpath, XmlNamespaceManager nsMgr, bool defaultValue)
        {
            var node = parent.SelectSingleNode(xpath, nsMgr);
            if (node != null && bool.TryParse(node.InnerText, out bool result))
                return result;
            return defaultValue;
        }

        private static T ReadEnum<T>(XmlNode parent, string xpath, XmlNamespaceManager nsMgr, T defaultValue) where T : struct
        {
            var node = parent.SelectSingleNode(xpath, nsMgr);
            if (node != null && Enum.TryParse(node.InnerText, out T result))
                return result;
            return defaultValue;
        }

        private static string ReadAttrString(XmlNode node, string name, string defaultValue)
        {
            var attr = node.Attributes[name];
            return attr != null ? attr.Value : defaultValue;
        }

        private static int ReadAttrInt(XmlNode node, string name, int defaultValue)
        {
            var attr = node.Attributes[name];
            if (attr != null && int.TryParse(attr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                return result;
            return defaultValue;
        }

        private static float ReadAttrFloat(XmlNode node, string name, float defaultValue)
        {
            var attr = node.Attributes[name];
            if (attr != null && float.TryParse(attr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                return result;
            return defaultValue;
        }

        private static T ReadAttrEnum<T>(XmlNode node, string name, T defaultValue) where T : struct
        {
            var attr = node.Attributes[name];
            if (attr != null && Enum.TryParse(attr.Value, out T result))
                return result;
            return defaultValue;
        }

        #endregion
    }
}
