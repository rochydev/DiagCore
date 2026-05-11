namespace DiagCore.App.Services;

public interface IPreferencesService
{
    AppPreferences Current { get; }

    void Save();

    /// <summary>Reloads from disk, discarding any in-memory edits.</summary>
    void Reload();
}
