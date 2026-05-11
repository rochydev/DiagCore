using System.Windows;
using DiagCore.App.ViewModels;
using Wpf.Ui.Controls;

namespace DiagCore.App;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        StateChanged += OnStateChanged;
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void OnMaximizeRestoreClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Swap the maximize icon between Square (will maximize) and SquareMultiple
    /// (will restore) so the affordance matches the current window state.
    /// </summary>
    private void OnStateChanged(object? sender, EventArgs e)
    {
        MaximizeIcon.Symbol = WindowState == WindowState.Maximized
            ? SymbolRegular.SquareMultiple24
            : SymbolRegular.Square24;
        MaximizeButton.ToolTip = WindowState == WindowState.Maximized
            ? "Restaurar"
            : "Maximizar";
    }
}
