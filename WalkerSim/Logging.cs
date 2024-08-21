using System;
using System.Diagnostics;

namespace WalkerSim
{
    internal static class Logging
    {
        public delegate void LogMessage(string message);

        [Flags]
        public enum Prefix
        {
            None = 0,
            Level = 1 << 0,
            WalkerSim = 1 << 1,
            Timestamp = 1 << 2,
        }

        public enum Level
        {
            Info,
            Warning,
            Error,
            Count,
        }

        public static Prefix Prefixes = Prefix.WalkerSim;

        static LogMessage[] logHandler = new LogMessage[(int)Level.Count];

        static Logging()
        {
        }

        private static string GetPrefix(Level level)
        {
            var res = "";
            if ((Prefixes & Prefix.WalkerSim) != Prefix.None)
            {
                res = "[WalkerSim]";
            }
            if ((Prefixes & Prefix.Level) != Prefix.None)
            {
                if (level == Level.Info)
                    res += "[INF]";
                else if (level == Level.Warning)
                    res += "[WRN]";
                else if (level == Level.Error)
                    res += "[ERR]";
                else
                    throw new Exception("Invalid logging level");
            }
            if ((Prefixes & Prefix.Timestamp) != Prefix.None)
            {
                res += DateTime.Now.ToString("[dd/MM/yy HH:mm:ss:fff]");
            }
            return res;
        }

        private static void Message(Level level, string message)
        {
            string output;
            if (Prefixes != Prefix.None)
            {
                output = String.Format("{0} {1}", GetPrefix(level), message);
            }
            else
            {
                output = message;
            }

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

        public static void Info(string message) => Out(message);

        public static void Info(string format, params object[] args) => Out(format, args);

        public static void Warn(string message) => Message(Level.Warning, message);

        public static void Warn(string format, params object[] args) => Message(Level.Warning, string.Format(format, args));

        public static void Err(string message) => Message(Level.Error, message);

        public static void Err(string format, params object[] args) => Message(Level.Error, string.Format(format, args));

        public static void Exception(System.Exception ex) => Message(Level.Error, ex.ToString());

        [Conditional("DEBUG")]
        public static void Debug(string message) => Message(Level.Info, message);

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args) => Message(Level.Info, string.Format(format, args));

        [Conditional("DEBUG")]
        public static void DebugErr(string message) => Message(Level.Error, message);

        [Conditional("DEBUG")]
        public static void DebugErr(string format, params object[] args) => Message(Level.Error, string.Format(format, args));
    }
}
