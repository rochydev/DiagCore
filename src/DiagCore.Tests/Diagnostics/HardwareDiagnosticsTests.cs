using DiagCore.Core.Diagnostics;
using DiagCore.Core.Models;

namespace DiagCore.Tests.Diagnostics;

public class HardwareDiagnosticsHelperTests
{
    // ---- TenthsKelvinToCelsius ----
    // Tolerance covers banker's-rounding ambiguity on the .5 boundary.

    [Theory]
    [InlineData(2731u, 0)]          // 273.1 K - around the freezing point
    [InlineData(2981u, 25)]         // ~25 C - typical idle CPU at low load
    [InlineData(3531u, 80)]         // ~80 C - hot
    [InlineData(0u, -273.15)]       // sensor reported 0 - absolute zero
    public void TenthsKelvinToCelsius_ConvertsCorrectly(uint raw, double expected)
    {
        HardwareDiagnostics.TenthsKelvinToCelsius(raw).Should().BeApproximately(expected, 0.1);
    }

    // ---- MapBatteryStatus ----

    [Theory]
    [InlineData((ushort)1, BatteryStatus.Discharging)]
    [InlineData((ushort)2, BatteryStatus.OnAcPower)]
    [InlineData((ushort)3, BatteryStatus.FullyCharged)]
    [InlineData((ushort)4, BatteryStatus.Low)]
    [InlineData((ushort)5, BatteryStatus.Critical)]
    [InlineData((ushort)6, BatteryStatus.Charging)]
    [InlineData((ushort)7, BatteryStatus.ChargingAndHigh)]
    [InlineData((ushort)8, BatteryStatus.ChargingAndLow)]
    [InlineData((ushort)9, BatteryStatus.ChargingAndCritical)]
    [InlineData((ushort)10, BatteryStatus.PartialPower)]
    [InlineData((ushort)11, BatteryStatus.OnBackupPower)]
    [InlineData((ushort)0, BatteryStatus.Unknown)]
    [InlineData((ushort)999, BatteryStatus.Unknown)]
    public void MapBatteryStatus_MapsAllDocumentedCodes(ushort raw, BatteryStatus expected)
    {
        HardwareDiagnostics.MapBatteryStatus(raw).Should().Be(expected);
    }
}
