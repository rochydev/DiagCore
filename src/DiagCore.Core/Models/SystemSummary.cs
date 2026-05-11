namespace DiagCore.Core.Models;

/// <summary>
/// Aggregated "machine identity" snapshot. Each section is its own
/// <see cref="Common.DiagnosticResult{T}"/> so a single WMI failure
/// doesn't void the whole summary.
/// </summary>
public sealed record SystemSummary
{
    public required Common.DiagnosticResult<OperatingSystemInfo> OperatingSystem { get; init; }

    public required Common.DiagnosticResult<ComputerSystemInfo> ComputerSystem { get; init; }

    public required Common.DiagnosticResult<BiosInfo> Bios { get; init; }

    public required Common.DiagnosticResult<MotherboardInfo> Motherboard { get; init; }
}
