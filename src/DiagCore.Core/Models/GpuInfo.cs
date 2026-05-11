namespace DiagCore.Core.Models;

/// <summary>
/// Graphics adapter snapshot. A machine can have several (integrated + discrete).
/// Sourced from <c>Win32_VideoController</c>.
/// </summary>
public sealed record GpuInfo
{
    public required string Name { get; init; }

    public required string VideoProcessor { get; init; }

    public required string DriverVersion { get; init; }

    public required DateTime? DriverDate { get; init; }

    /// <summary>
    /// VRAM in GB. <c>Win32_VideoController.AdapterRAM</c> is a uint32 and caps at
    /// ~4 GB on older drivers; cards beyond that report a clamped value.
    /// </summary>
    public required double? AdapterRamGB { get; init; }

    public required int CurrentHorizontalResolution { get; init; }

    public required int CurrentVerticalResolution { get; init; }

    public required int CurrentRefreshRateHz { get; init; }
}
