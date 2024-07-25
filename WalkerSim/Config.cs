using System.Xml.Serialization;

namespace WalkerSim
{
    [XmlRoot("WalkerSim")]
    public class Config
    {
        public enum SpawnLocation
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

        [XmlElement("RandomSeed")]
        public int RandomSeed = 1;

        [XmlElement("MaxAgents")]
        public int MaxAgents = 5000;

        [XmlElement("StartAgentsGrouped")]
        public bool StartAgentsGrouped = true;

        [XmlElement("GroupSize")]
        public int GroupSize = 10;

        [XmlElement("AgentStartLocation")]
        public SpawnLocation StartLocation = SpawnLocation.RandomPOI;

        [XmlElement("AgentRespawnLocation")]
        public SpawnLocation RespawnType = SpawnLocation.None;

        [XmlElement("PauseWithoutPlayers")]
        public bool PauseWithoutPlayers = false;

        [XmlElement("PauseDuringBloodmoon")]
        public bool PauseDuringBloodmoon = false;

        public static Config LoadFromFile(string filePath)
        {
            var serializer = new XmlSerializer(typeof(Config));
            try
            {
                using (var reader = new System.IO.StreamReader(filePath))
                {
                    return (Config)serializer.Deserialize(reader);
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }
}
