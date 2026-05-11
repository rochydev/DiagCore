using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DiagCore.Core.Common;
using DiagCore.Core.Diagnostics;
using DiagCore.Core.Models;

namespace DiagCore.App.ViewModels;

public sealed partial class StorageViewModel : ViewModelBase
{
    private readonly IStorageDiagnostics _storage;

    public StorageViewModel(IStorageDiagnostics storage)
    {
        _storage = storage;
        Volumes = new ObservableCollection<VolumeInfo>();
        PhysicalDisks = new ObservableCollection<PhysicalDiskInfo>();
        Partitions = new ObservableCollection<PartitionInfo>();
    }

    public ObservableCollection<VolumeInfo> Volumes { get; }

    public ObservableCollection<PhysicalDiskInfo> PhysicalDisks { get; }

    public ObservableCollection<PartitionInfo> Partitions { get; }

    [ObservableProperty]
    private string? _volumeError;

    [ObservableProperty]
    private string? _physicalDiskError;

    [ObservableProperty]
    private string? _partitionError;

    public VolumeInfo? SystemDrive =>
        Volumes.FirstOrDefault(v => v.DeviceId.Equals("C:", StringComparison.OrdinalIgnoreCase))
        ?? Volumes.FirstOrDefault();

    public double SystemDriveUsedPercent => SystemDrive?.UsedPercent ?? 0d;

    public string SystemDriveValueDisplay =>
        SystemDrive is null ? "—" : $"{SystemDrive.UsedGB:F0} / {SystemDrive.TotalGB:F0} GB";

    public string SystemDriveCaption =>
        SystemDrive is null ? string.Empty : $"Unidad {SystemDrive.DeviceId} · {SystemDrive.FileSystem}";

    public int VolumesCount => Volumes.Count;

    public string VolumesValueDisplay => Volumes.Count.ToString();

    public string VolumesCaption =>
        Volumes.Count == 0 ? "ninguna unidad" : $"{Volumes.Count} {(Volumes.Count == 1 ? "unidad" : "unidades")}";

    public int PhysicalDisksCount => PhysicalDisks.Count;

    public string PhysicalDisksValueDisplay => PhysicalDisks.Count.ToString();

    public string PhysicalDisksCaption
    {
        get
        {
            if (PhysicalDisks.Count == 0) return "sin discos";
            var ssd = PhysicalDisks.Count(d => d.MediaType == PhysicalDiskMediaType.Ssd);
            var hdd = PhysicalDisks.Count(d => d.MediaType == PhysicalDiskMediaType.Hdd);
            var parts = new List<string>();
            if (ssd > 0) parts.Add($"{ssd} SSD");
            if (hdd > 0) parts.Add($"{hdd} HDD");
            return parts.Count > 0 ? string.Join(" · ", parts) : $"{PhysicalDisks.Count} disco(s)";
        }
    }

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        var volumesTask = _storage.GetVolumesAsync(cancellationToken);
        var disksTask = _storage.GetPhysicalDisksAsync(cancellationToken);
        var partitionsTask = _storage.GetPartitionsAsync(cancellationToken);

        await Task.WhenAll(volumesTask, disksTask, partitionsTask).ConfigureAwait(true);

        ApplyList(volumesTask.Result, Volumes, v => VolumeError = v);
        ApplyList(disksTask.Result, PhysicalDisks, v => PhysicalDiskError = v);
        ApplyList(partitionsTask.Result, Partitions, v => PartitionError = v);

        OnPropertyChanged(nameof(SystemDrive));
        OnPropertyChanged(nameof(SystemDriveUsedPercent));
        OnPropertyChanged(nameof(SystemDriveValueDisplay));
        OnPropertyChanged(nameof(SystemDriveCaption));
        OnPropertyChanged(nameof(VolumesCount));
        OnPropertyChanged(nameof(VolumesValueDisplay));
        OnPropertyChanged(nameof(VolumesCaption));
        OnPropertyChanged(nameof(PhysicalDisksCount));
        OnPropertyChanged(nameof(PhysicalDisksValueDisplay));
        OnPropertyChanged(nameof(PhysicalDisksCaption));
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
