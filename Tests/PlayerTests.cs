using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WalkerSim.Tests
{
    [TestClass]
    public class PlayerTests
    {
        static Vector3 WorldMins = new Vector3(-5120, -5120, 0);
        static Vector3 WorldMaxs = new Vector3(5120, 5120, 255);

        private Simulation CreateSim()
        {
            var config = Config.GetDefault();
            config.PopulationDensity = 5;

            var sim = new Simulation();
            sim.SetWorldSize(WorldMins, WorldMaxs);
            sim.Reset(config);
            return sim;
        }

        [TestMethod]
        public void TestAddPlayer()
        {
            var sim = CreateSim();
            Assert.AreEqual(0, sim.PlayerCount);

            sim.AddPlayer(1, new Vector3(100, 200, 0), 0);
            Assert.AreEqual(1, sim.PlayerCount);
            Assert.IsTrue(sim.HasPlayer(1));
        }

        [TestMethod]
        public void TestRemovePlayer()
        {
            var sim = CreateSim();
            sim.AddPlayer(1, Vector3.Zero, 0);
            Assert.IsTrue(sim.HasPlayer(1));

            sim.RemovePlayer(1);
            Assert.IsFalse(sim.HasPlayer(1));
            Assert.AreEqual(0, sim.PlayerCount);
        }

        [TestMethod]
        public void TestRemoveNonexistentPlayer()
        {
            var sim = CreateSim();
            // Should not throw.
            sim.RemovePlayer(999);
            Assert.AreEqual(0, sim.PlayerCount);
        }

        [TestMethod]
        public void TestUpdatePlayerPosition()
        {
            var sim = CreateSim();
            sim.AddPlayer(1, new Vector3(100, 200, 0), 0);

            sim.UpdatePlayer(1, new Vector3(500, 600, 0), true);

            foreach (var kv in sim.Players)
            {
                if (kv.Key == 1)
                {
                    Assert.AreEqual(500f, kv.Value.Position.X);
                    Assert.AreEqual(600f, kv.Value.Position.Y);
                    Assert.IsTrue(kv.Value.IsAlive);
                }
            }
        }

        [TestMethod]
        public void TestMultiplePlayers()
        {
            var sim = CreateSim();
            sim.AddPlayer(1, new Vector3(100, 100, 0), 0);
            sim.AddPlayer(2, new Vector3(200, 200, 0), 0);
            sim.AddPlayer(3, new Vector3(300, 300, 0), 0);

            Assert.AreEqual(3, sim.PlayerCount);
            Assert.IsTrue(sim.HasPlayer(1));
            Assert.IsTrue(sim.HasPlayer(2));
            Assert.IsTrue(sim.HasPlayer(3));
            Assert.IsFalse(sim.HasPlayer(4));
        }

        [TestMethod]
        public void TestZombieRainToggle()
        {
            var sim = CreateSim();
            sim.AddPlayer(1, Vector3.Zero, 0);

            bool result1 = sim.EnableZombieRain(1);
            Assert.IsTrue(result1, "First toggle should enable");

            bool result2 = sim.EnableZombieRain(1);
            Assert.IsFalse(result2, "Second toggle should disable");
        }

        [TestMethod]
        public void TestZombieRainNonexistentPlayer()
        {
            var sim = CreateSim();
            bool result = sim.EnableZombieRain(999);
            Assert.IsFalse(result);
        }
    }
}
