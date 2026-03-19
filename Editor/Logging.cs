using System;

namespace Editor
{
    public class LogFileSink : WalkerSim.Logging.ISink
    {
        public static LogFileSink Instance { get; } = new LogFileSink();

        private readonly string _filePath = string.Empty;

        public LogFileSink()
        {
            var fileName = "Editor";

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logFolder = System.IO.Path.Combine(appData, "WalkerSim2");

            try
            {
                System.IO.Directory.CreateDirectory(logFolder);
            }
            catch (Exception)
            {
                return;
            }

            // Delete log files older than 7 days.
            try
            {
                var files = System.IO.Directory.GetFiles(logFolder, $"{fileName}_*.log");
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(file);
                        if (fileInfo.CreationTime < DateTime.Now.AddDays(-7))
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }

            var logFileName = $"{fileName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
            _filePath = System.IO.Path.Combine(logFolder, logFileName);
        }

        public void Message(WalkerSim.Logging.Level level, string message)
        {
            if (string.IsNullOrEmpty(_filePath))
                return;

            var levelString = level switch
            {
                WalkerSim.Logging.Level.Warning => "WRN",
                WalkerSim.Logging.Level.Error => "ERR",
                _ => "INF",
            };

            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{levelString}] {message}";
            System.IO.File.AppendAllText(_filePath, logMessage + Environment.NewLine);
        }
    }
}
