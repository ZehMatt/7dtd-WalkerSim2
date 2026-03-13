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
            AvaloniaXamlLoader.Load(this);

            ApplyTheme(EditorSettings.Instance.Theme);

            WalkerSim.Simulation.Instance.EditorMode = true;
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
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new EditorViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
