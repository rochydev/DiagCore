using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagCore.App.Services;
using DiagCore.App.Views;

namespace DiagCore.App.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IServiceProvider _services;
    private readonly IPreferencesService _preferences;

    public SettingsViewModel(IServiceProvider services, IPreferencesService preferences)
    {
        _services = services;
        _preferences = preferences;

        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString(3) ?? "0.1.0";
        AppVersion = $"v{version}";
        AppFramework = $".NET {Environment.Version}";
    }

    public string AppVersion { get; }

    public string AppFramework { get; }

    public string AppName => "DiagCore";

    public string AuthorFullName => "Roger Malgrat Gonzalez";

    public string AuthorHandle => "RochyDev";

    public string Repository => "github.com/RochyDev/DiagCore";

    public string License => "MIT";

    public string CurrentTheme => "Oscuro (forzado en MVP)";

    public string CurrentLanguage => "Español (es-ES)";

    [RelayCommand]
    private void ShowWelcome()
    {
        var welcome = (WelcomeWindow)_services.GetService(typeof(WelcomeWindow))!;
        welcome.Owner = Application.Current.MainWindow;
        welcome.ShowDialog();
    }
}
