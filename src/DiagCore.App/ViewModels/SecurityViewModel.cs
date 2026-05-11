using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DiagCore.Core.Common;
using DiagCore.Core.Diagnostics;
using DiagCore.Core.Models;

namespace DiagCore.App.ViewModels;

public sealed partial class SecurityViewModel : ViewModelBase
{
    private readonly ISecurityDiagnostics _security;

    public SecurityViewModel(ISecurityDiagnostics security)
    {
        _security = security;
        FirewallProfiles = new ObservableCollection<FirewallProfile>();
        LocalUsers = new ObservableCollection<LocalUser>();
        Administrators = new ObservableCollection<AdminGroupMember>();
    }

    [ObservableProperty]
    private DefenderStatus? _defenderStatus;

    public ObservableCollection<FirewallProfile> FirewallProfiles { get; }

    public ObservableCollection<LocalUser> LocalUsers { get; }

    public ObservableCollection<AdminGroupMember> Administrators { get; }

    [ObservableProperty]
    private string? _defenderError;

    [ObservableProperty]
    private string? _firewallError;

    [ObservableProperty]
    private string? _userError;

    [ObservableProperty]
    private string? _adminError;

    public string DefenderRtValueDisplay =>
        DefenderStatus is null ? "—"
            : DefenderStatus.RealTimeProtectionEnabled ? "Activa" : "Desactivada";

    public string DefenderRtCaption
    {
        get
        {
            if (DefenderStatus is null) return string.Empty;
            return DefenderStatus.AntivirusEnabled
                ? $"Motor {DefenderStatus.AntivirusEngineVersion}"
                : "Antivirus deshabilitado";
        }
    }

    public int FirewallProfilesEnabledCount => FirewallProfiles.Count(p => p.Enabled);

    public string FirewallValueDisplay => $"{FirewallProfilesEnabledCount} / {FirewallProfiles.Count}";

    public string FirewallCaption
    {
        get
        {
            if (FirewallProfiles.Count == 0) return "sin información";
            var profiles = FirewallProfiles.Where(p => p.Enabled).Select(p => p.Name);
            return profiles.Any() ? "activos: " + string.Join(", ", profiles) : "todos los perfiles desactivados";
        }
    }

    public string AdminsValueDisplay => Administrators.Count.ToString();

    public string AdminsCaption =>
        Administrators.Count == 0 ? "sin información"
            : $"{LocalUsers.Count} usuarios locales totales";

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        var defenderTask = _security.GetDefenderStatusAsync(cancellationToken);
        var firewallTask = _security.GetFirewallProfilesAsync(cancellationToken);
        var usersTask = _security.GetLocalUsersAsync(cancellationToken);
        var adminsTask = _security.GetLocalAdministratorsAsync(cancellationToken);

        await Task.WhenAll(defenderTask, firewallTask, usersTask, adminsTask).ConfigureAwait(true);

        ApplyDefender(defenderTask.Result);
        ApplyList(firewallTask.Result, FirewallProfiles, v => FirewallError = v);
        ApplyList(usersTask.Result, LocalUsers, v => UserError = v);
        ApplyList(adminsTask.Result, Administrators, v => AdminError = v);

        OnPropertyChanged(nameof(DefenderRtValueDisplay));
        OnPropertyChanged(nameof(DefenderRtCaption));
        OnPropertyChanged(nameof(FirewallProfilesEnabledCount));
        OnPropertyChanged(nameof(FirewallValueDisplay));
        OnPropertyChanged(nameof(FirewallCaption));
        OnPropertyChanged(nameof(AdminsValueDisplay));
        OnPropertyChanged(nameof(AdminsCaption));
    }

    private void ApplyDefender(DiagnosticResult<DefenderStatus> result)
    {
        if (result.IsSuccess)
        {
            DefenderStatus = result.Value;
            DefenderError = null;
        }
        else
        {
            DefenderStatus = null;
            DefenderError = result.ErrorMessage;
        }
    }

    private static void ApplyList<T>(
        DiagnosticResult<IReadOnlyList<T>> result,
        ObservableCollection<T> target,
        Action<string?> setError)
    {
        if (result.IsSuccess)
        {
            setError(null);
            target.Clear();
            foreach (var item in result.Value)
            {
                target.Add(item);
            }
        }
        else
        {
            setError(result.ErrorMessage);
            target.Clear();
        }
    }
}
