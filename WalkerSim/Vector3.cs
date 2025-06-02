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
        public static float Distance2D(Vector3 a, Vector3 b) =>
            (float)System.Math.Sqrt(Distance2DSqr(a, b));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance2DSqr(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude2D(Vector3 vec) =>
            (float)System.Math.Sqrt(Magnitude2DSqr(vec));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Magnitude2D() => Magnitude2D(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude2DSqr(Vector3 vec) =>
            vec.X * vec.X + vec.Y * vec.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3 a, Vector3 b) =>
            (float)System.Math.Sqrt(DistanceSqr(a, b));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(Vector3 vec) =>
            (float)System.Math.Sqrt(MagnitudeSqr(vec));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Magnitude() => Magnitude(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MagnitudeSqr(Vector3 vec) =>
            vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 vec)
        {
            float mag = Magnitude(vec);
            return mag > 0f ? vec / mag : Zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 a, Vector3 b) =>
            new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a, Vector3 b) =>
            new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 a, float b) =>
            new Vector3(a.X / b, a.Y / b, a.Z / b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 a, float b) =>
            new Vector3(a.X * b, a.Y * b, a.Z * b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(Vector3 v, Vector3 min, Vector3 max) =>
            new Vector3(
                Math.Clamp(v.X, min.X, max.X),
                Math.Clamp(v.Y, min.Y, max.Y),
                Math.Clamp(v.Z, min.Z, max.Z)
            );

        [Conditional("DEBUG")]
        public void Validate(
            [CallerMemberName] string callingMethod = "",
            [CallerFilePath] string callingFilePath = "",
            [CallerLineNumber] int callingFileLineNumber = 0)
        {
            if (float.IsNaN(X))
                throw new Exception($"NaN detected in X (Method: {callingMethod}, File: {callingFilePath}, Line: {callingFileLineNumber})");
            if (float.IsNaN(Y))
                throw new Exception($"NaN detected in Y (Method: {callingMethod}, File: {callingFilePath}, Line: {callingFileLineNumber})");
            if (float.IsNaN(Z))
                throw new Exception($"NaN detected in Z (Method: {callingMethod}, File: {callingFilePath}, Line: {callingFileLineNumber})");
        }

        public static Vector3 Lerp(Vector3 start, Vector3 end, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector3(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t,
                start.Z + (end.Z - start.Z) * t
            );
        }

        public override string ToString() => $"({X}, {Y}, {Z})";

        public static Vector3 Parse(string s, bool isUnity)
        {
            var parts = s.Split(',');
            if (parts.Length != 3)
                throw new FormatException("Invalid Vector3 format. Expected 3 comma-separated floats.");

            float x = float.Parse(parts[0].Trim());
            float y = float.Parse(parts[1].Trim());
            float z = float.Parse(parts[2].Trim());

            return isUnity ? new Vector3(x, -z, y) : new Vector3(x, y, z);
        }

        public bool Equals(Vector3 other) => X == other.X && Y == other.Y && Z == other.Z;

        public static bool operator ==(Vector3 lhs, Vector3 rhs) => lhs.Equals(rhs);

        public static bool operator !=(Vector3 lhs, Vector3 rhs) => !(lhs == rhs);

        public override bool Equals(object obj) => obj is Vector3 other && Equals(other);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
    }
}
