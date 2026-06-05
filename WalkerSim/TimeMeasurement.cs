using System.Diagnostics;

namespace WalkerSim
{
    internal class TimeMeasurement
    {
        private Stopwatch _sw = new Stopwatch();
        private float[] _samples = new float[64];

        private volatile uint _index = 0;
        private volatile int _count = 0;
        private float _average = 0.0f;

        public void Add(float time)
        {
            _samples[_index++ % _samples.Length] = time;
            _count = System.Math.Min(_count + 1, _samples.Length);
            ComputeAverage();
        }

        public void Reset()
        {
            _index = 0;
            _count = 0;
        }

        public void Restart()
        {
            _sw.Restart();
        }

        public float Capture()
        {
            var elapsed = (float)_sw.Elapsed.TotalSeconds;
            Add(elapsed);
            return elapsed;
        }

        public float Elapsed()
        {
            return (float)_sw.Elapsed.TotalSeconds;
        }

        private void ComputeAverage()
        {
            if (_count == 0)
            {
                return;
            }

            float sum = 0.0f;
            for (int i = 0; i < _count; i++)
            {
                sum += _samples[i];
            }
            _average = sum / _count;
        }

        public float Average => _average;
    }
}
