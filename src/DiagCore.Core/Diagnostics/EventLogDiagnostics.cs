using System.Diagnostics.Eventing.Reader;
using System.Runtime.Versioning;
using DiagCore.Core.Common;
using DiagCore.Core.Models;
using Microsoft.Extensions.Logging;

namespace DiagCore.Core.Diagnostics;

/// <inheritdoc cref="IEventLogDiagnostics"/>
[SupportedOSPlatform("windows")]
public sealed class EventLogDiagnostics : IEventLogDiagnostics
{
    private readonly ILogger<EventLogDiagnostics> _logger;

    public EventLogDiagnostics(ILogger<EventLogDiagnostics> logger)
    {
        _logger = logger;
    }

    public Task<DiagnosticResult<IReadOnlyList<EventLogEntry>>> GetCriticalEventsAsync(
        TimeSpan window,
        int maxEntries = 50,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => GetCriticalEvents(window, maxEntries, cancellationToken), cancellationToken);

    private DiagnosticResult<IReadOnlyList<EventLogEntry>> GetCriticalEvents(
        TimeSpan window,
        int maxEntries,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Reading Critical+Error events from System and Application for the last {Hours:0.0}h.", window.TotalHours);

            var since = DateTime.Now.Subtract(window);
            var sinceIso = since.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            // XPath query: Level 1 (Critical) or 2 (Error), within the time window.
            var xpath = $"*[System[(Level=1 or Level=2) and TimeCreated[@SystemTime >= '{sinceIso}']]]";

            var entries = new List<EventLogEntry>();
            entries.AddRange(ReadLog("System", xpath, maxEntries, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();
            if (entries.Count < maxEntries)
            {
                entries.AddRange(ReadLog("Application", xpath, maxEntries - entries.Count, cancellationToken));
            }

            var ordered = entries
                .OrderByDescending(e => e.TimeCreated)
                .Take(maxEntries)
                .ToList();

            return DiagnosticResult<IReadOnlyList<EventLogEntry>>.Success(ordered);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return DiagnosticResult<IReadOnlyList<EventLogEntry>>.FromException(ex, "Failed reading the Windows event log");
        }
    }

    private static IEnumerable<EventLogEntry> ReadLog(string logName, string xpath, int max, CancellationToken cancellationToken)
    {
        var query = new EventLogQuery(logName, PathType.LogName, xpath)
        {
            ReverseDirection = true,   // newest first
        };

        using var reader = new EventLogReader(query);
        var read = 0;
        while (read < max)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var ev = reader.ReadEvent();
            if (ev is null) yield break;

            string message;
            try { message = ev.FormatDescription() ?? string.Empty; }
            catch { message = "(message unavailable)"; }

            yield return new EventLogEntry
            {
                TimeCreated = ev.TimeCreated ?? DateTime.MinValue,
                Level = MapLevel(ev.Level ?? 0),
                LogName = logName,
                ProviderName = ev.ProviderName ?? string.Empty,
                EventId = ev.Id,
                Message = message,
            };

            read++;
        }
    }

    // ---- Pure helpers ----

    public static EventLogLevel MapLevel(byte raw) =>
        raw switch
        {
            0 => EventLogLevel.LogAlways,
            1 => EventLogLevel.Critical,
            2 => EventLogLevel.Error,
            3 => EventLogLevel.Warning,
            4 => EventLogLevel.Informational,
            5 => EventLogLevel.Verbose,
            _ => EventLogLevel.LogAlways,
        };
}
