namespace DiagCore.App.Services;

/// <summary>
/// User-tunable application preferences. Persisted as JSON in
/// <c>%LOCALAPPDATA%\DiagCore\preferences.json</c>. The plan calls for zero
/// telemetry and zero registry persistence, so a flat file under the user's
/// local profile is the right home.
/// </summary>
public sealed class AppPreferences
{
    /// <summary>
    /// True when the user has opted out of seeing the welcome on every start.
    /// The window can still be opened from the Configuration view.
    /// </summary>
    public bool SuppressWelcomeOnStartup { get; set; }
}
