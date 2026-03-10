using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WalkerSim
{
    public static class Logging
    {
        public delegate void LogMessage(string message);
        private static object _lock = new object();

        public enum Level
        {
            Info,
            Warning,
            Error,
        }

        public interface ISink
        {
            void Message(Level level, string message);
        }

        private static List<ISink> _sinks = new List<ISink>();

        static Logging()
        {
        }

        private static void Message(Level level, string message)
        {
            lock (_lock)
            {
                foreach (var sink in _sinks)
                {
                    sink.Message(level, message);
                }
            }
        }

        public static void AddSink(ISink sink)
        {
            if (sink == null)
                throw new ArgumentNullException(nameof(sink));

            _sinks.Add(sink);
        }

        // Unconditional.
        public static void Out(string message) => Message(Level.Info, message);

        public static void Out(string format, params object[] args) => Message(Level.Info, string.Format(format, args));

        public static void Info(string message) => Out(message);

        public static void Info(string format, params object[] args) => Out(format, args);

        public static void Warn(string message) => Message(Level.Warning, message);

        public static void Warn(string format, params object[] args) => Message(Level.Warning, string.Format(format, args));

        public static void Err(string message) => Message(Level.Error, message);

        public static void Err(string format, params object[] args) => Message(Level.Error, string.Format(format, args));

        public static void Exception(System.Exception ex) => Message(Level.Error, ex.ToString());

        // Conditional - fully lazy evaluation via delegate.
        public static void CondInfo(bool log, Func<string> messageFunc)
        {
            if (log) Message(Level.Info, messageFunc());
        }

        public static void CondErr(bool log, Func<string> messageFunc)
        {
            if (log) Message(Level.Error, messageFunc());
        }

        public static void CondWrn(bool log, Func<string> messageFunc)
        {
            if (log) Message(Level.Warning, messageFunc());
        }

        // Debug-only methods
        [Conditional("DEBUG")]
        public static void DbgInfo(string message) => Message(Level.Info, message);

        [Conditional("DEBUG")]
        public static void DbgInfo(string format, params object[] args) => Message(Level.Info, string.Format(format, args));

        [Conditional("DEBUG")]
        public static void DbgErr(string message) => Message(Level.Error, message);

        [Conditional("DEBUG")]
        public static void DbgErr(string format, params object[] args) => Message(Level.Error, string.Format(format, args));

        [Conditional("DEBUG")]
        public static void DbgWrn(string message) => Message(Level.Warning, message);

        [Conditional("DEBUG")]
        public static void DbgWrn(string format, params object[] args) => Message(Level.Warning, string.Format(format, args));
    }
}
