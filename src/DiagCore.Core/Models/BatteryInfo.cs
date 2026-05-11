namespace DiagCore.Core.Models;

/// <summary>
/// <c>Win32_Battery.BatteryStatus</c> code mapping.
/// </summary>
public enum BatteryStatus
{
    Unknown = 0,
    /// <summary>The battery is discharging.</summary>
    Discharging = 1,
    /// <summary>System is plugged in (AC) and the battery is not charging.</summary>
    OnAcPower = 2,
    /// <summary>The battery is fully charged.</summary>
    FullyCharged = 3,
    /// <summary>Battery is low.</summary>
    Low = 4,
    /// <summary>Battery is critically low.</summary>
    Critical = 5,
    /// <summary>The battery is charging.</summary>
    Charging = 6,
    /// <summary>The battery is charging and high.</summary>
    ChargingAndHigh = 7,
    /// <summary>The battery is charging and low.</summary>
    ChargingAndLow = 8,
    /// <summary>The battery is charging and critically low.</summary>
    ChargingAndCritical = 9,
    /// <summary>The battery is on partial AC power.</summary>
    PartialPower = 10,
    /// <summary>The battery is on backup power.</summary>
    OnBackupPower = 11,
}

/// <summary>
/// Battery snapshot. Sourced from <c>Win32_Battery</c>. Desktops with no battery
/// produce an empty list from <see cref="Diagnostics.IHardwareDiagnostics.GetBatteryAsync"/>.
/// </summary>
public sealed record BatteryInfo
{
    public required string Name { get; init; }

    public required ushort EstimatedChargeRemaining { get; init; }

    public required BatteryStatus Status { get; init; }

    /// <summary>Estimated run time on battery (minutes), or null when on AC.</summary>
    public required int? EstimatedRunTimeMinutes { get; init; }

    public required uint? DesignVoltageMV { get; init; }
}
