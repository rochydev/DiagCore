namespace DiagCore.Core.Models;

/// <summary>
/// Single processor package. A machine can have several entries for multi-socket
/// systems. Sourced from <c>Win32_Processor</c>.
/// </summary>
public sealed record CpuInfo
{
    public required string Name { get; init; }

    public required string Manufacturer { get; init; }

    public required uint PhysicalCores { get; init; }

    public required uint LogicalCores { get; init; }

    public required uint MaxClockMHz { get; init; }

    public required uint CurrentClockMHz { get; init; }

    public required string Socket { get; init; }

    /// <summary>Current load percentage as reported by WMI's snapshot, 0-100.</summary>
    public required ushort LoadPercentage { get; init; }
}
