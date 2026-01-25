using System.Diagnostics;

namespace WalkerSim
{
    internal class TimeMeasurement
    {
        private Stopwatch _sw = new Stopwatch();
        private float[] _samples = new float[64];

        private int _index = 0;
        private int _count = 0;

        public void Add(float time)
        {
            _samples[_index % _samples.Length] = time;
            _index++;
            _count = System.Math.Min(_count + 1, _samples.Length);
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

        public float Average
        {
            get
            {
                if (_count == 0)
                    return 0.0f;
                float sum = 0.0f;
                for (int i = 0; i < _count; i++)
                {
                    sum += _samples[i];
                }
                return sum / _count;
            }
        }
    }
}
