using DiagCore.Core.Common;
using DiagCore.Core.Models;

namespace DiagCore.Core.Diagnostics;

/// <summary>
/// Network diagnostics: adapters with their IP configuration, plus a handful of
/// active tests (ping, tracert, TCP-port probe, DNS resolution and an opt-in
/// public-IP lookup). Mirrors the Network menu from the legacy PowerShell
/// script (<c>Get-Adaptadores</c>, <c>Test-Ping</c>, <c>Test-Tracert</c>,
/// <c>Test-Puerto</c>, <c>Test-NSLookup</c>, <c>Get-IPPublica</c>).
/// </summary>
public interface INetworkDiagnostics
{
    Task<DiagnosticResult<IReadOnlyList<NetworkAdapterInfo>>> GetAdaptersAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticResult<PingResult>> PingAsync(string host, int count = 4, int timeoutMs = 2000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Traces the route to <paramref name="host"/> by sending ICMP echo requests
    /// with increasing TTL. Stops at <paramref name="maxHops"/> or when the
    /// destination is reached.
    /// </summary>
    Task<DiagnosticResult<IReadOnlyList<TracertHop>>> TraceRouteAsync(string host, int maxHops = 30, int timeoutMs = 2000, CancellationToken cancellationToken = default);

    Task<DiagnosticResult<TcpPortTestResult>> TestTcpPortAsync(string host, int port, int timeoutMs = 3000, CancellationToken cancellationToken = default);

    Task<DiagnosticResult<DnsResolutionResult>> ResolveDnsAsync(string host, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks the public IP up via <c>api.ipify.org</c>. This is the only call
    /// in the whole product that leaves the local machine, and it is opt-in:
    /// the UI must trigger it explicitly (the master plan requires zero
    /// telemetry, so this stays behind a user gesture).
    /// </summary>
    Task<DiagnosticResult<string>> GetPublicIpAsync(CancellationToken cancellationToken = default);
}
