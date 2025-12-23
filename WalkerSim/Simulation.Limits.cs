namespace WalkerSim
{
    internal partial class Simulation
    {
        public static class Limits
        {
            public const uint TicksToAdvanceOnStartup = 2000;
            public const int MinDensity = 1;
            public const int MaxDensity = 4_000;
            public const int MaxAgents = 1_000_000;
            public const double SpawnDespawnDelay = 0.03;
            public const float SoundDecayRate = 20.0f;
            public const uint MinSpawnDelayTicks = 300; // The time to wait before the next spawn attempt is made in ticks, 300 = 10 seconds.
        }
    }
}
