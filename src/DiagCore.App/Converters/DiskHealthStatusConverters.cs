using System.Globalization;
using System.Windows.Data;
using DiagCore.App.Controls;
using DiagCore.Core.Models;

namespace DiagCore.App.Converters;

/// <summary>
/// Maps <see cref="DiskHealthStatus"/> to a localised label.
/// </summary>
public sealed class DiskHealthStatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            DiskHealthStatus.Healthy => "Saludable",
            DiskHealthStatus.Warning => "Advertencia",
            DiskHealthStatus.Unhealthy => "Crítico",
            _ => "Desconocido",
        };

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps <see cref="DiskHealthStatus"/> to a <see cref="BadgeVariant"/> for
/// <see cref="StatusBadge"/>.
/// </summary>
public sealed class DiskHealthStatusToBadgeVariantConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            DiskHealthStatus.Healthy => BadgeVariant.Ok,
            DiskHealthStatus.Warning => BadgeVariant.Warning,
            DiskHealthStatus.Unhealthy => BadgeVariant.Danger,
            _ => BadgeVariant.Neutral,
        };

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts a boolean to a "Sí" / "No" label (used for things like Secure
/// Boot Enabled, Tamper Protection, ...).
/// </summary>
public sealed class BoolToYesNoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "Sí" : "No";

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            "Sí" => true,
            "No" => false,
            _ => throw new NotSupportedException(),
        };
}
