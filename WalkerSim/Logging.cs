using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WalkerSim
{
    public static class Logging
    {
        public delegate void LogMessage(string message);

        public enum Level
        {
            Info,
            Warning,
            Error,
            Count,
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
            foreach (var sink in _sinks)
            {
                sink.Message(level, message);
            }
        }

        public static void AddSink(ISink sink)
        {
            if (sink == null)
                throw new ArgumentNullException(nameof(sink));

            _sinks.Add(sink);
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

        [Conditional("DEBUG")]
        public static void DebugWarn(string message) => Message(Level.Warning, message);

        [Conditional("DEBUG")]
        public static void DebugWarn(string format, params object[] args) => Message(Level.Warning, string.Format(format, args));

        public class ConsoleSink : ISink
        {
            public void Message(Level level, string message)
            {
                switch (level)
                {
                    case Level.Info:
                        System.Console.WriteLine($"[INF] {message}");
                        break;
                    case Level.Warning:
                        System.Console.WriteLine($"[WRN] {message}");
                        break;
                    case Level.Error:
                        System.Console.Error.WriteLine($"[ERR] {message}");
                        break;
                }
            }
        }

        public class FileSink : ISink
        {
            private readonly string _filePath;

            public FileSink(string filePath)
            {
                _filePath = filePath;
            }

            public void Message(Level level, string message)
            {
                try
                {
                    System.IO.File.AppendAllText(_filePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}\n");
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
