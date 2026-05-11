namespace DiagCore.Core.Models;

/// <summary>Windows event log severity, mapped from <c>EventRecord.Level</c>.</summary>
public enum EventLogLevel
{
    LogAlways = 0,
    Critical = 1,
    Error = 2,
    Warning = 3,
    Informational = 4,
    Verbose = 5,
}

/// <summary>Single Windows event log entry.</summary>
public sealed record EventLogEntry
{
    public required DateTime TimeCreated { get; init; }

    public required EventLogLevel Level { get; init; }

    public required string LogName { get; init; }  // System, Application, ...

    public required string ProviderName { get; init; }

    public required int EventId { get; init; }

    public required string Message { get; init; }
}
