namespace DiagCore.Core.Models;

/// <summary>
/// Motherboard / baseboard facts. Sourced from <c>Win32_BaseBoard</c>.
/// </summary>
public sealed record MotherboardInfo
{
    public required string Manufacturer { get; init; }

    public required string Product { get; init; }

    public required string Version { get; init; }

    public required string SerialNumber { get; init; }
}
