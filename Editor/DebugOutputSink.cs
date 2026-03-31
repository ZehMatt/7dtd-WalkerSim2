using System.Diagnostics;

namespace Editor
{
    public class DebugOutputSink : WalkerSim.Logging.ISink
    {
        public static DebugOutputSink Instance { get; } = new DebugOutputSink();

        public void Message(WalkerSim.Logging.Level level, string message)
        {
            var prefix = level switch
            {
                WalkerSim.Logging.Level.Warning => "WRN",
                WalkerSim.Logging.Level.Error => "ERR",
                _ => "INF",
            };
            Trace.WriteLine($"[{prefix}] {message}", "WalkerSim");
        }
    }
}
