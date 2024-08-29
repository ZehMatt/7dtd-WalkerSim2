using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WalkerSim.Tests
{
    [TestClass]
    public class TestGridQuery
    {
        static Vector3 WorldMins = new Vector3(-5120, -5120, 0);
        static Vector3 WorldMaxs = new Vector3(5120, 5120, 255);

        static Vector3 GetRandomPos(Random prng)
        {
            var x = prng.NextDouble();
            var y = prng.NextDouble();
            return new Vector3(Math.Remap((float)x, 0.0f, 1.0f, WorldMins.X, WorldMaxs.X),
                Math.Remap((float)y, 0.0f, 1.0f, WorldMins.X, WorldMaxs.X));
        }

        static void SetAgentsAroundPos(Simulation sim, Vector3 pos, float radius)
        {
            for (int i = 0; i < sim.Agents.Count; i++)
            {
                var agent = sim.Agents[i];

                float groupOffset = (float)i / sim.Agents.Count;

                // Spawn in circle.
                float angle = groupOffset * (float)System.Math.PI * 2.0f;
                float offsetX = (float)System.Math.Cos(angle) * radius;
                float offsetY = (float)System.Math.Sin(angle) * radius;

                var centerPos = pos + new Vector3(offsetX, offsetY);

                agent.Position = centerPos;
                sim.MoveInGrid(agent);
            }
        }

        [TestMethod]
        public void TestPlayerRadius()
        {
            var prng = new Random(1);

            var config = Config.GetDefault();
            config.PopulationDensity = 5;

            var sim = new Simulation();
            sim.SetWorldSize(WorldMins, WorldMaxs);
            sim.Reset(config);
            sim.AddPlayer(0, Vector3.Zero, 96);

            for (int i = 0; i < 10; i++)
            {
                var centerPos = GetRandomPos(prng);
                sim.UpdatePlayer(0, centerPos, true);

                // Put all agents around the player.
                SetAgentsAroundPos(sim, centerPos, 360);

                var agents = sim.QueryCells(centerPos, -1, 380);
                Assert.IsNotNull(agents);

                Assert.AreEqual(sim.Agents.Count, agents.Count);
            }
        }
    }
}
