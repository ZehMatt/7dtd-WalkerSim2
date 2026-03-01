using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Editor.ViewModels;
using System;
using System.Linq;

namespace Editor.Views
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? _updateTimer;
        private ScrollViewer? _logScrollViewer;
        private const double NearBottomThreshold = 40.0;

        public MainWindow()
        {
            InitializeComponent();

            // Set up timer to update simulation stats and redraw canvas
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(66) // ~15 FPS
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            this.Closed += (s, e) =>
            {
                _updateTimer?.Stop();
                if (DataContext is EditorViewModel vm)
                    vm.StopSimulation();
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
                }
            };
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (DataContext is EditorViewModel viewModel)
                viewModel.UpdateSimulationStats();

            SimCanvas.InvalidateVisual();
        }

        // ── Tab switching ──────────────────────────────────────────────────────────

        private void OnBaseParamsTabClick(object? sender, RoutedEventArgs e)
        {
            BaseParamsTab.IsChecked = true;
            MovementSystemsTab.IsChecked = false;
            BaseParametersPanel.IsVisible = true;
            MovementSystemsPanel.IsVisible = false;
        }

        private void OnMovementSystemsTabClick(object? sender, RoutedEventArgs e)
        {
            MovementSystemsTab.IsChecked = true;
            BaseParamsTab.IsChecked = false;
            MovementSystemsPanel.IsVisible = true;
            BaseParametersPanel.IsVisible = false;
        }

        // ── Zoom ──────────────────────────────────────────────────────────────────

        private void OnZoomInClick(object? sender, RoutedEventArgs e) => SimCanvas.ZoomIn();
        private void OnZoomOutClick(object? sender, RoutedEventArgs e) => SimCanvas.ZoomOut();
        private void OnZoomResetClick(object? sender, RoutedEventArgs e) => SimCanvas.ZoomReset();

        // ── View toggles ──────────────────────────────────────────────────────────

        private void OnViewToggleClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;

            switch (item.Name)
            {
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
