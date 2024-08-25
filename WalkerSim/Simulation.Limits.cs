namespace WalkerSim
{
    internal partial class Simulation
    {
        public static class Limits
        {
            public const uint TicksToAdvanceOnStartup = 100;
            public const int MinAgents = 1;
            public const int MaxAgents = 30000;
            public const int MaxQuerySize = 128;
            public const double SpawnDespawnDelay = 0.03;
            public const float SoundDecayRate = 20.0f;
        }
    }
}
