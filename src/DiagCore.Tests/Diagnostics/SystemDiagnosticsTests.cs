using DiagCore.Core.Diagnostics;

namespace DiagCore.Tests.Diagnostics;

public class SystemDiagnosticsParserTests
{
    // ---- ParseCimDateTime ----

    [Fact]
    public void ParseCimDateTime_ValidUtcOffsetString_ReturnsDateTime()
    {
        // CIM date-time format: yyyymmddHHMMSS.mmmmmmsUUU
        // Example: 2025-11-01 12:04:15.123456 with +060 minute offset
        var parsed = SystemDiagnostics.ParseCimDateTime("20251101120415.123456+060");

        parsed.Year.Should().Be(2025);
        parsed.Month.Should().Be(11);
        parsed.Day.Should().Be(1);
        parsed.Hour.Should().Be(12);
        parsed.Minute.Should().Be(4);
        parsed.Second.Should().Be(15);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a date")]
    [InlineData("20251301120415.000000+060")]    // month 13: invalid
    public void ParseCimDateTime_InvalidOrEmpty_ReturnsMinValue(string input)
    {
        SystemDiagnostics.ParseCimDateTime(input).Should().Be(DateTime.MinValue);
    }

    // ---- ParseCimDateTimeOrNull ----

    [Fact]
    public void ParseCimDateTimeOrNull_ValidString_ReturnsDate()
    {
        var parsed = SystemDiagnostics.ParseCimDateTimeOrNull("20240715090000.000000+060");

        parsed.Should().NotBeNull();
        parsed!.Value.Year.Should().Be(2024);
        parsed.Value.Month.Should().Be(7);
        parsed.Value.Day.Should().Be(15);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("garbage")]
    public void ParseCimDateTimeOrNull_InvalidOrEmpty_ReturnsNull(string input)
    {
        SystemDiagnostics.ParseCimDateTimeOrNull(input).Should().BeNull();
    }

    // ---- LocaleHexToCulture ----

    [Theory]
    [InlineData("0c0a", "es-ES")]   // Spanish (Spain)
    [InlineData("0409", "en-US")]   // English (United States)
    [InlineData("040c", "fr-FR")]   // French (France)
    [InlineData("0407", "de-DE")]   // German (Germany)
    public void LocaleHexToCulture_KnownLcid_ReturnsIetfTag(string lcidHex, string expected)
    {
        SystemDiagnostics.LocaleHexToCulture(lcidHex).Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LocaleHexToCulture_BlankInput_ReturnsEmpty(string input)
    {
        SystemDiagnostics.LocaleHexToCulture(input).Should().BeEmpty();
    }

    [Fact]
    public void LocaleHexToCulture_InvalidHex_ReturnsInputAsFallback()
    {
        // Non-hex chars keep the input as-is rather than crashing.
        SystemDiagnostics.LocaleHexToCulture("not-hex").Should().Be("not-hex");
    }
}
