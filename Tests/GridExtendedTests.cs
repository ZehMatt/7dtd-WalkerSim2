using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WalkerSim.Tests
{
    [TestClass]
    public class GridExtendedTests
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
        public void TestQueryCellsExcludesIndex()
        {
            var sim = CreateSim(5);
            sim.Advance(5);

            // Query from an agent's position, excluding that agent.
            var agent = sim.Agents[0];
            var results = sim.QueryCells(agent.Position, agent.Index, 10000f);

            foreach (var found in results)
            {
                Assert.AreNotEqual(agent.Index, found.Index, "Excluded agent should not appear in results");
            }
        }

        [TestMethod]
        public void TestQueryCellsOnlyWanderingAgents()
        {
            var sim = CreateSim(50);
            sim.Advance(5);

            // Kill some agents first.
            sim.KillAgentsInRadius(new Vector3(0, 0, 0), 1000f);

            // Query a large area - should only return wandering agents.
            var results = sim.QueryCells(Vector3.Zero, -1, 10000f);
            foreach (var agent in results)
            {
                Assert.AreEqual(Agent.State.Wandering, agent.CurrentState,
                    $"Agent {agent.Index} in query result has state {agent.CurrentState}");
            }
        }

        [TestMethod]
        public void TestQueryCellsRespectDistance()
        {
            var sim = CreateSim(100);

            // Place a known agent at a known position.
            var agent = sim.Agents[0];
            agent.Position = new Vector3(0, 0, 0);
            agent.CurrentState = Agent.State.Wandering;
            sim.MoveInGrid(agent);

            // Query from far away with small radius.
            var results = sim.QueryCells(new Vector3(4000, 4000, 0), -1, 100f);
            bool foundAgent0 = false;
            foreach (var found in results)
            {
                if (found.Index == 0)
                {
                    foundAgent0 = true;
                    break;
                }
            }
            Assert.IsFalse(foundAgent0, "Agent at origin should not be found 4000+ units away with 100 radius");
        }

        [TestMethod]
        public void TestMoveInGridUpdatesCell()
        {
            var sim = CreateSim(5);

            var agent = sim.Agents[0];
            agent.CurrentState = Agent.State.Wandering;
            agent.Position = new Vector3(-4000, -4000, 0);
            sim.MoveInGrid(agent);
            int cellBefore = agent.CellIndex;

            agent.Position = new Vector3(4000, 4000, 0);
            sim.MoveInGrid(agent);
            int cellAfter = agent.CellIndex;

            Assert.AreNotEqual(cellBefore, cellAfter, "Cell should change when agent moves far");
        }

        [TestMethod]
        public void TestRebuildGridPreservesQueryResults()
        {
            var sim = CreateSim(50);
            sim.Advance(10);

            // Query before rebuild.
            var resultsBefore = sim.QueryCells(Vector3.Zero, -1, 2000f);
            var indicesBefore = new int[resultsBefore.Count];
            for (int i = 0; i < resultsBefore.Count; i++)
                indicesBefore[i] = resultsBefore[i].Index;
            System.Array.Sort(indicesBefore);

            // Rebuild.
            sim.RebuildGrid();

            // Query after rebuild.
            var resultsAfter = sim.QueryCells(Vector3.Zero, -1, 2000f);
            var indicesAfter = new int[resultsAfter.Count];
            for (int i = 0; i < resultsAfter.Count; i++)
                indicesAfter[i] = resultsAfter[i].Index;
            System.Array.Sort(indicesAfter);

            Assert.AreEqual(indicesBefore.Length, indicesAfter.Length,
                "Same number of agents should be found after rebuild");
            for (int i = 0; i < indicesBefore.Length; i++)
            {
                Assert.AreEqual(indicesBefore[i], indicesAfter[i],
                    $"Agent index mismatch at position {i}");
            }
        }

        [TestMethod]
        public void TestForEachNearbyMatchesQueryCells()
        {
            var sim = CreateSim(50);
            sim.Advance(10);

            var pos = new Vector3(0, 0, 0);
            float radius = 1000f;

            var queryResults = sim.QueryCells(pos, -1, radius);

            int forEachCount = 0;
            var counter = new CountProcessor();
            sim.ForEachNearby(pos, -1, radius, ref counter);
            forEachCount = counter.Count;

            Assert.AreEqual(queryResults.Count, forEachCount,
                "ForEachNearby and QueryCells should find the same number of agents");
        }

        private struct CountProcessor : Simulation.INeighborProcessor
        {
            public int Count;
            public void Process(Agent neighbor) => Count++;
        }

        [TestMethod]
        public void TestGridHandlesAgentAtWorldEdge()
        {
            var sim = CreateSim(5);

            // Place agent right at world edge.
            var agent = sim.Agents[0];
            agent.CurrentState = Agent.State.Wandering;
            agent.Position = new Vector3(WorldMaxs.X, WorldMaxs.Y, 0);
            sim.MoveInGrid(agent);

            Assert.IsTrue(agent.CellIndex >= 0, "Agent at world edge should still be in a valid cell");

            // Should be queryable.
            var results = sim.QueryCells(agent.Position, -1, 100f);
            bool found = false;
            foreach (var r in results)
            {
                if (r.Index == agent.Index) { found = true; break; }
            }
            Assert.IsTrue(found, "Agent at world edge should be found by query");
        }
    }
}
