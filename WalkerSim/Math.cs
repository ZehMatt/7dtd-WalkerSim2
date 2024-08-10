using System.Runtime.CompilerServices;

namespace WalkerSim
{
    internal class Math
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap(float value, float min1, float max1, float min2, float max2)
        {
            return (value - min1) / (max1 - min1) * (max2 - min2) + min2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int value)
        {
            return value < 0 ? -value : value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Ceiling(float value)
        {
            return (float)System.Math.Ceiling(value);
        }
    }
}
