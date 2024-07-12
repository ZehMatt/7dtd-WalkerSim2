using System.Diagnostics;

namespace WalkerSim
{
    internal static class VectorUtils
    {
        static VectorUtils()
        {
            TestConvert();
        }

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
            var vec1 = new Vector3(-30, -30, -30);
            var vec2 = ToUnity(vec1);
            var vec3 = ToSim(vec2);

            Debug.Assert(vec1 == vec3);
        }
    }
}
