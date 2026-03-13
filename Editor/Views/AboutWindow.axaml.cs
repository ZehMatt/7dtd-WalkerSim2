using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace Editor.Views
{
    public partial class AboutWindow : Window
    {
        private ChipSynth _synth;

        public AboutWindow()
        {
            InitializeComponent();

            var version = WalkerSim.BuildInfo.Version;
            var commit = WalkerSim.BuildInfo.Commit;

            VersionText.Text = $"Version {version}";
            CommitText.Text = commit != "unknown" ? $"Commit {commit}" : "";

            if (ChipSynth.IsSupported)
            {
                _synth = new ChipSynth();
                _synth.Play();
                VFX.Synth = _synth;
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            _synth?.Dispose();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _synth?.Dispose();
            base.OnClosed(e);
        }

        private void OnGithubLinkClick(object sender, PointerPressedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/ZehMatt/WalkerSim2",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}
