using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WalkerSim.Tests
{
    [TestClass]
    public class EventTests
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
        public void TestAddSoundEvent()
        {
            var sim = CreateSim();
            sim.AddSoundEvent(new Vector3(100, 100, 0), 500f, 10f);

            var events = sim.Events;
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(Simulation.EventType.Noise, events[0].Type);
            Assert.AreEqual(500f, events[0].Radius);
        }

        [TestMethod]
        public void TestEventsDecay()
        {
            var sim = CreateSim();
            // Short duration event.
            sim.AddSoundEvent(new Vector3(100, 100, 0), 500f, 0.05f);

            Assert.AreEqual(1, sim.Events.Count);

            // Tick enough for it to decay (TickRate ~= 0.0333s, so 2 ticks should expire 0.05s).
            sim.Advance(3);

            Assert.AreEqual(0, sim.Events.Count);
        }

        [TestMethod]
        public void TestEventMergeCloseEvents()
        {
            var sim = CreateSim();
            // Two events within merge distance (25m) and merge radius diff (5m).
            sim.AddSoundEvent(new Vector3(100, 100, 0), 500f, 10f);
            sim.AddSoundEvent(new Vector3(110, 110, 0), 502f, 5f);

            // Should merge into one event.
            Assert.AreEqual(1, sim.Events.Count);
            // Duration should be max of the two.
            Assert.AreEqual(10f, sim.Events[0].Duration);
        }

        [TestMethod]
        public void TestEventNoMergeFarEvents()
        {
            var sim = CreateSim();
            // Two events far apart.
            sim.AddSoundEvent(new Vector3(100, 100, 0), 500f, 10f);
            sim.AddSoundEvent(new Vector3(1000, 1000, 0), 500f, 10f);

            Assert.AreEqual(2, sim.Events.Count);
        }

        [TestMethod]
        public void TestEventSwallowSmaller()
        {
            var sim = CreateSim();
            // Add small event, then a larger one that encompasses it.
            sim.AddSoundEvent(new Vector3(100, 100, 0), 50f, 5f);
            sim.AddSoundEvent(new Vector3(105, 105, 0), 200f, 10f);

            // The small event should be swallowed.
            Assert.AreEqual(1, sim.Events.Count);
            Assert.AreEqual(200f, sim.Events[0].Radius);
        }

        [TestMethod]
        public void TestEventBlockedDuringBloodmoon()
        {
            var config = Config.GetDefault();
            config.PopulationDensity = 5;
            config.PauseDuringBloodmoon = true;

            var sim = new Simulation();
            sim.SetWorldSize(WorldMins, WorldMaxs);
            sim.Reset(config);
            sim.SetIsBloodmoon(true);

            sim.AddSoundEvent(new Vector3(100, 100, 0), 500f, 10f);
            Assert.AreEqual(0, sim.Events.Count, "Events should be blocked during bloodmoon");
        }

        [TestMethod]
        public void TestEventAllowedWhenBloodmoonPauseDisabled()
        {
            var config = Config.GetDefault();
            config.PopulationDensity = 5;
            config.PauseDuringBloodmoon = false;

            var sim = new Simulation();
            sim.SetWorldSize(WorldMins, WorldMaxs);
            sim.Reset(config);
            sim.SetIsBloodmoon(true);

            sim.AddSoundEvent(new Vector3(100, 100, 0), 500f, 10f);
            Assert.AreEqual(1, sim.Events.Count, "Events should still work when bloodmoon pause is off");
        }

        [TestMethod]
        public void TestMultipleEventsDecayIndependently()
        {
            var sim = CreateSim();
            sim.AddSoundEvent(new Vector3(100, 100, 0), 500f, 0.05f);    // Short
            sim.AddSoundEvent(new Vector3(5000, 5000, 0), 500f, 100f);   // Long, far away

            Assert.AreEqual(2, sim.Events.Count);

            sim.Advance(3);

            // Short event decayed, long one remains.
            Assert.AreEqual(1, sim.Events.Count);
        }
    }
}
