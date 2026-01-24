using System.Runtime.CompilerServices;

namespace WalkerSim
{
    internal class MathEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap(float value, float min1, float max1, float min2, float max2)
        {
            return (value - min1) / (max1 - min1) * (max2 - min2) + min2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            return System.Math.Min(System.Math.Max(value, min), max);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            return System.Math.Min(System.Math.Max(value, min), max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Ceiling(float value)
        {
            return (float)System.Math.Ceiling(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
