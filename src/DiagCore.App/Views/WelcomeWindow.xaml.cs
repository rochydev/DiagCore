using System.Windows;
using DiagCore.App.ViewModels;
using Wpf.Ui.Controls;

namespace DiagCore.App.Views;

public partial class WelcomeWindow : FluentWindow
{
    public WelcomeWindow(WelcomeViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnContinueClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
