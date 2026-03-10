using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Editor.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace Editor.Views
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? _updateTimer;
        private ScrollViewer? _logScrollViewer;
        private const double NearBottomThreshold = 40.0;
        private bool _renderingPaused;
        private double _savedSidebarWidth = 600;
        private double _savedLogHeight = 150;

        public MainWindow()
        {
            InitializeComponent();

            // Set up timer to update simulation stats and redraw canvas
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            this.Closed += (s, e) =>
            {
                _updateTimer?.Stop();
                if (DataContext is EditorViewModel vm)
                    vm.StopSimulation();
            };

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Escape && DataContext is EditorViewModel vmEsc)
                {
                    vmEsc.CancelToolCommand.Execute(null);
                    e.Handled = true;
                }
            };

            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is EditorViewModel vm2)
                {
                    vm2.WorldLoaded = () => SimCanvas.FitToView();
                    vm2.LogEntryAdded = entry =>
                    {
                        ScrollLogIfNeeded(entry.IsError);
                    };
                    vm2.NavigateToAgentRequested = a => SimCanvas.GoToAgent(a);
                    vm2.TrackAgentRequested      = a => SimCanvas.TrackAgent(a);
                    vm2.StopTrackingRequested    = () => SimCanvas.StopTracking();
                    vm2.GroupColorsChanged        = () => SimCanvas.InvalidateGroupBrushes();
                    SimCanvas.TrackingStopped    = () => vm2.IsTrackingAgent = false;
                    SimCanvas.OnCanvasClick      = pos => vm2.HandleCanvasClick(pos);
                }
            };
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (DataContext is EditorViewModel viewModel)
            {
                viewModel.UpdateSimulationStats();

                // Sync tool preview state to canvas every frame
                SimCanvas.ToolPreviewRadius = viewModel.ActiveToolPreviewRadius;
                SimCanvas.SetToolActive(viewModel.IsToolActive);
            }

            // Keep agents list live while that tab is open
            if (AgentsPanel.IsVisible && DataContext is EditorViewModel vm2)
                vm2.RefreshAgentModels();

            if (!_renderingPaused)
                SimCanvas.InvalidateVisual();
        }

        // ── Tab switching ──────────────────────────────────────────────────────────

        private void OnBaseParamsTabClick(object? sender, RoutedEventArgs e)
        {
            BaseParamsTab.IsChecked = true;
            MovementSystemsTab.IsChecked = false;
            AgentsTab.IsChecked = false;
            BaseParametersPanel.IsVisible = true;
            MovementSystemsPanel.IsVisible = false;
            AgentsPanel.IsVisible = false;
        }

        private void OnMovementSystemsTabClick(object? sender, RoutedEventArgs e)
        {
            MovementSystemsTab.IsChecked = true;
            BaseParamsTab.IsChecked = false;
            AgentsTab.IsChecked = false;
            MovementSystemsPanel.IsVisible = true;
            BaseParametersPanel.IsVisible = false;
            AgentsPanel.IsVisible = false;
        }

        private void OnAgentsTabClick(object? sender, RoutedEventArgs e)
        {
            AgentsTab.IsChecked = true;
            BaseParamsTab.IsChecked = false;
            MovementSystemsTab.IsChecked = false;
            AgentsPanel.IsVisible = true;
            BaseParametersPanel.IsVisible = false;
            MovementSystemsPanel.IsVisible = false;

            if (DataContext is EditorViewModel vm)
                vm.RefreshAgentModels();
        }

        // ── Zoom ──────────────────────────────────────────────────────────────────

        private void OnZoomInClick(object? sender, RoutedEventArgs e) => SimCanvas.ZoomIn();
        private void OnZoomOutClick(object? sender, RoutedEventArgs e) => SimCanvas.ZoomOut();
        private void OnZoomResetClick(object? sender, RoutedEventArgs e) => SimCanvas.ZoomReset();

        // ── Tools ─────────────────────────────────────────────────────────────────

        private void OnToolEmitSoundClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not EditorViewModel vm) return;
            if (sender is MenuItem { Tag: string tag } && float.TryParse(tag, out float radius))
                vm.SoundRadius = radius;
            vm.ActivateEmitSoundCommand.Execute(null);
        }

        private void OnToolKillClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is EditorViewModel vm)
                vm.ActivateKillCommand.Execute(null);
        }

        private void OnToolAddPlayerClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is EditorViewModel vm)
                vm.ActivateAddPlayerCommand.Execute(null);
        }

        private void OnToolSetPlayerPositionClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is EditorViewModel vm)
                vm.ActivateSetPlayerPositionCommand.Execute(null);
        }

        // ── Settings ──────────────────────────────────────────────────────────────

        private async void OnPreferencesClick(object? sender, RoutedEventArgs e)
        {
            var window = new PreferencesWindow();
            await window.ShowDialog(this);

            if (window.SettingsSaved)
                SimCanvas.ApplySettings();

            // Always repopulate worlds — folders may have changed
            if (DataContext is EditorViewModel vm)
                vm.ReloadWorldList();
        }

        // ── Help ──────────────────────────────────────────────────────────────────

        private void OnDocumentationClick(object? sender, RoutedEventArgs e)
        {
            var version = WalkerSim.BuildInfo.Version == "0.0.0" ? "nightly" : WalkerSim.BuildInfo.Version;
            var url = $"https://7dtd-walkersim2.readthedocs.io/{version}/";
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch { }
        }

        private void OnDiscordClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = "https://discord.gg/9QGHS4wbFu", UseShellExecute = true });
            }
            catch { }
        }

        private async void OnAboutClick(object? sender, RoutedEventArgs e)
        {
            var window = new AboutWindow();
            await window.ShowDialog(this);
        }

        // ── View toggles ──────────────────────────────────────────────────────────

        private void OnViewToggleClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;

            switch (item.Name)
            {
                case "MenuViewPauseRendering": _renderingPaused = item.IsChecked; return;
                case "MenuViewBiomes":       SimCanvas.ShowBiomes       = item.IsChecked; break;
                case "MenuViewRoads":        SimCanvas.ShowRoads        = item.IsChecked; break;
                case "MenuViewAgents":       SimCanvas.ShowAgents       = item.IsChecked; break;
                case "MenuViewActiveAgents": SimCanvas.ShowActiveAgents = item.IsChecked; break;
                case "MenuViewEvents":       SimCanvas.ShowEvents       = item.IsChecked; break;
                case "MenuViewPrefabs":      SimCanvas.ShowPrefabs      = item.IsChecked; break;
                case "MenuViewCities":       SimCanvas.ShowCities       = item.IsChecked; break;
            }

            SimCanvas.InvalidateVisual();
        }

        // ── Panel visibility ─────────────────────────────────────────────────────

        private void SetSidebarVisible(bool show)
        {
            var sidebarCol = MainGrid.ColumnDefinitions[0];
            var splitterCol = MainGrid.ColumnDefinitions[1];
            if (show)
            {
                sidebarCol.Width = new GridLength(_savedSidebarWidth, GridUnitType.Pixel);
                sidebarCol.MinWidth = 200;
                sidebarCol.MaxWidth = 600;
                splitterCol.Width = new GridLength(4, GridUnitType.Pixel);
                SidebarBorder.IsVisible = true;
                SidebarSplitter.IsVisible = true;
                SidebarShowButton.IsVisible = false;
            }
            else
            {
                _savedSidebarWidth = sidebarCol.ActualWidth;
                sidebarCol.MinWidth = 0;
                sidebarCol.MaxWidth = 0;
                sidebarCol.Width = new GridLength(0, GridUnitType.Pixel);
                splitterCol.Width = new GridLength(0, GridUnitType.Pixel);
                SidebarBorder.IsVisible = false;
                SidebarSplitter.IsVisible = false;
                SidebarShowButton.IsVisible = true;
            }
        }

        private void OnHideSidebarClick(object? sender, RoutedEventArgs e) => SetSidebarVisible(false);
        private void OnShowSidebarClick(object? sender, RoutedEventArgs e) => SetSidebarVisible(true);

        private void SetLogVisible(bool show)
        {
            var logRow = RightGrid.RowDefinitions[2];
            var splitterRow = RightGrid.RowDefinitions[1];
            if (show)
            {
                logRow.Height = new GridLength(_savedLogHeight, GridUnitType.Pixel);
                logRow.MinHeight = 50;
                logRow.MaxHeight = 400;
                splitterRow.Height = new GridLength(4, GridUnitType.Pixel);
                LogBorder.IsVisible = true;
                LogSplitter.IsVisible = true;
                LogShowBar.IsVisible = false;
            }
            else
            {
                _savedLogHeight = logRow.ActualHeight;
                logRow.MinHeight = 0;
                logRow.MaxHeight = double.PositiveInfinity;
                logRow.Height = GridLength.Auto;
                splitterRow.Height = new GridLength(0, GridUnitType.Pixel);
                LogBorder.IsVisible = false;
                LogSplitter.IsVisible = false;
                LogShowBar.IsVisible = true;
            }
        }

        private void OnHideLogClick(object? sender, RoutedEventArgs e) => SetLogVisible(false);
        private void OnShowLogClick(object? sender, RoutedEventArgs e) => SetLogVisible(true);

        // ── Log auto-scroll ──────────────────────────────────────────────────────

        private ScrollViewer? GetLogScrollViewer()
        {
            if (_logScrollViewer != null) return _logScrollViewer;
            _logScrollViewer = LogListBox.GetVisualDescendants()
                .OfType<ScrollViewer>()
                .FirstOrDefault();
            return _logScrollViewer;
        }

        private void ScrollLogIfNeeded(bool forceScroll)
        {
            var sv = GetLogScrollViewer();
            if (sv == null) return;

            bool nearBottom = (sv.Extent.Height - sv.Offset.Y - sv.Viewport.Height) <= NearBottomThreshold;
            if (forceScroll || nearBottom)
                sv.ScrollToEnd();
        }
    }
}
