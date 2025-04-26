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

        public static void FindWorlds()
        {
            var installPaths = GameLocator.FindGamePaths();
            foreach (var installPath in installPaths)
            {
                // Enumerate the worlds from game.
                var worldsPath = Path.Combine(installPath, "Data", "Worlds");
                if (Directory.Exists(worldsPath))
                {
                    foreach (var worldPath in Directory.EnumerateDirectories(worldsPath))
                    {
                        _worldFolders.Add(worldPath);
                    }
                }

                // Enumerate the generated worlds in %APPDATA%/7DaysToDie/GeneratedWorlds
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var pathToGeneratedWorlds = Path.Combine(appDataPath, "7DaysToDie", "GeneratedWorlds");

                if (Directory.Exists(pathToGeneratedWorlds))
                {
                    foreach (var worldPath in Directory.EnumerateDirectories(pathToGeneratedWorlds))
                    {
                        _worldFolders.Add(worldPath);
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
                                _worldFolders.Add(world);
                            }
                        }
                    }
                }
            }

            // Sort the list by folder name.
            _worldFolders.Sort((a, b) =>
            {
                var nameA = Path.GetFileName(a);
                var nameB = Path.GetFileName(b);
                return String.Compare(nameA, nameB);
            });
        }
    }
}
