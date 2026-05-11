namespace DiagCore.Core.Models;

/// <summary>
/// OS-level facts about the running machine. Sourced from
/// <c>Win32_OperatingSystem</c> + <c>Win32_ComputerSystem</c>.
/// </summary>
public sealed record OperatingSystemInfo
{
    public required string Caption { get; init; }

    public required string Version { get; init; }

    public required string BuildNumber { get; init; }

    public required string Architecture { get; init; }

    /// <summary>Locale, e.g. <c>es-ES</c>.</summary>
    public required string Locale { get; init; }

    public required string RegisteredUser { get; init; }

    public required string ComputerName { get; init; }

    public required string Domain { get; init; }

    public required bool PartOfDomain { get; init; }

    public required DateTime InstallDate { get; init; }

    public required DateTime LastBootUpTime { get; init; }

    public TimeSpan Uptime => DateTime.Now - LastBootUpTime;
}
