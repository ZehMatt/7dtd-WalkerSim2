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

        public static Vector3 GetRandomVector3(System.Random prng, Vector3 mins, Vector3 maxs, float borderSize = 250)
        {
            float x0 = (float)prng.NextDouble();
            float y0 = (float)prng.NextDouble();
            float x = Math.Remap(x0, 0f, 1f, mins.X + borderSize, maxs.X - borderSize);
            float y = Math.Remap(y0, 0f, 1f, mins.Y + borderSize, maxs.Y - borderSize);
            return new Vector3(x, y);
        }

        public static System.Drawing.Color ParseColor(string color)
        {
            if (color == "")
            {
                return System.Drawing.Color.Transparent;
            }
            try
            {
                var res = (System.Drawing.Color)System.Drawing.ColorTranslator.FromHtml(color);
                return res;
            }
            catch (Exception)
            {
                return System.Drawing.Color.Transparent;
            }
        }

        public static string ColorToHexString(System.Drawing.Color color)
        {
            return System.Drawing.ColorTranslator.ToHtml(color);
        }
    }
}
