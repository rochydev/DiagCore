using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DiagCore.App.Converters;

/// <summary>
/// not null / not empty → Visible, otherwise Collapsed. Pass <c>Invert</c>
/// to reverse. Treats <see cref="string.IsNullOrWhiteSpace"/> as empty.
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        bool hasValue = value switch
        {
            null => false,
            string str => !string.IsNullOrWhiteSpace(str),
            _ => true,
        };
        var effective = invert ? !hasValue : hasValue;
        return effective ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
