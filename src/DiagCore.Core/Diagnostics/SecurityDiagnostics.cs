using System.Management;
using System.Runtime.Versioning;
using DiagCore.Core.Common;
using DiagCore.Core.Models;
using Microsoft.Extensions.Logging;

namespace DiagCore.Core.Diagnostics;

/// <inheritdoc cref="ISecurityDiagnostics"/>
[SupportedOSPlatform("windows")]
public sealed class SecurityDiagnostics : ISecurityDiagnostics
{
    private const string DefenderScope = @"\\.\root\Microsoft\Windows\Defender";

    private readonly ILogger<SecurityDiagnostics> _logger;

    public SecurityDiagnostics(ILogger<SecurityDiagnostics> logger)
    {
        _logger = logger;
    }

    public Task<DiagnosticResult<DefenderStatus>> GetDefenderStatusAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetDefenderStatus, cancellationToken);

    public Task<DiagnosticResult<IReadOnlyList<FirewallProfile>>> GetFirewallProfilesAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetFirewallProfiles, cancellationToken);

    public Task<DiagnosticResult<IReadOnlyList<LocalUser>>> GetLocalUsersAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetLocalUsers, cancellationToken);

    public Task<DiagnosticResult<IReadOnlyList<AdminGroupMember>>> GetLocalAdministratorsAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetLocalAdministrators, cancellationToken);

    // ---- Defender ----

    private DiagnosticResult<DefenderStatus> GetDefenderStatus()
    {
        _logger.LogDebug("Querying MSFT_MpComputerStatus.");

        return WmiQuery.First(
            DefenderScope,
            "SELECT AntivirusEnabled, RealTimeProtectionEnabled, AntispywareEnabled, IsTamperProtected, AMEngineVersion, AntivirusSignatureVersion, AntivirusSignatureLastUpdated, QuickScanEndTime, FullScanEndTime FROM MSFT_MpComputerStatus",
            obj => new DefenderStatus
            {
                AntivirusEnabled = WmiQuery.GetValue(obj, "AntivirusEnabled", false),
                RealTimeProtectionEnabled = WmiQuery.GetValue(obj, "RealTimeProtectionEnabled", false),
                AntiSpywareEnabled = WmiQuery.GetValue(obj, "AntispywareEnabled", false),
                TamperProtectionEnabled = WmiQuery.GetValue(obj, "IsTamperProtected", false),
                AntivirusEngineVersion = WmiQuery.GetString(obj, "AMEngineVersion"),
                AntivirusSignatureVersion = WmiQuery.GetString(obj, "AntivirusSignatureVersion"),
                AntivirusSignatureLastUpdated = ReadDefenderDate(obj, "AntivirusSignatureLastUpdated"),
                QuickScanEndTime = ReadDefenderDate(obj, "QuickScanEndTime"),
                FullScanEndTime = ReadDefenderDate(obj, "FullScanEndTime"),
            });
    }

    private static DateTime? ReadDefenderDate(ManagementObject obj, string property)
    {
        try
        {
            var raw = obj[property];
            return raw switch
            {
                null => null,
                DateTime dt => dt,
                string s when !string.IsNullOrWhiteSpace(s) => SystemDiagnostics.ParseCimDateTimeOrNull(s),
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    // ---- Firewall ----

    private DiagnosticResult<IReadOnlyList<FirewallProfile>> GetFirewallProfiles()
    {
        _logger.LogDebug("Querying MSFT_NetFirewallProfile in {Scope}.", WmiQuery.StandardCimv2);

        return WmiQuery.Query(
            WmiQuery.StandardCimv2,
            "SELECT Name, Enabled, DefaultInboundAction, DefaultOutboundAction FROM MSFT_NetFirewallProfile",
            obj => new FirewallProfile
            {
                Name = WmiQuery.GetString(obj, "Name"),
                // Enabled is exposed as uint16 (0=False, 1=True, 2=NotConfigured).
                Enabled = MapFirewallEnabled(WmiQuery.GetValue<ushort>(obj, "Enabled", 0)),
                DefaultInboundAction = MapFirewallAction(WmiQuery.GetValue<ushort>(obj, "DefaultInboundAction", 0)),
                DefaultOutboundAction = MapFirewallAction(WmiQuery.GetValue<ushort>(obj, "DefaultOutboundAction", 0)),
            });
    }

    // ---- Local users ----

    private DiagnosticResult<IReadOnlyList<LocalUser>> GetLocalUsers()
    {
        _logger.LogDebug("Querying Win32_UserAccount with LocalAccount=True.");

        return WmiQuery.Query(
            "SELECT Name, FullName, Description, SID, Disabled, Lockout, PasswordExpires, PasswordRequired FROM Win32_UserAccount WHERE LocalAccount=True",
            obj => new LocalUser
            {
                Name = WmiQuery.GetString(obj, "Name"),
                FullName = WmiQuery.GetString(obj, "FullName"),
                Description = WmiQuery.GetString(obj, "Description"),
                Sid = WmiQuery.GetString(obj, "SID"),
                Disabled = WmiQuery.GetValue(obj, "Disabled", false),
                Lockout = WmiQuery.GetValue(obj, "Lockout", false),
                PasswordExpires = WmiQuery.GetValue(obj, "PasswordExpires", false),
                PasswordRequired = WmiQuery.GetValue(obj, "PasswordRequired", false),
            });
    }

    // ---- Local Administrators ----

    private DiagnosticResult<IReadOnlyList<AdminGroupMember>> GetLocalAdministrators()
    {
        _logger.LogDebug("Resolving local Administrators group via SID ending in -544 and enumerating Win32_GroupUser.");

        // 1) Find the Administrators group name (localized) by SID suffix.
        var groupNameResult = WmiQuery.First(
            "SELECT Name, SID FROM Win32_Group WHERE LocalAccount=True",
            obj => new
            {
                Name = WmiQuery.GetString(obj, "Name"),
                Sid = WmiQuery.GetString(obj, "SID"),
            });

        if (groupNameResult.IsFailure)
        {
            return DiagnosticResult<IReadOnlyList<AdminGroupMember>>.Failure(
                groupNameResult.ErrorMessage!, groupNameResult.Exception);
        }

        // We can't filter on SID suffix in WQL directly, so grab all then filter.
        var allGroupsResult = WmiQuery.Query(
            "SELECT Name, SID FROM Win32_Group WHERE LocalAccount=True",
            obj => new { Name = WmiQuery.GetString(obj, "Name"), Sid = WmiQuery.GetString(obj, "SID") });

        if (allGroupsResult.IsFailure)
        {
            return DiagnosticResult<IReadOnlyList<AdminGroupMember>>.Failure(
                allGroupsResult.ErrorMessage!, allGroupsResult.Exception);
        }

        var adminsGroup = allGroupsResult.Value
            .FirstOrDefault(g => g.Sid.EndsWith("-544", StringComparison.Ordinal));

        if (adminsGroup is null)
        {
            return DiagnosticResult<IReadOnlyList<AdminGroupMember>>.Failure(
                "Local Administrators group not found (no group with SID suffix -544).");
        }

        var computerName = Environment.MachineName;

        var membersResult = WmiQuery.Query(
            $"SELECT PartComponent FROM Win32_GroupUser WHERE GroupComponent=\"Win32_Group.Domain='{computerName}',Name='{adminsGroup.Name}'\"",
            obj =>
            {
                var part = WmiQuery.GetString(obj, "PartComponent");
                return ParseGroupMember(part);
            });

        return membersResult;
    }

    // ---- Pure helpers (testable without I/O) ----

    public static bool MapFirewallEnabled(ushort raw) => raw == 1;

    public static string MapFirewallAction(ushort raw) =>
        raw switch
        {
            0 => "NotConfigured",
            2 => "Allow",
            4 => "Block",
            _ => $"Code {raw}",
        };

    /// <summary>
    /// Parses a <c>Win32_GroupUser.PartComponent</c> reference string into an
    /// <see cref="AdminGroupMember"/>. Format examples:
    /// <code>
    /// \\\\HOST\\root\\cimv2:Win32_UserAccount.Domain="HOST",Name="rochy"
    /// \\\\HOST\\root\\cimv2:Win32_Group.Domain="HOST",Name="Network"
    /// </code>
    /// </summary>
    public static AdminGroupMember ParseGroupMember(string partComponent)
    {
        var accountType = partComponent.Contains("Win32_UserAccount", StringComparison.OrdinalIgnoreCase)
            ? "User"
            : partComponent.Contains("Win32_Group", StringComparison.OrdinalIgnoreCase)
                ? "Group"
                : "Other";

        var domain = ExtractQuoted(partComponent, "Domain=") ?? string.Empty;
        var name = ExtractQuoted(partComponent, "Name=") ?? string.Empty;

        return new AdminGroupMember
        {
            Name = name,
            Domain = domain,
            Sid = string.Empty,   // PartComponent doesn't carry the SID directly.
            AccountType = accountType,
        };
    }

    private static string? ExtractQuoted(string source, string key)
    {
        var keyIndex = source.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (keyIndex < 0) return null;
        var firstQuote = source.IndexOf('"', keyIndex + key.Length);
        if (firstQuote < 0) return null;
        var secondQuote = source.IndexOf('"', firstQuote + 1);
        if (secondQuote < 0) return null;
        return source.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
    }
}
