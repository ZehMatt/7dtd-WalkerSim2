using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace WalkerSim
{
    [XmlRoot("WalkerSim", Namespace = "http://zeh.matt/WalkerSim", IsNullable = false)]
    public class Config
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns { get; set; }

        [XmlAttribute("schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string SchemaLocation { get; set; }

        public Config()
        {
            Xmlns = new XmlSerializerNamespaces();
            Xmlns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            Xmlns.Add("xsd", "http://www.w3.org/2001/XMLSchema");

            // Define the schema location
            SchemaLocation = "http://zeh.matt/WalkerSim WalkerSimSchema.xsd";
        }

        public enum WanderingSpeed
        {
            [XmlEnum("NoOverride")]
            NoOverride = 0,
            [XmlEnum("Walk")]
            Walk,
            [XmlEnum("Jog")]
            Jog,
            [XmlEnum("Run")]
            Run,
            [XmlEnum("Sprint")]
            Sprint,
            [XmlEnum("Nightmare")]
            Nightmare,
        }

        public enum PostSpawnBehavior
        {
            [XmlEnum("Wander")]
            Wander = 0,
            [XmlEnum("ChaseActivator")]
            ChaseActivator,
            [XmlEnum("Nothing")]
            Nothing,
        }

        public enum WorldLocation
        {
            [XmlEnum("None")]
            None = 0,
            [XmlEnum("RandomBorderLocation")]
            RandomBorderLocation,
            [XmlEnum("RandomLocation")]
            RandomLocation,
            [XmlEnum("RandomPOI")]
            RandomPOI,
            [XmlEnum("Mixed")]
            Mixed,
        }

        public enum MovementProcessorType
        {
            Invalid = 0,
            [XmlEnum("FlockAnyGroup")]
            FlockAnyGroup,
            [XmlEnum("AlignAnyGroup")]
            AlignAnyGroup,
            [XmlEnum("AvoidAnyGroup")]
            AvoidAnyGroup,
            [XmlEnum("FlockSameGroup")]
            FlockSameGroup,
            [XmlEnum("AlignSameGroup")]
            AlignSameGroup,
            [XmlEnum("AvoidSameGroup")]
            AvoidSameGroup,
            [XmlEnum("FlockOtherGroup")]
            FlockOtherGroup,
            [XmlEnum("AlignOtherGroup")]
            AlignOtherGroup,
            [XmlEnum("AvoidOtherGroup")]
            AvoidOtherGroup,
            [XmlEnum("Wind")]
            Wind,
            [XmlEnum("WindInverted")]
            WindInverted,
            [XmlEnum("StickToRoads")]
            StickToRoads,
            [XmlEnum("AvoidRoads")]
            AvoidRoads,
            [XmlEnum("StickToPOIs")]
            StickToPOIs,
            [XmlEnum("AvoidPOIs")]
            AvoidPOIs,
            [XmlEnum("WorldEvents")]
            WorldEvents,
        }

        public class MovementProcessor
        {
            [XmlAttribute("Type")]
            public MovementProcessorType Type;

            [XmlAttribute("Distance")]
            public float Distance = 0.0f;

            [XmlAttribute("Power")]
            public float Power = 0.0f;
        }

        [XmlType("ProcessorGroup")]
        public class MovementProcessorGroup
        {
            [XmlAttribute("Group")]
            public int Group = -1;

            [XmlAttribute("SpeedScale")]
            public float SpeedScale = 1.0f;

            [XmlAttribute("PostSpawnBehavior")]
            public PostSpawnBehavior PostSpawnBehavior = PostSpawnBehavior.Wander;

            [XmlAttribute("PostSpawnWanderSpeed")]
            public WanderingSpeed PostSpawnWanderSpeed = WanderingSpeed.NoOverride;

            [XmlAttribute("Color")]
            public string Color = "";

            [XmlElement("Processor")]
            public List<MovementProcessor> Entries = new List<MovementProcessor>();
        }

        public class LoggingOptions
        {
            [XmlElement("General")]
            public bool General = true;

            [XmlElement("Spawns")]
            public bool Spawns = false;

            [XmlElement("Despawns")]
            public bool Despawns = false;

            [XmlElement("EntityClassSelection")]
            public bool EntityClassSelection = false;

            [XmlElement("Events")]
            public bool Events = false;
        }

        [XmlElement("Logging")]
        public LoggingOptions LoggingOpts;

        [XmlElement("RandomSeed")]
        public int RandomSeed = 1337;

        [XmlElement("PopulationDensity")]
        public int PopulationDensity = 300;

        [XmlElement("SpawnActivationRadius")]
        public int SpawnActivationRadius = 96;

        [XmlElement("StartAgentsGrouped")]
        public bool StartAgentsGrouped = true;

        [XmlElement("EnhancedSoundAwareness")]
        public bool EnhancedSoundAwareness = true;

        [XmlElement("SoundDistanceScale")]
        public float SoundDistanceScale = 1.0f;

        [XmlElement("FastForwardAtStart")]
        public bool FastForwardAtStart = true;

        [XmlElement("GroupSize")]
        public int GroupSize = 200;

        [XmlElement("AgentStartPosition")]
        public WorldLocation StartPosition = WorldLocation.RandomLocation;

        [XmlElement("AgentRespawnPosition")]
        public WorldLocation RespawnPosition = WorldLocation.None;

        [XmlElement("PauseDuringBloodmoon")]
        public bool PauseDuringBloodmoon = true;

        [XmlElement("SpawnProtectionTime")]
        public int SpawnProtectionTime = 300;

        [XmlElement("MaxSpawnedZombies")]
        public string MaxSpawnedZombies = "75%";

        [XmlArray("MovementProcessors")]
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
        }

        public static Config LoadFromFile(string filePath)
        {
            using (var reader = new System.IO.StreamReader(filePath))
            {
                return LoadFromStream(reader);
            }
        }

        public static Config LoadFromText(string text)
        {
            var stream = new StringReader(text);
            return LoadFromStream(stream);
        }

        public static Config LoadFromStream(System.IO.TextReader reader)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Config));

                var config = (Config)serializer.Deserialize(reader);
                if (config == null)
                    return null;

                SanitizeConfig(config);
                return config;
            }
            catch (System.Exception ex)
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
                FastForwardAtStart = true,
                PauseDuringBloodmoon = true,
                SpawnProtectionTime = 300,
                MaxSpawnedZombies = "75%",
                Processors = new List<MovementProcessorGroup>
                {
                    new MovementProcessorGroup {
                        Group = -1,
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
            var serializer = new XmlSerializer(typeof(Config));
            try
            {
                serializer.Serialize(writer, this);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
