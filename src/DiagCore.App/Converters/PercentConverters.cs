using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DiagCore.App.Converters;

/// <summary>
/// Converts a 0-100 double into a <see cref="GridLength"/> star value, so a
/// percent value can drive a two-column Grid that visualises usage as a bar:
/// <code>
/// &lt;Grid&gt;
///   &lt;Grid.ColumnDefinitions&gt;
///     &lt;ColumnDefinition Width="{Binding Pct, Converter={StaticResource PercentToStar}}" /&gt;
///     &lt;ColumnDefinition Width="{Binding Pct, Converter={StaticResource PercentToStar}, ConverterParameter=Remaining}" /&gt;
///   &lt;/Grid.ColumnDefinitions&gt;
/// &lt;/Grid&gt;
/// </code>
/// </summary>
public sealed class PercentToStarConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var pct = value is double d ? Math.Clamp(d, 0, 100) : 0;
        var remaining = parameter is string s
            && s.Equals("Remaining", StringComparison.OrdinalIgnoreCase);
        var raw = remaining ? (100 - pct) : pct;
        // GridLength does not accept zero star values - clamp to a sliver so the
        // remaining column still allocates space at 100% and the used column at 0%.
        return new GridLength(Math.Max(0.0001, raw), GridUnitType.Star);
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps a 0-100 percent to one of the three semantic brushes:
/// &lt;60 → OkBrush, &lt;85 → WarningBrush, otherwise → DangerBrush.
/// </summary>
public sealed class PercentToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var pct = value is double d ? d : 0;
        var key = pct switch
        {
            < 60 => "OkBrush",
            < 85 => "WarningBrush",
            _ => "DangerBrush",
        };
        return Application.Current.TryFindResource(key) is Brush b ? b : Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
