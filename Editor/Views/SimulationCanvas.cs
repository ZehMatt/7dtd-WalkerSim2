using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
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

        private void EnsureScaledPens()
        {
            if (_cachedPenZoom == _zoom) return;
            double t = 1.0 / _zoom;
            _playerPen  = new Pen(_playerPenBrush, t);
            _eventPen   = new Pen(_eventPenBrush, t);
            _cityPen    = new Pen(_cityPenBrush, t);
            _prefabPen  = new Pen(_prefabPenBrush, t * 0.5);
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
            if (w <= 0 || h <= 0) return;

            var viewSize = Math.Min(w, h);
            var worldHalf = (viewSize * _zoom) / 2.0;
            const double minVisible = 50.0;

            // World center is at (w/2 + panX, h/2 + panY) in screen space.
            // Ensure world right edge >= minVisible:  w/2 + panX + worldHalf >= minVisible
            // Ensure world left edge  <= w-minVisible: w/2 + panX - worldHalf <= w - minVisible
            _panX = Math.Clamp(_panX,
                minVisible - w / 2.0 - worldHalf,
                w / 2.0 - minVisible + worldHalf);
            _panY = Math.Clamp(_panY,
                minVisible - h / 2.0 - worldHalf,
                h / 2.0 - minVisible + worldHalf);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            var ctrl = (e.KeyModifiers & KeyModifiers.Control) != 0;
            if (ctrl)
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
            if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed ||
                e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _dragStart = e.GetPosition(this);
                Cursor = new Cursor(StandardCursorType.Hand);
                e.Pointer.Capture(this);
                e.Handled = true;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_isDragging)
            {
                var pos = e.GetPosition(this);
                _panX += pos.X - _dragStart.X;
                _panY += pos.Y - _dragStart.Y;
                _dragStart = pos;
                ClampPan();
                InvalidateVisual();
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_isDragging)
            {
                _isDragging = false;
                Cursor = new Cursor(StandardCursorType.Arrow);
                e.Pointer.Capture(null);
                e.Handled = true;
            }
        }

        // Convert a simulation world position to canvas pixel coordinates (unzoomed)
        private (double x, double y) SimToCanvas(Vector3 pos, double width, double height)
        {
            var mapped = _simulation.RemapPosition2D(pos, Vector3.Zero, new Vector3((float)width, (float)height));
            return (mapped.X, mapped.Y);
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
            if (width <= 0 || height <= 0) return;

            // Fill the entire control background.
            context.FillRectangle(Brushes.Black, new Rect(bounds.Size));

            // Keep a 1:1 square viewport centered in the canvas so the world
            // map is never stretched when the window is resized.
            var viewSize = Math.Min(width, height);
            var offsetX = Math.Floor((width - viewSize) / 2.0);
            var offsetY = Math.Floor((height - viewSize) / 2.0);

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

            using (context.PushClip(new Rect(bounds.Size)))
            using (context.PushTransform(transform))
            {
                if (ShowBiomes) RenderBiomes(context, viewSize, viewSize);
                if (ShowRoads) RenderRoads(context, viewSize, viewSize);
                if (ShowCities) RenderCities(context, viewSize, viewSize);
                if (ShowPrefabs) RenderPrefabs(context, viewSize, viewSize);
                if (ShowAgents) RenderAgents(context, viewSize, viewSize);
                if (ShowActiveAgents) RenderActiveAgents(context, viewSize, viewSize);
                RenderPlayers(context, viewSize, viewSize);
                if (ShowEvents) RenderEvents(context, viewSize, viewSize);
                RenderWindArrow(context);
            }
        }

        private void RenderRoads(DrawingContext context, double width, double height)
        {
            var mapData = _simulation.MapData;
            if (mapData?.Roads == null) return;

            var roads = mapData.Roads;
            if (roads.Width == 0 || roads.Height == 0) return;

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
                        if (roadType == RoadType.None) continue;

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
            if (mapData?.Biomes == null) return;

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
            if (w == 0 || h == 0) return null;

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
                    if (region.Type == Biomes.Type.Invalid) continue;

                    var wsc = Biomes.GetColorForType(region.Type);
                    byte ra = 128;
                    byte rr = (byte)(wsc.R * ra / 255);
                    byte rg = (byte)(wsc.G * ra / 255);
                    byte rb = (byte)(wsc.B * ra / 255);

                    foreach (var (px, py) in region.Points)
                    {
                        if (px < 0 || py < 0 || px >= w || py >= h) continue;
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
            if (agents == null) return;

            // Draw at 1/zoom so the rect is always 1 screen pixel after the zoom transform.
            double px = 1.0 / _zoom;
            foreach (var agent in agents)
            {
                if (agent.CurrentState != Agent.State.Wandering) continue;

                var (cx, cy) = SimToCanvas(agent.Position, width, height);
                var brush = _groupBrushes.Length > 0 ? _groupBrushes[agent.Group % _groupBrushes.Length] : Brushes.White;
                context.FillRectangle(brush, new Rect(cx, cy, px, px));
            }
        }

        private void RenderActiveAgents(DrawingContext context, double width, double height)
        {
            var active = _simulation.Active;
            if (active == null) return;

            double px = 1.0 / _zoom;
            foreach (var kv in active)
            {
                var agent = kv.Value;
                if (agent.CurrentState != Agent.State.Active) continue;

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
            if (mapData?.Prefabs?.Decorations == null) return;

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
            if (mapData?.Cities?.CityList == null) return;

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

        private void RenderWindArrow(DrawingContext context)
        {
            // Draw a simple wind direction arrow in the top-left corner
            var windDir = _simulation.WindDirection;
            double cx = 26, cy = 16, length = 14, headSize = 5;

            double dx = windDir.X * length;
            double dy = windDir.Y * length;

            var start = new Point(cx - dx, cy - dy);
            var end = new Point(cx + dx, cy + dy);

            var pen = new Pen(Brushes.White, 1.5);
            context.DrawLine(pen, start, end);

            // Simple arrowhead
            double angle = Math.Atan2(dy, dx);
            double a1 = angle + 2.5, a2 = angle - 2.5;
            context.DrawLine(pen, end, new Point(end.X - headSize * Math.Cos(a1), end.Y - headSize * Math.Sin(a1)));
            context.DrawLine(pen, end, new Point(end.X - headSize * Math.Cos(a2), end.Y - headSize * Math.Sin(a2)));
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
