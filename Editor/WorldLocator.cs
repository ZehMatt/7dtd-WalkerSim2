using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Editor
{
    internal static class WorldLocator
    {
        private const int AppID = 251570;

        public static List<string> FindWorldFolders()
        {
            var worldFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // User-generated worlds (location differs per OS)
            foreach (var generatedPath in GetUserGeneratedWorldPaths())
            {
                if (Directory.Exists(generatedPath))
                {
                    foreach (var w in Directory.EnumerateDirectories(generatedPath))
                        worldFolders.Add(w);
                }
            }

            foreach (var installPath in FindGamePaths())
            {
                // Worlds bundled with the game
                var worldsPath = Path.Combine(installPath, "Data", "Worlds");
                if (Directory.Exists(worldsPath))
                {
                    foreach (var w in Directory.EnumerateDirectories(worldsPath))
                        worldFolders.Add(w);
                }

                // Worlds inside Mods subdirectories
                var modsPath = Path.Combine(installPath, "Mods");
                if (Directory.Exists(modsPath))
                {
                    foreach (var mod in Directory.EnumerateDirectories(modsPath))
                    {
                        var worldPath = Path.Combine(mod, "Worlds");
                        if (Directory.Exists(worldPath))
                        {
                            foreach (var w in Directory.EnumerateDirectories(worldPath))
                                worldFolders.Add(w);
                        }
                    }
                }
            }

            var list = new List<string>(worldFolders);
            list.Sort((a, b) => string.Compare(Path.GetFileName(a), Path.GetFileName(b), StringComparison.OrdinalIgnoreCase));
            return list;
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

        // User-generated world paths

        private static IEnumerable<string> GetUserGeneratedWorldPaths()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                yield return Path.Combine(appData, "7DaysToDie", "GeneratedWorlds");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Native Linux: ~/.local/share/7DaysToDie/GeneratedWorlds
                var localShare = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                yield return Path.Combine(localShare, "7DaysToDie", "GeneratedWorlds");

                // Proton/Wine prefix fallback
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                yield return Path.Combine(home, ".wine", "drive_c", "users",
                    Environment.UserName, "AppData", "Roaming", "7DaysToDie", "GeneratedWorlds");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                yield return Path.Combine(home, "Library", "Application Support", "7DaysToDie", "GeneratedWorlds");
            }
        }

        // Game install discovery

        private static List<string> FindGamePaths()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Registry lookup (Windows only)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var reg = GetInstallPathFromRegistry();
                if (reg != null) paths.Add(reg);
            }

            // Steam library scan (all platforms)
            foreach (var steamRoot in GetSteamRootPaths())
            {
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
                                paths.Add(gamePath);
                        }
                    }
                }
            }

            // Handle case where editor is running from inside Mods/WalkerSim
            var exe = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (exe != null)
            {
                var candidate = Path.GetFullPath(Path.Combine(exe, "..", ".."));
                if (Directory.Exists(candidate))
                    paths.Add(candidate);
            }

            return paths.Where(p => !string.IsNullOrEmpty(p) && Directory.Exists(p)).ToList();
        }

        // Steam root paths (per OS)

        private static IEnumerable<string> GetSteamRootPaths()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var reg = GetSteamPathFromRegistry();
                if (!string.IsNullOrEmpty(reg)) yield return reg;

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

        // Registry helpers (Windows only)

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

        // Cross-platform Steam helpers

        private static List<string> GetSteamLibraryFolders(string steamRoot)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { steamRoot };

            var vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdf)) return result.ToList();

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
