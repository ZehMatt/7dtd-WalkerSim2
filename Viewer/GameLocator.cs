using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WalkerSim.Viewer
{
    public class GameLocator
    {
        private const int AppID = 251570;

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

        public static List<string> FindGamePaths()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Get the install path from the registry
            paths.Add(GetInstallPath());

            // Get Steam installation path from registry
            string steamPath = GetSteamPath();
            if (!string.IsNullOrEmpty(steamPath))
            {
                // Get all Steam library folders
                List<string> libraryFolders = GetSteamLibraryFolders(steamPath);

                foreach (string library in libraryFolders)
                {
                    string manifestPath = Path.Combine(library, "steamapps", $"appmanifest_{AppID}.acf");
                    if (File.Exists(manifestPath))
                    {
                        string installDir = ParseInstallDirFromManifest(manifestPath);
                        if (!string.IsNullOrEmpty(installDir))
                        {
                            string gamePath = Path.Combine(library, "steamapps", "common", installDir);
                            if (Directory.Exists(gamePath))
                            {
                                paths.Add(gamePath);
                            }
                        }
                    }
                }
            }

            return paths.ToList();
        }

        private static string GetSteamPath()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        object path = key.GetValue("SteamPath");
                        return path?.ToString().Replace('/', '\\');
                    }
                }
            }
            catch { }

            return null;
        }

        private static List<string> GetSteamLibraryFolders(string steamPath)
        {
            var uniqueLibraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string libraryFoldersFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersFile))
                return uniqueLibraries.ToList();

            try
            {
                string content = File.ReadAllText(libraryFoldersFile);
                var matches = Regex.Matches(content, @"\""path\""\s+\""([^\""]+)\""");

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        string path = match.Groups[1].Value.Replace(@"\\", @"\");
                        if (Directory.Exists(path))
                            uniqueLibraries.Add(path);
                    }
                }
            }
            catch { }

            return uniqueLibraries.ToList();
        }

        private static string ParseInstallDirFromManifest(string manifestPath)
        {
            try
            {
                string content = File.ReadAllText(manifestPath);
                Match match = Regex.Match(content, @"\""installdir\""\s+\""([^\""]+)\""");
                return match.Success ? match.Groups[1].Value : null;
            }
            catch { }

            return null;
        }
    }

}
