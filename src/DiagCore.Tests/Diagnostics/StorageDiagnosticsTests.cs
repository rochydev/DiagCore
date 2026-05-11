using DiagCore.Core.Diagnostics;
using DiagCore.Core.Models;

namespace DiagCore.Tests.Diagnostics;

public class StorageDiagnosticsHelperTests
{
    // ---- MapMediaType ----

    [Theory]
    [InlineData((ushort)3, PhysicalDiskMediaType.Hdd)]
    [InlineData((ushort)4, PhysicalDiskMediaType.Ssd)]
    [InlineData((ushort)5, PhysicalDiskMediaType.Scm)]
    [InlineData((ushort)0, PhysicalDiskMediaType.Unknown)]
    [InlineData((ushort)42, PhysicalDiskMediaType.Unknown)]
    public void MapMediaType_KnownCodes(ushort raw, PhysicalDiskMediaType expected)
    {
        StorageDiagnostics.MapMediaType(raw).Should().Be(expected);
    }

    // ---- MapBusType ----

    [Theory]
    [InlineData((ushort)7, PhysicalDiskBusType.Usb)]
    [InlineData((ushort)11, PhysicalDiskBusType.Sata)]
    [InlineData((ushort)17, PhysicalDiskBusType.Nvme)]
    [InlineData((ushort)10, PhysicalDiskBusType.Sas)]
    [InlineData((ushort)0, PhysicalDiskBusType.Unknown)]
    [InlineData((ushort)99, PhysicalDiskBusType.Unknown)]
    public void MapBusType_KnownCodes(ushort raw, PhysicalDiskBusType expected)
    {
        StorageDiagnostics.MapBusType(raw).Should().Be(expected);
    }

    // ---- MapHealthStatus ----

    [Theory]
    [InlineData((ushort)0, DiskHealthStatus.Healthy)]
    [InlineData((ushort)1, DiskHealthStatus.Warning)]
    [InlineData((ushort)2, DiskHealthStatus.Unhealthy)]
    [InlineData((ushort)5, DiskHealthStatus.Unknown)]
    [InlineData((ushort)99, DiskHealthStatus.Unknown)]
    public void MapHealthStatus_KnownCodes(ushort raw, DiskHealthStatus expected)
    {
        StorageDiagnostics.MapHealthStatus(raw).Should().Be(expected);
    }

    // ---- FormatOperationalStatus ----

    [Fact]
    public void FormatOperationalStatus_Empty_ReturnsEmpty()
    {
        StorageDiagnostics.FormatOperationalStatus(Array.Empty<ushort>()).Should().BeEmpty();
    }

    [Fact]
    public void FormatOperationalStatus_SingleOk_ReturnsLabel()
    {
        StorageDiagnostics.FormatOperationalStatus([2]).Should().Be("OK");
    }

    [Fact]
    public void FormatOperationalStatus_MultipleCodes_Joined()
    {
        StorageDiagnostics.FormatOperationalStatus([2, 3]).Should().Be("OK, Degraded");
    }

    [Fact]
    public void FormatOperationalStatus_UnknownCode_FallsBackToCodeLabel()
    {
        StorageDiagnostics.FormatOperationalStatus([777]).Should().Be("Code 777");
    }
}
