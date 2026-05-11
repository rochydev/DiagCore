using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using DiagCore.App.Resources;
using Wpf.Ui.Controls;

namespace DiagCore.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = Strings.AppTitle;

    [ObservableProperty]
    private NavigationItem? _selectedItem;

    public string AppVersionDisplay { get; }

    public string FooterCredit => "DiagCore · developed by RochyDev";

    /// <summary>
    /// All sidebar items, including the footer-docked Settings entry. The
    /// shell uses a single ListBox with a DockPanel + DataTrigger so only
    /// one selection state exists across the whole navigation surface.
    /// </summary>
    public ObservableCollection<NavigationItem> NavigationItems { get; }

    public MainWindowViewModel(
        HomeViewModel home,
        HardwareViewModel hardware,
        StorageViewModel storage,
        NetworkViewModel network,
        SecurityViewModel security,
        ReportsViewModel reports,
        SettingsViewModel settings)
    {
        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";
        AppVersionDisplay = $"v{assemblyVersion}";

        NavigationItems =
        [
            new NavigationItem("home",     Strings.Nav_Home,     SymbolRegular.Home24,         home),
            new NavigationItem("hardware", Strings.Nav_Hardware, SymbolRegular.Desktop24,      hardware),
            new NavigationItem("storage",  Strings.Nav_Storage,  SymbolRegular.Database24,     storage),
            new NavigationItem("network",  Strings.Nav_Network,  SymbolRegular.Globe24,        network),
            new NavigationItem("security", Strings.Nav_Security, SymbolRegular.Shield24,       security),
            new NavigationItem("reports",  Strings.Nav_Reports,  SymbolRegular.DocumentText24, reports),
            new NavigationItem("settings", Strings.Nav_Settings, SymbolRegular.Settings24,     settings, isFooter: true),
        ];

        SelectedItem = NavigationItems[0];
    }
}
