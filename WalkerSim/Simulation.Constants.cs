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
            // Version 11: State for CityVisitor.
            public const uint SaveVersion = 11;

            public const uint MaxUpdateCountPerTick = 2000;

            public const uint TicksPerSecond = 40;
            public const float TickRate = 1f / TicksPerSecond;
            public const uint TickRateMs = 1000 / TicksPerSecond;

            public const float WalkSpeed = 1.1f; // 1.1 meters per second
        }
    }
}
