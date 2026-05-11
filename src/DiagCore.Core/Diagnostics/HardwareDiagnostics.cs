using System.Runtime.Versioning;
using DiagCore.Core.Common;
using DiagCore.Core.Models;
using Microsoft.Extensions.Logging;

namespace DiagCore.Core.Diagnostics;

/// <inheritdoc cref="IHardwareDiagnostics"/>
[SupportedOSPlatform("windows")]
public sealed class HardwareDiagnostics : IHardwareDiagnostics
{
    private const double BytesPerGB = 1024d * 1024d * 1024d;
    private const double KBPerGB = 1024d * 1024d;

    private readonly ILogger<HardwareDiagnostics> _logger;

    public HardwareDiagnostics(ILogger<HardwareDiagnostics> logger)
    {
        _logger = logger;
    }

    public Task<DiagnosticResult<IReadOnlyList<CpuInfo>>> GetProcessorsAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetProcessors, cancellationToken);

    public Task<DiagnosticResult<MemoryInfo>> GetMemoryAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetMemory, cancellationToken);

    public Task<DiagnosticResult<IReadOnlyList<GpuInfo>>> GetGraphicsAdaptersAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetGraphicsAdapters, cancellationToken);

    public Task<DiagnosticResult<IReadOnlyList<BatteryInfo>>> GetBatteriesAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetBatteries, cancellationToken);

    public Task<DiagnosticResult<IReadOnlyList<ThermalReading>>> GetThermalsAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetThermals, cancellationToken);

    // ---- CPU ----

    private DiagnosticResult<IReadOnlyList<CpuInfo>> GetProcessors()
    {
        _logger.LogDebug("Querying Win32_Processor.");

        return WmiQuery.Query(
            "SELECT Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, CurrentClockSpeed, SocketDesignation, LoadPercentage FROM Win32_Processor",
            obj => new CpuInfo
            {
                Name = WmiQuery.GetString(obj, "Name").Trim(),
                Manufacturer = WmiQuery.GetString(obj, "Manufacturer").Trim(),
                PhysicalCores = WmiQuery.GetValue(obj, "NumberOfCores", 0u),
                LogicalCores = WmiQuery.GetValue(obj, "NumberOfLogicalProcessors", 0u),
                MaxClockMHz = WmiQuery.GetValue(obj, "MaxClockSpeed", 0u),
                CurrentClockMHz = WmiQuery.GetValue(obj, "CurrentClockSpeed", 0u),
                Socket = WmiQuery.GetString(obj, "SocketDesignation"),
                LoadPercentage = WmiQuery.GetValue<ushort>(obj, "LoadPercentage", 0),
            });
    }

    // ---- Memory ----

    private DiagnosticResult<MemoryInfo> GetMemory()
    {
        _logger.LogDebug("Querying Win32_OperatingSystem (totals) and Win32_PhysicalMemory (modules).");

        var totalsResult = WmiQuery.First(
            "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem",
            obj => new
            {
                TotalKB = WmiQuery.GetValue<ulong>(obj, "TotalVisibleMemorySize", 0UL),
                FreeKB = WmiQuery.GetValue<ulong>(obj, "FreePhysicalMemory", 0UL),
            });

        if (totalsResult.IsFailure)
        {
            return DiagnosticResult<MemoryInfo>.Failure(totalsResult.ErrorMessage!, totalsResult.Exception);
        }

        var modulesResult = WmiQuery.Query(
            "SELECT Capacity, Speed, Manufacturer, DeviceLocator, SerialNumber, PartNumber FROM Win32_PhysicalMemory",
            obj => new MemoryModule
            {
                CapacityGB = Math.Round(WmiQuery.GetValue<ulong>(obj, "Capacity", 0UL) / BytesPerGB, 2),
                SpeedMHz = WmiQuery.GetValue<uint>(obj, "Speed", 0u),
                Manufacturer = WmiQuery.GetString(obj, "Manufacturer").Trim(),
                DeviceLocator = WmiQuery.GetString(obj, "DeviceLocator"),
                SerialNumber = WmiQuery.GetString(obj, "SerialNumber").Trim(),
                PartNumber = WmiQuery.GetString(obj, "PartNumber").Trim(),
            });

        var totalGB = Math.Round(totalsResult.Value.TotalKB / KBPerGB, 2);
        var freeGB = Math.Round(totalsResult.Value.FreeKB / KBPerGB, 2);
        var usedGB = Math.Round(totalGB - freeGB, 2);
        var usedPercent = totalGB > 0 ? Math.Round(usedGB / totalGB * 100d, 1) : 0d;

        return DiagnosticResult<MemoryInfo>.Success(new MemoryInfo
        {
            TotalGB = totalGB,
            UsedGB = usedGB,
            FreeGB = freeGB,
            UsedPercent = usedPercent,
            Modules = modulesResult.IsSuccess ? modulesResult.Value : [],
        });
    }

    // ---- GPU ----

    private DiagnosticResult<IReadOnlyList<GpuInfo>> GetGraphicsAdapters()
    {
        _logger.LogDebug("Querying Win32_VideoController.");

        return WmiQuery.Query(
            "SELECT Name, VideoProcessor, DriverVersion, DriverDate, AdapterRAM, CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate FROM Win32_VideoController",
            obj =>
            {
                var ramBytes = WmiQuery.GetValue<uint>(obj, "AdapterRAM", 0u);
                return new GpuInfo
                {
                    Name = WmiQuery.GetString(obj, "Name"),
                    VideoProcessor = WmiQuery.GetString(obj, "VideoProcessor"),
                    DriverVersion = WmiQuery.GetString(obj, "DriverVersion"),
                    DriverDate = SystemDiagnostics.ParseCimDateTimeOrNull(WmiQuery.GetString(obj, "DriverDate")),
                    AdapterRamGB = ramBytes > 0 ? Math.Round(ramBytes / BytesPerGB, 2) : null,
                    CurrentHorizontalResolution = WmiQuery.GetValue(obj, "CurrentHorizontalResolution", 0),
                    CurrentVerticalResolution = WmiQuery.GetValue(obj, "CurrentVerticalResolution", 0),
                    CurrentRefreshRateHz = WmiQuery.GetValue(obj, "CurrentRefreshRate", 0),
                };
            });
    }

    // ---- Battery ----

    private DiagnosticResult<IReadOnlyList<BatteryInfo>> GetBatteries()
    {
        _logger.LogDebug("Querying Win32_Battery.");

        return WmiQuery.Query(
            "SELECT Name, EstimatedChargeRemaining, BatteryStatus, EstimatedRunTime, DesignVoltage FROM Win32_Battery",
            obj =>
            {
                var rawRunTime = WmiQuery.GetValue<uint>(obj, "EstimatedRunTime", 0u);
                // Per WMI docs, EstimatedRunTime == 0x4FFFFFFF (1342177279) means "on AC, unknown".
                int? runMinutes = rawRunTime is 0u or 0x4FFFFFFF ? null : (int)rawRunTime;
                var voltage = WmiQuery.GetValue<uint>(obj, "DesignVoltage", 0u);

                return new BatteryInfo
                {
                    Name = WmiQuery.GetString(obj, "Name"),
                    EstimatedChargeRemaining = WmiQuery.GetValue<ushort>(obj, "EstimatedChargeRemaining", 0),
                    Status = MapBatteryStatus(WmiQuery.GetValue<ushort>(obj, "BatteryStatus", 0)),
                    EstimatedRunTimeMinutes = runMinutes,
                    DesignVoltageMV = voltage > 0 ? voltage : null,
                };
            });
    }

    // ---- Thermals ----

    private DiagnosticResult<IReadOnlyList<ThermalReading>> GetThermals()
    {
        _logger.LogDebug("Querying MSAcpi_ThermalZoneTemperature in root\\wmi.");

        return WmiQuery.Query(
            WmiQuery.WmiCimV2,
            "SELECT InstanceName, CurrentTemperature FROM MSAcpi_ThermalZoneTemperature",
            obj => new ThermalReading
            {
                Zone = WmiQuery.GetString(obj, "InstanceName"),
                Celsius = TenthsKelvinToCelsius(WmiQuery.GetValue<uint>(obj, "CurrentTemperature", 0u)),
            });
    }

    // ---- Pure helpers (testable without I/O) ----

    /// <summary>
    /// Converts a raw <c>MSAcpi_ThermalZoneTemperature.CurrentTemperature</c>
    /// value (tenths of a kelvin) to Celsius. Rounded to one decimal place.
    /// </summary>
    public static double TenthsKelvinToCelsius(uint tenthsKelvin) =>
        Math.Round(tenthsKelvin / 10d - 273.15, 1);

    /// <summary>
    /// Maps a <c>Win32_Battery.BatteryStatus</c> ushort code to the strongly
    /// typed <see cref="BatteryStatus"/> enum.
    /// </summary>
    public static BatteryStatus MapBatteryStatus(ushort raw) =>
        raw switch
        {
            1 => BatteryStatus.Discharging,
            2 => BatteryStatus.OnAcPower,
            3 => BatteryStatus.FullyCharged,
            4 => BatteryStatus.Low,
            5 => BatteryStatus.Critical,
            6 => BatteryStatus.Charging,
            7 => BatteryStatus.ChargingAndHigh,
            8 => BatteryStatus.ChargingAndLow,
            9 => BatteryStatus.ChargingAndCritical,
            10 => BatteryStatus.PartialPower,
            11 => BatteryStatus.OnBackupPower,
            _ => BatteryStatus.Unknown,
        };
}
