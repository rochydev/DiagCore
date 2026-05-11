using DiagCore.Core.Diagnostics;

namespace DiagCore.Tests.Diagnostics;

public class NetworkDiagnosticsHelperTests
{
    // ---- FormatMac ----

    [Fact]
    public void FormatMac_StandardSixBytes_ProducesColonHex()
    {
        byte[] mac = [0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF];
        NetworkDiagnostics.FormatMac(mac).Should().Be("AA:BB:CC:DD:EE:FF");
    }

    [Fact]
    public void FormatMac_LowerBytes_StillUpperHex()
    {
        byte[] mac = [0x01, 0x02, 0x0A, 0x0B];
        NetworkDiagnostics.FormatMac(mac).Should().Be("01:02:0A:0B");
    }

    [Fact]
    public void FormatMac_Empty_ReturnsEmpty()
    {
        NetworkDiagnostics.FormatMac(Array.Empty<byte>()).Should().BeEmpty();
    }
}
