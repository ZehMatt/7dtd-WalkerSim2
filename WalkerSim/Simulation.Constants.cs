namespace WalkerSim
{
    internal partial class Simulation
    {
        internal class Constants
        {
            public const uint SaveMagic = 0x4D534B57; // WKSM

            public const uint MaxUpdateCountPerTick = 500;

            public const uint TicksPerSecond = 30;
            public const float TickRate = 1f / TicksPerSecond;
            public const uint TickRateMs = 1000 / TicksPerSecond;

            public const uint SpawnCheckDelayMs = 25;
            public const int SpawnBorderSize = 12;
            public const int MaxSpawnsPerCheck = 3;

            // Increment this in case of a breaking change in the save format.
            // Version 8: New configuration SoundDistanceScale.
            // Version 9: Might have saved an empty state, skip loading the old one.
            // Version 11: State for CityVisitor.
            // Version 12: Change type of Ticks to uint.
            // Version 13: Unscaled Ticks.
            // Version 14: Max Health.
            // Version 15: Dismemberment state.
            // Version 16: Walk type.
            // Version 17: Boo
            public const uint SaveVersion = 17;
        }
    }
}
