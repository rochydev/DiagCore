using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DiagCore.App.Converters;

/// <summary>
/// True → Visible, False → Collapsed. Pass <c>Invert</c> as the parameter to
/// reverse the mapping. Useful as a one-stop binding for empty-state /
/// loading-spinner / error-banner visibility.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        var raw = value is bool b && b;
        var effective = invert ? !raw : raw;
        return effective ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
