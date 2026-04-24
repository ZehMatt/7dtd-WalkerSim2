using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Editor.ViewModels;
using System;
using System.ComponentModel;
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

            // Listen on the tunneling phase so the focused control (TextBox, NumericUpDown,
            // ComboBox, etc.) can't swallow Escape before we see it. Bubbling KeyDown only
            // fires if no descendant handled the event, which is why this used to be flaky.
            this.AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);

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
                    vm2.TrackAgentRequested = a => SimCanvas.TrackAgent(a);
                    vm2.StopTrackingRequested = () => SimCanvas.StopTracking();
                    vm2.GroupColorsChanged = () => SimCanvas.InvalidateGroupBrushes();
                    SimCanvas.TrackingStopped = () => vm2.IsTrackingAgent = false;
                    SimCanvas.OnCanvasClick = pos => vm2.HandleCanvasClick(pos);
                }
            };
        }

        private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
        {
            // Cancel an active tool on Escape, regardless of which descendant has focus.
            // Gated on IsToolActive so we don't steal Escape from menus / dialogs / popups
            // when no tool is selected.
            if (e.Key == Key.Escape && DataContext is EditorViewModel vm && vm.IsToolActive)
            {
                vm.CancelToolCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (DataContext is EditorViewModel viewModel)
            {
                viewModel.UpdateSimulationStats();

                // Sync tool preview state to canvas every frame
                SimCanvas.ToolPreviewRadius = viewModel.ActiveToolPreviewRadius;
                SimCanvas.ToolPreviewHint = viewModel.ActiveToolHint;
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

        // ── Exit / Close ────────────────────────────────────────────────────────

        private void OnExitClick(object? sender, RoutedEventArgs e) => Close();

        private bool _forceClose = false;

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);

            if (_forceClose)
                return;

            if (DataContext is EditorViewModel vm && vm.HasUnsavedChanges)
            {
                e.Cancel = true;

                var result = await ShowUnsavedChangesDialog();
                if (result == true)
                {
                    _forceClose = true;
                    Close();
                }
            }
        }

        // Returns: true = discard, false = cancel, null = saved
        private async System.Threading.Tasks.Task<bool?> ShowUnsavedChangesDialog()
        {
            bool? dialogResult = false;

            var dialog = new Window
            {
                Title = "Unsaved Changes",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false,
            };

            var saveButton = new Button { Content = "Save", Width = 80 };
            var discardButton = new Button { Content = "Discard", Width = 80 };
            var cancelButton = new Button { Content = "Cancel", Width = 80 };

            saveButton.Click += async (_, _) =>
            {
                if (DataContext is EditorViewModel vm)
                {
                    await vm.ExportConfigurationCommand.ExecuteAsync(null);
                    if (!vm.HasUnsavedChanges)
                    {
                        dialogResult = true;
                        dialog.Close();
                    }
                }
            };
            discardButton.Click += (_, _) => { dialogResult = true; dialog.Close(); };
            cancelButton.Click += (_, _) => { dialogResult = false; dialog.Close(); };

            dialog.Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(16, 12),
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "You have unsaved changes. Would you like to save your configuration before closing?",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        MaxWidth = 350,
                    },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 8,
                        Children = { saveButton, discardButton, cancelButton }
                    }
                }
            };

            await dialog.ShowDialog(this);
            return dialogResult;
        }

        // ── Zoom ──────────────────────────────────────────────────────────────────

        private void OnZoomInClick(object? sender, RoutedEventArgs e) => SimCanvas.ZoomIn();
        private void OnZoomOutClick(object? sender, RoutedEventArgs e) => SimCanvas.ZoomOut();
        private void OnZoomResetClick(object? sender, RoutedEventArgs e) => SimCanvas.ZoomReset();

        // ── Tools ─────────────────────────────────────────────────────────────────

        private void OnToolEmitSoundClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not EditorViewModel vm)
                return;
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
            {
                SimCanvas.ApplySettings();
                
                // Folders may have changed — repopulate worlds.
                if (DataContext is EditorViewModel vm)
                    vm.ReloadWorldList();
            }
        }

        // ── Help ──────────────────────────────────────────────────────────────────

        private void OnDocumentationClick(object? sender, RoutedEventArgs e)
        {
            var url = $"https://7dtd-walkersim2.readthedocs.io/{WalkerSim.BuildInfo.Branch}/";
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
            if (sender is not MenuItem item)
                return;

            switch (item.Name)
            {
                case "MenuViewPauseRendering":
                    _renderingPaused = item.IsChecked;
                    return;
                case "MenuViewBiomes":
                    SimCanvas.ShowBiomes = item.IsChecked;
                    break;
                case "MenuViewRoads":
                    SimCanvas.ShowRoads = item.IsChecked;
                    break;
                case "MenuViewAgents":
                    SimCanvas.ShowAgents = item.IsChecked;
                    break;
                case "MenuViewActiveAgents":
                    SimCanvas.ShowActiveAgents = item.IsChecked;
                    break;
                case "MenuViewEvents":
                    SimCanvas.ShowEvents = item.IsChecked;
                    break;
                case "MenuViewPrefabs":
                    SimCanvas.ShowPrefabs = item.IsChecked;
                    break;
                case "MenuViewCities":
                    SimCanvas.ShowCities = item.IsChecked;
                    break;
                case "MenuViewRoadNetwork":
                    SimCanvas.ShowRoadNetwork = item.IsChecked;
                    break;
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
                sidebarCol.MaxWidth = double.PositiveInfinity;
                splitterCol.Width = new GridLength(4, GridUnitType.Pixel);
                SidebarBorder.IsVisible = true;
                SidebarSplitter.IsVisible = true;
                SidebarShowButton.IsVisible = false;
            }
            else
            {
                _savedSidebarWidth = sidebarCol.ActualWidth;
                sidebarCol.MinWidth = 0;
                sidebarCol.MaxWidth = double.PositiveInfinity;
                sidebarCol.Width = new GridLength(1, GridUnitType.Auto);
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
            if (_logScrollViewer != null)
                return _logScrollViewer;
            _logScrollViewer = LogListBox.GetVisualDescendants()
                .OfType<ScrollViewer>()
                .FirstOrDefault();
            return _logScrollViewer;
        }

        private void ScrollLogIfNeeded(bool forceScroll)
        {
            var sv = GetLogScrollViewer();
            if (sv == null)
                return;

            bool nearBottom = (sv.Extent.Height - sv.Offset.Y - sv.Viewport.Height) <= NearBottomThreshold;
            if (forceScroll || nearBottom)
                sv.ScrollToEnd();
        }
    }
}
