using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Editor
{
    internal static class GameLocator
    {
        private const int AppID = 251570;

        public static WalkerSim.Paths.Context BuildContext()
        {
            return new WalkerSim.Paths.Context
            {
                UserDataFolder = ResolveUserDataFolder(),
                InstallRoots = FindInstallRoots(),
            };
        }

        public static string ResolveUserDataFolder()
        {
            var overrideValue = EditorSettings.Instance.UserDataFolder;
            if (!string.IsNullOrWhiteSpace(overrideValue) && Directory.Exists(overrideValue))
                return overrideValue;

            return GetDefaultUserDataFolder();
        }

        public static string GetDefaultUserDataFolder()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "7DaysToDie");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var localShare = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localShare, "7DaysToDie");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(home, "Library", "Application Support", "7DaysToDie");
            }
            return null;
        }

        public static List<string> FindInstallRoots()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WalkerSim.Logging.Info("Checking registry for game install...");
                var reg = GetInstallPathFromRegistry();
                if (reg != null)
                {
                    WalkerSim.Logging.Info("Found game via registry: {0}", reg);
                    paths.Add(reg);
                }
            }

            WalkerSim.Logging.Info("Scanning Steam libraries...");
            foreach (var steamRoot in GetSteamRootPaths())
            {
                WalkerSim.Logging.Info("Checking Steam root: {0}", steamRoot);
                foreach (var lib in GetSteamLibraryFolders(steamRoot))
                {
                    var manifest = Path.Combine(lib, "steamapps", $"appmanifest_{AppID}.acf");
                    if (File.Exists(manifest))
                    {
                        var dir = ParseInstallDirFromManifest(manifest);
                        if (!string.IsNullOrEmpty(dir))
                        {
                            var gamePath = Path.Combine(lib, "steamapps", "common", dir);
                            if (Directory.Exists(gamePath))
                            {
                                WalkerSim.Logging.Info("Found game via Steam: {0}", gamePath);
                                paths.Add(gamePath);
                            }
                        }
                    }
                }
            }

            var exe = Path.GetDirectoryName(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
            if (exe != null)
            {
                var candidate = Path.GetFullPath(Path.Combine(exe, "..", ".."));
                if (Directory.Exists(candidate))
                    paths.Add(candidate);
            }

            foreach (var extra in EditorSettings.Instance.GameFolders)
            {
                if (Directory.Exists(extra))
                    paths.Add(extra);
            }

            return paths.Where(p => !string.IsNullOrEmpty(p) && Directory.Exists(p)).ToList();
        }

        private static IEnumerable<string> GetSteamRootPaths()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var reg = GetSteamPathFromRegistry();
                if (!string.IsNullOrEmpty(reg))
                    yield return reg;

                var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                yield return Path.Combine(pf, "Steam");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                yield return Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam");
                yield return Path.Combine(home, ".local", "share", "Steam");
                yield return Path.Combine(home, ".steam", "steam");
                yield return Path.Combine(home, ".steam", "root");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                yield return Path.Combine(home, "Library", "Application Support", "Steam");
            }
        }

        private static string GetInstallPathFromRegistry()
        {
#pragma warning disable CA1416
            foreach (var view in new[] { Microsoft.Win32.RegistryView.Registry64, Microsoft.Win32.RegistryView.Registry32 })
            {
                try
                {
                    using var hive = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, view);
                    using var key = hive.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {AppID}", false);
                    if (key?.GetValue("InstallLocation") is string loc)
                        return loc;
                }
                catch { }
            }
#pragma warning restore CA1416
            return null;
        }

        private static string GetSteamPathFromRegistry()
        {
#pragma warning disable CA1416
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                return key?.GetValue("SteamPath")?.ToString().Replace("/", @"\");
            }
            catch { return null; }
#pragma warning restore CA1416
        }

        private static List<string> GetSteamLibraryFolders(string steamRoot)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { steamRoot };

            var vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdf))
                return result.ToList();

            try
            {
                var content = File.ReadAllText(vdf);
                foreach (Match m in Regex.Matches(content, @"""path""\s+""([^""]+)"""))
                {
                    var p = m.Groups[1].Value.Replace(@"\\", @"\");
                    if (Directory.Exists(p))
                        result.Add(p);
                }
            }
            catch { }

            return result.ToList();
        }

        private static string ParseInstallDirFromManifest(string manifestPath)
        {
            try
            {
                var m = Regex.Match(File.ReadAllText(manifestPath), @"""installdir""\s+""([^""]+)""");
                return m.Success ? m.Groups[1].Value : null;
            }
            catch { return null; }
        }
    }
}
