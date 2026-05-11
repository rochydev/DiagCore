using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Controls;

namespace DiagCore.App.ViewModels;

public partial class PlaceholderViewModel : ObservableObject
{
    public PlaceholderViewModel(string title, SymbolRegular icon)
    {
        Title = title;
        Icon = icon;
    }

    public string Title { get; }

    public SymbolRegular Icon { get; }
}
