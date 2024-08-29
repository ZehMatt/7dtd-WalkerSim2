using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WalkerSim.Tests
{
    [TestClass]
    public class LoadSaveTests
    {
        static Vector3 WorldMins = new Vector3(-5120, -5120, 0);
        static Vector3 WorldMaxs = new Vector3(5120, 5120, 255);

        private void CompareSimulations(Simulation simA, Simulation simB)
        {
            // Compare Config.
            var configA = simA.Config;
            Assert.IsNotNull(configA);
            var configB = simB.Config;
            Assert.IsNotNull(configB);

            Assert.AreEqual(configA.RandomSeed, configB.RandomSeed);
            Assert.AreEqual(configA.PopulationDensity, configB.PopulationDensity);
            Assert.AreEqual(configA.StartAgentsGrouped, configB.StartAgentsGrouped);
            Assert.AreEqual(configA.GroupSize, configB.GroupSize);
            Assert.AreEqual(configA.StartPosition, configB.StartPosition);
            Assert.AreEqual(configA.RespawnPosition, configB.RespawnPosition);
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

            // Compare State
            Assert.AreEqual(simA.WorldMins, simB.WorldMins);
            Assert.AreEqual(simA.WorldMaxs, simB.WorldMaxs);
            Assert.AreEqual(simA.SlowIterator, simB.SlowIterator);
            Assert.AreEqual(simA.WindDirection, simB.WindDirection);
            Assert.AreEqual(simA.WindDirectionTarget, simB.WindDirectionTarget);
            Assert.AreEqual(simA.WindTime, simB.WindTime);
            Assert.AreEqual(simA.Ticks, simB.Ticks);
            Assert.AreEqual(simA.TickNextWindChange, simB.TickNextWindChange);
            Assert.AreEqual(simA.GroupCount, simB.GroupCount);
            Assert.AreEqual(simA.MaxNeighbourDistance, simB.MaxNeighbourDistance);
            Assert.AreEqual(simA.PRNG.State0, simB.PRNG.State0);
            Assert.AreEqual(simA.PRNG.State1, simB.PRNG.State1);

            // Compare Agents.
            Assert.AreEqual(simA.Agents.Count, simB.Agents.Count);
            for (int i = 0; i < simA.Agents.Count; i++)
            {
                var agentA = simA.Agents[i];
                var agentB = simB.Agents[i];

                Assert.AreEqual(agentA.Index, agentB.Index);
                Assert.AreEqual(agentA.Group, agentB.Group);
                Assert.AreEqual(agentA.LastUpdateTick, agentB.LastUpdateTick);
                Assert.AreEqual(agentA.CellIndex, agentB.CellIndex);
                Assert.AreEqual(agentA.Velocity, agentB.Velocity);
                Assert.AreEqual(agentA.Position, agentB.Position);
                Assert.AreEqual(agentA.CurrentState, agentB.CurrentState);
            }

            // Compare Events.
            Assert.AreEqual(simA.Events.Count, simB.Events.Count);
            for (int i = 0; i < simA.Events.Count; i++)
            {
                var eventA = simA.Events[i];
                var eventB = simB.Events[i];

                Assert.AreEqual(eventA.Type, eventB.Type);
                Assert.AreEqual(eventA.Duration, eventB.Duration);
                Assert.AreEqual(eventA.Position, eventB.Position);
                Assert.AreEqual(eventA.Radius, eventB.Radius);
            }

            // Query Agents at random positions validating the grid is correctly setup.
            var prng = new System.Random(1333);
            for (int i = 0; i < 100; i++)
            {
                var randomPos = Utils.GetRandomVector3(prng, simA.WorldMins, simA.WorldMaxs);

                var queryA = simA.QueryCells(randomPos, -1, 500.0f);
                var queryB = simA.QueryCells(randomPos, -1, 500.0f);

                Assert.AreEqual(queryA.Count, queryB.Count);
                for (int x = 0; x < queryA.Count; x++)
                {
                    Assert.AreEqual(queryA[x].Index, queryB[x].Index);
                }
            }
        }

        [TestMethod]
        public void TestSaveLoad5k()
        {
            var config = Config.GetDefault();
            config.PopulationDensity = 150;

            var simA = new Simulation();
            simA.SetWorldSize(WorldMins, WorldMaxs);
            simA.Reset(config);
            simA.Deterministic = true;

            // Add a few events.
            simA.AddSoundEvent(new Vector3(-100, -100, 0), 653.212f, 10.0f);
            simA.AddSoundEvent(new Vector3(700, 100, 0), 653.212f, 10.0f);
            simA.AddSoundEvent(new Vector3(1700, 500, 0), 653.212f, 10.0f);
            simA.Advance(20);

            var ms = new MemoryStream();
            Assert.IsTrue(simA.Save(ms));
            ms.Position = 0;

            var simB = new Simulation();
            simB.Deterministic = true;
            Assert.IsTrue(simB.Load(ms));

            // Compare current state of both.
            CompareSimulations(simA, simB);

            // Advance a few ticks.
            for (int i = 0; i < 10; i++)
            {
                simA.Tick();
                simB.Tick();

                // Compare after advancing.
                CompareSimulations(simA, simB);
            }

        }
    }
}
