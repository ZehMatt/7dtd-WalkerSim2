using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Editor.ViewModels
{
    public partial class ColorPickerViewModel : ObservableObject
    {
        private bool _updating = false;

        [ObservableProperty]
        private string _colorString = "#808080";

        [ObservableProperty]
        private int _colorR = 128;

        [ObservableProperty]
        private int _colorG = 128;

        [ObservableProperty]
        private int _colorB = 128;

        // Raised whenever the color changes (string or RGB sliders), so the control can sync its StyledProperty.
        internal Action<string>? ColorChanged;

        partial void OnColorStringChanged(string value)
        {
            if (_updating) return;
            _updating = true;
            try
            {
                var c = ParseColor(value);
                ColorR = c.R;
                ColorG = c.G;
                ColorB = c.B;
            }
            finally { _updating = false; }

            OnPropertyChanged(nameof(ColorValue));
            ColorChanged?.Invoke(value);
        }

        partial void OnColorRChanged(int value) { if (value < 0 || value > 255) { ColorR = Math.Clamp(value, 0, 255); return; } SyncStringFromRgb(); }
        partial void OnColorGChanged(int value) { if (value < 0 || value > 255) { ColorG = Math.Clamp(value, 0, 255); return; } SyncStringFromRgb(); }
        partial void OnColorBChanged(int value) { if (value < 0 || value > 255) { ColorB = Math.Clamp(value, 0, 255); return; } SyncStringFromRgb(); }

        private void SyncStringFromRgb()
        {
            if (_updating) return;
            _updating = true;
            var str = $"#{ColorR:X2}{ColorG:X2}{ColorB:X2}";
            try { ColorString = str; }
            finally { _updating = false; }

            OnPropertyChanged(nameof(ColorValue));
            ColorChanged?.Invoke(str);
        }

        public Color ColorValue
        {
            get
            {
                try { return Color.Parse(ColorString); }
                catch { return Colors.Gray; }
            }
        }

        private static Color ParseColor(string value)
        {
            if (!string.IsNullOrEmpty(value))
                try { return Color.Parse(value); }
                catch { }
            return Colors.Gray;
        }
    }
}
