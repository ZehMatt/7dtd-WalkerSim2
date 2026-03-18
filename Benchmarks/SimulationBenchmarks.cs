using BenchmarkDotNet.Attributes;

namespace WalkerSim.Benchmarks
{
    [MemoryDiagnoser]
    public class SimulationBenchmarks
    {
        private Simulation _sim;
        private Agent _agent;

        [GlobalSetup]
        public void Setup()
        {
            _sim = SimulationFixture.Create();
            _agent = _sim.Agents[0];
        }

        [Benchmark]
        public void Tick()
        {
            _sim.Tick();
        }

        [Benchmark]
        public void UpdateAgent()
        {
            _sim.UpdateAgent(_agent);
        }

        [Benchmark]
        public void ApplyMovement()
        {
            _sim.ApplyMovement(_agent, Simulation.Constants.TickRate, 1.0f);
        }

        [Benchmark]
        public void MoveInGrid()
        {
            _sim.MoveInGrid(_agent);
        }

        [Benchmark]
        public int AgentHash()
        {
            return Simulation.AgentHash(42, 1000, 0);
        }
    }

    [MemoryDiagnoser]
    public class SimulationSetupBenchmarks
    {
        private Simulation _sim;

        [GlobalSetup]
        public void Setup()
        {
            _sim = new Simulation();
            _sim.EditorMode = true;
            _sim.SetWorldSize(
                new Vector3(-5120, -5120, 0),
                new Vector3(5120, 5120, 255));
        }

        [Benchmark]
        public void Reset()
        {
            var config = Config.GetDefault();
            config.PopulationDensity = 5;
            _sim.Reset(config);
        }
    }

    [MemoryDiagnoser]
    public class SimulationSaveBenchmarks
    {
        private Simulation _sim;

        [GlobalSetup]
        public void Setup()
        {
            _sim = SimulationFixture.Create();
        }

        [Benchmark]
        public long Save()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                _sim.Save(ms);
                return ms.Length;
            }
        }

        [Benchmark]
        public bool SaveLoad()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                _sim.Save(ms);
                ms.Position = 0;
                return _sim.Load(ms);
            }
        }
    }
}
