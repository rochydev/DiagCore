using System.Windows;
using System.Windows.Controls;
using DiagCore.App.ViewModels;

namespace DiagCore.App.Views;

public partial class StorageView : UserControl
{
    public StorageView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is StorageViewModel vm && !vm.HasLoadedOnce && !vm.IsLoading)
        {
            await vm.RefreshAsync(CancellationToken.None).ConfigureAwait(true);
        }
    }
}
