using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Controls;

namespace DiagCore.App.ViewModels;

public partial class NavigationItem : ObservableObject
{
    public NavigationItem(string key, string title, SymbolRegular icon, object content, bool isFooter = false)
    {
        Key = key;
        Title = title;
        Icon = icon;
        Content = content;
        IsFooter = isFooter;
    }

    public string Key { get; }

    public string Title { get; }

    public SymbolRegular Icon { get; }

    public object Content { get; }

    /// <summary>
    /// True when this item should be docked at the bottom of the sidebar
    /// (Settings). Drives the DockPanel.Dock attached property in
    /// MainWindow.xaml.
    /// </summary>
    public bool IsFooter { get; }
}
