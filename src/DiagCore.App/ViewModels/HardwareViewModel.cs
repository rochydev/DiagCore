using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DiagCore.Core.Common;
using DiagCore.Core.Diagnostics;
using DiagCore.Core.Models;

namespace DiagCore.App.ViewModels;

/// <summary>
/// Hardware section view model. Loads the five hardware sub-sections plus the
/// machine identity from <see cref="ISystemDiagnostics"/> in parallel, surfaces
/// each one's success / failure state independently and exposes them to the
/// view via observable properties.
/// </summary>
public sealed partial class HardwareViewModel : ViewModelBase
{
    private readonly IHardwareDiagnostics _hardware;
    private readonly ISystemDiagnostics _system;

    public HardwareViewModel(IHardwareDiagnostics hardware, ISystemDiagnostics system)
    {
        _hardware = hardware;
        _system = system;
        Processors = new ObservableCollection<CpuInfo>();
        MemoryModules = new ObservableCollection<MemoryModule>();
        Gpus = new ObservableCollection<GpuInfo>();
        Batteries = new ObservableCollection<BatteryInfo>();
        Thermals = new ObservableCollection<ThermalReading>();
    }

    // ---- Identity ----

    [ObservableProperty]
    private OperatingSystemInfo? _operatingSystem;

    [ObservableProperty]
    private ComputerSystemInfo? _computerSystem;

    [ObservableProperty]
    private BiosInfo? _bios;

    [ObservableProperty]
    private MotherboardInfo? _motherboard;

    // ---- Hardware ----

    public ObservableCollection<CpuInfo> Processors { get; }

    public ObservableCollection<MemoryModule> MemoryModules { get; }

    public ObservableCollection<GpuInfo> Gpus { get; }

    public ObservableCollection<BatteryInfo> Batteries { get; }

    public ObservableCollection<ThermalReading> Thermals { get; }

    [ObservableProperty]
    private MemoryInfo? _memorySummary;

    // ---- Per-section error states (so a single WMI failure does not blank the whole tab)  ----

    [ObservableProperty]
    private string? _systemError;

    [ObservableProperty]
    private string? _cpuError;

    [ObservableProperty]
    private string? _memoryError;

    [ObservableProperty]
    private string? _gpuError;

    [ObservableProperty]
    private string? _batteryError;

    [ObservableProperty]
    private string? _thermalError;

    [ObservableProperty]
    private string? _biosError;

    [ObservableProperty]
    private string? _motherboardError;

    public bool HasNoBattery =>
        !IsLoading && Batteries.Count == 0 && string.IsNullOrEmpty(BatteryError);

    public bool HasNoThermal =>
        !IsLoading && Thermals.Count == 0 && string.IsNullOrEmpty(ThermalError);

    // ---- Derived properties for the hero tiles ----

    public double CpuLoadPercent => Processors.FirstOrDefault()?.LoadPercentage ?? 0d;

    public string CpuLoadDisplay => $"{CpuLoadPercent:F0} %";

    public string CpuName => Processors.FirstOrDefault()?.Name ?? string.Empty;

    public string CpuSummary
    {
        get
        {
            var cpu = Processors.FirstOrDefault();
            if (cpu is null) return string.Empty;
            return $"{cpu.PhysicalCores} núcleos · {cpu.LogicalCores} hilos · {cpu.CurrentClockMHz} MHz";
        }
    }

    public double MemoryUsedPercent => MemorySummary?.UsedPercent ?? 0d;

    public string MemoryValueDisplay =>
        MemorySummary is null ? string.Empty : $"{MemorySummary.UsedGB:F1} / {MemorySummary.TotalGB:F0} GB";

    public string MemorySummaryCaption =>
        MemorySummary is null ? string.Empty : $"{MemoryModules.Count} módulos · {MemorySummary.FreeGB:F1} GB libres";

    public string UptimeDisplay
    {
        get
        {
            var uptime = OperatingSystem?.Uptime ?? TimeSpan.Zero;
            return uptime == TimeSpan.Zero ? "—" : $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }
    }

    public string UptimeSubCaption =>
        OperatingSystem is null ? string.Empty : $"desde {OperatingSystem.LastBootUpTime:dd/MM HH:mm}";

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        // Fire every section concurrently. Each one returns its own
        // DiagnosticResult so a single WMI failure does not cancel the others.
        var summaryTask = _system.GetSummaryAsync(cancellationToken);
        var cpuTask = _hardware.GetProcessorsAsync(cancellationToken);
        var memTask = _hardware.GetMemoryAsync(cancellationToken);
        var gpuTask = _hardware.GetGraphicsAdaptersAsync(cancellationToken);
        var batteryTask = _hardware.GetBatteriesAsync(cancellationToken);
        var thermalTask = _hardware.GetThermalsAsync(cancellationToken);

        await Task.WhenAll(summaryTask, cpuTask, memTask, gpuTask, batteryTask, thermalTask)
            .ConfigureAwait(true);

        ApplySummary(summaryTask.Result);
        ApplyList(cpuTask.Result, Processors, value => CpuError = value);
        ApplyMemory(memTask.Result);
        ApplyList(gpuTask.Result, Gpus, value => GpuError = value);
        ApplyList(batteryTask.Result, Batteries, value => BatteryError = value);
        ApplyList(thermalTask.Result, Thermals, value => ThermalError = value);

        OnPropertyChanged(nameof(HasNoBattery));
        OnPropertyChanged(nameof(HasNoThermal));
        OnPropertyChanged(nameof(CpuLoadPercent));
        OnPropertyChanged(nameof(CpuLoadDisplay));
        OnPropertyChanged(nameof(CpuName));
        OnPropertyChanged(nameof(CpuSummary));
        OnPropertyChanged(nameof(MemoryUsedPercent));
        OnPropertyChanged(nameof(MemoryValueDisplay));
        OnPropertyChanged(nameof(MemorySummaryCaption));
        OnPropertyChanged(nameof(UptimeDisplay));
        OnPropertyChanged(nameof(UptimeSubCaption));
    }

    private void ApplySummary(SystemSummary summary)
    {
        if (summary.OperatingSystem.IsSuccess)
        {
            OperatingSystem = summary.OperatingSystem.Value;
            SystemError = null;
        }
        else
        {
            OperatingSystem = null;
            SystemError = summary.OperatingSystem.ErrorMessage;
        }

        if (summary.ComputerSystem.IsSuccess)
        {
            ComputerSystem = summary.ComputerSystem.Value;
        }
        else
        {
            ComputerSystem = null;
            SystemError ??= summary.ComputerSystem.ErrorMessage;
        }

        if (summary.Bios.IsSuccess)
        {
            Bios = summary.Bios.Value;
            BiosError = null;
        }
        else
        {
            Bios = null;
            BiosError = summary.Bios.ErrorMessage;
        }

        if (summary.Motherboard.IsSuccess)
        {
            Motherboard = summary.Motherboard.Value;
            MotherboardError = null;
        }
        else
        {
            Motherboard = null;
            MotherboardError = summary.Motherboard.ErrorMessage;
        }
    }

    private void ApplyMemory(DiagnosticResult<MemoryInfo> result)
    {
        if (result.IsSuccess)
        {
            MemorySummary = result.Value;
            MemoryError = null;
            MemoryModules.Clear();
            foreach (var module in result.Value.Modules)
            {
                MemoryModules.Add(module);
            }
        }
        else
        {
            MemorySummary = null;
            MemoryError = result.ErrorMessage;
            MemoryModules.Clear();
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
