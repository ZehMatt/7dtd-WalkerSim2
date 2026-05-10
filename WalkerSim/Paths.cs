using System;
using System.Collections.Generic;
using System.IO;

namespace WalkerSim
{
    public static class Paths
    {
        public sealed class Context
        {
            public string UserDataFolder;
            public List<string> InstallRoots = new List<string>();
            public List<string> LoadedModPaths;
        }

        public static List<string> GetPrefabSearchPaths(Context ctx)
        {
            var result = new List<string>();
            if (ctx == null)
                return result;

            if (!string.IsNullOrEmpty(ctx.UserDataFolder))
                AddIfExists(result, Path.Combine(ctx.UserDataFolder, "LocalPrefabs"));

            foreach (var modPath in EnumerateModPaths(ctx))
                AddIfExists(result, Path.Combine(modPath, "Prefabs"));

            foreach (var install in ctx.InstallRoots)
            {
                if (string.IsNullOrEmpty(install))
                    continue;
                AddIfExists(result, Path.Combine(install, "Data", "Prefabs"));
            }

            return result;
        }

        public static List<string> GetWorldFolders(Context ctx)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (ctx == null)
                return result;

            if (!string.IsNullOrEmpty(ctx.UserDataFolder))
                AddWorldChildren(result, seen, Path.Combine(ctx.UserDataFolder, "GeneratedWorlds"));

            foreach (var modPath in EnumerateModPaths(ctx))
                AddWorldChildren(result, seen, Path.Combine(modPath, "Worlds"));

            foreach (var install in ctx.InstallRoots)
            {
                if (string.IsNullOrEmpty(install))
                    continue;
                AddWorldChildren(result, seen, Path.Combine(install, "Data", "Worlds"));
            }

            return result;
        }

        public static bool TryGetWorldPath(IEnumerable<string> worldFolders, string worldName, out string worldPath)
        {
            foreach (var folder in worldFolders)
            {
                if (Path.GetFileName(folder).Equals(worldName, StringComparison.OrdinalIgnoreCase))
                {
                    worldPath = folder;
                    return true;
                }
            }
            worldPath = null;
            return false;
        }

        private static IEnumerable<string> EnumerateModPaths(Context ctx)
        {
            if (ctx.LoadedModPaths != null)
            {
                foreach (var p in ctx.LoadedModPaths)
                {
                    if (!string.IsNullOrEmpty(p) && Directory.Exists(p))
                        yield return p;
                }
                yield break;
            }

            if (!string.IsNullOrEmpty(ctx.UserDataFolder))
            {
                foreach (var p in EnumerateModSubdirs(Path.Combine(ctx.UserDataFolder, "Mods")))
                    yield return p;
            }

            foreach (var install in ctx.InstallRoots)
            {
                if (string.IsNullOrEmpty(install))
                    continue;
                foreach (var p in EnumerateModSubdirs(Path.Combine(install, "Mods")))
                    yield return p;
            }
        }

        private static IEnumerable<string> EnumerateModSubdirs(string modsRoot)
        {
            if (!Directory.Exists(modsRoot))
                yield break;

            string[] dirs;
            try
            {
                dirs = Directory.GetDirectories(modsRoot);
            }
            catch
            {
                yield break;
            }

            Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
            foreach (var d in dirs)
                yield return d;
        }

        private static void AddIfExists(List<string> result, string path)
        {
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                result.Add(path);
        }

        private static void AddWorldChildren(List<string> result, HashSet<string> seen, string parent)
        {
            if (!Directory.Exists(parent))
                return;

            string[] dirs;
            try
            {
                dirs = Directory.GetDirectories(parent);
            }
            catch
            {
                return;
            }

            Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
            foreach (var d in dirs)
            {
                var name = Path.GetFileName(d);
                if (seen.Add(name))
                    result.Add(d);
            }
        }
    }
}
