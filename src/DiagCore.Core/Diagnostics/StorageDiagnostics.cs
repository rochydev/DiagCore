using System.Runtime.Versioning;
using DiagCore.Core.Common;
using DiagCore.Core.Models;
using Microsoft.Extensions.Logging;

namespace DiagCore.Core.Diagnostics;

/// <inheritdoc cref="IStorageDiagnostics"/>
[SupportedOSPlatform("windows")]
public sealed class StorageDiagnostics : IStorageDiagnostics
{
    private const double BytesPerGB = 1024d * 1024d * 1024d;

    private readonly ILogger<StorageDiagnostics> _logger;

    public StorageDiagnostics(ILogger<StorageDiagnostics> logger)
    {
        _logger = logger;
    }

    public Task<DiagnosticResult<IReadOnlyList<VolumeInfo>>> GetVolumesAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetVolumes, cancellationToken);

    public Task<DiagnosticResult<IReadOnlyList<PhysicalDiskInfo>>> GetPhysicalDisksAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetPhysicalDisks, cancellationToken);

    public Task<DiagnosticResult<IReadOnlyList<PartitionInfo>>> GetPartitionsAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetPartitions, cancellationToken);

    // ---- Volumes ----

    private DiagnosticResult<IReadOnlyList<VolumeInfo>> GetVolumes()
    {
        _logger.LogDebug("Querying Win32_LogicalDisk with DriveType=3.");

        return WmiQuery.Query(
            "SELECT DeviceID, VolumeName, FileSystem, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3",
            obj =>
            {
                var totalBytes = WmiQuery.GetValue<ulong>(obj, "Size", 0UL);
                var freeBytes = WmiQuery.GetValue<ulong>(obj, "FreeSpace", 0UL);
                var totalGB = Math.Round(totalBytes / BytesPerGB, 2);
                var freeGB = Math.Round(freeBytes / BytesPerGB, 2);
                var usedGB = Math.Round(totalGB - freeGB, 2);
                var usedPercent = totalGB > 0 ? Math.Round(usedGB / totalGB * 100d, 1) : 0d;

                return new VolumeInfo
                {
                    DeviceId = WmiQuery.GetString(obj, "DeviceID"),
                    Label = WmiQuery.GetString(obj, "VolumeName"),
                    FileSystem = WmiQuery.GetString(obj, "FileSystem"),
                    TotalGB = totalGB,
                    FreeGB = freeGB,
                    UsedGB = usedGB,
                    UsedPercent = usedPercent,
                };
            });
    }

    // ---- Physical disks ----

    private DiagnosticResult<IReadOnlyList<PhysicalDiskInfo>> GetPhysicalDisks()
    {
        _logger.LogDebug("Querying MSFT_PhysicalDisk in {Scope}.", WmiQuery.MicrosoftWindowsStorage);

        return WmiQuery.Query(
            WmiQuery.MicrosoftWindowsStorage,
            "SELECT DeviceId, FriendlyName, Model, SerialNumber, MediaType, BusType, Size, HealthStatus, OperationalStatus FROM MSFT_PhysicalDisk",
            obj =>
            {
                var sizeBytes = WmiQuery.GetValue<ulong>(obj, "Size", 0UL);
                var operationalStatus = (obj["OperationalStatus"] as ushort[])
                    ?? Array.Empty<ushort>();

                int.TryParse(WmiQuery.GetString(obj, "DeviceId"), out var deviceId);

                return new PhysicalDiskInfo
                {
                    DeviceId = deviceId,
                    FriendlyName = WmiQuery.GetString(obj, "FriendlyName"),
                    Model = WmiQuery.GetString(obj, "Model"),
                    SerialNumber = WmiQuery.GetString(obj, "SerialNumber").Trim(),
                    MediaType = MapMediaType(WmiQuery.GetValue<ushort>(obj, "MediaType", 0)),
                    BusType = MapBusType(WmiQuery.GetValue<ushort>(obj, "BusType", 0)),
                    SizeGB = Math.Round(sizeBytes / BytesPerGB, 2),
                    HealthStatus = MapHealthStatus(WmiQuery.GetValue<ushort>(obj, "HealthStatus", 5)),
                    OperationalStatus = FormatOperationalStatus(operationalStatus),
                };
            });
    }

    // ---- Partitions ----

    private DiagnosticResult<IReadOnlyList<PartitionInfo>> GetPartitions()
    {
        _logger.LogDebug("Querying MSFT_Partition in {Scope}.", WmiQuery.MicrosoftWindowsStorage);

        return WmiQuery.Query(
            WmiQuery.MicrosoftWindowsStorage,
            "SELECT DiskNumber, PartitionNumber, DriveLetter, Size, Type, IsBoot, IsSystem FROM MSFT_Partition",
            obj =>
            {
                var sizeBytes = WmiQuery.GetValue<ulong>(obj, "Size", 0UL);
                var letterChar = obj["DriveLetter"];
                // DriveLetter comes as a single char wrapped in object, or 0 char when unassigned.
                string letter = letterChar switch
                {
                    char c when c != '\0' => $"{c}:",
                    string s when !string.IsNullOrWhiteSpace(s) && s[0] != '\0' => $"{s[0]}:",
                    _ => string.Empty,
                };

                return new PartitionInfo
                {
                    DiskNumber = (int)WmiQuery.GetValue<uint>(obj, "DiskNumber", 0u),
                    PartitionNumber = (int)WmiQuery.GetValue<uint>(obj, "PartitionNumber", 0u),
                    DriveLetter = letter,
                    SizeGB = Math.Round(sizeBytes / BytesPerGB, 2),
                    PartitionType = WmiQuery.GetString(obj, "Type"),
                    IsBoot = WmiQuery.GetValue(obj, "IsBoot", false),
                    IsSystem = WmiQuery.GetValue(obj, "IsSystem", false),
                };
            });
    }

    // ---- Pure helpers (testable without I/O) ----

    public static PhysicalDiskMediaType MapMediaType(ushort raw) =>
        raw switch
        {
            3 => PhysicalDiskMediaType.Hdd,
            4 => PhysicalDiskMediaType.Ssd,
            5 => PhysicalDiskMediaType.Scm,
            _ => PhysicalDiskMediaType.Unknown,
        };

    public static PhysicalDiskBusType MapBusType(ushort raw) =>
        raw switch
        {
            1 => PhysicalDiskBusType.Scsi,
            2 => PhysicalDiskBusType.Atapi,
            3 => PhysicalDiskBusType.Ata,
            4 => PhysicalDiskBusType.Ieee1394,
            5 => PhysicalDiskBusType.Ssa,
            6 => PhysicalDiskBusType.FibreChannel,
            7 => PhysicalDiskBusType.Usb,
            8 => PhysicalDiskBusType.Raid,
            9 => PhysicalDiskBusType.Iscsi,
            10 => PhysicalDiskBusType.Sas,
            11 => PhysicalDiskBusType.Sata,
            12 => PhysicalDiskBusType.Sd,
            13 => PhysicalDiskBusType.Mmc,
            14 => PhysicalDiskBusType.Virtual,
            15 => PhysicalDiskBusType.FileBackedVirtual,
            17 => PhysicalDiskBusType.Nvme,
            _ => PhysicalDiskBusType.Unknown,
        };

    public static DiskHealthStatus MapHealthStatus(ushort raw) =>
        raw switch
        {
            0 => DiskHealthStatus.Healthy,
            1 => DiskHealthStatus.Warning,
            2 => DiskHealthStatus.Unhealthy,
            _ => DiskHealthStatus.Unknown,
        };

    /// <summary>
    /// MSFT_PhysicalDisk exposes OperationalStatus as a uint16 array. The most
    /// common values are 1 (Other), 2 (OK), 3 (Degraded), 6 (Error), 11 (In Service),
    /// 0xD010 (Lost Communication). We render a flat comma-separated label.
    /// </summary>
    public static string FormatOperationalStatus(ushort[] codes)
    {
        if (codes.Length == 0) return string.Empty;
        return string.Join(", ", codes.Select(MapOperationalStatusCode));
    }

    private static string MapOperationalStatusCode(ushort code) =>
        code switch
        {
            1 => "Other",
            2 => "OK",
            3 => "Degraded",
            4 => "Stressed",
            5 => "Predictive Failure",
            6 => "Error",
            7 => "Non-Recoverable Error",
            8 => "Starting",
            9 => "Stopping",
            10 => "Stopped",
            11 => "In Service",
            12 => "No Contact",
            13 => "Lost Communication",
            14 => "Aborted",
            15 => "Dormant",
            16 => "Supporting Entity in Error",
            17 => "Completed",
            18 => "Power Mode",
            0xD010 => "Lost Communication",
            _ => $"Code {code}",
        };
}
