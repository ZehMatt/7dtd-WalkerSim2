using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Editor.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public static readonly StringToBrushConverter Instance = new StringToBrushConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string colorStr && !string.IsNullOrWhiteSpace(colorStr))
            {
                try
                {
                    var color = Color.Parse(colorStr);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // Fall through to default
                }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
