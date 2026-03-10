using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using WalkerSim;

namespace Editor.Views
{
    public class SimulationCanvas : Control
    {
        private readonly Simulation _simulation = Simulation.Instance;

        // Zoom state
        private double _zoom = 1.0;
        private const double MinZoom = 0.1;
        private const double MaxZoom = 16.0;
        private const double BarHeight = 48.0;

        // Pan state
        private double _panX = 0.0;
        private double _panY = 0.0;
        private bool _isDragging = false;
        private Point _dragStart;

        // Cached bitmaps
        private WriteableBitmap _roadsBitmap;
        private string _cachedRoadsPath;
        private WriteableBitmap _biomesBitmap;
        private string _cachedBiomesPath;

        // Cached group brushes
        private IBrush[] _groupBrushes = Array.Empty<IBrush>();
        private int _cachedGroupCount = 0;

        // Reusable brushes (pens are created per-frame with 1/zoom thickness)
        private readonly IBrush _activeAgentBrush = new SolidColorBrush(Color.FromArgb(255, 0, 220, 0));
        private readonly IBrush _playerBrush = new SolidColorBrush(Colors.Magenta);
        private readonly IBrush _playerPenBrush = Brushes.Blue;
        private readonly IBrush _eventPenBrush = Brushes.Red;
        private readonly IBrush _cityPenBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 0));
        private readonly IBrush _cityBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 0));
        private readonly IBrush _prefabPenBrush = Brushes.Green;

        // Zoom-scaled pens, rebuilt whenever _zoom changes.
        private double _cachedPenZoom = 0;
        private IPen _playerPen;
        private IPen _eventPen;
        private IPen _cityPen;
        private IPen _prefabPen;
        private IPen _highlightPen;

        // ── Agent tracking / highlight ────────────────────────────────────────────
        private Agent _trackedAgent;
        private Agent _highlightAgent;
        private double _highlightTimer;      // seconds remaining; double.MaxValue = forever
        private double _blinkPhase;          // 0..2, draw when < 1
        private const double BlinkRate = 5.0; // Hz
        private const double TrackZoom = 6.0;

        // Smooth pan / zoom target
        private bool _smoothActive;
        private double _smoothTargetPanX;
        private double _smoothTargetPanY;
        private double _smoothTargetZoom = 1.0;

        private DateTime _lastRenderTime = DateTime.UtcNow;

        /// <summary>Called by the canvas when the user drags and cancels tracking.</summary>
        public Action? TrackingStopped;

        // Tool support
        public Action<Vector3>? OnCanvasClick;

        /// <summary>
        /// World-space radius for the tool preview circle drawn at the cursor.
        /// Set to NaN to disable the preview.
        /// </summary>
        public float ToolPreviewRadius { get; set; } = float.NaN;

        // Mouse position for tool preview
        private Vector3 _mouseWorldPosition = Vector3.Zero;
        // Whether the pointer has moved enough to count as a pan rather than a click
        private bool _hasPanned = false;
        private const double PanThreshold = 4.0;

        // Cached settings
        private Editor.MouseButton _panButton = EditorSettings.Instance.PanButton;
        private Editor.ZoomModifier _zoomModifier = EditorSettings.Instance.ZoomModifier;

        // Cached HUD bar brushes and objects (avoid per-frame allocations)
        private static readonly SolidColorBrush _hudBarBgDark = new SolidColorBrush(Color.FromArgb(210, 30, 30, 30));
        private static readonly SolidColorBrush _hudBarBgLight = new SolidColorBrush(Color.FromArgb(220, 240, 240, 240));
        private static readonly SolidColorBrush _hudBarBorderDark = new SolidColorBrush(Color.FromArgb(255, 58, 58, 58));
        private static readonly SolidColorBrush _hudBarBorderLight = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
        private static readonly SolidColorBrush _hudArrowDark = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush _hudArrowLight = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        private static readonly SolidColorBrush _hudTextPrimaryDark = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200));
        private static readonly SolidColorBrush _hudTextPrimaryLight = new SolidColorBrush(Color.FromArgb(220, 30, 30, 30));
        private static readonly SolidColorBrush _hudTextSecondaryDark = new SolidColorBrush(Color.FromArgb(140, 180, 180, 180));
        private static readonly SolidColorBrush _hudTextSecondaryLight = new SolidColorBrush(Color.FromArgb(180, 80, 80, 80));
        private static readonly Typeface _hudTypeface = new Typeface(FontFamily.Default);
        private static readonly SolidColorBrush _highlightPenBrush = new SolidColorBrush(Color.FromArgb(255, 255, 220, 0));

        // Reusable PathGeometry for wind arrow (mutated in-place each frame)
        private readonly PathGeometry _arrowGeo;
        private readonly PathFigure _arrowFig;
        private readonly LineSegment _arrowSeg1;
        private readonly LineSegment _arrowSeg2;

        private static double Lerp(double a, double b, double t) => a + (b - a) * t;

        private void EnsureScaledPens()
        {
            if (_cachedPenZoom == _zoom)
                return;
            double t = 1.0 / _zoom;
            _playerPen = new Pen(_playerPenBrush, t);
            _eventPen = new Pen(_eventPenBrush, t);
            _cityPen = new Pen(_cityPenBrush, t);
            _prefabPen = new Pen(_prefabPenBrush, t * 0.5);
            _highlightPen = new Pen(_highlightPenBrush, t * 2.0);
            _cachedPenZoom = _zoom;
        }

        // View toggles
        public bool ShowAgents { get; set; } = true;
        public bool ShowActiveAgents { get; set; } = true;
        public bool ShowRoads { get; set; } = true;
        public bool ShowBiomes { get; set; } = false;
        public bool ShowEvents { get; set; } = true;
        public bool ShowPrefabs { get; set; } = false;
        public bool ShowCities { get; set; } = true;

        public SimulationCanvas()
        {
            ClipToBounds = true;
            Cursor = new Cursor(StandardCursorType.Arrow);

            // Pre-build reusable arrow geometry
            _arrowSeg1 = new LineSegment { Point = default };
            _arrowSeg2 = new LineSegment { Point = default };
            _arrowFig = new PathFigure { IsClosed = true, IsFilled = true };
            _arrowFig.Segments.Add(_arrowSeg1);
            _arrowFig.Segments.Add(_arrowSeg2);
            _arrowGeo = new PathGeometry();
            _arrowGeo.Figures.Add(_arrowFig);
        }

        public void ApplySettings()
        {
            var s = EditorSettings.Instance;
            _panButton = s.PanButton;
            _zoomModifier = s.ZoomModifier;
        }

        private bool _toolActive = false;

        /// <summary>Call this whenever the active tool changes to update the cursor.</summary>
        public void SetToolActive(bool active)
        {
            _toolActive = active;
            if (!_isDragging)
                Cursor = new Cursor(active ? StandardCursorType.Cross : StandardCursorType.Arrow);
        }

        public double Zoom => _zoom;

        public void ZoomIn() => ZoomAroundCenter(_zoom * 1.25);
        public void ZoomOut() => ZoomAroundCenter(_zoom / 1.25);

        private void ZoomAroundCenter(double newZoom)
        {
            newZoom = Math.Clamp(newZoom, MinZoom, MaxZoom);
            var cx = Bounds.Width / 2.0;
            var cy = Bounds.Height / 2.0;
            _panX = cx - (cx - _panX) * (newZoom / _zoom);
            _panY = cy - (cy - _panY) * (newZoom / _zoom);
            _zoom = newZoom;
            ClampPan();
            InvalidateVisual();
        }
        public void ZoomReset()
        {
            _zoom = 1.0;
            _panX = 0.0;
            _panY = 0.0;
            InvalidateVisual();
        }

        // Fit the world so all contents are visible (fills the short axis, letterboxes the long axis).
        public void FitToView()
        {
            _zoom = 1.0;
            _panX = 0.0;
            _panY = 0.0;
            var w = Bounds.Width;
            var h = Bounds.Height;
            if (w <= 0 || h <= 0)
            {
                Dispatcher.UIThread.Post(FitToView, DispatcherPriority.Render);
                return;
            }
            InvalidateVisual();
        }

        private void SetZoom(double zoom)
        {
            _zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
            InvalidateVisual();
        }

        // Clamp pan so at least minVisible pixels of the world remain on screen.
        private void ClampPan()
        {
            var w = Bounds.Width;
            var h = Bounds.Height;
            if (w <= 0 || h <= 0)
                return;

            var viewSize = Math.Min(w, h - BarHeight);
            var worldHalf = (viewSize * _zoom) / 2.0;
            const double minVisible = 50.0;

            // World center is at (w/2 + panX, BarHeight + (h-BarHeight)/2 + panY) in screen space.
            _panX = Math.Clamp(_panX,
                minVisible - w / 2.0 - worldHalf,
                w / 2.0 - minVisible + worldHalf);
            _panY = Math.Clamp(_panY,
                minVisible - (h - BarHeight) / 2.0 - worldHalf,
                (h - BarHeight) / 2.0 - minVisible + worldHalf);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            var zoomActive = _zoomModifier switch
            {
                Editor.ZoomModifier.Ctrl => (e.KeyModifiers & KeyModifiers.Control) != 0,
                Editor.ZoomModifier.Shift => (e.KeyModifiers & KeyModifiers.Shift) != 0,
                Editor.ZoomModifier.None => true,
                _ => (e.KeyModifiers & KeyModifiers.Control) != 0,
            };
            if (zoomActive)
            {
                // Zoom toward the cursor position.
                var pos = e.GetPosition(this);
                var factor = e.Delta.Y > 0 ? 1.1 : 1.0 / 1.1;
                var newZoom = Math.Clamp(_zoom * factor, MinZoom, MaxZoom);
                var ratio = newZoom / _zoom;
                // The render transform zooms around the canvas center (width/2, height/2).
                // To keep the world point under the cursor fixed, adjust pan relative to that center.
                var screenCx = Bounds.Width / 2.0;
                var screenCy = Bounds.Height / 2.0;
                _panX = (pos.X - screenCx) * (1 - ratio) + _panX * ratio;
                _panY = (pos.Y - screenCy) * (1 - ratio) + _panY * ratio;
                _zoom = newZoom;
            }
            else
            {
                // Scroll vertically (or horizontally with shift).
                var shift = (e.KeyModifiers & KeyModifiers.Shift) != 0;
                double scrollAmount = 60.0;
                if (shift)
                    _panX += e.Delta.Y * scrollAmount;
                else
                    _panY += e.Delta.Y * scrollAmount;
            }
            ClampPan();
            InvalidateVisual();
            e.Handled = true;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var props = e.GetCurrentPoint(this).Properties;
            bool isPanButton = _panButton switch
            {
                Editor.MouseButton.Left => props.IsLeftButtonPressed,
                Editor.MouseButton.Right => props.IsRightButtonPressed,
                Editor.MouseButton.Middle => props.IsMiddleButtonPressed,
                _ => props.IsRightButtonPressed,
            };
            if (props.IsMiddleButtonPressed || isPanButton)
            {
                _isDragging = true;
                _hasPanned = false;
                _dragStart = e.GetPosition(this);
                Cursor = new Cursor(StandardCursorType.Hand);
                e.Pointer.Capture(this);
                e.Handled = true;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            var pos = e.GetPosition(this);
            _mouseWorldPosition = CanvasToWorld(pos);

            if (_isDragging)
            {
                var dx = pos.X - _dragStart.X;
                var dy = pos.Y - _dragStart.Y;

                if (Math.Abs(dx) > PanThreshold || Math.Abs(dy) > PanThreshold)
                    _hasPanned = true;

                // Stop tracking if the user manually pans the view.
                if (_hasPanned && _trackedAgent != null)
                {
                    _trackedAgent = null;
                    _highlightAgent = null;
                    _highlightTimer = 0;
                    _smoothActive = false;
                    TrackingStopped?.Invoke();
                }

                _panX += pos.X - _dragStart.X;
                _panY += pos.Y - _dragStart.Y;
                _dragStart = pos;
                ClampPan();
                InvalidateVisual();
            }
            else
            {
                // Hovering — redraw so preview circle follows the cursor
                if (!float.IsNaN(ToolPreviewRadius))
                    InvalidateVisual();
            }

            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_isDragging)
            {
                _isDragging = false;
                Cursor = new Cursor(_toolActive ? StandardCursorType.Cross : StandardCursorType.Arrow);
                e.Pointer.Capture(null);
                e.Handled = true;
            }
            else if (e.InitialPressMouseButton == Avalonia.Input.MouseButton.Left
                && _panButton != Editor.MouseButton.Left
                && _toolActive && OnCanvasClick != null)
            {
                OnCanvasClick(_mouseWorldPosition);
                e.Handled = true;
            }
        }

        // Convert a simulation world position to canvas pixel coordinates (unzoomed)
        private (double x, double y) SimToCanvas(Vector3 pos, double width, double height)
        {
            var mapped = _simulation.RemapPosition2D(pos, Vector3.Zero, new Vector3((float)width, (float)height));
            return (mapped.X, mapped.Y);
        }

        /// <summary>
        /// Convert a screen/pointer position to simulation world coordinates,
        /// accounting for viewport letterboxing, zoom and pan.
        /// Inverse of the Render transform:
        ///   screen = (world - vcx) * zoom + vcx + offsetX + panX
        ///   → world = (screen - offsetX - panX - vcx) / zoom + vcx
        /// </summary>
        private Vector3 CanvasToWorld(Point screenPos)
        {
            var bounds = Bounds;
            var width = bounds.Width;
            var height = bounds.Height;
            if (width <= 0 || height <= 0)
                return Vector3.Zero;

            var worldHeight = height - BarHeight;
            var viewSize = Math.Min(width, worldHeight);
            var offsetX = Math.Floor((width - viewSize) / 2.0);
            var offsetY = BarHeight + Math.Floor((worldHeight - viewSize) / 2.0);
            var vcx = viewSize / 2.0;
            var vcy = viewSize / 2.0;

            // Invert the render transform
            var worldX = (screenPos.X - offsetX - _panX - vcx) / _zoom + vcx;
            var worldY = (screenPos.Y - offsetY - _panY - vcy) / _zoom + vcy;

            var simX = (float)MathEx.Remap((float)worldX, 0f, (float)viewSize, _simulation.WorldMins.X, _simulation.WorldMaxs.X);
            var simY = (float)MathEx.Remap((float)worldY, 0f, (float)viewSize, _simulation.WorldMins.Y, _simulation.WorldMaxs.Y);

            return new Vector3(simX, simY, 0);
        }

        /// <summary>Compute the pan values that would center <paramref name="agent"/> at the given zoom.</summary>
        private (double panX, double panY) ComputeCenterPan(Agent agent, double zoom, double width, double height)
        {
            var viewSize = Math.Min(width, height);
            var (cx, cy) = SimToCanvas(agent.Position, viewSize, viewSize);
            // Derived from: screenCenter = (canvasPos - viewSize/2) * zoom + viewSize/2 + offset + pan
            // Solving for pan such that screenCenter = width/2:
            return ((viewSize / 2.0 - cx) * zoom, (viewSize / 2.0 - cy) * zoom);
        }

        /// <summary>Smoothly pan and zoom to center on the agent, then blink it briefly.</summary>
        public void GoToAgent(Agent agent)
        {
            _highlightAgent = agent;
            _highlightTimer = 2.5;
            _blinkPhase = 0;

            var bounds = Bounds;
            _smoothTargetZoom = Math.Max(_zoom, TrackZoom);
            if (bounds.Width > 0)
                (_smoothTargetPanX, _smoothTargetPanY) = ComputeCenterPan(agent, _smoothTargetZoom, bounds.Width, bounds.Height);
            _smoothActive = true;
            InvalidateVisual();
        }

        /// <summary>Continuously follow the agent, blinking it until tracking is stopped.</summary>
        public void TrackAgent(Agent agent)
        {
            _trackedAgent = agent;
            _highlightAgent = agent;
            _highlightTimer = double.MaxValue;
            _blinkPhase = 0;

            var bounds = Bounds;
            _smoothTargetZoom = TrackZoom;
            if (bounds.Width > 0)
                (_smoothTargetPanX, _smoothTargetPanY) = ComputeCenterPan(agent, _smoothTargetZoom, bounds.Width, bounds.Height);
            _smoothActive = true;
            InvalidateVisual();
        }

        /// <summary>Stop tracking and clear highlight.</summary>
        public void StopTracking()
        {
            _trackedAgent = null;
            _highlightAgent = null;
            _highlightTimer = 0;
            _smoothActive = false;
        }

        public void InvalidateGroupBrushes()
        {
            _cachedGroupCount = -1;
        }

        private void EnsureGroupBrushes()
        {
            var groupCount = _simulation.GroupCount;
            if (groupCount == _cachedGroupCount && _groupBrushes.Length > 0)
                return;

            var count = Math.Max(groupCount, 1);
            _groupBrushes = new IBrush[count];
            for (int i = 0; i < count; i++)
            {
                var c = _simulation.GetGroupColor(i);
                _groupBrushes[i] = new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
            }
            _cachedGroupCount = groupCount;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var bounds = Bounds;
            var width = bounds.Width;
            var height = bounds.Height;
            if (width <= 0 || height <= 0)
                return;

            // ── Animation step ─────────────────────────────────────────────────────
            var now = DateTime.UtcNow;
            var dt = Math.Min((now - _lastRenderTime).TotalSeconds, 0.1);
            _lastRenderTime = now;

            // Update tracking target every frame so the camera follows the moving agent.
            if (_trackedAgent != null)
            {
                _smoothTargetZoom = TrackZoom;
                (_smoothTargetPanX, _smoothTargetPanY) = ComputeCenterPan(_trackedAgent, _smoothTargetZoom, width, height);
                _smoothActive = true;
            }

            // Lerp pan and zoom toward targets.
            if (_smoothActive)
            {
                const double speed = 10.0;
                double lerpT = 1.0 - Math.Exp(-speed * dt);
                _panX = Lerp(_panX, _smoothTargetPanX, lerpT);
                _panY = Lerp(_panY, _smoothTargetPanY, lerpT);
                _zoom = Lerp(_zoom, _smoothTargetZoom, lerpT);
                ClampPan();

                // Stop one-shot smooth pan once close enough (tracking keeps it going).
                if (_trackedAgent == null &&
                    Math.Abs(_panX - _smoothTargetPanX) < 0.5 &&
                    Math.Abs(_panY - _smoothTargetPanY) < 0.5)
                    _smoothActive = false;
            }

            // Advance blink.
            if (_highlightAgent != null)
            {
                _blinkPhase = (_blinkPhase + dt * BlinkRate) % 2.0;
                if (_highlightTimer > 0 && _highlightTimer < double.MaxValue)
                {
                    _highlightTimer -= dt;
                    if (_highlightTimer <= 0)
                    {
                        _highlightTimer = 0;
                        _highlightAgent = null;
                    }
                }
            }

            // Fill the entire control background.
            context.FillRectangle(Brushes.Black, new Rect(bounds.Size));

            // Keep a 1:1 square viewport centered in the canvas so the world
            // map is never stretched when the window is resized.
            var worldHeight = height - BarHeight;
            var viewSize = Math.Min(width, worldHeight);
            var offsetX = Math.Floor((width - viewSize) / 2.0);
            var offsetY = BarHeight + Math.Floor((worldHeight - viewSize) / 2.0);

            EnsureGroupBrushes();
            EnsureScaledPens();

            // Zoom around the center of the square viewport, apply pan, then
            // translate to the centered position on the canvas.
            var vcx = viewSize / 2.0;
            var vcy = viewSize / 2.0;
            var transform = Matrix.CreateTranslation(-vcx, -vcy)
                          * Matrix.CreateScale(_zoom, _zoom)
                          * Matrix.CreateTranslation(vcx, vcy)
                          * Matrix.CreateTranslation(offsetX + _panX, offsetY + _panY);

            using (context.PushClip(new Rect(0, BarHeight, width, height - BarHeight)))
            using (context.PushTransform(transform))
            {
                if (ShowBiomes)
                    RenderBiomes(context, viewSize, viewSize);
                if (ShowRoads)
                    RenderRoads(context, viewSize, viewSize);
                if (ShowCities)
                    RenderCities(context, viewSize, viewSize);
                if (ShowPrefabs)
                    RenderPrefabs(context, viewSize, viewSize);
                if (ShowAgents)
                    RenderAgents(context, viewSize, viewSize);
                if (ShowActiveAgents)
                    RenderActiveAgents(context, viewSize, viewSize);
                RenderPlayers(context, viewSize, viewSize);
                if (ShowEvents)
                    RenderEvents(context, viewSize, viewSize);
                // Draw highlight/blink on top of everything.
                if (_highlightAgent != null && _blinkPhase < 1.0)
                    RenderHighlightAgent(context, viewSize, viewSize);
                // Draw active tool preview on top.
                if (OnCanvasClick != null && !float.IsNaN(ToolPreviewRadius))
                    RenderToolPreview(context, viewSize, viewSize);
            }

            // HUD bar — drawn in screen space, never affected by zoom/pan.
            RenderHUDBar(context, width);

            // Keep animating while tracking, smooth-panning, or blinking.
            bool animating = _smoothActive || _trackedAgent != null || _highlightAgent != null;
            if (animating)
                Dispatcher.UIThread.Post(() => InvalidateVisual(), Avalonia.Threading.DispatcherPriority.Render);
        }

        private void RenderHighlightAgent(DrawingContext context, double width, double height)
        {
            var agent = _highlightAgent;
            if (agent == null)
                return;

            EnsureScaledPens();
            var (cx, cy) = SimToCanvas(agent.Position, width, height);
            double r = 6.0 / _zoom;
            context.DrawEllipse(null, _highlightPen, new Point(cx, cy), r, r);
        }

        private IPen _toolPreviewPen;
        private double _cachedToolPreviewPenZoom = -1;

        private void RenderToolPreview(DrawingContext context, double width, double height)
        {
            // Rebuild the pen only when zoom changes so we don't allocate every frame.
            if (_cachedToolPreviewPenZoom != _zoom)
            {
                _toolPreviewPen = new Pen(Brushes.Red, 1.5 / _zoom);
                _cachedToolPreviewPenZoom = _zoom;
            }

            var (cx, cy) = SimToCanvas(_mouseWorldPosition, width, height);

            // Convert world-space radius to canvas-space radius
            var worldSize = _simulation.WorldSize;
            double r = ToolPreviewRadius / worldSize.X * width;

            context.DrawEllipse(null, _toolPreviewPen, new Point(cx, cy), r, r);
        }

        private void RenderRoads(DrawingContext context, double width, double height)
        {
            var mapData = _simulation.MapData;
            if (mapData?.Roads == null)
                return;

            var roads = mapData.Roads;
            if (roads.Width == 0 || roads.Height == 0)
                return;

            if (_roadsBitmap == null || _cachedRoadsPath != roads.Name)
            {
                _roadsBitmap?.Dispose();
                _roadsBitmap = BuildRoadsBitmap(roads);
                _cachedRoadsPath = roads.Name;
            }

            if (_roadsBitmap != null)
                context.DrawImage(_roadsBitmap, new Rect(0, 0, width, height));
        }

        private WriteableBitmap BuildRoadsBitmap(Roads roads)
        {
            var bmp = new WriteableBitmap(
                new PixelSize(roads.Width, roads.Height),
                new Vector(96, 96),
                Avalonia.Platform.PixelFormat.Bgra8888,
                Avalonia.Platform.AlphaFormat.Premul);

            using (var buf = bmp.Lock())
            {
                int stride = buf.RowBytes;
                var pixels = new byte[roads.Height * stride];

                for (int y = 0; y < roads.Height; y++)
                {
                    for (int x = 0; x < roads.Width; x++)
                    {
                        var roadType = roads.GetRoadType(x, y);
                        if (roadType == RoadType.None)
                            continue;

                        byte alpha = roadType == RoadType.Asphalt ? (byte)100 : (byte)50;
                        int idx = y * stride + x * 4;
                        // Bgra8888 premultiplied: B,G,R,A
                        byte pv = (byte)(255 * alpha / 255);
                        pixels[idx + 0] = pv; // B
                        pixels[idx + 1] = pv; // G
                        pixels[idx + 2] = pv; // R
                        pixels[idx + 3] = alpha; // A
                    }
                }

                Marshal.Copy(pixels, 0, buf.Address, pixels.Length);
            }

            return bmp;
        }

        private void RenderBiomes(DrawingContext context, double width, double height)
        {
            var mapData = _simulation.MapData;
            if (mapData?.Biomes == null)
                return;

            var biomes = mapData.Biomes;
            if (_biomesBitmap == null || _cachedBiomesPath != biomes.Name)
            {
                _biomesBitmap?.Dispose();
                _biomesBitmap = BuildBiomesBitmap(biomes);
                _cachedBiomesPath = biomes.Name;
            }

            if (_biomesBitmap != null)
                context.DrawImage(_biomesBitmap, new Rect(0, 0, width, height));
        }

        private WriteableBitmap BuildBiomesBitmap(Biomes biomes)
        {
            int w = biomes.Width;
            int h = biomes.Height;
            if (w == 0 || h == 0)
                return null;

            var bmp = new WriteableBitmap(
                new PixelSize(w, h),
                new Vector(96, 96),
                Avalonia.Platform.PixelFormat.Bgra8888,
                Avalonia.Platform.AlphaFormat.Premul);

            using (var buf = bmp.Lock())
            {
                int stride = buf.RowBytes;
                var pixels = new byte[h * stride];

                foreach (var region in biomes.Regions)
                {
                    if (region.Type == Biomes.Type.Invalid)
                        continue;

                    var wsc = Biomes.GetColorForType(region.Type);
                    byte ra = 128;
                    byte rr = (byte)(wsc.R * ra / 255);
                    byte rg = (byte)(wsc.G * ra / 255);
                    byte rb = (byte)(wsc.B * ra / 255);

                    foreach (var (px, py) in region.Points)
                    {
                        if (px < 0 || py < 0 || px >= w || py >= h)
                            continue;
                        int idx = py * stride + px * 4;
                        pixels[idx + 0] = rb;
                        pixels[idx + 1] = rg;
                        pixels[idx + 2] = rr;
                        pixels[idx + 3] = ra;
                    }
                }

                Marshal.Copy(pixels, 0, buf.Address, pixels.Length);
            }

            return bmp;
        }

        private void RenderAgents(DrawingContext context, double width, double height)
        {
            var agents = _simulation.Agents;
            if (agents == null)
                return;

            // Draw at 1/zoom so the rect is always 1 screen pixel after the zoom transform.
            double px = 1.0 / _zoom;
            foreach (var agent in agents)
            {
                if (agent.CurrentState != Agent.State.Wandering)
                    continue;

                var (cx, cy) = SimToCanvas(agent.Position, width, height);
                var brush = _groupBrushes.Length > 0 ? _groupBrushes[agent.Group % _groupBrushes.Length] : Brushes.White;
                context.FillRectangle(brush, new Rect(cx, cy, px, px));
            }
        }

        private void RenderActiveAgents(DrawingContext context, double width, double height)
        {
            var active = _simulation.Active;
            if (active == null)
                return;

            double px = 1.0 / _zoom;
            foreach (var kv in active)
            {
                var agent = kv.Value;
                if (agent.CurrentState != Agent.State.Active)
                    continue;

                var (cx, cy) = SimToCanvas(agent.Position, width, height);
                context.FillRectangle(_activeAgentBrush, new Rect(cx, cy, px, px));
            }
        }

        private void RenderPlayers(DrawingContext context, double width, double height)
        {
            var worldSize = _simulation.WorldSize;
            var plyViewRadius = _simulation.Config?.SpawnActivationRadius ?? 96;

            foreach (var kv in _simulation.Players)
            {
                var player = kv.Value;
                var (cx, cy) = SimToCanvas(player.Position, width, height);

                double pd = 3.0 / _zoom;
                context.FillRectangle(_playerBrush, new Rect(cx - pd, cy - pd, pd * 2, pd * 2));

                var viewRadiusPx = MathEx.Remap(plyViewRadius, 0, worldSize.X, 0, (float)width);
                context.DrawEllipse(null, _playerPen,
                    new Point(cx, cy), viewRadiusPx, viewRadiusPx);
            }
        }

        private void RenderEvents(DrawingContext context, double width, double height)
        {
            var worldSize = _simulation.WorldSize;

            foreach (var ev in _simulation.Events)
            {
                var (cx, cy) = SimToCanvas(ev.Position, width, height);
                var radiusPx = MathEx.Remap(ev.Radius, 0, worldSize.X, 0, (float)width);
                context.DrawEllipse(null, _eventPen, new Point(cx, cy), radiusPx, radiusPx);
            }
        }

        private void RenderPrefabs(DrawingContext context, double width, double height)
        {
            var mapData = _simulation.MapData;
            if (mapData?.Prefabs?.Decorations == null)
                return;

            var worldSize = _simulation.WorldSize;

            foreach (var poi in mapData.Prefabs.Decorations)
            {
                var (cx, cy) = SimToCanvas(poi.Position, width, height);
                var sw = MathEx.Remap(poi.Bounds.X, 0, worldSize.X, 0, (float)width);
                var sh = MathEx.Remap(poi.Bounds.Y, 0, worldSize.Y, 0, (float)height);
                context.DrawRectangle(null, _prefabPen, new Rect(cx - sw / 2, cy - sh / 2, sw, sh));
            }
        }

        private void RenderCities(DrawingContext context, double width, double height)
        {
            var mapData = _simulation.MapData;
            if (mapData?.Cities?.CityList == null)
                return;

            var worldSize = _simulation.WorldSize;

            foreach (var city in mapData.Cities.CityList)
            {
                var (cx, cy) = SimToCanvas(city.Position, width, height);
                var sw = MathEx.Remap(city.Bounds.X, 0, worldSize.X, 0, (float)width);
                var sh = MathEx.Remap(city.Bounds.Y, 0, worldSize.Y, 0, (float)height);
                var rect = new Rect(cx - sw / 2, cy - sh / 2, sw, sh);
                context.FillRectangle(_cityBrush, rect);
                context.DrawRectangle(null, _cityPen, rect);
            }
        }

        // Cached pen for wind arrow shaft
        private static readonly Pen _arrowPenDark = new Pen(_hudArrowDark, 1.5);
        private static readonly Pen _arrowPenLight = new Pen(_hudArrowLight, 1.5);

        // Cached HUD header texts (static labels, created once per theme)
        private FormattedText _hudH1, _hudH2, _hudH3;
        // Cached HUD value texts (recreated only when string content changes)
        private string _hudV1Str, _hudV2Str, _hudV3Str;
        private FormattedText _hudV1, _hudV2, _hudV3;
        private bool _hudLastDark;

        private void EnsureHudHeaders(bool dark)
        {
            if (_hudH1 != null && _hudLastDark == dark)
                return;
            _hudLastDark = dark;
            var sec = dark ? _hudTextSecondaryDark : _hudTextSecondaryLight;
            _hudH1 = new FormattedText("Wind Dir", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _hudTypeface, 10, sec);
            _hudH2 = new FormattedText("Wind Target", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _hudTypeface, 10, sec);
            _hudH3 = new FormattedText("Next Change", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _hudTypeface, 10, sec);
            _hudV1Str = _hudV2Str = _hudV3Str = null;
        }

        private FormattedText HudVal(string s, ref string prev, ref FormattedText ft, bool dark)
        {
            if (s == prev && ft != null)
                return ft;
            prev = s;
            ft = new FormattedText(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _hudTypeface, 12,
                dark ? _hudTextPrimaryDark : _hudTextPrimaryLight);
            return ft;
        }

        private void RenderHUDBar(DrawingContext context, double width)
        {
            bool dark = Application.Current?.ActualThemeVariant != ThemeVariant.Light;

            var barBg = dark ? _hudBarBgDark : _hudBarBgLight;
            var barBorder = dark ? _hudBarBorderDark : _hudBarBorderLight;
            IBrush arrowBrush = dark ? _hudArrowDark : _hudArrowLight;

            // Background and border
            context.FillRectangle(barBg, new Rect(0, 0, width, BarHeight));
            context.FillRectangle(barBorder, new Rect(0, BarHeight - 1, width, 1));

            var windDir = _simulation.WindDirection;
            double cx = 28, cy = BarHeight / 2.0;
            double shaft = 8.0;
            double headL = 7.0;
            double headW = 4.0;

            double angle = Math.Atan2(windDir.Y, windDir.X);
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            double perpCos = Math.Cos(angle + Math.PI / 2), perpSin = Math.Sin(angle + Math.PI / 2);

            var tail = new Point(cx - cos * shaft, cy - sin * shaft);
            var headBase = new Point(cx + cos * (shaft - headL), cy + sin * (shaft - headL));
            var tip = new Point(cx + cos * shaft, cy + sin * shaft);

            context.DrawLine(dark ? _arrowPenDark : _arrowPenLight, tail, headBase);

            // Reuse cached arrow geometry, just update points
            var wing1 = new Point(headBase.X + perpCos * headW, headBase.Y + perpSin * headW);
            var wing2 = new Point(headBase.X - perpCos * headW, headBase.Y - perpSin * headW);
            _arrowFig.StartPoint = tip;
            _arrowSeg1.Point = wing1;
            _arrowSeg2.Point = wing2;
            context.DrawGeometry(arrowBrush, null, _arrowGeo);

            EnsureHudHeaders(dark);

            double textX = cx + shaft + 8;
            var windTarget = _simulation.WindDirectionTarget;
            var nextChange = _simulation.TickNextWindChange;

            var v1 = HudVal($"{windDir.X:0.00}, {windDir.Y:0.00}", ref _hudV1Str, ref _hudV1, dark);
            var v2 = HudVal($"{windTarget.X:0.00}, {windTarget.Y:0.00}", ref _hudV2Str, ref _hudV2, dark);
            var v3 = HudVal(nextChange.ToString(), ref _hudV3Str, ref _hudV3, dark);

            context.DrawText(_hudH1, new Point(textX, cy - _hudH1.Height - 1));
            context.DrawText(v1, new Point(textX, cy + 1));
            textX += Math.Max(_hudH1.Width, v1.Width) + 16;

            context.DrawText(_hudH2, new Point(textX, cy - _hudH2.Height - 1));
            context.DrawText(v2, new Point(textX, cy + 1));
            textX += Math.Max(_hudH2.Width, v2.Width) + 16;

            context.DrawText(_hudH3, new Point(textX, cy - _hudH3.Height - 1));
            context.DrawText(v3, new Point(textX, cy + 1));
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _roadsBitmap?.Dispose();
            _roadsBitmap = null;
            _biomesBitmap?.Dispose();
            _biomesBitmap = null;
        }
    }
}
