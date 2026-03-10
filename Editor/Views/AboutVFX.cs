using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Editor.Views
{
    public class AboutVFX : Control
    {
        public ChipSynth Synth { get; set; }

        private const int PixelScale = 2;
        private int _bufWidth;
        private int _bufHeight;
        private WriteableBitmap _bitmap;
        private readonly DispatcherTimer _timer;

        private byte[] _pixels;
        private byte[] _bloomBuf;
        private int _pixelBufSize;

        private double _scrollOffset = 0;
        private double _time = 0;
        private const double ScrollSpeed = 40.0;
        private const double SineAmplitude = 3.0;
        private const double SineFrequency = 0.08;
        private const double SineTimeSpeed = 2.0;
        private static readonly string[] Credits = new[]
        {
            "",
            "-- Shoutouts to friends and supporters --",
            "",
            "ALo",
            "AaronVanGeffen",
            "Abaddon",
            "Broxzier",
            "Duncanspumpkin",
            "Fin (FNS)",
            "Frantic_Dan",
            "General_Nuisance",
            "Guppycur",
            "HellsJanitor",
            "Ixel",
            "JonahBirch",
            "Kugi",
            "MrExodia",
            "PotcFdk",
            "Python1320",
            "TheUmyBomber",
            "Yusuke",
            "_Kilburn",
            "ev0",
            "jak9527",
            "knoxed",
            "pharrisee",
            "",
            "-- Special thanks to the creators of 7 Days to Die --",
            "The Fun Pimps",
            "",
            "",
            "Love you all <3",
            "",
        };
        private static readonly Typeface CreditTypeface = new Typeface("Consolas,Courier New,monospace");

        public AboutVFX()
        {
            ClipToBounds = true;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            _timer.Tick += (_, _) =>
            {
                double dt = _timer.Interval.TotalSeconds;
                _time += dt;
                _scrollOffset += ScrollSpeed * dt;
                InvalidateVisual();
            };
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _timer.Start();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _timer.Stop();
            base.OnDetachedFromVisualTree(e);
        }

        protected override Size MeasureOverride(Size availableSize) => availableSize;

        private void EnsureBuffers(int width, int height)
        {
            int fw = Math.Max(1, width / PixelScale);
            int fh = Math.Max(1, height / PixelScale);
            if (_bitmap != null && _bufWidth == fw && _bufHeight == fh)
                return;

            _bufWidth = fw;
            _bufHeight = fh;
            _bitmap = new WriteableBitmap(
                new PixelSize(fw, fh),
                new Vector(96, 96),
                Avalonia.Platform.PixelFormat.Bgra8888,
                Avalonia.Platform.AlphaFormat.Premul);

        }

        public override void Render(DrawingContext context)
        {
            var bounds = Bounds;
            int w = (int)bounds.Width;
            int h = (int)bounds.Height;
            if (w < 1 || h < 1)
                return;

            EnsureBuffers(w, h);
            RenderTunnel();

            if (_bitmap != null)
            {
                using (context.PushRenderOptions(_upscaleOptions))
                {
                    context.DrawImage(_bitmap, new Rect(0, 0, w, h));
                }
            }

            RenderCredits(context, w, h);
        }

        private const int MaxIter = 32;

        private static int JuliaIterate(double zr, double zi, double cr, double ci)
        {
            for (int i = 0; i < MaxIter; i++)
            {
                double zr2 = zr * zr;
                double zi2 = zi * zi;
                if (zr2 + zi2 > 4.0)
                    return i;
                zi = 2.0 * zr * zi + ci;
                zr = zr2 - zi2 + cr;
            }
            return MaxIter;
        }

        private void RenderTunnel()
        {
            if (_bitmap == null)
                return;

            int w = _bufWidth;
            int h = _bufHeight;

            using var fb = _bitmap.Lock();
            int totalBytes = fb.RowBytes * fb.Size.Height;
            if (_pixels == null || _pixelBufSize != totalBytes)
            {
                _pixels = new byte[totalBytes];
                _bloomBuf = new byte[totalBytes];
                _pixelBufSize = totalBytes;
            }
            var pixels = _pixels;

            double t = _time;

            double bassE = 0, leadE = 0, percE = 0, leadNote = 0;
            int chord = 0;
            var synth = Synth;
            if (synth != null)
            {
                bassE = synth.VisBass;
                leadE = synth.VisLead;
                percE = synth.VisPerc;
                leadNote = synth.VisLeadNote;
                chord = synth.VisChord;
            }

            double cr = -0.7 + 0.15 * Math.Cos(t * 0.3);
            double ci = 0.27015 + 0.15 * Math.Sin(t * 0.25);

            double shiftU = t * 0.4;
            double shiftV = t * 0.15;

            double lookX = Math.Sin(t * 0.15) * w * 0.12 + Math.Sin(t * 0.27) * w * 0.05;
            double lookY = Math.Cos(t * 0.11) * h * 0.10 + Math.Cos(t * 0.23) * h * 0.04;

            double hueBase = chord * 1.571 + t * 0.08;
            double hueFromMusic = leadNote * 0.8 + leadE * 1.5;
            double saturation = 0.6 + bassE * 0.35;
            double percBright = 1.0 + percE * 0.4;

            double cx = w * 0.5;
            double cy = h * 0.5;
            double maxDist = Math.Sqrt(w * w + h * h) * 0.5;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double dx2 = x - cx - lookX;
                    double dy2 = y - cy - lookY;
                    double dist2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);

                    double u = ((int)(32.0 * 128.0 / (dist2 + 1.0)) / 256.0 + shiftU) % 1.0;
                    double v = ((int)(256.0 * Math.Atan2(dy2, dx2) / (2.0 * Math.PI)) / 256.0 + shiftV) % 1.0;

                    double zr = (u - 0.5) * 3.0;
                    double zi = (v - 0.5) * 3.0;
                    int iter = JuliaIterate(zr, zi, cr, ci);
                    double frac = (double)iter / MaxIter;

                    double depthFade = Math.Clamp(dist2 / maxDist, 0, 1);
                    double brightness = (0.15 + 0.85 * (1.0 - depthFade)) * percBright;

                    int r, g, b;
                    if (iter == MaxIter)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    else
                    {
                        double hue = frac * 4.0 + hueBase + hueFromMusic;
                        double hr = Math.Sin(hue) * 0.5 + 0.5;
                        double hg = Math.Sin(hue + 2.094) * 0.5 + 0.5;
                        double hb = Math.Sin(hue + 4.189) * 0.5 + 0.5;

                        double gray = (hr + hg + hb) / 3.0;
                        hr = gray + (hr - gray) * saturation;
                        hg = gray + (hg - gray) * saturation;
                        hb = gray + (hb - gray) * saturation;

                        double iterBright = frac * frac;
                        r = (int)(hr * iterBright * 255 * brightness);
                        g = (int)(hg * iterBright * 255 * brightness);
                        b = (int)(hb * iterBright * 255 * brightness);
                    }

                    // Bell hit: bright white flash on Julia set edges
                    if (leadE > 0.01 && iter > MaxIter / 3 && iter < MaxIter)
                    {
                        double proximity = (double)(iter - MaxIter / 3) / (MaxIter - MaxIter / 3);
                        int white = (int)(proximity * proximity * leadE * 400);
                        r = Math.Min(255, r + white);
                        g = Math.Min(255, g + white);
                        b = Math.Min(255, b + white);
                    }

                    int px = y * fb.RowBytes + x * 4;
                    if (px + 3 < totalBytes)
                    {
                        pixels[px + 0] = (byte)Math.Clamp(b, 0, 255);
                        pixels[px + 1] = (byte)Math.Clamp(g, 0, 255);
                        pixels[px + 2] = (byte)Math.Clamp(r, 0, 255);
                        pixels[px + 3] = 255;
                    }
                }
            }

            // Blur pass
            int stride = fb.RowBytes;
            Array.Copy(pixels, _bloomBuf, totalBytes);
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int pi = y * stride + x * 4;
                    for (int c = 0; c < 3; c++)
                    {
                        int sum = 0;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                                sum += _bloomBuf[(y + dy) * stride + (x + dx) * 4 + c];
                        pixels[pi + c] = (byte)(sum / 9);
                    }
                }
            }

            // Bloom pass
            Array.Copy(pixels, _bloomBuf, totalBytes);
            for (int y = 2; y < h - 2; y++)
            {
                for (int x = 2; x < w - 2; x++)
                {
                    int pi = y * stride + x * 4;
                    int pb = _bloomBuf[pi];
                    int pg = _bloomBuf[pi + 1];
                    int pr = _bloomBuf[pi + 2];
                    if (pr + pg + pb < 200)
                        continue;

                    int sumB = 0, sumG = 0, sumR = 0;
                    for (int dy = -2; dy <= 2; dy++)
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            int ni = (y + dy) * stride + (x + dx) * 4;
                            sumB += _bloomBuf[ni];
                            sumG += _bloomBuf[ni + 1];
                            sumR += _bloomBuf[ni + 2];
                        }
                    pixels[pi] = (byte)Math.Min(255, pixels[pi] + sumB / 25 / 2);
                    pixels[pi + 1] = (byte)Math.Min(255, pixels[pi + 1] + sumG / 25 / 2);
                    pixels[pi + 2] = (byte)Math.Min(255, pixels[pi + 2] + sumR / 25 / 2);
                }
            }

            Marshal.Copy(pixels, 0, fb.Address, totalBytes);
        }

        private static readonly SolidColorBrush _creditFgBrush = new SolidColorBrush(Color.FromRgb(150, 255, 150));
        private static readonly SolidColorBrush _creditShBrush = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));

        private static readonly RenderOptions _upscaleOptions = new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.HighQuality };

        private double _charWidth;
        private bool _charWidthMeasured;

        private FormattedText[] _creditTextCache;
        private FormattedText[] _creditShadowCache;

        private void RenderCredits(DrawingContext context, int w, int h)
        {
            const double lineHeight = 22.0;
            const double fontSize = 13.0;
            const double fadeZone = 60.0;
            const double topExclusion = 180.0;

            if (!_charWidthMeasured)
            {
                var measure = new FormattedText("M", CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, CreditTypeface, fontSize, _creditFgBrush);
                _charWidth = measure.Width;
                _charWidthMeasured = true;

                _creditTextCache = new FormattedText[Credits.Length];
                _creditShadowCache = new FormattedText[Credits.Length];
                for (int i = 0; i < Credits.Length; i++)
                {
                    if (!string.IsNullOrEmpty(Credits[i]))
                    {
                        _creditTextCache[i] = new FormattedText(Credits[i], CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, CreditTypeface, fontSize, _creditFgBrush);
                        _creditShadowCache[i] = new FormattedText(Credits[i], CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, CreditTypeface, fontSize, _creditShBrush);
                    }
                }
            }

            double visibleH = h - topExclusion;
            if (visibleH < lineHeight)
                return;

            double totalHeight = Credits.Length * lineHeight;
            double baseY = topExclusion + visibleH - _scrollOffset % (totalHeight + visibleH);

            for (int lineIdx = 0; lineIdx < Credits.Length; lineIdx++)
            {
                string line = Credits[lineIdx];
                if (string.IsNullOrEmpty(line))
                    continue;

                double lineY = baseY + lineIdx * lineHeight;
                if (lineY < topExclusion - lineHeight || lineY > h + lineHeight)
                    continue;

                double fade = 1.0;
                if (lineY < topExclusion + fadeZone)
                    fade = Math.Max(0, (lineY - topExclusion) / fadeZone);
                else if (lineY > h - fadeZone)
                    fade = Math.Max(0, (h - lineY) / fadeZone);

                if (fade < 0.01)
                    continue;

                double textWidth = line.Length * _charWidth;
                double baseX = (w - textWidth) / 2.0;
                double sineY = Math.Sin((_time * SineTimeSpeed) + (baseX * SineFrequency)) * SineAmplitude;

                using (context.PushOpacity(fade))
                {
                    context.DrawText(_creditShadowCache[lineIdx], new Point(baseX + 1, lineY + sineY + 1));
                    context.DrawText(_creditTextCache[lineIdx], new Point(baseX, lineY + sineY));
                }
            }
        }
    }
}
