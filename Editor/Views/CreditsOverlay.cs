using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using System;
using System.Globalization;

namespace Editor.Views
{
    // Scrolling credits — sized/placed as a transparent overlay sibling over
    // AboutVFXGl (or any other background). Does not render a background
    // itself; only text.
    public sealed class CreditsOverlay : Control
    {
        private const double ScrollSpeed = 18.0;
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
        private static readonly ImmutableSolidColorBrush _fgBrush = new ImmutableSolidColorBrush(Color.FromRgb(100, 210, 255));
        private static readonly ImmutableSolidColorBrush _shBrush = new ImmutableSolidColorBrush(Color.FromArgb(235, 0, 0, 0));

        private readonly DispatcherTimer _timer;
        private double _time;
        private double _scrollOffset;

        private double _charWidth;
        private bool _charWidthMeasured;
        private FormattedText[] _textCache;
        private FormattedText[] _shadowCache;

        // One-shot fade at end of credits
        private const double FadeDuration = 3.0;
        private double _fadeTimer;
        private double _fadeOpacity = 1.0;

        // Raised every frame once fade starts; listeners can sync their own
        // opacity (e.g. the info card) to the credits' fade.
        public event Action<double> FadeOpacityChanged;

        public CreditsOverlay()
        {
            IsHitTestVisible = false;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            _timer.Tick += (_, _) =>
            {
                double dt = _timer.Interval.TotalSeconds;
                _time += dt;
                _scrollOffset += ScrollSpeed * dt;

                // After all credits have scrolled off-screen, fade this
                // control and notify listeners (info card) to fade in sync.
                double lineHeight = 22.0;
                double topExclusion = 180.0;
                double visibleH = Math.Max(0, Bounds.Height - topExclusion);
                double completeOffset = visibleH + Credits.Length * lineHeight;
                if (_scrollOffset > completeOffset)
                {
                    _fadeTimer += dt;
                    _fadeOpacity = Math.Max(0.0, 1.0 - _fadeTimer / FadeDuration);
                    Opacity = _fadeOpacity;
                    FadeOpacityChanged?.Invoke(_fadeOpacity);
                }

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

        public override void Render(DrawingContext context)
        {
            const double lineHeight = 22.0;
            const double fontSize = 13.0;
            const double fadeZone = 60.0;
            const double topExclusion = 180.0;

            var bounds = Bounds;
            int w = (int)bounds.Width;
            int h = (int)bounds.Height;
            if (w < 1 || h < 1)
                return;

            if (!_charWidthMeasured)
            {
                var measure = new FormattedText("M", CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, CreditTypeface, fontSize, _fgBrush);
                _charWidth = measure.Width;
                _charWidthMeasured = true;

                _textCache = new FormattedText[Credits.Length];
                _shadowCache = new FormattedText[Credits.Length];
                for (int i = 0; i < Credits.Length; i++)
                {
                    if (!string.IsNullOrEmpty(Credits[i]))
                    {
                        _textCache[i] = new FormattedText(Credits[i], CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, CreditTypeface, fontSize, _fgBrush);
                        _shadowCache[i] = new FormattedText(Credits[i], CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, CreditTypeface, fontSize, _shBrush);
                    }
                }
            }

            double visibleH = h - topExclusion;
            if (visibleH < lineHeight)
                return;

            double baseY = topExclusion + visibleH - _scrollOffset;

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
                    // 8-direction outline — readable against any background
                    var shadow = _shadowCache[lineIdx];
                    double tx = baseX, ty = lineY + sineY;
                    context.DrawText(shadow, new Point(tx - 1, ty - 1));
                    context.DrawText(shadow, new Point(tx, ty - 1));
                    context.DrawText(shadow, new Point(tx + 1, ty - 1));
                    context.DrawText(shadow, new Point(tx - 1, ty));
                    context.DrawText(shadow, new Point(tx + 1, ty));
                    context.DrawText(shadow, new Point(tx - 1, ty + 1));
                    context.DrawText(shadow, new Point(tx, ty + 1));
                    context.DrawText(shadow, new Point(tx + 1, ty + 1));
                    context.DrawText(_textCache[lineIdx], new Point(tx, ty));
                }
            }
        }
    }
}
