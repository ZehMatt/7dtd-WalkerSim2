using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Editor.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public static readonly ColorToBrushConverter Instance = new ColorToBrushConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
                return new SolidColorBrush(color);
            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
