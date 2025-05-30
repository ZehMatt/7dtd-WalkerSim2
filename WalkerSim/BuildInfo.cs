using System.Linq;
using System.Reflection;

[assembly: AssemblyMetadata("Commit", "local")]
[assembly: AssemblyVersion("0.0.0")]
[assembly: AssemblyFileVersion("0.0.0.0")]

namespace WalkerSim
{
    public static class BuildInfo
    {
        public static string Version
        {
            get
            {
                var version = (Assembly.GetExecutingAssembly()
                        .GetCustomAttribute<AssemblyFileVersionAttribute>()
                        ?.Version ?? "0.0.0.0");

                // Remove .0 from end, we never use that.
                if (version.EndsWith(".0"))
                {
                    version = version.Substring(0, version.Length - 2);
                }
                return version;
            }
        }

        public static string Commit => Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "Commit")?.Value ?? "unknown";
    }
}
