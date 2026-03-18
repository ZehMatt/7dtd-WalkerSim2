using BenchmarkDotNet.Attributes;

namespace WalkerSim.Benchmarks
{
    [MemoryDiagnoser]
    public class GridBenchmarks
    {
        private Simulation _sim;
        private Vector3 _queryPos;
        private FixedBufferList<Agent> _buffer;

        [GlobalSetup]
        public void Setup()
        {
            _sim = SimulationFixture.Create();
            _queryPos = Vector3.Zero;
            _buffer = new FixedBufferList<Agent>(1024);
        }

        [Benchmark]
        public int QueryCells_200()
        {
            _buffer.Clear();
            _sim.QueryCells(_queryPos, -1, 200f, _buffer);
            return _buffer.Count;
        }

        [Benchmark]
        public int QueryCells_500()
        {
            _buffer.Clear();
            _sim.QueryCells(_queryPos, -1, 500f, _buffer);
            return _buffer.Count;
        }

        [Benchmark]
        public int QueryCells_1000()
        {
            _buffer.Clear();
            _sim.QueryCells(_queryPos, -1, 1000f, _buffer);
            return _buffer.Count;
        }

        [Benchmark]
        public void ForEachNearby_200()
        {
            var processor = new CountProcessor();
            _sim.ForEachNearby(_queryPos, -1, 200f, ref processor);
        }

        [Benchmark]
        public void ForEachNearby_500()
        {
            var processor = new CountProcessor();
            _sim.ForEachNearby(_queryPos, -1, 500f, ref processor);
        }

        private struct CountProcessor : Simulation.INeighborProcessor
        {
            public int Count;
            public void Process(Agent neighbor) => Count++;
        }
    }
}
