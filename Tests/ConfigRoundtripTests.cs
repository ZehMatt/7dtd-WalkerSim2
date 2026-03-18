using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace WalkerSim.Tests
{
    [TestClass]
    public class ConfigRoundtripTests
    {
        [TestMethod]
        public void TestDefaultConfigRoundtrip()
        {
            var original = Config.GetDefault();

            // Export -> load -> export, then load -> export again. Both post-sanitize exports must match.
            string xml1;
            using (var sw = new StringWriter())
            {
                original.Export(sw);
                xml1 = sw.ToString();
            }

            var loaded1 = Config.LoadFromText(xml1);
            Assert.IsNotNull(loaded1);

            string xml2;
            using (var sw = new StringWriter())
            {
                loaded1.Export(sw);
                xml2 = sw.ToString();
            }

            var loaded2 = Config.LoadFromText(xml2);
            Assert.IsNotNull(loaded2);

            string xml3;
            using (var sw = new StringWriter())
            {
                loaded2.Export(sw);
                xml3 = sw.ToString();
            }

            // After one round-trip through sanitize, output should stabilize.
            Assert.AreEqual(xml2, xml3);
        }

        [TestMethod]
        public void TestConfigRoundtripPreservesAllFields()
        {
            var original = Config.GetDefault();
            original.RandomSeed = 42;
            original.PopulationDensity = 500;
            original.SpawnActivationRadius = 128;
            original.StartAgentsGrouped = false;
            original.EnhancedSoundAwareness = false;
            original.SoundDistanceScale = 2.5f;
            original.GroupSize = 64;
            original.StartPosition = Config.WorldLocation.RandomBorderLocation;
            original.RespawnPosition = Config.WorldLocation.RandomCity;
            original.PauseDuringBloodmoon = false;
            original.SpawnProtectionTime = 600;
            original.InfiniteZombieLifetime = true;
            original.MaxSpawnedZombies = "50%";
            original.PopulationStartPercent = 25.0f;
            original.FullPopulationAtDay = 10;

            string xml;
            using (var sw = new StringWriter())
            {
                original.Export(sw);
                xml = sw.ToString();
            }

            var loaded = Config.LoadFromText(xml);
            Assert.IsNotNull(loaded);

            Assert.AreEqual(original.RandomSeed, loaded.RandomSeed);
            Assert.AreEqual(original.PopulationDensity, loaded.PopulationDensity);
            Assert.AreEqual(original.SpawnActivationRadius, loaded.SpawnActivationRadius);
            Assert.AreEqual(original.StartAgentsGrouped, loaded.StartAgentsGrouped);
            Assert.AreEqual(original.EnhancedSoundAwareness, loaded.EnhancedSoundAwareness);
            Assert.AreEqual(original.SoundDistanceScale, loaded.SoundDistanceScale);
            Assert.AreEqual(original.GroupSize, loaded.GroupSize);
            Assert.AreEqual(original.StartPosition, loaded.StartPosition);
            Assert.AreEqual(original.RespawnPosition, loaded.RespawnPosition);
            Assert.AreEqual(original.PauseDuringBloodmoon, loaded.PauseDuringBloodmoon);
            Assert.AreEqual(original.SpawnProtectionTime, loaded.SpawnProtectionTime);
            Assert.AreEqual(original.InfiniteZombieLifetime, loaded.InfiniteZombieLifetime);
            Assert.AreEqual(original.MaxSpawnedZombies, loaded.MaxSpawnedZombies);
            Assert.AreEqual(original.PopulationStartPercent, loaded.PopulationStartPercent);
            Assert.AreEqual(original.FullPopulationAtDay, loaded.FullPopulationAtDay);
        }

        [TestMethod]
        public void TestBackwardsCompatPopulationFullDay()
        {
            // Old XML used <PopulationFullDay>, new code should read it as FullPopulationAtDay.
            var xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<WalkerSim xmlns=""http://zeh.matt/WalkerSim"" Version=""2"">
  <PopulationFullDay>7</PopulationFullDay>
</WalkerSim>";

            var config = Config.LoadFromText(xml);
            Assert.IsNotNull(config);
            Assert.AreEqual(7, config.FullPopulationAtDay);
        }

        [TestMethod]
        public void TestNewNameTakesPriorityOverOld()
        {
            // If both old and new names are present, new name wins.
            var xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<WalkerSim xmlns=""http://zeh.matt/WalkerSim"" Version=""2"">
  <PopulationFullDay>7</PopulationFullDay>
  <FullPopulationAtDay>14</FullPopulationAtDay>
</WalkerSim>";

            var config = Config.LoadFromText(xml);
            Assert.IsNotNull(config);
            Assert.AreEqual(14, config.FullPopulationAtDay);
        }

        [TestMethod]
        public void TestConfigCompareDetectsDifference()
        {
            var a = Config.GetDefault();
            var b = Config.GetDefault();
            Assert.IsTrue(a.Compare(b));

            b.PopulationDensity = 999;
            Assert.IsFalse(a.Compare(b));
        }

        [TestMethod]
        public void TestConfigLoadFromFileRoundtrip()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configFile = Path.Combine(assemblyPath, "WalkerSim.xml");
            var config = Config.LoadFromFile(configFile);
            Assert.IsNotNull(config);

            // Re-export and re-load, should be identical.
            string xml;
            using (var sw = new StringWriter())
            {
                config.Export(sw);
                xml = sw.ToString();
            }

            var reloaded = Config.LoadFromText(xml);
            Assert.IsNotNull(reloaded);
            Assert.IsTrue(config.Compare(reloaded));
        }

        [TestMethod]
        public void TestConfigProcessorRoundtrip()
        {
            var config = Config.GetDefault();
            config.Processors.Add(new Config.MovementProcessorGroup
            {
                Name = "TestGroup",
                Weight = 0.5f,
                SpeedScale = 2.0f,
                PostSpawnBehavior = Config.PostSpawnBehavior.ChaseActivator,
                PostSpawnWanderSpeed = Config.WanderingSpeed.Run,
                Color = "#FF0000",
                Entries = new System.Collections.Generic.List<Config.MovementProcessor>
                {
                    new Config.MovementProcessor
                    {
                        Type = Config.MovementProcessorType.Wind,
                        Distance = 100f,
                        Power = 0.5f,
                    },
                    new Config.MovementProcessor
                    {
                        Type = Config.MovementProcessorType.CityVisitor,
                        Distance = 200f,
                        Power = 1.0f,
                        Param1 = 15f,
                        Param2 = 30f,
                    },
                }
            });

            string xml;
            using (var sw = new StringWriter())
            {
                config.Export(sw);
                xml = sw.ToString();
            }

            var loaded = Config.LoadFromText(xml);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(2, loaded.Processors.Count);

            var group = loaded.Processors[1];
            Assert.AreEqual("TestGroup", group.Name);
            Assert.AreEqual(0.5f, group.Weight);
            Assert.AreEqual(2.0f, group.SpeedScale);
            Assert.AreEqual(Config.PostSpawnBehavior.ChaseActivator, group.PostSpawnBehavior);
            Assert.AreEqual(Config.WanderingSpeed.Run, group.PostSpawnWanderSpeed);
            Assert.AreEqual("#FF0000", group.Color);
            Assert.AreEqual(2, group.Entries.Count);

            Assert.AreEqual(Config.MovementProcessorType.CityVisitor, group.Entries[1].Type);
            Assert.AreEqual(15f, group.Entries[1].Param1);
            Assert.AreEqual(30f, group.Entries[1].Param2);
        }

        [TestMethod]
        public void TestInvalidXmlReturnsNull()
        {
            var config = Config.LoadFromText("this is not xml");
            Assert.IsNull(config);
        }

        [TestMethod]
        public void TestMinimalXmlUsesDefaults()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<WalkerSim xmlns=""http://zeh.matt/WalkerSim"" Version=""2"">
</WalkerSim>";

            var config = Config.LoadFromText(xml);
            Assert.IsNotNull(config);
            Assert.AreEqual(1337, config.RandomSeed);
            Assert.AreEqual(300, config.PopulationDensity);
            Assert.AreEqual(true, config.StartAgentsGrouped);
            Assert.AreEqual(100.0f, config.PopulationStartPercent);
            Assert.AreEqual(1, config.FullPopulationAtDay);
        }
    }
}
