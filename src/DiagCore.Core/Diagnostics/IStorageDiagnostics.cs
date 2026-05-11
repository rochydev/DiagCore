using DiagCore.Core.Common;
using DiagCore.Core.Models;

namespace DiagCore.Core.Diagnostics;

/// <summary>
/// Storage diagnostics: logical volumes (drive letters), physical disks (with
/// SMART health) and partitions. Mirrors the Storage menu from the legacy
/// PowerShell script (<c>Get-Volumenes</c>, <c>Get-Particiones</c>,
/// <c>Get-InfoDiscos</c>).
/// </summary>
public interface IStorageDiagnostics
{
    Task<DiagnosticResult<IReadOnlyList<VolumeInfo>>> GetVolumesAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<IReadOnlyList<PhysicalDiskInfo>>> GetPhysicalDisksAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<IReadOnlyList<PartitionInfo>>> GetPartitionsAsync(CancellationToken cancellationToken = default);
}
