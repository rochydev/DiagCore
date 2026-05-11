using DiagCore.Core.Common;
using DiagCore.Core.Models;

namespace DiagCore.Core.Diagnostics;

/// <summary>
/// Security posture diagnostics: Defender / Microsoft Defender status, firewall
/// profile state, local users and members of the local Administrators group.
/// Mirrors the Security menu from the legacy PowerShell script
/// (<c>Get-DefenderStatus</c>, <c>Get-FirewallStatus</c>,
/// <c>Get-UsuariosLocales</c>, <c>Get-AdminsGrupo</c>).
/// </summary>
public interface ISecurityDiagnostics
{
    Task<DiagnosticResult<DefenderStatus>> GetDefenderStatusAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<IReadOnlyList<FirewallProfile>>> GetFirewallProfilesAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<IReadOnlyList<LocalUser>>> GetLocalUsersAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<IReadOnlyList<AdminGroupMember>>> GetLocalAdministratorsAsync(CancellationToken cancellationToken = default);
}
