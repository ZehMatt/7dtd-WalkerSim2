using System;

namespace WalkerSim
{
    internal static class Utils
    {
        public static TimeSpan Measure(Action action)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            action();
            watch.Stop();
            return watch.Elapsed;
        }
    }
}
