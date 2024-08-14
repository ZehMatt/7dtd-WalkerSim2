using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace WalkerSim.Viewer
{
    internal static class Worlds
    {
        static List<string> _worldFolders = new List<string>();

        public static IReadOnlyList<string> WorldFolders
        {
            get => _worldFolders;
        }

        private static string GetInstallPath64()
        {
            try
            {
                using (var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var steamReg = view32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 251570", false))
                    {
                        return steamReg.GetValue("InstallLocation") as string;
                    }
                }
            }
            catch (System.Exception)
            {
                return null;
            }

        }

        private static string GetInstallPath32()
        {
            try
            {
                using (var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (var steamReg = view32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 251570", false))
                    {
                        return steamReg.GetValue("InstallLocation") as string;
                    }
                }
            }
            catch (System.Exception)
            {
                return null;
            }

        }

        private static string GetInstallPath()
        {
            var path = GetInstallPath64();
            if (path == null)
            {
                path = GetInstallPath32();
            }
            return path;
        }

        public static void FindWorlds()
        {
            var installPath = GetInstallPath();
            if (installPath == null || !Directory.Exists(installPath))
            {
                // TODO: Add a few fall-backs here.
                return;
            }

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
