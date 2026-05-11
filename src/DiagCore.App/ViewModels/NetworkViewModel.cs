using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagCore.Core.Common;
using DiagCore.Core.Diagnostics;
using DiagCore.Core.Models;

namespace DiagCore.App.ViewModels;

public sealed partial class NetworkViewModel : ViewModelBase
{
    private readonly INetworkDiagnostics _network;

    public NetworkViewModel(INetworkDiagnostics network)
    {
        _network = network;
        Adapters = new ObservableCollection<NetworkAdapterInfo>();
    }

    public ObservableCollection<NetworkAdapterInfo> Adapters { get; }

    [ObservableProperty]
    private string? _adapterError;

    // ---- Ping tool ----

    [ObservableProperty]
    private string _pingHost = "8.8.8.8";

    [ObservableProperty]
    private PingResult? _pingResult;

    [ObservableProperty]
    private string? _pingError;

    [ObservableProperty]
    private bool _isPinging;

    // ---- Public IP ----

    [ObservableProperty]
    private string? _publicIp;

    [ObservableProperty]
    private string? _publicIpError;

    [ObservableProperty]
    private bool _isLookingUpPublicIp;

    public int ConnectedAdaptersCount =>
        Adapters.Count(a => a.Status.Equals("Up", StringComparison.OrdinalIgnoreCase));

    public string ConnectedAdaptersValueDisplay => ConnectedAdaptersCount.ToString();

    public string ConnectedAdaptersCaption =>
        $"{Adapters.Count} adaptador(es) totales";

    public NetworkAdapterInfo? PrimaryAdapter =>
        Adapters.FirstOrDefault(a => a.Status.Equals("Up", StringComparison.OrdinalIgnoreCase)
            && a.IPv4Addresses.Count > 0);

    public string PrimaryAdapterValueDisplay =>
        PrimaryAdapter?.IPv4Addresses.FirstOrDefault()?.Split('/')[0] ?? "—";

    public string PrimaryAdapterCaption => PrimaryAdapter?.Name ?? "sin conexión activa";

    public string PublicIpValueDisplay => PublicIp ?? "—";

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        var result = await _network.GetAdaptersAsync(cancellationToken).ConfigureAwait(true);
        if (result.IsSuccess)
        {
            AdapterError = null;
            Adapters.Clear();
            foreach (var adapter in result.Value)
            {
                Adapters.Add(adapter);
            }
        }
        else
        {
            AdapterError = result.ErrorMessage;
            Adapters.Clear();
        }

        OnPropertyChanged(nameof(ConnectedAdaptersCount));
        OnPropertyChanged(nameof(ConnectedAdaptersValueDisplay));
        OnPropertyChanged(nameof(ConnectedAdaptersCaption));
        OnPropertyChanged(nameof(PrimaryAdapter));
        OnPropertyChanged(nameof(PrimaryAdapterValueDisplay));
        OnPropertyChanged(nameof(PrimaryAdapterCaption));
    }

    [RelayCommand]
    private async Task PingAsync(CancellationToken cancellationToken)
    {
        if (IsPinging || string.IsNullOrWhiteSpace(PingHost)) return;
        IsPinging = true;
        PingError = null;
        PingResult = null;
        try
        {
            var result = await _network.PingAsync(PingHost, count: 4, timeoutMs: 2000, cancellationToken).ConfigureAwait(true);
            if (result.IsSuccess)
            {
                PingResult = result.Value;
            }
            else
            {
                PingError = result.ErrorMessage;
            }
        }
        finally
        {
            IsPinging = false;
        }
    }

    [RelayCommand]
    private async Task LookupPublicIpAsync(CancellationToken cancellationToken)
    {
        if (IsLookingUpPublicIp) return;
        IsLookingUpPublicIp = true;
        PublicIpError = null;
        try
        {
            var result = await _network.GetPublicIpAsync(cancellationToken).ConfigureAwait(true);
            if (result.IsSuccess)
            {
                PublicIp = result.Value;
            }
            else
            {
                PublicIpError = result.ErrorMessage;
                PublicIp = null;
            }
            OnPropertyChanged(nameof(PublicIpValueDisplay));
        }
        finally
        {
            IsLookingUpPublicIp = false;
        }
    }
}
