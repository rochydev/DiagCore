using DiagCore.Core.Common;
using DiagCore.Core.Models;

namespace DiagCore.Core.Diagnostics;

/// <summary>
/// Physical-hardware diagnostics: processor, memory, graphics adapters, battery
/// and ACPI thermals. Mirrors the Hardware menu from the legacy PowerShell
/// script (<c>Get-InfoCPU</c>, <c>Get-InfoRAM</c>, <c>Get-InfoGPU</c>,
/// <c>Get-InfoBateria</c>, <c>Get-InfoTemperatura</c>).
/// </summary>
public interface IHardwareDiagnostics
{
    Task<DiagnosticResult<IReadOnlyList<CpuInfo>>> GetProcessorsAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<MemoryInfo>> GetMemoryAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<IReadOnlyList<GpuInfo>>> GetGraphicsAdaptersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns one entry per battery. An empty success result means the device
    /// has no battery (a desktop or a server); a failure means the WMI query
    /// itself blew up.
    /// </summary>
    Task<DiagnosticResult<IReadOnlyList<BatteryInfo>>> GetBatteriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns ACPI thermal zone readings (Celsius). An empty success result is
    /// the most common outcome on consumer machines, which do not export these
    /// sensors. Document the legacy PowerShell behaviour: this is a best-effort
    /// readout, not a reliable temperature source.
    /// </summary>
    Task<DiagnosticResult<IReadOnlyList<ThermalReading>>> GetThermalsAsync(CancellationToken cancellationToken = default);
}
