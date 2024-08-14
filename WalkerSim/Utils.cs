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

        public static string GetWorldLocationString(Config.WorldLocation value)
        {
            switch (value)
            {
                case Config.WorldLocation.None:
                    return "None";
                case Config.WorldLocation.RandomBorderLocation:
                    return "Random Border Location";
                case Config.WorldLocation.RandomLocation:
                    return "Random Location";
                case Config.WorldLocation.RandomPOI:
                    return "Random POI";
                case Config.WorldLocation.Mixed:
                    return "Mixed";
                default:
                    break;
            }
            throw new Exception("Invalid value");
        }
    }
}
