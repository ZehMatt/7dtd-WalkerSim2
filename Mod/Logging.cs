using System;

namespace WalkerSim
{
    public class LogGameConsoleSink : Logging.ISink
    {
        public static LogGameConsoleSink Instance { get; } = new LogGameConsoleSink();

        public void Message(Logging.Level level, string message)
        {
            switch (level)
            {
                case Logging.Level.Info:
                    Log.Out($"[WalkerSim] [INF] {message}");
                    break;
                case Logging.Level.Warning:
                    Log.Warning($"[WalkerSim] [WRN] {message}");
                    break;
                case Logging.Level.Error:
                    Log.Error($"[WalkerSim] [ERR] {message}");
                    break;
            }
        }
    }

    public class LogFileSink : Logging.ISink
    {
        public static LogFileSink Instance { get; } = new LogFileSink();

        private readonly string _filePath = string.Empty;

        public LogFileSink()
        {
            var fileName = "WalkerSim";

            // Get local application data path
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var logFolder = System.IO.Path.Combine(localAppData, "7DaysToDie", "logs");

            // Ensure the log folder exists
            if (!System.IO.Directory.Exists(logFolder))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(logFolder);
                }
                catch (Exception)
                {
                    return;
                }
            }

            // Don't have too many WalkerSim log files, delete old ones.
            try
            {
                var files = System.IO.Directory.GetFiles(logFolder, $"{fileName}_*.log");
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(file);
                        if (fileInfo.CreationTime < DateTime.Now.AddDays(-7)) // Keep logs for 7 days
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore errors deleting old log files
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors listing files
            }

            var logFileName = $"{fileName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

            _filePath = System.IO.Path.Combine(logFolder, logFileName);
        }

        public void Message(Logging.Level level, string message)
        {
            if (string.IsNullOrEmpty(_filePath))
                return;

            var levelString = "INF";
            switch (level)
            {
                case Logging.Level.Info:
                    levelString = "INF";
                    break;
                case Logging.Level.Warning:
                    levelString = "WRN";
                    break;
                case Logging.Level.Error:
                    levelString = "ERR";
                    break;
            }

            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{levelString}] {message}";
            System.IO.File.AppendAllText(_filePath, logMessage + Environment.NewLine);
        }
    }
}
