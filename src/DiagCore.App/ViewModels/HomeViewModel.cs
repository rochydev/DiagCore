using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DiagCore.Core.Diagnostics;
using DiagCore.Core.Models;

namespace DiagCore.App.ViewModels;

/// <summary>
/// Home / dashboard view model. Aggregates a snapshot from every diagnostic
/// service and computes a single "health score" so the user can land on the
/// app and tell at a glance if anything needs attention.
/// </summary>
public sealed partial class HomeViewModel : ViewModelBase
{
    private readonly ISystemDiagnostics _system;
    private readonly IHardwareDiagnostics _hardware;
    private readonly IStorageDiagnostics _storage;
    private readonly ISecurityDiagnostics _security;
    private readonly IProcessDiagnostics _process;
    private readonly IEventLogDiagnostics _eventLog;

    public HomeViewModel(
        ISystemDiagnostics system,
        IHardwareDiagnostics hardware,
        IStorageDiagnostics storage,
        ISecurityDiagnostics security,
        IProcessDiagnostics process,
        IEventLogDiagnostics eventLog)
    {
        _system = system;
        _hardware = hardware;
        _storage = storage;
        _security = security;
        _process = process;
        _eventLog = eventLog;

        TopProcessesByCpu = new ObservableCollection<ProcessSnapshot>();
        TopProcessesByRam = new ObservableCollection<ProcessSnapshot>();
        CriticalEvents = new ObservableCollection<EventLogEntry>();
    }

    [ObservableProperty]
    private OperatingSystemInfo? _operatingSystem;

    [ObservableProperty]
    private ComputerSystemInfo? _computerSystem;

    [ObservableProperty]
    private MemoryInfo? _memorySummary;

    [ObservableProperty]
    private VolumeInfo? _systemDrive;

    [ObservableProperty]
    private DefenderStatus? _defenderStatus;

    [ObservableProperty]
    private int _criticalEventsCount;

    [ObservableProperty]
    private double _cpuLoadPercent;

    [ObservableProperty]
    private int _healthScore = 100;

    [ObservableProperty]
    private string _healthVerdict = "Sin datos";

    public ObservableCollection<ProcessSnapshot> TopProcessesByCpu { get; }

    public ObservableCollection<ProcessSnapshot> TopProcessesByRam { get; }

    public ObservableCollection<EventLogEntry> CriticalEvents { get; }

    // ---- Derived display properties ----

    public string CpuValueDisplay => $"{CpuLoadPercent:F0} %";

    public string MemoryValueDisplay =>
        MemorySummary is null ? "—" : $"{MemorySummary.UsedPercent:F0} %";

    public string MemoryCaption =>
        MemorySummary is null ? string.Empty
            : $"{MemorySummary.UsedGB:F1} / {MemorySummary.TotalGB:F0} GB";

    public double MemoryUsedPercent => MemorySummary?.UsedPercent ?? 0d;

    public string DiskValueDisplay =>
        SystemDrive is null ? "—" : $"{SystemDrive.UsedPercent:F0} %";

    public string DiskCaption =>
        SystemDrive is null ? string.Empty
            : $"Unidad {SystemDrive.DeviceId} · {SystemDrive.FreeGB:F0} GB libres";

    public double DiskUsedPercent => SystemDrive?.UsedPercent ?? 0d;

    public string UptimeDisplay
    {
        get
        {
            var uptime = OperatingSystem?.Uptime ?? TimeSpan.Zero;
            return uptime == TimeSpan.Zero ? "—" : $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }
    }

    public string MachineSubtitle
    {
        get
        {
            if (ComputerSystem is null && OperatingSystem is null) return string.Empty;
            var manufacturer = ComputerSystem?.Manufacturer ?? string.Empty;
            var model = ComputerSystem?.Model ?? string.Empty;
            var os = OperatingSystem?.Caption ?? string.Empty;
            return string.Join(" · ", new[] { manufacturer, model, os }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        var systemTask = _system.GetOperatingSystemAsync(cancellationToken);
        var computerTask = _system.GetComputerSystemAsync(cancellationToken);
        var cpuTask = _hardware.GetProcessorsAsync(cancellationToken);
        var memTask = _hardware.GetMemoryAsync(cancellationToken);
        var volumesTask = _storage.GetVolumesAsync(cancellationToken);
        var defenderTask = _security.GetDefenderStatusAsync(cancellationToken);
        var topCpuTask = _process.GetTopProcessesByCpuAsync(5, cancellationToken);
        var topRamTask = _process.GetTopProcessesByRamAsync(5, cancellationToken);
        var eventsTask = _eventLog.GetCriticalEventsAsync(TimeSpan.FromHours(24), 20, cancellationToken);

        await Task.WhenAll(
            systemTask, computerTask, cpuTask, memTask, volumesTask,
            defenderTask, topCpuTask, topRamTask, eventsTask)
            .ConfigureAwait(true);

        OperatingSystem = systemTask.Result.IsSuccess ? systemTask.Result.Value : null;
        ComputerSystem = computerTask.Result.IsSuccess ? computerTask.Result.Value : null;
        MemorySummary = memTask.Result.IsSuccess ? memTask.Result.Value : null;
        DefenderStatus = defenderTask.Result.IsSuccess ? defenderTask.Result.Value : null;

        CpuLoadPercent = cpuTask.Result.IsSuccess && cpuTask.Result.Value.Count > 0
            ? cpuTask.Result.Value[0].LoadPercentage
            : 0;

        SystemDrive = volumesTask.Result.IsSuccess
            ? (volumesTask.Result.Value.FirstOrDefault(v => v.DeviceId.Equals("C:", StringComparison.OrdinalIgnoreCase))
                ?? volumesTask.Result.Value.FirstOrDefault())
            : null;

        TopProcessesByCpu.Clear();
        if (topCpuTask.Result.IsSuccess)
        {
            foreach (var p in topCpuTask.Result.Value) TopProcessesByCpu.Add(p);
        }

        TopProcessesByRam.Clear();
        if (topRamTask.Result.IsSuccess)
        {
            foreach (var p in topRamTask.Result.Value) TopProcessesByRam.Add(p);
        }

        CriticalEvents.Clear();
        if (eventsTask.Result.IsSuccess)
        {
            foreach (var ev in eventsTask.Result.Value) CriticalEvents.Add(ev);
        }
        CriticalEventsCount = CriticalEvents.Count;

        ComputeHealthScore();
        RaiseDerivedNotifications();
    }

    private void ComputeHealthScore()
    {
        var score = 100;

        if (MemorySummary is not null)
        {
            if (MemorySummary.UsedPercent > 90) score -= 15;
            else if (MemorySummary.UsedPercent > 80) score -= 8;
        }

        if (SystemDrive is not null)
        {
            if (SystemDrive.UsedPercent > 95) score -= 25;
            else if (SystemDrive.UsedPercent > 90) score -= 12;
            else if (SystemDrive.UsedPercent > 80) score -= 5;
        }

        if (CpuLoadPercent > 90) score -= 10;

        if (DefenderStatus is not null && (!DefenderStatus.AntivirusEnabled || !DefenderStatus.RealTimeProtectionEnabled))
        {
            score -= 20;
        }

        if (CriticalEventsCount > 20) score -= 12;
        else if (CriticalEventsCount > 5) score -= 5;

        HealthScore = Math.Clamp(score, 0, 100);
        HealthVerdict = HealthScore switch
        {
            >= 85 => "Equipo en buen estado",
            >= 60 => "Atención: hay cosas por revisar",
            _ => "Estado crítico: revisar urgente",
        };
    }

    private void RaiseDerivedNotifications()
    {
        OnPropertyChanged(nameof(CpuValueDisplay));
        OnPropertyChanged(nameof(MemoryValueDisplay));
        OnPropertyChanged(nameof(MemoryCaption));
        OnPropertyChanged(nameof(MemoryUsedPercent));
        OnPropertyChanged(nameof(DiskValueDisplay));
        OnPropertyChanged(nameof(DiskCaption));
        OnPropertyChanged(nameof(DiskUsedPercent));
        OnPropertyChanged(nameof(UptimeDisplay));
        OnPropertyChanged(nameof(MachineSubtitle));
    }
}
