using DiagCore.Core.Common;
using DiagCore.Core.Models;

namespace DiagCore.Core.Diagnostics;

/// <summary>
/// Windows event log diagnostics. Mirrors <c>Get-EventosCriticos</c> from the
/// legacy PowerShell script.
/// </summary>
public interface IEventLogDiagnostics
{
    /// <summary>
    /// Returns Critical + Error events from the System and Application logs
    /// within the last <paramref name="window"/>, ordered most-recent first.
    /// Capped at <paramref name="maxEntries"/>.
    /// </summary>
    Task<DiagnosticResult<IReadOnlyList<EventLogEntry>>> GetCriticalEventsAsync(
        TimeSpan window,
        int maxEntries = 50,
        CancellationToken cancellationToken = default);
}
