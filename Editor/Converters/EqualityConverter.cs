using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Editor.Converters
{
    /// <summary>
    /// Returns true when the bound value equals the ConverterParameter (compared as int).
    /// Used to drive IsChecked on speed menu items.
    /// </summary>
    public class EqualityConverter : IValueConverter
    {
        public static readonly EqualityConverter Instance = new EqualityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intVal && parameter is int intParam)
                return intVal == intParam;

            // Parameter arrives as string from XAML
            if (value is int v && parameter is string s && int.TryParse(s, out var p))
                return v == p;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
