using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WalkerSim.Tests
{
    [TestClass]
    public class SimulationTests
    {
        static Vector3 WorldMins = new Vector3(-5120, -5120, 0);
        static Vector3 WorldMaxs = new Vector3(5120, 5120, 255);

        private Simulation CreateSim(int density = 50)
        {
            var config = Config.GetDefault();
            config.PopulationDensity = density;

            var sim = new Simulation();
            sim.SetWorldSize(WorldMins, WorldMaxs);
            sim.Reset(config);
            return sim;
        }

        [TestMethod]
        public void TestPopulationScalesWithDensity()
        {
            var simLow = CreateSim(10);
            var simHigh = CreateSim(100);

            Assert.IsTrue(simHigh.Agents.Count > simLow.Agents.Count,
                $"Higher density ({simHigh.Agents.Count}) should have more agents than lower ({simLow.Agents.Count})");
        }

        [TestMethod]
        public void TestResetClearsState()
        {
            var sim = CreateSim(50);
            sim.AddSoundEvent(new Vector3(100, 100, 0), 500f, 100f);
            sim.Advance(10);

            int agentsBefore = sim.Agents.Count;
            uint ticksBefore = sim.Ticks;

            // Reset with same config.
            sim.Reset(Config.GetDefault());

            Assert.AreEqual((uint)0, sim.Ticks, "Ticks should reset");
            Assert.AreEqual(0, sim.Events.Count, "Events should be cleared");
        }

        [TestMethod]
        public void TestAdvanceIncrementsTicks()
        {
            var sim = CreateSim(5);
            Assert.AreEqual((uint)0, sim.Ticks);

            sim.Advance(10);
            Assert.AreEqual((uint)10, sim.Ticks);

            sim.Advance(5);
            Assert.AreEqual((uint)15, sim.Ticks);
        }

        [TestMethod]
        public void TestAgentsStayInBounds()
        {
            var sim = CreateSim(50);
            sim.Advance(100);

            foreach (var agent in sim.Agents)
            {
                if (agent.CurrentState != Agent.State.Wandering)
                    continue;

                Assert.IsTrue(agent.Position.X >= WorldMins.X && agent.Position.X <= WorldMaxs.X,
                    $"Agent X={agent.Position.X} out of bounds [{WorldMins.X}, {WorldMaxs.X}]");
                Assert.IsTrue(agent.Position.Y >= WorldMins.Y && agent.Position.Y <= WorldMaxs.Y,
                    $"Agent Y={agent.Position.Y} out of bounds [{WorldMins.Y}, {WorldMaxs.Y}]");
            }
        }

        [TestMethod]
        public void TestDeterministicSimulation()
        {
            // Two simulations with same config should produce identical results.
            var configA = Config.GetDefault();
            configA.PopulationDensity = 50;
            var configB = Config.GetDefault();
            configB.PopulationDensity = 50;

            var simA = new Simulation();
            simA.SetWorldSize(WorldMins, WorldMaxs);
            simA.Reset(configA);

            var simB = new Simulation();
            simB.SetWorldSize(WorldMins, WorldMaxs);
            simB.Reset(configB);

            Assert.AreEqual(simA.Agents.Count, simB.Agents.Count);

            // Add identical events.
            simA.AddSoundEvent(new Vector3(100, 100, 0), 500f, 10f);
            simB.AddSoundEvent(new Vector3(100, 100, 0), 500f, 10f);

            simA.Advance(50);
            simB.Advance(50);

            // All agents should be in same positions.
            for (int i = 0; i < simA.Agents.Count; i++)
            {
                Assert.AreEqual(simA.Agents[i].Position.X, simB.Agents[i].Position.X, 0.001f,
                    $"Agent {i} X mismatch");
                Assert.AreEqual(simA.Agents[i].Position.Y, simB.Agents[i].Position.Y, 0.001f,
                    $"Agent {i} Y mismatch");
            }
        }

        [TestMethod]
        public void TestDifferentSeedsProduceDifferentResults()
        {
            var configA = Config.GetDefault();
            configA.PopulationDensity = 50;
            configA.RandomSeed = 1;

            var configB = Config.GetDefault();
            configB.PopulationDensity = 50;
            configB.RandomSeed = 999;

            var simA = new Simulation();
            simA.SetWorldSize(WorldMins, WorldMaxs);
            simA.Reset(configA);

            var simB = new Simulation();
            simB.SetWorldSize(WorldMins, WorldMaxs);
            simB.Reset(configB);

            simA.Advance(10);
            simB.Advance(10);

            // At least some agents should differ.
            bool anyDifference = false;
            for (int i = 0; i < simA.Agents.Count && i < simB.Agents.Count; i++)
            {
                if (simA.Agents[i].Position.X != simB.Agents[i].Position.X ||
                    simA.Agents[i].Position.Y != simB.Agents[i].Position.Y)
                {
                    anyDifference = true;
                    break;
                }
            }
            Assert.IsTrue(anyDifference, "Different seeds should produce different positions");
        }

        [TestMethod]
        public void TestKillAgentsInRadius()
        {
            var sim = CreateSim(50);
            sim.Advance(5);

            // Kill all agents in a large radius from center.
            int killed = sim.KillAgentsInRadius(Vector3.Zero, 2000f);
            Assert.IsTrue(killed > 0, "Should have killed some agents");

            int dead = 0;
            foreach (var agent in sim.Agents)
            {
                if (agent.CurrentState == Agent.State.Dead ||
                    agent.CurrentState == Agent.State.Respawning)
                    dead++;
            }
            Assert.AreEqual(killed, dead, "Killed count should match dead+respawning agents");
        }

        [TestMethod]
        public void TestSetWorldSize()
        {
            var sim = new Simulation();
            var mins = new Vector3(-1000, -1000, 0);
            var maxs = new Vector3(1000, 1000, 255);
            sim.SetWorldSize(mins, maxs);

            Assert.AreEqual(mins, sim.WorldMins);
            Assert.AreEqual(maxs, sim.WorldMaxs);
        }

        [TestMethod]
        public void TestGroupCount()
        {
            var config = Config.GetDefault();
            config.PopulationDensity = 100;
            config.GroupSize = 32;

            var sim = new Simulation();
            sim.SetWorldSize(WorldMins, WorldMaxs);
            sim.Reset(config);

            Assert.IsTrue(sim.GroupCount > 0);
            // GroupCount = ceil(agentCount / groupSize)
            int expectedGroups = (sim.Agents.Count + config.GroupSize - 1) / config.GroupSize;
            Assert.AreEqual(expectedGroups, sim.GroupCount);
        }

        [TestMethod]
        public void TestBloodmoonState()
        {
            var sim = CreateSim(5);

            Assert.IsFalse(sim.IsBloodmoon);
            sim.SetIsBloodmoon(true);
            Assert.IsTrue(sim.IsBloodmoon);
            sim.SetIsBloodmoon(false);
            Assert.IsFalse(sim.IsBloodmoon);
        }

        [TestMethod]
        public void TestDayTimeState()
        {
            var sim = CreateSim(5);

            sim.SetIsDayTime(true);
            Assert.IsTrue(sim.IsDayTime);
            sim.SetIsDayTime(false);
            Assert.IsFalse(sim.IsDayTime);
        }

        [TestMethod]
        public void TestSaveLoadEmpty()
        {
            // Save/load with no agents (density 0 not allowed, use no processors).
            var config = Config.GetDefault();
            config.PopulationDensity = 1;

            var sim = new Simulation();
            sim.SetWorldSize(new Vector3(-100, -100, 0), new Vector3(100, 100, 255));
            sim.Reset(config);

            var ms = new MemoryStream();
            Assert.IsTrue(sim.Save(ms));

            ms.Position = 0;
            var sim2 = new Simulation();
            Assert.IsTrue(sim2.Load(ms));

            Assert.AreEqual(sim.Agents.Count, sim2.Agents.Count);
            Assert.AreEqual(sim.Ticks, sim2.Ticks);
        }

        [TestMethod]
        public void TestSaveLoadWithEvents()
        {
            var sim = CreateSim(10);
            sim.AddSoundEvent(new Vector3(500, 500, 0), 300f, 100f);
            sim.AddSoundEvent(new Vector3(-500, -500, 0), 200f, 50f);
            sim.Advance(5);

            var ms = new MemoryStream();
            Assert.IsTrue(sim.Save(ms));

            ms.Position = 0;
            var sim2 = new Simulation();
            Assert.IsTrue(sim2.Load(ms));

            Assert.AreEqual(sim.Events.Count, sim2.Events.Count);
            for (int i = 0; i < sim.Events.Count; i++)
            {
                Assert.AreEqual(sim.Events[i].Type, sim2.Events[i].Type);
                Assert.AreEqual(sim.Events[i].Position.X, sim2.Events[i].Position.X, 0.01f);
                Assert.AreEqual(sim.Events[i].Position.Y, sim2.Events[i].Position.Y, 0.01f);
                Assert.AreEqual(sim.Events[i].Radius, sim2.Events[i].Radius, 0.01f);
            }
        }

        [TestMethod]
        public void TestTickConversions()
        {
            Assert.AreEqual(30u, Simulation.SecondsToTicks(1));
            Assert.AreEqual(60u, Simulation.SecondsToTicks(2));
            Assert.AreEqual(1800u, Simulation.MinutesToTicks(1));
            Assert.AreEqual(3u, Simulation.MillisecondsToTicks(100));
        }

        [TestMethod]
        public void TestWindChangesOverTime()
        {
            // Use minimal density to keep this fast.
            var sim = CreateSim(1);
            var initialWind = sim.WindDirection;

            // Wind changes every 1-3 minutes. Advance 4 minutes worth of ticks.
            sim.Advance(Simulation.MinutesToTicks(4));

            // Wind direction should have changed at least slightly.
            var finalWind = sim.WindDirection;
            bool changed = initialWind.X != finalWind.X || initialWind.Y != finalWind.Y;
            Assert.IsTrue(changed, "Wind should change over time");
        }
    }
}
