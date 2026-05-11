using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DiagCore.App.Services;

/// <inheritdoc cref="IPreferencesService"/>
public sealed class PreferencesService : IPreferencesService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private static readonly string PreferencesDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DiagCore");

    private static readonly string PreferencesFile = Path.Combine(PreferencesDirectory, "preferences.json");

    private readonly ILogger<PreferencesService> _logger;

    public PreferencesService(ILogger<PreferencesService> logger)
    {
        _logger = logger;
        Current = Load();
    }

    public AppPreferences Current { get; private set; }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(PreferencesDirectory);
            File.WriteAllText(PreferencesFile, JsonSerializer.Serialize(Current, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write preferences file to {Path}.", PreferencesFile);
        }
    }

    public void Reload() => Current = Load();

    private AppPreferences Load()
    {
        try
        {
            if (!File.Exists(PreferencesFile))
            {
                return new AppPreferences();
            }
            var json = File.ReadAllText(PreferencesFile);
            return JsonSerializer.Deserialize<AppPreferences>(json, JsonOptions) ?? new AppPreferences();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Preferences file at {Path} is malformed; falling back to defaults.", PreferencesFile);
            return new AppPreferences();
        }
    }
}
