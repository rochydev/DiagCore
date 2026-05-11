namespace DiagCore.Core.Models;

/// <summary>
/// ACPI thermal zone temperature reading. Many desktops do not expose this and
/// the corresponding diagnostic returns an empty list with a note.
///
/// Source: <c>MSAcpi_ThermalZoneTemperature</c> in the <c>root\wmi</c> namespace.
/// The raw value is in tenths of a kelvin: <c>celsius = (raw / 10) - 273.15</c>.
/// </summary>
public sealed record ThermalReading
{
    public required string Zone { get; init; }

    public required double Celsius { get; init; }
}
