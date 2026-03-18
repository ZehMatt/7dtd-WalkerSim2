namespace WalkerSim.Benchmarks
{
    internal static class SimulationFixture
    {
        static readonly Vector3 WorldMins = new Vector3(-5120, -5120, 0);
        static readonly Vector3 WorldMaxs = new Vector3(5120, 5120, 255);

        public static Simulation Create(int populationDensity = 5)
        {
            var config = Config.GetDefault();
            config.PopulationDensity = populationDensity;

            var sim = new Simulation();
            sim.EditorMode = true;
            sim.SetWorldSize(WorldMins, WorldMaxs);
            sim.Reset(config);

            // Advance so all agents are wandering and have some velocity.
            sim.SetGameTime(2.0);
            sim.Tick();
            for (int i = 0; i < 100; i++)
                sim.Tick();

            // Benchmarks should measure the normal game path, not the editor path.
            sim.EditorMode = false;

            return sim;
        }
    }
}
