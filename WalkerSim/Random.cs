using System;

namespace WalkerSim
{
    internal class Random
    {
        public UInt32 State0;
        public UInt32 State1;

        private const UInt32 MaxValue = 0x7FFFFFFFU;

        public Random(Int32 seed)
        {
            State0 = (UInt32)seed;
            State1 = (UInt32)seed * 10515173;
        }

        public Random(UInt32 state0, UInt32 state1)
        {
            State0 = state0;
            State1 = state1;
        }

        private static UInt32 Rotr(UInt32 value, Int32 count)
        {
            return (value >> count) | (value << (32 - count));
        }

        public UInt32 Generate()
        {
            var state0 = State0;
            var state1 = State1;
            State0 += Rotr(state1 ^ 0x1234567F, 7);
            State1 = Rotr(state0, 3);
            return State1;
        }

        public Int32 Next()
        {
            // Stick to the rules of C# Random, negatives are never returned.
            return (Int32)(Generate() & MaxValue);
        }

        public Int32 Next(Int32 max)
        {
            if (max <= 0)
                throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than 0.");

            return Next() % max;
        }

        public Int32 Next(Int32 min, Int32 max)
        {
            if (min > max)
                throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");

            if (min == max)
                return min;

            return min + Next(max - min);
        }

        public Single NextSingle()
        {
            return (Next() / (Single)(Int32.MaxValue + 1.0));
        }

        public Double NextDouble()
        {
            return (Next() / (Double)(Int32.MaxValue + 1.0));
        }
    }
}
