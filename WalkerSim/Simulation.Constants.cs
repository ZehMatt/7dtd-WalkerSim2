namespace WalkerSim
{
    internal partial class Simulation
    {
        internal class Constants
        {
            public const uint SaveMagic = 0x4D534B57; // WKSM

            // Increment this in case of a breaking change in the save format.
            // Version 8: New configuration SoundDistanceScale.
            // Version 9: Might have saved an empty state, skip loading the old one.
            public const uint SaveVersion = 9;

            public const int MaxUpdateCountPerTick = 1000;

            public const int TicksPerSecond = 30;
            public const float TickRate = 1f / TicksPerSecond;
            public const int TickRateMs = 1000 / TicksPerSecond;

            public const float WalkSpeed = 0.8f; // 0.8 meters per second
        }
    }
}
