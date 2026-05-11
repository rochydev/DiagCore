namespace DiagCore.Core.Models;

/// <summary>Running process snapshot. Sourced from <see cref="System.Diagnostics.Process"/>.</summary>
public sealed record ProcessSnapshot
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    /// <summary>Cumulative total CPU time (TotalProcessorTime) since process start.</summary>
    public required double CpuSeconds { get; init; }

    /// <summary>Working set in MB at sampling time.</summary>
    public required double RamMB { get; init; }

    public required int ThreadCount { get; init; }

    /// <summary>Process start time (null if access denied).</summary>
    public required DateTime? StartTime { get; init; }
}

/// <summary>Windows service snapshot. Sourced from <c>Win32_Service</c>.</summary>
public sealed record ServiceSnapshot
{
    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public required string State { get; init; }   // Running / Stopped / Paused / StartPending / StopPending

    public required string StartMode { get; init; }   // Auto / Manual / Disabled / Boot / System

    public required string Status { get; init; }   // OK / Error / Degraded / Unknown / ...

    public required string PathName { get; init; }
}

/// <summary>Autorun entry. Sourced from <c>Win32_StartupCommand</c>.</summary>
public sealed record AutorunEntry
{
    public required string Name { get; init; }

    public required string Command { get; init; }

    public required string Location { get; init; }

    public required string User { get; init; }
}

/// <summary>Installed Windows update hotfix. Sourced from <c>Win32_QuickFixEngineering</c>.</summary>
public sealed record HotfixEntry
{
    public required string HotfixId { get; init; }   // KB number

    public required string Description { get; init; }

    public required DateTime? InstalledOn { get; init; }

    public required string InstalledBy { get; init; }
}
