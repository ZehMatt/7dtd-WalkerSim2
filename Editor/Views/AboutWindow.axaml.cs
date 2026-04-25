using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Editor.Audio;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Editor.Views
{
    public partial class AboutWindow : Window
    {
        private WavPlayer _synth;
        private AboutVFXHost _vfx;

        public AboutWindow()
        {
            InitializeComponent();

            // Set the version/commit text immediately — these don't depend on
            // anything that takes time, so the info card pops up as soon as
            // the window opens.
            var version = WalkerSim.BuildInfo.Version;
            var commit = WalkerSim.BuildInfo.Commit;
            VersionText.Text = $"Version {version}";
            CommitText.Text = commit != "unknown" ? $"Commit {commit}" : "";

            // Defer the VFX host creation until after the window has
            // rendered at least once. Attaching the OpenGL control triggers
            // shader compilation which can block the UI thread; doing it on
            // a Background-priority dispatcher post lets the credits and
            // version text paint first.
            Opened += (_, _) =>
                Dispatcher.UIThread.Post(CreateVfx, DispatcherPriority.Background);
        }

        private void CreateVfx()
        {
            _vfx = new AboutVFXHost();
            // Insert at index 0 so credits overlay and info card stay on top.
            VfxGrid.Children.Insert(0, _vfx);
            _vfx.Ready += OnVfxReady;
        }

        private void OnVfxReady()
        {
            // Runs on the GL thread. Hop to the UI thread to touch UI and
            // kick audio init onto a background task.
            Dispatcher.UIThread.Post(() =>
            {
                if (LoadingSpinner != null)
                    LoadingSpinner.IsVisible = false;

                if (!WavPlayer.IsSupported)
                    return;
                if (!IsVisible || _vfx == null)
                    return;
                Task.Run(() =>
                {
                    try
                    {
                        var synth = new WavPlayer();
                        synth.Play();
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (!IsVisible || _vfx == null)
                            {
                                Task.Run(() => synth.Dispose());
                                return;
                            }
                            _synth = synth;
                            _vfx.Synth = synth;
                        });
                    }
                    catch
                    {
                    }
                });
            });
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Detach the VFX BEFORE base.OnClosed runs so that the GL deinit
            // happens here under our control rather than during the window
            // teardown sequence (which can block the compositor on Windows).
            var vfx = _vfx;
            _vfx = null;
            if (vfx != null && vfx.Parent is Panel parentPanel)
                parentPanel.Children.Remove(vfx);

            // Dispose audio on a background thread — WavPlayer.Dispose joins
            // the audio thread (up to 500ms) and calls WinMM/CoreAudio teardown
            // that can block the UI thread.
            var synth = _synth;
            _synth = null;
            if (synth != null)
                Task.Run(() => synth.Dispose());

            base.OnClosed(e);
        }

        private void OnGithubLinkClick(object sender, PointerPressedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/ZehMatt/7dtd-WalkerSim2",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}
