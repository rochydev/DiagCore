using DiagCore.Core.Diagnostics;
using DiagCore.Core.Models;

namespace DiagCore.Tests.Diagnostics;

public class EventLogDiagnosticsHelperTests
{
    [Theory]
    [InlineData((byte)0, EventLogLevel.LogAlways)]
    [InlineData((byte)1, EventLogLevel.Critical)]
    [InlineData((byte)2, EventLogLevel.Error)]
    [InlineData((byte)3, EventLogLevel.Warning)]
    [InlineData((byte)4, EventLogLevel.Informational)]
    [InlineData((byte)5, EventLogLevel.Verbose)]
    [InlineData((byte)99, EventLogLevel.LogAlways)]
    public void MapLevel_CoversDocumentedLevels(byte raw, EventLogLevel expected)
    {
        EventLogDiagnostics.MapLevel(raw).Should().Be(expected);
    }
}
