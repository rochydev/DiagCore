namespace DiagCore.Core.Models;

public enum FirmwareBootMode
{
    Unknown,
    Legacy,
    Uefi,
}

/// <summary>
/// BIOS/UEFI firmware facts. Sourced from <c>Win32_BIOS</c> and the registry
/// value <c>HKLM\System\CurrentControlSet\Control\PEFirmwareType</c> for the
/// boot mode (1 = Legacy, 2 = UEFI).
/// </summary>
public sealed record BiosInfo
{
    public required string Manufacturer { get; init; }

    public required string Version { get; init; }

    public required string SmbiosVersion { get; init; }

    public required string SerialNumber { get; init; }

    public required DateTime? ReleaseDate { get; init; }

    public required FirmwareBootMode BootMode { get; init; }

    public required bool SecureBootEnabled { get; init; }
}
