using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WalkerSim.Tests
{
    [TestClass]
    public class PopulationRampTests
    {
        static Vector3 WorldMins = new Vector3(-5120, -5120, 0);
        static Vector3 WorldMaxs = new Vector3(5120, 5120, 255);

        private Simulation CreateSim(float startPercent, int fullDay)
        {
            var config = Config.GetDefault();
            config.PopulationDensity = 50;
            config.PopulationStartPercent = startPercent;
            config.FullPopulationAtDay = fullDay;

            var sim = new Simulation();
            sim.SetWorldSize(WorldMins, WorldMaxs);
            sim.Reset(config);
            return sim;
        }

        [TestMethod]
        public void TestFullPopulationWhenRampDisabled()
        {
            var sim = CreateSim(100f, 1);
            // All agents should start as Wandering when ramp is disabled.
            foreach (var agent in sim.Agents)
            {
                Assert.AreEqual(Agent.State.Wandering, agent.CurrentState);
            }
        }

        [TestMethod]
        public void TestPartialPopulationAtStart()
        {
            var sim = CreateSim(50f, 8);
            int wandering = 0;
            int inactive = 0;
            foreach (var agent in sim.Agents)
            {
                if (agent.CurrentState == Agent.State.Wandering)
                    wandering++;
                else if (agent.CurrentState == Agent.State.Inactive)
                    inactive++;
            }

            // Roughly 50% should be wandering at game time 0.
            Assert.IsTrue(wandering > 0, "Should have some wandering agents");
            Assert.IsTrue(inactive > 0, "Should have some inactive agents");

            float ratio = (float)wandering / sim.Agents.Count;
            Assert.IsTrue(ratio >= 0.4f && ratio <= 0.6f,
                $"Expected ~50% wandering, got {ratio * 100:F1}%");
        }

        [TestMethod]
        public void TestGetPopulationFractionDay0()
        {
            var sim = CreateSim(25f, 8);
            sim.SetGameTime(0.0);
            float frac = sim.GetPopulationFraction();
            Assert.AreEqual(0.25f, frac, 0.001f);
        }

        [TestMethod]
        public void TestGetPopulationFractionAtFullDay()
        {
            var sim = CreateSim(25f, 8);
            sim.SetGameTime(8.0);
            float frac = sim.GetPopulationFraction();
            Assert.AreEqual(1.0f, frac, 0.001f);
        }

        [TestMethod]
        public void TestGetPopulationFractionPastFullDay()
        {
            var sim = CreateSim(25f, 8);
            sim.SetGameTime(100.0);
            float frac = sim.GetPopulationFraction();
            Assert.AreEqual(1.0f, frac, 0.001f);
        }

        [TestMethod]
        public void TestGetPopulationFractionMidway()
        {
            // 25% start, full at day 8.
            // At day 1: should be startFraction (25%).
            // Linear interp from day 1 to day 8: t = (day - 1) / (8 - 1)
            var sim = CreateSim(25f, 8);

            // At day 1, t=0 -> fraction = 0.25
            sim.SetGameTime(1.0);
            float frac1 = sim.GetPopulationFraction();
            Assert.AreEqual(0.25f, frac1, 0.001f);

            // At day 4.5, t = 3.5/7 = 0.5 -> fraction = 0.25 + 0.75*0.5 = 0.625
            sim.SetGameTime(4.5);
            float frac2 = sim.GetPopulationFraction();
            Assert.AreEqual(0.625f, frac2, 0.001f);
        }

        [TestMethod]
        public void TestPopulationRampWakesAgents()
        {
            var sim = CreateSim(10f, 8);
            sim.SetGameTime(0.0);

            int initialWandering = 0;
            foreach (var agent in sim.Agents)
            {
                if (agent.CurrentState == Agent.State.Wandering)
                    initialWandering++;
            }

            // Advance game time to full day and tick enough for the ramp to take effect.
            sim.SetGameTime(8.0);
            // UpdatePopulationRamp checks every TicksPerSecond ticks.
            sim.Advance(Simulation.Constants.TicksPerSecond * 2);

            int finalWandering = 0;
            foreach (var agent in sim.Agents)
            {
                if (agent.CurrentState == Agent.State.Wandering)
                    finalWandering++;
            }

            Assert.IsTrue(finalWandering > initialWandering,
                $"Expected more agents after ramp, had {initialWandering} now {finalWandering}");
            Assert.AreEqual(sim.Agents.Count, finalWandering,
                "All agents should be wandering at full population day");
        }

        [TestMethod]
        public void TestGetPopulationFractionNoRampNeeded()
        {
            // 100% start, day 1 -> always 1.0
            var sim = CreateSim(100f, 1);
            sim.SetGameTime(0.0);
            Assert.AreEqual(1.0f, sim.GetPopulationFraction(), 0.001f);
            sim.SetGameTime(5.0);
            Assert.AreEqual(1.0f, sim.GetPopulationFraction(), 0.001f);
        }
    }
}
