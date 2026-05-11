using System.Windows;
using System.Windows.Controls;
using DiagCore.App.ViewModels;

namespace DiagCore.App.Views;

public partial class SecurityView : UserControl
{
    public SecurityView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SecurityViewModel vm && !vm.HasLoadedOnce && !vm.IsLoading)
        {
            await vm.RefreshAsync(CancellationToken.None).ConfigureAwait(true);
        }
    }
}
