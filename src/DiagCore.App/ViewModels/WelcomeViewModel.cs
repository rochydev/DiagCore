using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagCore.App.Services;

namespace DiagCore.App.ViewModels;

public sealed partial class WelcomeViewModel : ObservableObject
{
    private const string RepositoryUrl = "https://github.com/RochyDev/DiagCore";
    private const string IssuesUrl = "https://github.com/RochyDev/DiagCore/issues";

    private readonly IPreferencesService _preferences;

    public WelcomeViewModel(IPreferencesService preferences)
    {
        _preferences = preferences;
        Entries = new ObservableCollection<ChangelogEntry>(ChangelogData.Entries);

        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";
        AppVersion = $"v{assemblyVersion}";

        SuppressWelcomeOnStartup = _preferences.Current.SuppressWelcomeOnStartup;
    }

    public ObservableCollection<ChangelogEntry> Entries { get; }

    public string AppVersion { get; }

    public string LatestVersion => Entries.FirstOrDefault()?.Version ?? "—";

    public string AuthorFullName => "Roger Malgrat Gonzalez";

    public string AuthorHandle => "RochyDev";

    public string AuthorTagline => "Creador y desarrollador principal";

    public string RepositoryDisplay => "github.com/RochyDev/DiagCore";

    [ObservableProperty]
    private bool _suppressWelcomeOnStartup;

    partial void OnSuppressWelcomeOnStartupChanged(bool value)
    {
        _preferences.Current.SuppressWelcomeOnStartup = value;
        _preferences.Save();
    }

    /// <summary>
    /// Marks the latest changelog entry as acknowledged so the welcome won't
    /// auto-open again until a new version lands.
    /// </summary>
    public void AcknowledgeCurrentVersion()
    {
        _preferences.Current.LastWelcomeVersionShown = LatestVersion;
        _preferences.Save();
    }

    [RelayCommand]
    private void OpenRepository() => OpenUrl(RepositoryUrl);

    [RelayCommand]
    private void OpenIssues() => OpenUrl(IssuesUrl);

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
        catch
        {
            // The user has no default browser, or shell exec is blocked.
            // Silently swallow - this is a convenience link, not a critical path.
        }
    }
}
