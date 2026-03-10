using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using WalkerSim;

namespace Editor.Models
{
    public partial class LogEntry : ObservableObject
    {
        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private string _timestamp = string.Empty;

        public IBrush? Foreground
        {
            get
            {
                var key = Level switch
                {
                    WalkerSim.Logging.Level.Warning => "LogWarning",
                    WalkerSim.Logging.Level.Error   => "LogError",
                    _                               => "LogInfo"
                };

                var app = Application.Current;
                if (app != null &&
                    app.Resources.TryGetResource(key, app.ActualThemeVariant, out var resource) &&
                    resource is IBrush brush)
                    return brush;

                // Fallback when resources are unavailable
                return Level switch
                {
                    WalkerSim.Logging.Level.Warning => Brushes.Yellow,
                    WalkerSim.Logging.Level.Error   => Brushes.Red,
                    _                               => Brushes.White
                };
            }
        }

        public WalkerSim.Logging.Level Level { get; set; } = WalkerSim.Logging.Level.Info;

        public bool IsError => Level == WalkerSim.Logging.Level.Error;

        public LogEntry(WalkerSim.Logging.Level level, string message)
        {
            Level = level;
            Message = message;
            Timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
        }
    }
}
