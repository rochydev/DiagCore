using DiagCore.Core.Diagnostics;

namespace DiagCore.Tests.Diagnostics;

public class SecurityDiagnosticsHelperTests
{
    // ---- MapFirewallEnabled ----

    [Theory]
    [InlineData((ushort)0, false)]
    [InlineData((ushort)1, true)]
    [InlineData((ushort)2, false)]   // NotConfigured -> treat as disabled
    [InlineData((ushort)99, false)]
    public void MapFirewallEnabled_OnlyOneMeansEnabled(ushort raw, bool expected)
    {
        SecurityDiagnostics.MapFirewallEnabled(raw).Should().Be(expected);
    }

    // ---- MapFirewallAction ----

    [Theory]
    [InlineData((ushort)0, "NotConfigured")]
    [InlineData((ushort)2, "Allow")]
    [InlineData((ushort)4, "Block")]
    public void MapFirewallAction_DocumentedCodes(ushort raw, string expected)
    {
        SecurityDiagnostics.MapFirewallAction(raw).Should().Be(expected);
    }

    [Fact]
    public void MapFirewallAction_UnknownCode_ReturnsCodeLabel()
    {
        SecurityDiagnostics.MapFirewallAction(42).Should().Be("Code 42");
    }

    // ---- ParseGroupMember ----

    [Fact]
    public void ParseGroupMember_UserAccountReference_IsClassifiedAsUser()
    {
        const string raw = @"\\HOST\root\cimv2:Win32_UserAccount.Domain=""HOST"",Name=""rochy""";

        var member = SecurityDiagnostics.ParseGroupMember(raw);

        member.AccountType.Should().Be("User");
        member.Domain.Should().Be("HOST");
        member.Name.Should().Be("rochy");
    }

    [Fact]
    public void ParseGroupMember_GroupReference_IsClassifiedAsGroup()
    {
        const string raw = @"\\HOST\root\cimv2:Win32_Group.Domain=""HOST"",Name=""Network""";

        var member = SecurityDiagnostics.ParseGroupMember(raw);

        member.AccountType.Should().Be("Group");
        member.Domain.Should().Be("HOST");
        member.Name.Should().Be("Network");
    }

    [Fact]
    public void ParseGroupMember_UnknownClass_IsOther()
    {
        const string raw = @"\\HOST\root\cimv2:Win32_NTLogEvent.Domain=""HOST"",Name=""x""";

        SecurityDiagnostics.ParseGroupMember(raw).AccountType.Should().Be("Other");
    }
}
