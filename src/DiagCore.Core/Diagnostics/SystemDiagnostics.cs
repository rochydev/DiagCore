using System.Globalization;
using System.Management;
using System.Runtime.Versioning;
using DiagCore.Core.Common;
using DiagCore.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace DiagCore.Core.Diagnostics;

/// <inheritdoc cref="ISystemDiagnostics"/>
[SupportedOSPlatform("windows")]
public sealed class SystemDiagnostics : ISystemDiagnostics
{
    private readonly ILogger<SystemDiagnostics> _logger;

    public SystemDiagnostics(ILogger<SystemDiagnostics> logger)
    {
        _logger = logger;
    }

    public Task<DiagnosticResult<OperatingSystemInfo>> GetOperatingSystemAsync(CancellationToken cancellationToken = default) =>
        Task.Run(() => GetOperatingSystem(), cancellationToken);

    public Task<DiagnosticResult<ComputerSystemInfo>> GetComputerSystemAsync(CancellationToken cancellationToken = default) =>
        Task.Run(() => GetComputerSystem(), cancellationToken);

    public Task<DiagnosticResult<BiosInfo>> GetBiosAsync(CancellationToken cancellationToken = default) =>
        Task.Run(() => GetBios(), cancellationToken);

    public Task<DiagnosticResult<MotherboardInfo>> GetMotherboardAsync(CancellationToken cancellationToken = default) =>
        Task.Run(() => GetMotherboard(), cancellationToken);

    public async Task<SystemSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var osTask = GetOperatingSystemAsync(cancellationToken);
        var csTask = GetComputerSystemAsync(cancellationToken);
        var biosTask = GetBiosAsync(cancellationToken);
        var moboTask = GetMotherboardAsync(cancellationToken);

        await Task.WhenAll(osTask, csTask, biosTask, moboTask).ConfigureAwait(false);

        return new SystemSummary
        {
            OperatingSystem = osTask.Result,
            ComputerSystem = csTask.Result,
            Bios = biosTask.Result,
            Motherboard = moboTask.Result,
        };
    }

    private DiagnosticResult<OperatingSystemInfo> GetOperatingSystem()
    {
        _logger.LogDebug("Querying Win32_OperatingSystem and Win32_ComputerSystem.");

        var osResult = WmiQuery.First(
            "SELECT Caption, Version, BuildNumber, OSArchitecture, Locale, RegisteredUser, InstallDate, LastBootUpTime, CSName FROM Win32_OperatingSystem",
            obj => new
            {
                Caption = WmiQuery.GetString(obj, "Caption").Trim(),
                Version = WmiQuery.GetString(obj, "Version"),
                BuildNumber = WmiQuery.GetString(obj, "BuildNumber"),
                Architecture = WmiQuery.GetString(obj, "OSArchitecture"),
                Locale = LocaleHexToCulture(WmiQuery.GetString(obj, "Locale")),
                RegisteredUser = WmiQuery.GetString(obj, "RegisteredUser"),
                ComputerName = WmiQuery.GetString(obj, "CSName"),
                InstallDate = ParseCimDateTime(WmiQuery.GetString(obj, "InstallDate")),
                LastBootUpTime = ParseCimDateTime(WmiQuery.GetString(obj, "LastBootUpTime")),
            });

        if (osResult.IsFailure)
        {
            return DiagnosticResult<OperatingSystemInfo>.Failure(osResult.ErrorMessage!, osResult.Exception);
        }

        var csResult = WmiQuery.First(
            "SELECT Domain, PartOfDomain FROM Win32_ComputerSystem",
            obj => new
            {
                Domain = WmiQuery.GetString(obj, "Domain"),
                PartOfDomain = WmiQuery.GetValue(obj, "PartOfDomain", false),
            });

        var info = new OperatingSystemInfo
        {
            Caption = osResult.Value.Caption,
            Version = osResult.Value.Version,
            BuildNumber = osResult.Value.BuildNumber,
            Architecture = osResult.Value.Architecture,
            Locale = osResult.Value.Locale,
            RegisteredUser = osResult.Value.RegisteredUser,
            ComputerName = osResult.Value.ComputerName,
            InstallDate = osResult.Value.InstallDate,
            LastBootUpTime = osResult.Value.LastBootUpTime,
            Domain = csResult.IsSuccess ? csResult.Value.Domain : string.Empty,
            PartOfDomain = csResult.IsSuccess && csResult.Value.PartOfDomain,
        };

        return DiagnosticResult<OperatingSystemInfo>.Success(info);
    }

    private DiagnosticResult<ComputerSystemInfo> GetComputerSystem()
    {
        _logger.LogDebug("Querying Win32_ComputerSystem and Win32_ComputerSystemProduct.");

        var csResult = WmiQuery.First(
            "SELECT Manufacturer, Model, SystemFamily, SystemType FROM Win32_ComputerSystem",
            obj => new
            {
                Manufacturer = WmiQuery.GetString(obj, "Manufacturer"),
                Model = WmiQuery.GetString(obj, "Model"),
                SystemFamily = WmiQuery.GetString(obj, "SystemFamily"),
                SystemType = WmiQuery.GetString(obj, "SystemType"),
            });

        if (csResult.IsFailure)
        {
            return DiagnosticResult<ComputerSystemInfo>.Failure(csResult.ErrorMessage!, csResult.Exception);
        }

        var prodResult = WmiQuery.First(
            "SELECT IdentifyingNumber, UUID FROM Win32_ComputerSystemProduct",
            obj => new
            {
                Serial = WmiQuery.GetString(obj, "IdentifyingNumber"),
                Uuid = WmiQuery.GetString(obj, "UUID"),
            });

        var info = new ComputerSystemInfo
        {
            Manufacturer = csResult.Value.Manufacturer,
            Model = csResult.Value.Model,
            SystemFamily = csResult.Value.SystemFamily,
            SystemType = csResult.Value.SystemType,
            SerialNumber = prodResult.IsSuccess ? prodResult.Value.Serial : string.Empty,
            Uuid = prodResult.IsSuccess ? prodResult.Value.Uuid : string.Empty,
        };

        return DiagnosticResult<ComputerSystemInfo>.Success(info);
    }

    private DiagnosticResult<BiosInfo> GetBios()
    {
        _logger.LogDebug("Querying Win32_BIOS and reading PEFirmwareType / SecureBoot from registry.");

        var biosResult = WmiQuery.First(
            "SELECT Manufacturer, SMBIOSBIOSVersion, Version, SerialNumber, ReleaseDate FROM Win32_BIOS",
            obj => new BiosInfo
            {
                Manufacturer = WmiQuery.GetString(obj, "Manufacturer"),
                Version = WmiQuery.GetString(obj, "Version"),
                SmbiosVersion = WmiQuery.GetString(obj, "SMBIOSBIOSVersion"),
                SerialNumber = WmiQuery.GetString(obj, "SerialNumber"),
                ReleaseDate = ParseCimDateTimeOrNull(WmiQuery.GetString(obj, "ReleaseDate")),
                BootMode = ReadBootMode(),
                SecureBootEnabled = ReadSecureBootEnabled(),
            });

        return biosResult;
    }

    private DiagnosticResult<MotherboardInfo> GetMotherboard()
    {
        _logger.LogDebug("Querying Win32_BaseBoard.");

        return WmiQuery.First(
            "SELECT Manufacturer, Product, Version, SerialNumber FROM Win32_BaseBoard",
            obj => new MotherboardInfo
            {
                Manufacturer = WmiQuery.GetString(obj, "Manufacturer"),
                Product = WmiQuery.GetString(obj, "Product"),
                Version = WmiQuery.GetString(obj, "Version"),
                SerialNumber = WmiQuery.GetString(obj, "SerialNumber"),
            });
    }

    // ---- Static helpers (testable; no I/O required) ----

    /// <summary>
    /// Parses a CIM/WMI datetime string such as <c>20251101120415.123456+060</c>.
    /// Falls back to <see cref="DateTime.MinValue"/> on malformed input.
    /// </summary>
    public static DateTime ParseCimDateTime(string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DateTime.MinValue;
            }
            return ManagementDateTimeConverter.ToDateTime(value);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>Same as <see cref="ParseCimDateTime"/> but returns null on failure / blank.</summary>
    public static DateTime? ParseCimDateTimeOrNull(string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return ManagementDateTimeConverter.ToDateTime(value);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a Windows locale id (hexadecimal LCID string, e.g. <c>0c0a</c>)
    /// to the matching IETF tag (<c>es-ES</c>). Returns the raw value when
    /// resolution fails.
    /// </summary>
    public static string LocaleHexToCulture(string lcidHex)
    {
        if (string.IsNullOrWhiteSpace(lcidHex))
        {
            return string.Empty;
        }
        try
        {
            var lcid = int.Parse(lcidHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return new CultureInfo(lcid).Name;
        }
        catch
        {
            return lcidHex;
        }
    }

    private FirmwareBootMode ReadBootMode()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control");
            var value = key?.GetValue("PEFirmwareType");
            return value is int i
                ? i switch
                {
                    1 => FirmwareBootMode.Legacy,
                    2 => FirmwareBootMode.Uefi,
                    _ => FirmwareBootMode.Unknown,
                }
                : FirmwareBootMode.Unknown;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed reading PEFirmwareType from registry.");
            return FirmwareBootMode.Unknown;
        }
    }

    private bool ReadSecureBootEnabled()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
            var value = key?.GetValue("UEFISecureBootEnabled");
            return value is int i && i == 1;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed reading SecureBoot state from registry (likely missing rights).");
            return false;
        }
    }
}
