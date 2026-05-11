using DiagCore.Core.Common;
using DiagCore.Core.Models;

namespace DiagCore.Core.Diagnostics;

/// <summary>
/// Process / service / autorun / hotfix diagnostics. Mirrors the Processes menu
/// from the legacy PowerShell script (top 15 by CPU/RAM, find by name, list
/// services, autoruns, installed hotfixes).
/// </summary>
public interface IProcessDiagnostics
{
    Task<DiagnosticResult<IReadOnlyList<ProcessSnapshot>>> GetProcessesAsync(CancellationToken cancellationToken = default);

    /// <summary>Top processes ordered by total CPU time (decreasing).</summary>
    Task<DiagnosticResult<IReadOnlyList<ProcessSnapshot>>> GetTopProcessesByCpuAsync(int take = 15, CancellationToken cancellationToken = default);

    /// <summary>Top processes ordered by working-set memory (decreasing).</summary>
    Task<DiagnosticResult<IReadOnlyList<ProcessSnapshot>>> GetTopProcessesByRamAsync(int take = 15, CancellationToken cancellationToken = default);

    Task<DiagnosticResult<IReadOnlyList<ServiceSnapshot>>> GetServicesAsync(CancellationToken cancellationToken = default);

    /// <summary>Services with start mode Auto whose current state is not Running.</summary>
    Task<DiagnosticResult<IReadOnlyList<ServiceSnapshot>>> GetStoppedAutomaticServicesAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<IReadOnlyList<AutorunEntry>>> GetAutorunsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Installed hotfixes / Windows updates. Optional <paramref name="take"/> caps the result.
    /// </summary>
    Task<DiagnosticResult<IReadOnlyList<HotfixEntry>>> GetInstalledHotfixesAsync(int? take = null, CancellationToken cancellationToken = default);
}
