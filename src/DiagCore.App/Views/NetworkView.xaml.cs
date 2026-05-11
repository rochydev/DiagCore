using System.Windows;
using System.Windows.Controls;
using DiagCore.App.ViewModels;

namespace DiagCore.App.Views;

public partial class NetworkView : UserControl
{
    public NetworkView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is NetworkViewModel vm && !vm.HasLoadedOnce && !vm.IsLoading)
        {
            await vm.RefreshAsync(CancellationToken.None).ConfigureAwait(true);
        }
    }
}
