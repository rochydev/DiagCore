using System.IO;
using System.Windows;
using System.Windows.Threading;
using DiagCore.App.Services;
using DiagCore.App.ViewModels;
using DiagCore.App.Views;
using DiagCore.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DiagCore.App;

public partial class App : Application
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DiagCore",
        "logs");

    private IHost? _host;

    public static IServiceProvider Services =>
        ((App)Current)._host?.Services
        ?? throw new InvalidOperationException("Host has not been initialised.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        Directory.CreateDirectory(LogDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: Path.Combine(LogDirectory, "diagcore-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((_, services) =>
                {
                    services.AddDiagCoreDiagnostics();

                    // App-side services
                    services.AddSingleton<IPreferencesService, PreferencesService>();

                    // View models (singleton: no per-call state)
                    services.AddSingleton<HomeViewModel>();
                    services.AddSingleton<HardwareViewModel>();
                    services.AddSingleton<StorageViewModel>();
                    services.AddSingleton<NetworkViewModel>();
                    services.AddSingleton<SecurityViewModel>();
                    services.AddSingleton<ReportsViewModel>();
                    services.AddSingleton<SettingsViewModel>();
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<MainWindow>();

                    // Welcome flow - transient so the user can re-open it from
                    // Settings without keeping a stale view-model around.
                    services.AddTransient<WelcomeViewModel>();
                    services.AddTransient<WelcomeWindow>();
                })
                .Build();

            await _host.StartAsync();

            Log.Information("DiagCore starting up.");

            var window = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            window.Show();

            TryShowWelcome();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during application startup.");
            throw;
        }
    }

    /// <summary>
    /// Shows the welcome window on top of the main window on every startup,
    /// unless the user has explicitly opted out via the
    /// "no volver a mostrar al iniciar" checkbox.
    /// </summary>
    private void TryShowWelcome()
    {
        if (_host is null) return;

        var prefs = _host.Services.GetRequiredService<IPreferencesService>().Current;
        if (prefs.SuppressWelcomeOnStartup) return;

        Log.Information("Showing welcome window on startup.");
        var welcome = _host.Services.GetRequiredService<WelcomeWindow>();
        welcome.Owner = MainWindow;
        welcome.ShowDialog();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_host is not null)
            {
                Log.Information("DiagCore shutting down.");
                await _host.StopAsync(TimeSpan.FromSeconds(5));
                _host.Dispose();
            }
        }
        finally
        {
            await Log.CloseAndFlushAsync();
            base.OnExit(e);
        }
    }

    private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Unhandled AppDomain exception (terminating: {Terminating}).", e.IsTerminating);
        }
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled Dispatcher exception.");
        e.Handled = true;
    }
}
