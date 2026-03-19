using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Editor.ViewModels;
using Editor.Views;

namespace Editor
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            WalkerSim.Logging.Info("Loading XAML...");
            AvaloniaXamlLoader.Load(this);

            WalkerSim.Logging.Info("Loading settings...");
            ApplyTheme(EditorSettings.Instance.Theme);

            WalkerSim.Simulation.Instance.EditorMode = true;
            WalkerSim.Logging.Info("App initialized.");
        }

        public void ApplyTheme(AppTheme theme)
        {
            RequestedThemeVariant = theme switch
            {
                AppTheme.Light => ThemeVariant.Light,
                AppTheme.System => ThemeVariant.Default,
                _ => ThemeVariant.Dark,
            };
        }

        public override void OnFrameworkInitializationCompleted()
        {
            WalkerSim.Logging.Info("Creating main window...");
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new EditorViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
            WalkerSim.Logging.Info("Framework initialization completed.");
        }
    }
}
