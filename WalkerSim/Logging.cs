using System.Diagnostics;

namespace WalkerSim
{
    internal static class Logging
    {
        public delegate void LogMessage(string message);

        public enum Level
        {
            Info,
            Warning,
            Error,
            Count,
        }

        static LogMessage[] logHandler = new LogMessage[(int)Level.Count];

        static Logging()
        {
        }

        private static void Message(Level level, string message)
        {
            var output = "[WalkerSim] " + message;

            LogMessage handler = logHandler[(int)level];
            if (handler == null)
                return;

            handler(output);
        }

        public static void SetHandler(Level level, LogMessage logOut)
        {
            logHandler[(int)level] = logOut;
        }

        public static void Out(string message) => Message(Level.Info, message);

        public static void Out(string format, params object[] args) => Message(Level.Info, string.Format(format, args));

        public static void Warn(string message) => Message(Level.Warning, message);

        public static void Warn(string format, params object[] args) => Message(Level.Warning, string.Format(format, args));

        public static void Err(string message) => Message(Level.Error, message);

        public static void Err(string format, params object[] args) => Message(Level.Error, string.Format(format, args));

        public static void Exception(System.Exception ex) => Message(Level.Error, ex.ToString());

        [Conditional("DEBUG")]
        public static void Debug(string message) => Message(Level.Info, message);

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args) => Message(Level.Error, string.Format(format, args));

        [Conditional("DEBUG")]
        public static void DebugErr(string message) => Message(Level.Info, message);

        [Conditional("DEBUG")]
        public static void DebugErr(string format, params object[] args) => Message(Level.Error, string.Format(format, args));
    }
}
