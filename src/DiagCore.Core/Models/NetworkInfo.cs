using System.Net;

namespace DiagCore.Core.Models;

/// <summary>Network adapter snapshot. Sourced from <see cref="System.Net.NetworkInformation.NetworkInterface"/>.</summary>
public sealed record NetworkAdapterInfo
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string MacAddress { get; init; }

    public required string Status { get; init; }   // Up / Down / Testing / Unknown

    public required string InterfaceType { get; init; }   // Ethernet / Wireless80211 / Loopback / ...

    /// <summary>Negotiated link speed in bits per second. -1 when unknown.</summary>
    public required long SpeedBps { get; init; }

    public required IReadOnlyList<string> IPv4Addresses { get; init; }

    public required IReadOnlyList<string> IPv6Addresses { get; init; }

    public required IReadOnlyList<string> Gateways { get; init; }

    public required IReadOnlyList<string> DnsServers { get; init; }
}

/// <summary>Single reply in a ping run.</summary>
public sealed record PingReply
{
    public required int Sequence { get; init; }

    public required string Status { get; init; }    // Success / TimedOut / DestinationHostUnreachable / ...

    public required long RoundtripMs { get; init; }

    public required string? Address { get; init; }
}

/// <summary>Aggregated ping result.</summary>
public sealed record PingResult
{
    public required string Host { get; init; }

    public required IReadOnlyList<PingReply> Replies { get; init; }

    public required int Sent { get; init; }

    public required int Received { get; init; }

    public required int Lost { get; init; }

    public required double LossPercent { get; init; }

    public required long MinMs { get; init; }

    public required long MaxMs { get; init; }

    public required long AvgMs { get; init; }
}

/// <summary>Single hop in a traceroute.</summary>
public sealed record TracertHop
{
    public required int HopNumber { get; init; }

    public required string? Address { get; init; }

    public required long RoundtripMs { get; init; }

    public required string Status { get; init; }
}

/// <summary>Outcome of a single TCP-port connectivity test.</summary>
public sealed record TcpPortTestResult
{
    public required string Host { get; init; }

    public required int Port { get; init; }

    public required bool Open { get; init; }

    public required long LatencyMs { get; init; }

    public required string? ErrorMessage { get; init; }
}

/// <summary>DNS resolution result for a hostname.</summary>
public sealed record DnsResolutionResult
{
    public required string Host { get; init; }

    public required IReadOnlyList<IPAddress> Addresses { get; init; }
}
