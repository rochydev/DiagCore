using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using DiagCore.Core.Common;
using DiagCore.Core.Models;
using Microsoft.Extensions.Logging;
using SystemPingReply = System.Net.NetworkInformation.PingReply;
using PingReply = DiagCore.Core.Models.PingReply;

namespace DiagCore.Core.Diagnostics;

/// <inheritdoc cref="INetworkDiagnostics"/>
[SupportedOSPlatform("windows")]
public sealed class NetworkDiagnostics : INetworkDiagnostics
{
    private const string PublicIpEndpoint = "https://api.ipify.org";
    private const int PublicIpTimeoutSeconds = 5;

    private static readonly HttpClient PublicIpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(PublicIpTimeoutSeconds),
    };

    private readonly ILogger<NetworkDiagnostics> _logger;

    public NetworkDiagnostics(ILogger<NetworkDiagnostics> logger)
    {
        _logger = logger;
    }

    // ---- Adapters ----

    public Task<DiagnosticResult<IReadOnlyList<NetworkAdapterInfo>>> GetAdaptersAsync(CancellationToken cancellationToken = default) =>
        Task.Run(GetAdapters, cancellationToken);

    private DiagnosticResult<IReadOnlyList<NetworkAdapterInfo>> GetAdapters()
    {
        try
        {
            _logger.LogDebug("Enumerating NetworkInterface.GetAllNetworkInterfaces.");

            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus != OperationalStatus.NotPresent)
                .Select(MapAdapter)
                .ToList();

            return DiagnosticResult<IReadOnlyList<NetworkAdapterInfo>>.Success(adapters);
        }
        catch (Exception ex)
        {
            return DiagnosticResult<IReadOnlyList<NetworkAdapterInfo>>.FromException(ex, "Failed enumerating network adapters");
        }
    }

    private static NetworkAdapterInfo MapAdapter(NetworkInterface n)
    {
        var props = n.GetIPProperties();
        var ipv4 = props.UnicastAddresses
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(a => $"{a.Address}/{PrefixFromMask(a)}")
            .ToList();
        var ipv6 = props.UnicastAddresses
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetworkV6)
            .Select(a => a.Address.ToString())
            .ToList();
        var gateways = props.GatewayAddresses
            .Select(g => g.Address.ToString())
            .Distinct()
            .ToList();
        var dns = props.DnsAddresses
            .Select(a => a.ToString())
            .Distinct()
            .ToList();

        return new NetworkAdapterInfo
        {
            Name = n.Name,
            Description = n.Description,
            MacAddress = FormatMac(n.GetPhysicalAddress().GetAddressBytes()),
            Status = n.OperationalStatus.ToString(),
            InterfaceType = n.NetworkInterfaceType.ToString(),
            SpeedBps = n.Speed,
            IPv4Addresses = ipv4,
            IPv6Addresses = ipv6,
            Gateways = gateways,
            DnsServers = dns,
        };
    }

    // ---- Ping ----

    public async Task<DiagnosticResult<PingResult>> PingAsync(string host, int count = 4, int timeoutMs = 2000, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return DiagnosticResult<PingResult>.Failure("Host is required.");
        }

        try
        {
            _logger.LogDebug("Ping {Host} count={Count} timeout={Timeout}ms.", host, count, timeoutMs);

            using var pinger = new Ping();
            var replies = new List<PingReply>(count);

            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                SystemPingReply raw = await pinger.SendPingAsync(host, timeoutMs).ConfigureAwait(false);
                replies.Add(new PingReply
                {
                    Sequence = i + 1,
                    Status = raw.Status.ToString(),
                    RoundtripMs = raw.Status == IPStatus.Success ? raw.RoundtripTime : 0,
                    Address = raw.Address?.ToString(),
                });
            }

            var received = replies.Count(r => r.Status == nameof(IPStatus.Success));
            var lost = count - received;
            var lossPercent = count > 0 ? Math.Round((double)lost / count * 100d, 1) : 0d;
            var successful = replies.Where(r => r.Status == nameof(IPStatus.Success)).ToList();
            var min = successful.Count > 0 ? successful.Min(r => r.RoundtripMs) : 0L;
            var max = successful.Count > 0 ? successful.Max(r => r.RoundtripMs) : 0L;
            var avg = successful.Count > 0 ? (long)successful.Average(r => r.RoundtripMs) : 0L;

            return DiagnosticResult<PingResult>.Success(new PingResult
            {
                Host = host,
                Replies = replies,
                Sent = count,
                Received = received,
                Lost = lost,
                LossPercent = lossPercent,
                MinMs = min,
                MaxMs = max,
                AvgMs = avg,
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return DiagnosticResult<PingResult>.FromException(ex, $"Ping to '{host}' failed");
        }
    }

    // ---- Tracert ----

    public async Task<DiagnosticResult<IReadOnlyList<TracertHop>>> TraceRouteAsync(string host, int maxHops = 30, int timeoutMs = 2000, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return DiagnosticResult<IReadOnlyList<TracertHop>>.Failure("Host is required.");
        }

        try
        {
            _logger.LogDebug("Traceroute {Host} maxHops={MaxHops}.", host, maxHops);

            using var pinger = new Ping();
            var hops = new List<TracertHop>();
            var payload = Encoding.ASCII.GetBytes("DiagCore-trace");

            for (var ttl = 1; ttl <= maxHops; ttl++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var options = new PingOptions(ttl, dontFragment: true);
                var sw = Stopwatch.StartNew();
                var reply = await pinger.SendPingAsync(host, timeoutMs, payload, options).ConfigureAwait(false);
                sw.Stop();

                hops.Add(new TracertHop
                {
                    HopNumber = ttl,
                    Address = reply.Address?.ToString(),
                    RoundtripMs = reply.Status is IPStatus.Success or IPStatus.TtlExpired
                        ? Math.Max(reply.RoundtripTime, sw.ElapsedMilliseconds)
                        : 0L,
                    Status = reply.Status.ToString(),
                });

                if (reply.Status == IPStatus.Success)
                {
                    break;
                }
            }

            return DiagnosticResult<IReadOnlyList<TracertHop>>.Success(hops);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return DiagnosticResult<IReadOnlyList<TracertHop>>.FromException(ex, $"Traceroute to '{host}' failed");
        }
    }

    // ---- TCP port test ----

    public async Task<DiagnosticResult<TcpPortTestResult>> TestTcpPortAsync(string host, int port, int timeoutMs = 3000, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return DiagnosticResult<TcpPortTestResult>.Failure("Host is required.");
        }
        if (port is < 1 or > 65535)
        {
            return DiagnosticResult<TcpPortTestResult>.Failure("Port must be in the range 1-65535.");
        }

        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new TcpClient();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeoutMs);

            await client.ConnectAsync(host, port, timeoutCts.Token).ConfigureAwait(false);
            sw.Stop();

            return DiagnosticResult<TcpPortTestResult>.Success(new TcpPortTestResult
            {
                Host = host,
                Port = port,
                Open = true,
                LatencyMs = sw.ElapsedMilliseconds,
                ErrorMessage = null,
            });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            return DiagnosticResult<TcpPortTestResult>.Success(new TcpPortTestResult
            {
                Host = host,
                Port = port,
                Open = false,
                LatencyMs = sw.ElapsedMilliseconds,
                ErrorMessage = "Timeout",
            });
        }
        catch (Exception ex)
        {
            sw.Stop();
            return DiagnosticResult<TcpPortTestResult>.Success(new TcpPortTestResult
            {
                Host = host,
                Port = port,
                Open = false,
                LatencyMs = sw.ElapsedMilliseconds,
                ErrorMessage = ex.Message,
            });
        }
    }

    // ---- DNS ----

    public async Task<DiagnosticResult<DnsResolutionResult>> ResolveDnsAsync(string host, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return DiagnosticResult<DnsResolutionResult>.Failure("Host is required.");
        }

        try
        {
            _logger.LogDebug("Resolving DNS for {Host}.", host);
            var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false);
            return DiagnosticResult<DnsResolutionResult>.Success(new DnsResolutionResult
            {
                Host = host,
                Addresses = addresses,
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return DiagnosticResult<DnsResolutionResult>.FromException(ex, $"DNS resolution failed for '{host}'");
        }
    }

    // ---- Public IP ----

    public async Task<DiagnosticResult<string>> GetPublicIpAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Querying public IP from {Endpoint}.", PublicIpEndpoint);
            using var response = await PublicIpClient.GetAsync(PublicIpEndpoint, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var raw = (await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)).Trim();
            return DiagnosticResult<string>.Success(raw);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return DiagnosticResult<string>.FromException(ex, "Failed retrieving public IP");
        }
    }

    // ---- Pure helpers (testable without I/O) ----

    /// <summary>
    /// Formats a MAC address byte array (6 bytes) as <c>AA:BB:CC:DD:EE:FF</c>.
    /// Returns empty string for an empty array (loopback / tunnel adapters).
    /// </summary>
    public static string FormatMac(byte[] bytes) =>
        bytes.Length == 0 ? string.Empty : string.Join(':', bytes.Select(b => b.ToString("X2")));

    /// <summary>
    /// Returns the prefix length (CIDR) for an IPv4 unicast address, computed
    /// from <see cref="UnicastIPAddressInformation.IPv4Mask"/>. Falls back to 32
    /// when the mask is unavailable (link-local / APIPA).
    /// </summary>
    public static int PrefixFromMask(UnicastIPAddressInformation info)
    {
        try
        {
            var mask = info.IPv4Mask;
            if (mask is null) return 32;
            var bytes = mask.GetAddressBytes();
            var count = 0;
            foreach (var b in bytes)
            {
                count += System.Numerics.BitOperations.PopCount(b);
            }
            return count;
        }
        catch
        {
            return 32;
        }
    }
}
