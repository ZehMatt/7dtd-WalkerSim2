using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using WalkerSim;

namespace Editor.Models
{
    public partial class LogEntry : ObservableObject
    {
        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private string _timestamp = string.Empty;

        private IBrush? _foreground;

        public IBrush? Foreground
        {
            get
            {
                if (_foreground == null)
                {
                    // Fallback colors
                    _foreground = Level switch
                    {
                        WalkerSim.Logging.Level.Info => new SolidColorBrush(Colors.White),
                        WalkerSim.Logging.Level.Warning => new SolidColorBrush(Colors.Yellow),
                        WalkerSim.Logging.Level.Error => new SolidColorBrush(Colors.Red),
                        _ => new SolidColorBrush(Colors.White)
                    };
                }

                return _foreground;
            }
            set => _foreground = value;
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
