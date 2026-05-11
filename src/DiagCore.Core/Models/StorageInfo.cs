namespace DiagCore.Core.Models;

/// <summary><c>MSFT_PhysicalDisk.MediaType</c> values.</summary>
public enum PhysicalDiskMediaType
{
    Unknown = 0,
    Hdd = 3,
    Ssd = 4,
    Scm = 5,    // Storage-class memory (NVDIMM, Optane)
}

/// <summary><c>MSFT_PhysicalDisk.BusType</c> values. Subset of the common ones.</summary>
public enum PhysicalDiskBusType
{
    Unknown = 0,
    Scsi = 1,
    Atapi = 2,
    Ata = 3,
    Ieee1394 = 4,
    Ssa = 5,
    FibreChannel = 6,
    Usb = 7,
    Raid = 8,
    Iscsi = 9,
    Sas = 10,
    Sata = 11,
    Sd = 12,
    Mmc = 13,
    Virtual = 14,
    FileBackedVirtual = 15,
    Nvme = 17,
}

/// <summary>Aggregate <c>MSFT_PhysicalDisk.HealthStatus</c> mapping.</summary>
public enum DiskHealthStatus
{
    Unknown = 5,
    Healthy = 0,
    Warning = 1,
    Unhealthy = 2,
}

/// <summary>
/// Logical volume / drive letter snapshot. Sourced from <c>Win32_LogicalDisk</c>
/// filtered to <c>DriveType=3</c> (local disk).
/// </summary>
public sealed record VolumeInfo
{
    /// <summary>Drive letter with colon, e.g. <c>C:</c>.</summary>
    public required string DeviceId { get; init; }

    /// <summary>Volume label, e.g. <c>Windows</c> or empty string.</summary>
    public required string Label { get; init; }

    public required string FileSystem { get; init; }

    public required double TotalGB { get; init; }

    public required double FreeGB { get; init; }

    public required double UsedGB { get; init; }

    public required double UsedPercent { get; init; }
}

/// <summary>
/// Physical disk snapshot. Sourced from <c>MSFT_PhysicalDisk</c> in
/// <c>root\Microsoft\Windows\Storage</c>.
/// </summary>
public sealed record PhysicalDiskInfo
{
    public required int DeviceId { get; init; }

    public required string FriendlyName { get; init; }

    public required string Model { get; init; }

    public required string SerialNumber { get; init; }

    public required PhysicalDiskMediaType MediaType { get; init; }

    public required PhysicalDiskBusType BusType { get; init; }

    public required double SizeGB { get; init; }

    public required DiskHealthStatus HealthStatus { get; init; }

    public required string OperationalStatus { get; init; }
}

/// <summary>
/// Partition snapshot. Sourced from <c>MSFT_Partition</c> in
/// <c>root\Microsoft\Windows\Storage</c>.
/// </summary>
public sealed record PartitionInfo
{
    public required int DiskNumber { get; init; }

    public required int PartitionNumber { get; init; }

    /// <summary>Empty if the partition has no assigned letter (boot / recovery).</summary>
    public required string DriveLetter { get; init; }

    public required double SizeGB { get; init; }

    /// <summary>Friendly type name, e.g. <c>System</c>, <c>Reserved</c>, <c>Basic</c>, <c>Recovery</c>.</summary>
    public required string PartitionType { get; init; }

    public required bool IsBoot { get; init; }

    public required bool IsSystem { get; init; }
}
