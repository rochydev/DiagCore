using DiagCore.Core.Diagnostics;

namespace DiagCore.Tests.Diagnostics;

public class ProcessDiagnosticsHelperTests
{
    [Theory]
    [InlineData("11/15/2024")]
    [InlineData("2024-11-15")]
    [InlineData("2024-11-15T09:30:00Z")]
    public void ParseHotfixInstalledOn_RecognisesCommonDateFormats(string raw)
    {
        var parsed = ProcessDiagnostics.ParseHotfixInstalledOn(raw);

        parsed.Should().NotBeNull();
        parsed!.Value.Year.Should().Be(2024);
        parsed.Value.Month.Should().Be(11);
        parsed.Value.Day.Should().Be(15);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("totally-not-a-date")]
    public void ParseHotfixInstalledOn_BlankOrGarbage_ReturnsNull(string raw)
    {
        ProcessDiagnostics.ParseHotfixInstalledOn(raw).Should().BeNull();
    }
}
