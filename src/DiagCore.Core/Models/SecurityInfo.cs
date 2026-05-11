namespace DiagCore.Core.Models;

/// <summary>
/// Windows Defender / Microsoft Defender status snapshot. Sourced from
/// <c>MSFT_MpComputerStatus</c> in <c>root\Microsoft\Windows\Defender</c>.
/// </summary>
public sealed record DefenderStatus
{
    public required bool AntivirusEnabled { get; init; }

    public required bool RealTimeProtectionEnabled { get; init; }

    public required bool AntiSpywareEnabled { get; init; }

    public required bool TamperProtectionEnabled { get; init; }

    public required string AntivirusEngineVersion { get; init; }

    public required string AntivirusSignatureVersion { get; init; }

    public required DateTime? AntivirusSignatureLastUpdated { get; init; }

    public required DateTime? QuickScanEndTime { get; init; }

    public required DateTime? FullScanEndTime { get; init; }
}

/// <summary>One firewall profile (Domain / Private / Public).</summary>
public sealed record FirewallProfile
{
    public required string Name { get; init; }

    public required bool Enabled { get; init; }

    public required string DefaultInboundAction { get; init; }     // Allow / Block / NotConfigured

    public required string DefaultOutboundAction { get; init; }
}

/// <summary>Local user account. Sourced from <c>Win32_UserAccount</c> filtered to <c>LocalAccount=True</c>.</summary>
public sealed record LocalUser
{
    public required string Name { get; init; }

    public required string FullName { get; init; }

    public required string Description { get; init; }

    public required string Sid { get; init; }

    public required bool Disabled { get; init; }

    public required bool Lockout { get; init; }

    public required bool PasswordExpires { get; init; }

    public required bool PasswordRequired { get; init; }
}

/// <summary>Member of the local Administrators group.</summary>
public sealed record AdminGroupMember
{
    public required string Name { get; init; }

    public required string Domain { get; init; }

    public required string Sid { get; init; }

    /// <summary>Kind of account: <c>User</c>, <c>Group</c>, <c>Other</c>.</summary>
    public required string AccountType { get; init; }
}
