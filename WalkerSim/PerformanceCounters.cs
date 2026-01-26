using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace WalkerSim
{
    internal static class PerformanceCounters
    {
#if PROFILE
        private static readonly ConcurrentDictionary<string, TimeMeasurement> _counters = new ConcurrentDictionary<string, TimeMeasurement>();
#endif

        public struct ProfileScope : IDisposable
        {
#if PROFILE
            private readonly string _name;
            private readonly Stopwatch _sw;

            internal ProfileScope(string name)
            {
                _name = name;
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _sw.Stop();
                var elapsed = (float)_sw.Elapsed.TotalSeconds;
                
                var counter = _counters.GetOrAdd(_name, _ => new TimeMeasurement());
                counter.Add(elapsed);
            }
#else
            public void Dispose() { }
#endif
        }

        public static ProfileScope Profile(string name)
        {
#if PROFILE
            return new ProfileScope(name);
#else
            return default(ProfileScope);
#endif
        }

        public static double GetAverage(string name)
        {
#if PROFILE
            if (_counters.TryGetValue(name, out var counter))
            {
                return counter.Average;
            }
#endif
            return 0.0;
        }

        [Conditional("PROFILE")]
        public static void Report()
        {
#if PROFILE
            var sb = new System.Text.StringBuilder();
            sb.Append("Profile:");
            int count = 0;
            
            var sortedCounters = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, TimeMeasurement>>(_counters);
            sortedCounters.Sort((a, b) => string.Compare(a.Key, b.Key, System.StringComparison.Ordinal));
            
            foreach (var kvp in sortedCounters)
            {
                if (count > 0 && count % 5 == 0)
                {
                    Logging.Out(sb.ToString());
                    sb.Clear();
                    sb.Append("  ");
                }
                sb.Append($" {kvp.Key}={kvp.Value.Average * 1000:F4}ms");
                count++;
            }
            if (sb.Length > 0)
            {
                Logging.Out(sb.ToString());
            }
#endif
        }
    }
}
