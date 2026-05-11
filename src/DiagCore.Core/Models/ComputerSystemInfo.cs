namespace DiagCore.Core.Models;

/// <summary>
/// Identity of the physical machine. Sourced from <c>Win32_ComputerSystem</c>
/// and <c>Win32_ComputerSystemProduct</c>.
/// </summary>
public sealed record ComputerSystemInfo
{
    public required string Manufacturer { get; init; }

    public required string Model { get; init; }

    public required string SystemFamily { get; init; }

    public required string SystemType { get; init; }

    /// <summary>Hardware serial number (from <c>Win32_ComputerSystemProduct.IdentifyingNumber</c>).</summary>
    public required string SerialNumber { get; init; }

    /// <summary>Hardware UUID (from <c>Win32_ComputerSystemProduct.UUID</c>).</summary>
    public required string Uuid { get; init; }
}
