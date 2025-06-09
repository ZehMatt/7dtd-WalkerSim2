using System.Diagnostics;

namespace WalkerSim
{
    internal static class VectorUtils
    {
        static VectorUtils()
        {
            TestConvert();
        }

        // The game has following coordinate system viewed from the map perspective.
        // -X = Left, +X = Right
        // -Z = Down, +Z = Up
        // Simulation operates in top-left origin coordinate system, so +Z = Down, -Z = Up
        public static Vector3 ToSim(UnityEngine.Vector3 vec)
        {
            return new Vector3(vec.x, -vec.z, vec.y);
        }

        public static Vector3 ToSim(Vector3i vec)
        {
            return new Vector3(vec.x, -vec.z, vec.y);
        }

        public static UnityEngine.Vector3 ToUnity(Vector3 vec)
        {
            return new UnityEngine.Vector3(vec.X, vec.Z, -vec.Y);
        }

        public static void TestConvert()
        {
            var vec1 = new Vector3(-15, -16, -17);
            var vec2 = ToUnity(vec1);
            var vec3 = ToSim(vec2);

            Debug.Assert(vec1 == vec3);
        }
    }
}
