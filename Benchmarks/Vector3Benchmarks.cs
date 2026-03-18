using BenchmarkDotNet.Attributes;

namespace WalkerSim.Benchmarks
{
    [MemoryDiagnoser]
    public class Vector3Benchmarks
    {
        private Vector3 _a;
        private Vector3 _b;

        [GlobalSetup]
        public void Setup()
        {
            _a = new Vector3(100.5f, 200.3f, 10f);
            _b = new Vector3(-50.2f, 300.1f, 20f);
        }

        [Benchmark]
        public float Distance2D() => Vector3.Distance2D(_a, _b);

        [Benchmark]
        public float Distance2DSqr() => Vector3.Distance2DSqr(_a, _b);

        [Benchmark]
        public float Distance3D() => Vector3.Distance(_a, _b);

        [Benchmark]
        public float Magnitude() => Vector3.Magnitude(_a);

        [Benchmark]
        public float Magnitude2D() => Vector3.Magnitude2D(_a);

        [Benchmark]
        public Vector3 Normalize() => Vector3.Normalize(_a);

        [Benchmark]
        public Vector3 Lerp() => Vector3.Lerp(_a, _b, 0.5f);

        [Benchmark]
        public Vector3 Add() => _a + _b;

        [Benchmark]
        public Vector3 Subtract() => _a - _b;

        [Benchmark]
        public Vector3 Scale() => _a * 2.5f;

        [Benchmark]
        public Vector3 Clamp() => Vector3.Clamp(_a, Vector3.Zero, new Vector3(150f, 150f, 150f));
    }
}
