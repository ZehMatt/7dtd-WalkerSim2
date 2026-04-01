using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
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

        private double _scrollOffset;
        private double _time;
        private double _musicPhase;
        private const double ScrollSpeed = 10.0;
        private const double SineAmplitude = 3.0;
        private const double SineFrequency = 0.08;
        private const double SineTimeSpeed = 2.0;

        private static readonly string[] Credits = new[]
        {
            "",
            "-- Shoutouts to friends and supporters --",
            "",
            "Aaron",
            "Abaddon",
            "ALo",
            "Andrew McWatters",
            "Beretta_HD",
            "Broxzier",
            "CapsAdmin",
            "Duncanspumpkin",
            "ev0",
            "Fin (FNS)",
            "Frantic_Dan",
            "Frilioth",
            "General_Nuisance",
            "Guppycur",
            "HellsJanitor",
            "Ixel",
            "jak9527",
            "JaWoodle",
            "JonahBirch",
            "knoxed",
            "Kugi",
            "Left of Zen",
            "LRFLEW",
            "MrExodia",
            "PotcFdk",
            "Python1320",
            "TheUmyBomber",
            "Yusuke",
            "_Kilburn",
            "",
            "-- Special thanks to the creators of 7 Days to Die --",
            "The Fun Pimps",
            "",
            "",
            "Love you all <3",
            "",
        };

        private static readonly Typeface CreditTypeface = new Typeface("Consolas,Courier New,monospace");
        private static readonly SolidColorBrush _creditFgBrush = new SolidColorBrush(Color.FromRgb(100, 210, 255));
        private static readonly SolidColorBrush _creditShBrush = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));
        private static readonly RenderOptions _upscaleOptions = new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.HighQuality };

        private double _charWidth;
        private bool _charWidthMeasured;
        private FormattedText[] _creditTextCache;
        private FormattedText[] _creditShadowCache;

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
            RenderCity();

            if (_bitmap != null)
            {
                using (context.PushRenderOptions(_upscaleOptions))
                {
                    context.DrawImage(_bitmap, new Rect(0, 0, w, h));
                }
            }

            RenderCredits(context, w, h);
        }

        // --- Hash / math ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Fract(double x) => x - Math.Floor(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Mix(double a, double b, double t) => a + (b - a) * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Mod(double x, double y) => x - y * Math.Floor(x / y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IHash(int n)
        {
            n = (n << 13) ^ n;
            return (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Hash(int x, int y)
        {
            return IHash(x * 73856093 ^ y * 19349663) / (double)0x7fffffff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Hash(int x, int y, int seed)
        {
            return Hash(x + seed * 137, y + seed * 251);
        }

        // --- SDF ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double SdBox(double px, double py, double pz, double bx, double by, double bz)
        {
            double dx = Math.Abs(px) - bx;
            double dy = Math.Abs(py) - by;
            double dz = Math.Abs(pz) - bz;
            double mx = Math.Max(dx, Math.Max(dy, dz));
            double ox = Math.Max(dx, 0);
            double oy = Math.Max(dy, 0);
            double oz = Math.Max(dz, 0);
            return Math.Sqrt(ox * ox + oy * oy + oz * oz) + Math.Min(mx, 0);
        }

        private const double Cell = 8.0;
        private const double Street = 2.5;
        private const double MaxDist = 22.0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double BldgHeight(int idX, int idZ)
        {
            double h = Hash(idX, idZ);
            double r = 1.5 + h * 5.5;
            if (h > 0.72)
                r += 3.5 + Hash(idX, idZ, 1) * 2.5;
            if (h < 0.1)
                r = 0.3;
            return r;
        }

        // Cell ID aligned with building centers (buildings sit at x=0, Cell, 2*Cell...)
        // Cell boundaries fall in the streets, so crossing a street doesn't flip IDs
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CellId(double v)
        {
            return (int)Math.Floor((v + Cell * 0.5) / Cell);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Map(double px, double py, double pz)
        {
            double d = py; // ground
            double qx = Mod(px + Cell * 0.5, Cell) - Cell * 0.5;
            double qz = Mod(pz + Cell * 0.5, Cell) - Cell * 0.5;
            int idX = CellId(px);
            int idZ = CellId(pz);
            double h = BldgHeight(idX, idZ);
            double bw = (Cell - Street) * 0.5;
            double bldg = SdBox(qx, py - h * 0.5, qz, bw, h * 0.5, bw);
            return Math.Min(d, bldg);
        }

        private static double March(double ox, double oy, double oz, double dx, double dy, double dz, double maxT)
        {
            double t = 0;
            for (int i = 0; i < 36; i++)
            {
                double d = Map(ox + dx * t, oy + dy * t, oz + dz * t);
                if (d < 0.005)
                    return t;
                // Clamp step so we never skip a neighboring cell
                t += Math.Min(d, Cell * 0.45);
                if (t > maxT)
                    break;
            }
            return maxT + 1;
        }

        private static void CalcNormal(double px, double py, double pz, out double nx, out double ny, out double nz)
        {
            const double e = 0.01;
            nx = Map(px + e, py, pz) - Map(px - e, py, pz);
            ny = Map(px, py + e, pz) - Map(px, py - e, pz);
            nz = Map(px, py, pz + e) - Map(px, py, pz - e);
            double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len > 0.0001)
            { nx /= len; ny /= len; nz /= len; }
        }

        // Blackwall-style building surface shading
        // World-space dots using surface normal to pick correct UV axes
        private void ShadeBlackwall(double px, double py, double pz,
            double nx, double ny, double nz,
            int cidX, int cidZ, double bldgH,
            double bassE, double leadE, double energy,
            double time,
            out double cr, out double cg, out double cb)
        {
            cr = 0;
            cg = 0;
            cb = 0;

            // Pick 2 world axes based on which face we hit (avoid the perpendicular axis)
            double u, v;
            if (Math.Abs(ny) > Math.Abs(nx) && Math.Abs(ny) > Math.Abs(nz))
            { u = px; v = pz; } // top face
            else if (Math.Abs(nx) > Math.Abs(nz))
            { u = pz; v = py; } // X-facing wall
            else
            { u = px; v = py; } // Z-facing wall

            // Dot grid — same frequency on both axes so dots stay circular
            double freq = 7.0;
            double gu = u * freq;
            double gv = v * freq;
            int dotIx = (int)Math.Floor(gu);
            int dotIy = (int)Math.Floor(gv);
            double fracU = gu - dotIx;
            double fracV = gv - dotIy;

            // Per-dot properties seeded by building + position
            double dotHash = Hash(dotIx + cidX * 97, dotIy + cidZ * 53);
            double dotAlive = dotHash > 0.15 ? 1.0 : 0.0;
            double dotBaseB = 0.4 + dotHash * 0.6;
            double dotFlicker = 0.5 + 0.5 * Math.Sin(time * (1.0 + dotHash * 7.0) + dotHash * 6.28);

            // Circular dot with jittered center
            double cx = 0.5 + (Hash(dotIx, dotIy, 3) - 0.5) * 0.25;
            double cy = 0.5 + (Hash(dotIx, dotIy, 4) - 0.5) * 0.25;
            double dd = Math.Sqrt((fracU - cx) * (fracU - cx) + (fracV - cy) * (fracV - cy));
            double dotShape = Clamp01(1.0 - dd / 0.30);
            dotShape *= dotShape;

            // Height glow
            double heightRatio = Clamp01(py / Math.Max(bldgH, 0.1));
            double topGlow = Clamp01(heightRatio - 0.85) * 8.0;
            // VU meter level — music energy determines how high dots light up
            double level = energy * 0.6 + bassE * 0.25 + leadE * 0.15;
            double vuThreshold = Clamp01(level);
            double aboveLevel = heightRatio - vuThreshold;
            double vuFade = (aboveLevel > 0) ? Clamp01(1.0 - aboveLevel * 6.0) : 1.0;

            // Pattern determines which dots are ON — rest stay dark
            int pattern = IHash(cidX * 31 + cidZ * 59) % 4;
            double p;
            switch (pattern)
            {
                case 0: // Ripple from center
                    double rdx = dotIx - 3.5;
                    double rdy = dotIy - 3.5;
                    p = Math.Sin(Math.Sqrt(rdx * rdx + rdy * rdy) * 1.8 - time * 2.5);
                    break;
                case 1: // Horizontal sweep
                    p = Math.Sin(dotIx * 1.0 - time * 2.0 + cidX);
                    break;
                case 2: // Vertical sweep
                    p = Math.Sin(dotIy * 1.0 + time * 2.2 + cidZ);
                    break;
                default: // Diagonal sweep
                    p = Math.Sin((dotIx + dotIy) * 0.7 - time * 1.8 + cidX * 0.5);
                    break;
            }
            // Sharp threshold — dot is either on or off
            double onOff = p > 0.3 ? 1.0 : 0.05;

            double dot = dotBaseB * dotShape * dotAlive * vuFade * onOff;
            dot += topGlow * dotShape * dotAlive * onOff;
            dot *= 14.0;

            double bright = dot + 0.005;

            // Per-building color: red, green, or blue
            int colorSlot = IHash(cidX * 71 + cidZ * 37) % 3;
            switch (colorSlot)
            {
                case 0: cr = bright; cg = bright * 0.04; cb = bright * 0.02; break;
                case 1: cr = bright * 0.02; cg = bright; cb = bright * 0.04; break;
                default: cr = bright * 0.02; cg = bright * 0.04; cb = bright; break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HsvToRgb(double h, double s, double v, out double r, out double g, out double b)
        {
            h = (h % 1.0 + 1.0) % 1.0;
            h *= 6.0;
            int i = (int)h;
            double f = h - i;
            double p = v * (1 - s);
            double q = v * (1 - s * f);
            double t = v * (1 - s * (1 - f));
            switch (i % 6)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                default:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }
        }

        private void RenderCity()
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

            double t = _time;
            double bassE = 0, percE = 0, energy = 0, leadE = 0, arpE = 0;
            double bassNote = 0, leadNote = 0;
            int chord = 0;
            var synth = Synth;
            if (synth != null)
            {
                bassE = synth.VisBass;
                percE = synth.VisPerc;
                energy = synth.VisEnergy;
                leadE = synth.VisLead;
                arpE = synth.VisFlute;
                chord = synth.VisChord;
                bassNote = synth.VisBassNote;
                leadNote = synth.VisLeadNote;
            }
            // Music energy advances color phase — louder = faster drift
            _musicPhase += (0.15 + energy * 0.4) * _timer.Interval.TotalSeconds;

            // Camera
            double spd = 1.8;
            double camX = t * spd + Math.Sin(t * 0.2) * 2.0;
            double camY = 14.0 + Math.Sin(t * 0.35) * 1.0;
            double camZ = t * spd * 0.5 + Math.Cos(t * 0.25) * 3.0;

            double la = t * 0.12;
            double tgtX = camX + Math.Cos(la) * 5.0;
            double tgtY = camY - 6.0;
            double tgtZ = camZ + Math.Sin(la) * 2.0 + 4.0;

            double fwdX = tgtX - camX, fwdY = tgtY - camY, fwdZ = tgtZ - camZ;
            double fwdLen = Math.Sqrt(fwdX * fwdX + fwdY * fwdY + fwdZ * fwdZ);
            fwdX /= fwdLen;
            fwdY /= fwdLen;
            fwdZ /= fwdLen;

            double rtX = fwdZ, rtY = 0, rtZ = -fwdX;
            double rtLen = Math.Sqrt(rtX * rtX + rtZ * rtZ);
            rtX /= rtLen;
            rtZ /= rtLen;

            double upX = rtY * fwdZ - rtZ * fwdY;
            double upY = rtZ * fwdX - rtX * fwdZ;
            double upZ = rtX * fwdY - rtY * fwdX;

            int stride = fb.RowBytes;
            double invH = 1.0 / h;
            double localTime = _time;
            double localPhase = _musicPhase;

            System.Threading.Tasks.Parallel.For(0, h, py =>
            {
                double vy = -(py * 2.0 * invH - 1.0);
                for (int px = 0; px < w; px++)
                {
                    double vx = (px * 2.0 / h - (double)w / h);

                    double rdx = vx * rtX + vy * upX + 1.5 * fwdX;
                    double rdy = vx * rtY + vy * upY + 1.5 * fwdY;
                    double rdz = vx * rtZ + vy * upZ + 1.5 * fwdZ;
                    double rdLen = Math.Sqrt(rdx * rdx + rdy * rdy + rdz * rdz);
                    rdx /= rdLen;
                    rdy /= rdLen;
                    rdz /= rdLen;

                    double dist = March(camX, camY, camZ, rdx, rdy, rdz, MaxDist);

                    // Sky — dark with subtle crimson fog
                    double cr = 0.03, cg = 0.005, cb = 0.01;
                    double horizGlow = Math.Max(0, 1.0 - Math.Abs(rdy) * 3.0);
                    cr += 0.08 * horizGlow;
                    cg += 0.01 * horizGlow;
                    cb += 0.02 * horizGlow;

                    if (dist < MaxDist)
                    {
                        double hitX = camX + rdx * dist;
                        double hitY = camY + rdy * dist;
                        double hitZ = camZ + rdz * dist;

                        if (hitY < 0.02)
                        {
                            // Ground — dark with reflected building glow
                            cr = 0.01;
                            cg = 0.005;
                            cb = 0.008;

                            int refIdX = CellId(hitX);
                            int refIdZ = CellId(hitZ);
                            double bh = BldgHeight(refIdX, refIdZ);
                            if (bh > 1.0)
                            {
                                double refHue = Hash(refIdX * 7, refIdZ * 13) + localPhase + bassE * 0.03;
                                HsvToRgb(refHue, 0.85, 4.0, out double nr, out double ng, out double nb);
                                double distToB = Math.Abs(Mod(hitX + Cell * 0.5, Cell) - Cell * 0.5);
                                double falloff = Math.Exp(-distToB * 0.6) * (0.5 + 1.5 * bassE + 0.8 * energy);
                                double fresnel = 0.15 + 0.85 * Math.Pow(Clamp01(1.0 - Math.Abs(rdy)), 4);
                                cr += nr * falloff * fresnel;
                                cg += ng * falloff * fresnel;
                                cb += nb * falloff * fresnel;
                            }

                            // Subtle scattered bright dots on ground
                            int dotX = (int)Math.Floor(hitX * 3);
                            int dotZ = (int)Math.Floor(hitZ * 3);
                            if (Hash(dotX, dotZ, 20) > 0.94)
                            {
                                double dotBright = 0.15 * (0.3 + bassE * 0.7);
                                cr += dotBright * 0.9;
                                cg += dotBright * 0.1;
                                cb += dotBright * 0.15;
                            }
                        }
                        else
                        {
                            int cidX = CellId(hitX);
                            int cidZ = CellId(hitZ);
                            double bh = BldgHeight(cidX, cidZ);

                            CalcNormal(hitX, hitY, hitZ, out double nx, out double ny, out double nz);

                            // Edge detection via SDF gradient discontinuity
                            const double edgeE = 0.06;
                            double dR = Map(hitX + edgeE, hitY, hitZ);
                            double dU = Map(hitX, hitY + edgeE, hitZ);
                            double dF = Map(hitX, hitY, hitZ + edgeE);
                            double dC = Map(hitX, hitY, hitZ);
                            double edgeGrad = Math.Abs(dR - dC) + Math.Abs(dU - dC) + Math.Abs(dF - dC);
                            // Sharp threshold — only actual edges, not gradual surfaces
                            double edgeFactor = Clamp01((edgeGrad - 0.3) * 8.0);

                            ShadeBlackwall(hitX, hitY, hitZ, nx, ny, nz, cidX, cidZ, bh,
                                bassE, leadE, energy,
                                localTime,
                                out cr, out cg, out cb);

                            // Thin dark red edge lines
                            if (edgeFactor > 0.01)
                            {
                                double edgeBright = 2.0 + 1.5 * bassE;
                                cr = Mix(cr, edgeBright * 0.6, edgeFactor * 0.5);
                                cg = Mix(cg, edgeBright * 0.01, edgeFactor * 0.5);
                                cb = Mix(cb, edgeBright * 0.03, edgeFactor * 0.5);
                            }
                        }

                        // Distance fog — crimson tinted, fully obscures at MaxDist
                        double fog = 1.0 - Math.Exp(-dist * 0.12);
                        double fogR = 0.06 + 0.04 * energy + 0.03 * bassE;
                        double fogG = 0.01 + 0.01 * energy;
                        double fogB = 0.02 + 0.02 * energy;
                        cr = Mix(cr, fogR, fog);
                        cg = Mix(cg, fogG, fog);
                        cb = Mix(cb, fogB, fog);
                    }

                    // Bass pulse on atmosphere
                    cr += 0.04 * bassE;
                    cg += 0.005 * bassE;
                    cb += 0.01 * bassE;

                    // Vignette
                    double vigX = (double)px / w - 0.5;
                    double vigY = (double)py / h - 0.5;
                    double vig = 1.0 - (vigX * vigX + vigY * vigY) * 1.3;
                    cr *= vig;
                    cg *= vig;
                    cb *= vig;

                    // Tonemap — extended Reinhard with whitepoint for HDR glow
                    const double W = 15.0; // whitepoint — values above this clip to 1
                    cr = cr * (1.0 + cr / (W * W)) / (1.0 + cr);
                    cg = cg * (1.0 + cg / (W * W)) / (1.0 + cg);
                    cb = cb * (1.0 + cb / (W * W)) / (1.0 + cb);

                    // Gamma — slightly bright to keep glow visible
                    cr = Math.Pow(Math.Max(0, cr), 0.75);
                    cg = Math.Pow(Math.Max(0, cg), 0.75);
                    cb = Math.Pow(Math.Max(0, cb), 0.75);

                    // Scanlines
                    double scan = 0.95 + 0.05 * Math.Sin(py * 6.28);
                    cr *= scan;
                    cg *= scan;
                    cb *= scan;

                    int pidx = py * stride + px * 4;
                    if (pidx + 3 < totalBytes)
                    {
                        _pixels[pidx + 0] = (byte)Math.Clamp((int)(cb * 255), 0, 255);
                        _pixels[pidx + 1] = (byte)Math.Clamp((int)(cg * 255), 0, 255);
                        _pixels[pidx + 2] = (byte)Math.Clamp((int)(cr * 255), 0, 255);
                        _pixels[pidx + 3] = 255;
                    }
                }
            });

            // Two-pass bloom for emissive glow
            Array.Copy(_pixels, _bloomBuf, totalBytes);

            // Pass 1: horizontal blur of bright pixels
            for (int y = 0; y < h; y++)
            {
                for (int x = 2; x < w - 2; x++)
                {
                    int pi = y * stride + x * 4;
                    int sum = _bloomBuf[pi] + _bloomBuf[pi + 1] + _bloomBuf[pi + 2];
                    if (sum < 120)
                        continue;

                    for (int c = 0; c < 3; c++)
                    {
                        int s = 0;
                        for (int dx = -2; dx <= 2; dx++)
                            s += _bloomBuf[y * stride + (x + dx) * 4 + c];
                        _pixels[pi + c] = (byte)Math.Min(255, _pixels[pi + c] + s / 5 / 2);
                    }
                }
            }

            // Pass 2: vertical blur
            Array.Copy(_pixels, _bloomBuf, totalBytes);
            for (int y = 2; y < h - 2; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int pi = y * stride + x * 4;
                    int sum = _bloomBuf[pi] + _bloomBuf[pi + 1] + _bloomBuf[pi + 2];
                    if (sum < 120)
                        continue;

                    for (int c = 0; c < 3; c++)
                    {
                        int s = 0;
                        for (int dy = -2; dy <= 2; dy++)
                            s += _bloomBuf[(y + dy) * stride + x * 4 + c];
                        _pixels[pi + c] = (byte)Math.Min(255, _pixels[pi + c] + s / 5 / 3);
                    }
                }
            }

            Marshal.Copy(_pixels, 0, fb.Address, totalBytes);
        }

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
