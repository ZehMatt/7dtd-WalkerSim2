using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WalkerSim
{
    public struct Vector3 : IEquatable<Vector3>
    {
        public static readonly Vector3 Zero = new Vector3(0f, 0f, 0f);
        public static readonly Vector3 One = new Vector3(1f, 1f, 1f);

        public float X;
        public float Y;
        public float Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(float x, float y)
        {
            X = x;
            Y = y;
            Z = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Magnitude()
        {
            return Vector3.Magnitude(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3 a, Vector3 b)
        {
            return (float)System.Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(Vector3 a, Vector3 b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(Vector3 vec)
        {
            return (float)System.Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MagnitudeSqr(Vector3 vec)
        {
            return vec.X * vec.X + vec.Y * vec.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 vec)
        {
            float mag = Vector3.Magnitude(vec);
            if (mag == 0)
                return Vector3.Zero;
            if (vec.X != 0)
                vec.X /= mag;
            if (vec.Y != 0)
                vec.Y /= mag;
            if (vec.Z != 0)
                vec.Z /= mag;
            return vec;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 a, float b)
        {
            return new Vector3(a.X / b, a.Y / b, a.Z / b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 a, float b)
        {
            return new Vector3(a.X * b, a.Y * b, a.Z * b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(Vector3 v, Vector3 min, Vector3 max)
        {
            return new Vector3(
                System.Math.Max(System.Math.Min(v.X, max.X), min.X),
                System.Math.Max(System.Math.Min(v.Y, max.Y), min.Y),
                System.Math.Max(System.Math.Min(v.Z, max.Z), min.Z)
                );
        }

        [Conditional("DEBUG")]
        public void Validate(
            [CallerMemberName] string callingMethod = "",
            [CallerFilePath] string callingFilePath = "",
            [CallerLineNumber] int callingFileLineNumber = 0
            )
        {
            if (float.IsNaN(X))
                throw new Exception(string.Format("Nan Detected on {0}, Func: {1}, File: {2}, Line: {3}", "X", callingMethod, callingFilePath, callingFileLineNumber));
            if (float.IsNaN(Y))
                throw new Exception(string.Format("Nan Detected on {0}, Func: {1}, File: {2}, Line: {3}", "Y", callingMethod, callingFilePath, callingFileLineNumber));
            if (float.IsNaN(Z))
                throw new Exception(string.Format("Nan Detected on {0}, Func: {1}, File: {2}, Line: {3}", "Z", callingMethod, callingFilePath, callingFileLineNumber));
        }

        public static Vector3 Lerp(Vector3 start, Vector3 end, float t)
        {
            t = Math.Clamp(t, 0, 1); // Ensure t is within the [0, 1] range
            return new Vector3(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t,
                start.Z + (end.Z - start.Z) * t
            );
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }

        public static Vector3 Parse(string s, bool isUnity)
        {
            var parts = s.Split(',');
            if (parts.Length != 3)
            {
                throw new FormatException("Invalid position format");
            }

            if (isUnity)
                return new Vector3(float.Parse(parts[0]), -float.Parse(parts[2]), float.Parse(parts[1]));
            else
                return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        public bool Equals(Vector3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Vector3 lhs, Vector3 rhs) => !(lhs == rhs);

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals((Vector3)obj);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
        }
    }
}
