using System;
using System.Collections.Generic;
using System.IO;

namespace WalkerSim.Editor
{
    internal static class Worlds
    {
        static List<string> _worldFolders = new List<string>();

        public static IReadOnlyList<string> WorldFolders
        {
            get => _worldFolders;
        }

        public static bool GetWorldPath(string worldName, out string worldPath)
        {
            foreach (var world in _worldFolders)
            {
                if (Path.GetFileName(world).Equals(worldName, StringComparison.OrdinalIgnoreCase))
                {
                    worldPath = world;
                    return true;
                }
            }
            worldPath = null;
            return false;
        }

        public static void FindWorlds()
        {
            var worldFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var installPaths = GameLocator.FindGamePaths();
            foreach (var installPath in installPaths)
            {
                // Enumerate the worlds from game.
                var worldsPath = Path.Combine(installPath, "Data", "Worlds");
                if (Directory.Exists(worldsPath))
                {
                    foreach (var worldPath in Directory.EnumerateDirectories(worldsPath))
                    {
                        worldFolders.Add(worldPath);
                    }
                }

                // Enumerate the generated worlds in %APPDATA%/7DaysToDie/GeneratedWorlds
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var pathToGeneratedWorlds = Path.Combine(appDataPath, "7DaysToDie", "GeneratedWorlds");

                if (Directory.Exists(pathToGeneratedWorlds))
                {
                    foreach (var worldPath in Directory.EnumerateDirectories(pathToGeneratedWorlds))
                    {
                        worldFolders.Add(worldPath);
                    }
                }

                // Enumerate from Mods folder.
                var modsPath = Path.Combine(installPath, "Mods");
                if (Directory.Exists(modsPath))
                {
                    foreach (var modPath in Directory.EnumerateDirectories(modsPath))
                    {
                        var worldPath = Path.Combine(modPath, "Worlds");
                        if (Directory.Exists(worldPath))
                        {
                            foreach (var world in Directory.EnumerateDirectories(worldPath))
                            {
                                worldFolders.Add(world);
                            }
                        }
                    }
                }
            }

            _worldFolders = new List<string>(worldFolders);

            // Sort the list by folder name.
            _worldFolders.Sort((a, b) =>
            {
                var nameA = Path.GetFileName(a);
                var nameB = Path.GetFileName(b);
                return String.Compare(nameA, nameB);
            });

            // Log the found worlds.
            Logging.Info($"Found {_worldFolders.Count} worlds:");
            foreach (var world in _worldFolders)
            {
                Logging.Info($"- {Path.GetFileName(world)} ({world})");
            }
        }
    }
}
