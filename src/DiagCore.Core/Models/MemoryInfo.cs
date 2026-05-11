namespace DiagCore.Core.Models;

/// <summary>
/// Aggregate physical-memory snapshot. Totals come from
/// <c>Win32_OperatingSystem</c> (in KB) and the per-module list comes from
/// <c>Win32_PhysicalMemory</c>.
/// </summary>
public sealed record MemoryInfo
{
    public required double TotalGB { get; init; }

    public required double UsedGB { get; init; }

    public required double FreeGB { get; init; }

    public required double UsedPercent { get; init; }

    public required IReadOnlyList<MemoryModule> Modules { get; init; }
}

/// <summary>Physical memory stick. Sourced from <c>Win32_PhysicalMemory</c>.</summary>
public sealed record MemoryModule
{
    public required double CapacityGB { get; init; }

    public required uint SpeedMHz { get; init; }

    public required string Manufacturer { get; init; }

    /// <summary>Slot label, e.g. <c>DIMM_A1</c>.</summary>
    public required string DeviceLocator { get; init; }

    public required string SerialNumber { get; init; }

    public required string PartNumber { get; init; }
}
