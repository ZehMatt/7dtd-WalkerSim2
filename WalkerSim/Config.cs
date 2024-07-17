using System.Xml.Serialization;

namespace WalkerSim
{
    [XmlRoot("WalkerSim")]
    public class Config
    {
        [XmlElement("RandomSeed")]
        public int RandomSeed = 1;

        [XmlElement("MaxAgents")]
        public int MaxAgents = 5000;

        [XmlElement("GroupSize")]
        public int GroupSize = 10;

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
