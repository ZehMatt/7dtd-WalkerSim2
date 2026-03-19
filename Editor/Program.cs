using Avalonia;
using System;

namespace Editor
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            WalkerSim.Logging.AddSink(LogFileSink.Instance);
            WalkerSim.Logging.Info("Editor starting...");
            WalkerSim.Drawing.Loader = new Editor.Drawing.ImageLoader();
            WalkerSim.Logging.Info("Initializing Avalonia...");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            WalkerSim.Logging.Info("Editor exiting.");
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
