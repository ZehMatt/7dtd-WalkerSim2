
namespace WalkerSim
{
    internal static class Utils
    {
        public static System.TimeSpan Measure(System.Action action)
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
            throw new System.Exception("Invalid value");
        }

        public static string GetPostSpawnBehaviorString(Config.PostSpawnBehavior value)
        {
            switch (value)
            {
                case Config.PostSpawnBehavior.Wander:
                    return "Wander";
                case Config.PostSpawnBehavior.ChaseActivator:
                    return "Chase Activator";
                case Config.PostSpawnBehavior.Nothing:
                    return "Do Nothing";
                default:
                    break;
            }
            throw new System.Exception("Invalid value");
        }

        public static Vector3 GetRandomVector3(System.Random prng, Vector3 mins, Vector3 maxs, float borderSize = 250)
        {
            float x0 = (float)prng.NextDouble();
            float y0 = (float)prng.NextDouble();
            float x = Math.Remap(x0, 0f, 1f, mins.X + borderSize, maxs.X - borderSize);
            float y = Math.Remap(y0, 0f, 1f, mins.Y + borderSize, maxs.Y - borderSize);
            return new Vector3(x, y);
        }

        public static Drawing.Color ParseColor(string value)
        {
            if (value == "")
            {
                return Drawing.Color.Transparent;
            }
            try
            {
                var res = Drawing.Color.FromHtml(value);
                return res;
            }
            catch (System.Exception)
            {
                return Drawing.Color.Transparent;
            }
        }

        public static string ColorToHexString(Drawing.Color color)
        {
            return color.ToHtml();
        }

        public static bool IsDebugMode()
        {
            // Check if the DEBUG constant is defined
#if DEBUG
            return true;
#else
            return false;
#endif
        }

    }
}
