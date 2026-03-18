using BenchmarkDotNet.Attributes;

namespace WalkerSim.Benchmarks
{
    [MemoryDiagnoser]
    public class ProcessorBenchmarks
    {
        private Simulation _sim;
        private Simulation.State _state;
        private Agent _agent;

        [GlobalSetup]
        public void Setup()
        {
            _sim = SimulationFixture.Create();
            _state = _sim._state;
            _agent = _sim.Agents[0];
        }

        // --- Flock ---
        [Benchmark]
        public Vector3 FlockAny() => Simulation.FlockAny(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 FlockSame() => Simulation.FlockSame(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 FlockOther() => Simulation.FlockOther(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        // --- Align ---
        [Benchmark]
        public Vector3 AlignAny() => Simulation.AlignAny(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 AlignSame() => Simulation.AlignSame(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 AlignOther() => Simulation.AlignOther(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        // --- Avoid ---
        [Benchmark]
        public Vector3 AvoidAny() => Simulation.AvoidAny(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 AvoidSame() => Simulation.AvoidSame(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 AvoidOther() => Simulation.AvoidOther(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        // --- Wind ---
        [Benchmark]
        public Vector3 Wind() => Simulation.Wind(_sim, _state, _agent, 0f, 0.05f, 0f, 0f);

        [Benchmark]
        public Vector3 WindInverted() => Simulation.WindInverted(_sim, _state, _agent, 0f, 0.05f, 0f, 0f);

        // --- World Events ---
        [Benchmark]
        public Vector3 WorldEvents() => Simulation.WorldEvents(_sim, _state, _agent, 0f, 0.01f, 0f, 0f);

        // --- Map-dependent processors (early-out path without MapData) ---
        [Benchmark]
        public Vector3 StickToRoads() => Simulation.StickToRoads(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 AvoidRoads() => Simulation.AvoidRoads(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 StickToPOIs() => Simulation.StickToPOIs(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 AvoidPOIs() => Simulation.AvoidPOIs(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 PreferCities() => Simulation.PreferCities(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 AvoidCities() => Simulation.AvoidCities(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 CityVisitor() => Simulation.CityVisitor(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 StickToBiome() => Simulation.StickToBiome(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);

        [Benchmark]
        public Vector3 AvoidBiome() => Simulation.AvoidBiome(_sim, _state, _agent, 200f, 0.01f, 0f, 0f);
    }
}
