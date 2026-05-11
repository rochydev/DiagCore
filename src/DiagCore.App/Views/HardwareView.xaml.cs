using System.Windows;
using System.Windows.Controls;
using DiagCore.App.ViewModels;

namespace DiagCore.App.Views;

public partial class HardwareView : UserControl
{
    public HardwareView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Kick off the first load when the view enters the visual tree.
        if (DataContext is HardwareViewModel vm && !vm.HasLoadedOnce && !vm.IsLoading)
        {
            await vm.RefreshAsync(CancellationToken.None).ConfigureAwait(true);
        }
    }
}
