using DiagCore.Core.Common;
using DiagCore.Core.Models;

namespace DiagCore.Core.Diagnostics;

/// <summary>
/// Provides identity-level information about the running machine: operating
/// system, computer make/model, BIOS/UEFI firmware and motherboard. Each
/// method returns a <see cref="DiagnosticResult{T}"/> so partial WMI failures
/// don't crash the surrounding scan.
/// </summary>
public interface ISystemDiagnostics
{
    Task<DiagnosticResult<OperatingSystemInfo>> GetOperatingSystemAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<ComputerSystemInfo>> GetComputerSystemAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<BiosInfo>> GetBiosAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<MotherboardInfo>> GetMotherboardAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the four section calls in parallel and returns them aggregated. Each
    /// section retains its own success/failure state.
    /// </summary>
    Task<SystemSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
}
