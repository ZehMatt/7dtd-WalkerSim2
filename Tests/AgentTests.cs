using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WalkerSim.Tests
{
    [TestClass]
    public class AgentTests
    {
        [TestMethod]
        public void TestAgentConstructor()
        {
            var agent = new Agent(5, 2);
            Assert.AreEqual(5, agent.Index);
            Assert.AreEqual(2, agent.Group);
            Assert.AreEqual(Agent.State.Inactive, agent.CurrentState);
            Assert.AreEqual(-1, agent.EntityId);
            Assert.AreEqual(-1, agent.CellIndex);
        }

        [TestMethod]
        public void TestResetSpawnData()
        {
            var agent = new Agent(0, 0);
            agent.EntityId = 42;
            agent.EntityClassId = 7;
            agent.Health = 100;
            agent.MaxHealth = 100;
            agent.TimeToDie = 1000;
            agent.Dismemberment = Agent.DismembermentMask.Head;
            agent.WalkType = Agent.MoveType.Crawling;

            agent.ResetSpawnData();

            Assert.AreEqual(-1, agent.EntityId);
            Assert.AreEqual(-1, agent.EntityClassId);
            Assert.AreEqual(-1f, agent.Health);
            Assert.AreEqual(-1f, agent.MaxHealth);
            Assert.AreEqual(ulong.MaxValue, agent.TimeToDie);
            Assert.AreEqual(Agent.DismembermentMask.None, agent.Dismemberment);
            Assert.AreEqual(Agent.MoveType.Normal, agent.WalkType);
        }

        [TestMethod]
        public void TestRoadNodeHistoryCircularBuffer()
        {
            var agent = new Agent(0, 0);

            // Push some nodes.
            agent.PushRoadNodeHistory(10);
            agent.PushRoadNodeHistory(20);
            agent.PushRoadNodeHistory(30);

            Assert.AreEqual(3, agent.RoadNodeHistoryCount);
            Assert.IsTrue(agent.IsInRoadNodeHistory(10));
            Assert.IsTrue(agent.IsInRoadNodeHistory(20));
            Assert.IsTrue(agent.IsInRoadNodeHistory(30));
            Assert.IsFalse(agent.IsInRoadNodeHistory(40));
        }

        [TestMethod]
        public void TestRoadNodeHistoryOverflow()
        {
            var agent = new Agent(0, 0);

            // Fill beyond capacity (20 entries).
            for (int i = 0; i < 25; i++)
            {
                agent.PushRoadNodeHistory(i);
            }

            // Count should be capped at history size.
            Assert.AreEqual(Agent.RoadNodeHistorySize, agent.RoadNodeHistoryCount);

            // Old entries (0-4) should have been overwritten.
            Assert.IsFalse(agent.IsInRoadNodeHistory(0));
            Assert.IsFalse(agent.IsInRoadNodeHistory(4));

            // Recent entries should be present.
            Assert.IsTrue(agent.IsInRoadNodeHistory(24));
            Assert.IsTrue(agent.IsInRoadNodeHistory(20));
        }

        [TestMethod]
        public void TestClearRoadNodeHistory()
        {
            var agent = new Agent(0, 0);
            agent.PushRoadNodeHistory(10);
            agent.PushRoadNodeHistory(20);

            agent.ClearRoadNodeHistory();

            Assert.AreEqual(0, agent.RoadNodeHistoryCount);
            Assert.IsFalse(agent.IsInRoadNodeHistory(10));
            Assert.IsFalse(agent.IsInRoadNodeHistory(20));
        }

        [TestMethod]
        public void TestGetDistance()
        {
            var a = new Agent(0, 0);
            a.Position = new Vector3(0, 0, 0);

            var b = new Agent(1, 0);
            b.Position = new Vector3(3, 4, 0);

            // 2D distance: sqrt(9 + 16) = 5
            Assert.AreEqual(5f, a.GetDistance(b), 0.001f);
            Assert.AreEqual(5f, a.GetDistance(new Vector3(3, 4, 0)), 0.001f);
        }

        [TestMethod]
        public void TestDismembermentFlags()
        {
            var agent = new Agent(0, 0);
            agent.Dismemberment = Agent.DismembermentMask.Head | Agent.DismembermentMask.LeftUpperArm;

            Assert.IsTrue(agent.Dismemberment.HasFlag(Agent.DismembermentMask.Head));
            Assert.IsTrue(agent.Dismemberment.HasFlag(Agent.DismembermentMask.LeftUpperArm));
            Assert.IsFalse(agent.Dismemberment.HasFlag(Agent.DismembermentMask.RightUpperLeg));

            // LowerBody is a composite flag.
            agent.Dismemberment = Agent.DismembermentMask.LowerBody;
            Assert.IsTrue(agent.Dismemberment.HasFlag(Agent.DismembermentMask.LeftUpperLeg));
            Assert.IsTrue(agent.Dismemberment.HasFlag(Agent.DismembermentMask.RightLowerLeg));
        }
    }
}
