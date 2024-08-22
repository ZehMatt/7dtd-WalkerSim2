using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WalkerSim.Tests
{
    [TestClass]
    public class LoadSaveTests
    {
        static Vector3 WorldMins = new Vector3(-5120, -5120, 0);
        static Vector3 WorldMaxs = new Vector3(5120, 5120, 255);

        [TestMethod]
        public void TestSaveLoad5k()
        {
            var config = Config.GetDefault();
            config.MaxAgents = 5000;

            var simA = new Simulation();
            simA.Reset(WorldMins, WorldMaxs, config);
            simA.FastAdvance(100);

            var ms = new MemoryStream();
            Assert.IsTrue(simA.Save(ms));
            ms.Position = 0;

            var simB = new Simulation();
            Assert.IsTrue(simB.Load(ms));

            // Compare Config.
            var configA = simA.Config;
            Assert.IsNotNull(configA);
            var configB = simB.Config;
            Assert.IsNotNull(configB);

            Assert.AreEqual(configA.RandomSeed, configB.RandomSeed);
            Assert.AreEqual(configA.MaxAgents, configB.MaxAgents);
            Assert.AreEqual(configA.TicksToAdvanceOnStartup, configB.TicksToAdvanceOnStartup);
            Assert.AreEqual(configA.StartAgentsGrouped, configB.StartAgentsGrouped);
            Assert.AreEqual(configA.GroupSize, configB.GroupSize);
            Assert.AreEqual(configA.StartPosition, configB.StartPosition);
            Assert.AreEqual(configA.RespawnPosition, configB.RespawnPosition);
            Assert.AreEqual(configA.PauseWithoutPlayers, configB.PauseWithoutPlayers);
            Assert.AreEqual(configA.PauseDuringBloodmoon, configB.PauseDuringBloodmoon);

            Assert.AreEqual(configA.Processors.Count, configB.Processors.Count);
            for (int i = 0; i < configA.Processors.Count; i++)
            {
                var processorsA = configA.Processors[i];
                var processorsB = configB.Processors[i];
                Assert.AreEqual(processorsA.SpeedScale, processorsB.SpeedScale);
                Assert.AreEqual(processorsA.Group, processorsB.Group);

                Assert.AreEqual(processorsA.Entries.Count, processorsB.Entries.Count);
                for (int y = 0; y < processorsA.Entries.Count; y++)
                {
                    var processorA = processorsA.Entries[y];
                    var processorB = processorsB.Entries[y];

                    Assert.AreEqual(processorA.Type, processorB.Type);
                    Assert.AreEqual(processorA.Distance, processorB.Distance);
                    Assert.AreEqual(processorA.Power, processorB.Power);
                }
            }

            // Compare Agents.
            Assert.AreEqual(simA.Agents.Count, simB.Agents.Count);
            for (int i = 0; i < simA.Agents.Count; i++)
            {
                var agentA = simA.Agents[i];
                var agentB = simB.Agents[i];

                Assert.AreEqual(agentA.Index, agentB.Index);
                Assert.AreEqual(agentA.Group, agentB.Group);
                Assert.AreEqual(agentA.CellIndex, agentB.CellIndex);
                Assert.AreEqual(agentA.Position, agentB.Position);
                Assert.AreEqual(agentA.Velocity, agentB.Velocity);
                Assert.AreEqual(agentA.CurrentState, agentB.CurrentState);
            }
        }
    }
}
