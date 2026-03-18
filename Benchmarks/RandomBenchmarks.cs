using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace WalkerSim.Benchmarks
{
    [MemoryDiagnoser]
    public class RandomBenchmarks
    {
        private Random _rng;
        private List<int> _shuffleList;

        [GlobalSetup]
        public void Setup()
        {
            _rng = new Random(12345);
            _shuffleList = new List<int>(100);
            for (int i = 0; i < 100; i++)
                _shuffleList.Add(i);
        }

        [Benchmark]
        public int Next() => _rng.Next();

        [Benchmark]
        public int NextMax() => _rng.Next(1000);

        [Benchmark]
        public int NextRange() => _rng.Next(50, 500);

        [Benchmark]
        public float NextSingle() => _rng.NextSingle();

        [Benchmark]
        public double NextDouble() => _rng.NextDouble();

        [Benchmark]
        public void ShuffleList() => _rng.ShuffleList(_shuffleList);
    }
}
