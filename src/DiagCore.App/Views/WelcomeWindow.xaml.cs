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
        if (DataContext is WelcomeViewModel vm)
        {
            vm.AcknowledgeCurrentVersion();
        }
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// Treat the close button the same as "Empezar" - acknowledges the
    /// version so the welcome won't reappear on the next startup.
    /// </summary>
    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is WelcomeViewModel vm)
        {
            vm.AcknowledgeCurrentVersion();
        }
        DialogResult = true;
        Close();
    }
}
