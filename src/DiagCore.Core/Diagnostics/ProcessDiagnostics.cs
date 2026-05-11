using System.Runtime.Versioning;
using DiagCore.Core.Common;
using DiagCore.Core.Models;
using Microsoft.Extensions.Logging;
using SysProcess = System.Diagnostics.Process;

namespace DiagCore.Core.Diagnostics;

/// <inheritdoc cref="IProcessDiagnostics"/>
[SupportedOSPlatform("windows")]
public sealed class ProcessDiagnostics : IProcessDiagnostics
{
    private const double BytesPerMB = 1024d * 1024d;

    private readonly ILogger<ProcessDiagnostics> _logger;

    public ProcessDiagnostics(ILogger<ProcessDiagnostics> logger)
    {
        _logger = logger;
    }

    // ---- Processes ----

    public Task<DiagnosticResult<IReadOnlyList<ProcessSnapshot>>> GetProcessesAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetProcesses, cancellationToken);

    public async Task<DiagnosticResult<IReadOnlyList<ProcessSnapshot>>> GetTopProcessesByCpuAsync(int take = 15, CancellationToken cancellationToken = default)
    {
        var all = await GetProcessesAsync(cancellationToken).ConfigureAwait(false);
        return all.IsFailure
            ? all
            : DiagnosticResult<IReadOnlyList<ProcessSnapshot>>.Success(
                all.Value.OrderByDescending(p => p.CpuSeconds).Take(take).ToList());
    }

    public async Task<DiagnosticResult<IReadOnlyList<ProcessSnapshot>>> GetTopProcessesByRamAsync(int take = 15, CancellationToken cancellationToken = default)
    {
        var all = await GetProcessesAsync(cancellationToken).ConfigureAwait(false);
        return all.IsFailure
            ? all
            : DiagnosticResult<IReadOnlyList<ProcessSnapshot>>.Success(
                all.Value.OrderByDescending(p => p.RamMB).Take(take).ToList());
    }

    private DiagnosticResult<IReadOnlyList<ProcessSnapshot>> GetProcesses()
    {
        try
        {
            _logger.LogDebug("Snapshotting Process.GetProcesses.");

            var snapshots = SysProcess.GetProcesses()
                .Select(SafeMap)
                .Where(p => p is not null)
                .Select(p => p!)
                .ToList();

            return DiagnosticResult<IReadOnlyList<ProcessSnapshot>>.Success(snapshots);
        }
        catch (Exception ex)
        {
            return DiagnosticResult<IReadOnlyList<ProcessSnapshot>>.FromException(ex, "Failed enumerating processes");
        }
    }

    private static ProcessSnapshot? SafeMap(SysProcess process)
    {
        try
        {
            using (process)
            {
                double cpuSeconds;
                DateTime? start;
                try { cpuSeconds = process.TotalProcessorTime.TotalSeconds; } catch { cpuSeconds = 0; }
                try { start = process.StartTime; } catch { start = null; }

                return new ProcessSnapshot
                {
                    Id = process.Id,
                    Name = process.ProcessName,
                    CpuSeconds = Math.Round(cpuSeconds, 2),
                    RamMB = Math.Round(process.WorkingSet64 / BytesPerMB, 2),
                    ThreadCount = process.Threads.Count,
                    StartTime = start,
                };
            }
        }
        catch
        {
            // Process exited or access denied between enumeration and inspection.
            return null;
        }
    }

    // ---- Services ----

    public Task<DiagnosticResult<IReadOnlyList<ServiceSnapshot>>> GetServicesAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetServices, cancellationToken);

    public async Task<DiagnosticResult<IReadOnlyList<ServiceSnapshot>>> GetStoppedAutomaticServicesAsync(CancellationToken cancellationToken = default)
    {
        var all = await GetServicesAsync(cancellationToken).ConfigureAwait(false);
        return all.IsFailure
            ? all
            : DiagnosticResult<IReadOnlyList<ServiceSnapshot>>.Success(
                all.Value
                    .Where(s => string.Equals(s.StartMode, "Auto", StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(s.State, "Running", StringComparison.OrdinalIgnoreCase))
                    .ToList());
    }

    private DiagnosticResult<IReadOnlyList<ServiceSnapshot>> GetServices()
    {
        _logger.LogDebug("Querying Win32_Service.");

        return WmiQuery.Query(
            "SELECT Name, DisplayName, State, StartMode, Status, PathName FROM Win32_Service",
            obj => new ServiceSnapshot
            {
                Name = WmiQuery.GetString(obj, "Name"),
                DisplayName = WmiQuery.GetString(obj, "DisplayName"),
                State = WmiQuery.GetString(obj, "State"),
                StartMode = WmiQuery.GetString(obj, "StartMode"),
                Status = WmiQuery.GetString(obj, "Status"),
                PathName = WmiQuery.GetString(obj, "PathName"),
            });
    }

    // ---- Autoruns ----

    public Task<DiagnosticResult<IReadOnlyList<AutorunEntry>>> GetAutorunsAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetAutoruns, cancellationToken);

    private DiagnosticResult<IReadOnlyList<AutorunEntry>> GetAutoruns()
    {
        _logger.LogDebug("Querying Win32_StartupCommand.");

        return WmiQuery.Query(
            "SELECT Name, Command, Location, User FROM Win32_StartupCommand",
            obj => new AutorunEntry
            {
                Name = WmiQuery.GetString(obj, "Name"),
                Command = WmiQuery.GetString(obj, "Command"),
                Location = WmiQuery.GetString(obj, "Location"),
                User = WmiQuery.GetString(obj, "User"),
            });
    }

    // ---- Hotfixes ----

    public Task<DiagnosticResult<IReadOnlyList<HotfixEntry>>> GetInstalledHotfixesAsync(int? take = null, CancellationToken cancellationToken = default) =>
        Task.Run(() => GetInstalledHotfixes(take), cancellationToken);

    private DiagnosticResult<IReadOnlyList<HotfixEntry>> GetInstalledHotfixes(int? take)
    {
        _logger.LogDebug("Querying Win32_QuickFixEngineering.");

        var allResult = WmiQuery.Query(
            "SELECT HotFixID, Description, InstalledOn, InstalledBy FROM Win32_QuickFixEngineering",
            obj => new HotfixEntry
            {
                HotfixId = WmiQuery.GetString(obj, "HotFixID"),
                Description = WmiQuery.GetString(obj, "Description"),
                InstalledOn = ParseHotfixInstalledOn(WmiQuery.GetString(obj, "InstalledOn")),
                InstalledBy = WmiQuery.GetString(obj, "InstalledBy"),
            });

        if (allResult.IsFailure)
        {
            return allResult;
        }

        var sorted = allResult.Value
            .OrderByDescending(h => h.InstalledOn ?? DateTime.MinValue)
            .ToList();

        if (take is int n && n > 0 && sorted.Count > n)
        {
            return DiagnosticResult<IReadOnlyList<HotfixEntry>>.Success(sorted.Take(n).ToList());
        }
        return DiagnosticResult<IReadOnlyList<HotfixEntry>>.Success(sorted);
    }

    // ---- Pure helpers ----

    /// <summary>
    /// Parses <c>Win32_QuickFixEngineering.InstalledOn</c>. The value comes in one
    /// of two shapes depending on locale: an ISO-ish date string or a hex epoch
    /// dword. We accept either and return null for unrecognised inputs.
    /// </summary>
    public static DateTime? ParseHotfixInstalledOn(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        // ISO-ish date (e.g. 11/15/2024 or 2024-11-15)
        if (DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeLocal, out var parsed))
        {
            return parsed;
        }

        // Hex dword (seconds since epoch) - rare locales export this.
        if (long.TryParse(raw, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out var epoch))
        {
            return DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime;
        }

        return null;
    }
}
