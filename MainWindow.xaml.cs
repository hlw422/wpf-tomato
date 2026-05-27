using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Wpf_Tomato.ViewModels;

namespace Wpf_Tomato;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Initialize(this);
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_viewModel.MinimizeToTray)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            _viewModel.MinimizeToTrayAction();
        }
        else
        {
            _viewModel.Dispose();
        }
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized && _viewModel.MinimizeToTray)
        {
            Hide();
            _viewModel.MinimizeToTrayAction();
        }
    }

    private void TimerTab_Click(object sender, RoutedEventArgs e)
    {
        TimerView.Visibility = Visibility.Visible;
        StatsView.Visibility = Visibility.Collapsed;
        SettingsView.Visibility = Visibility.Collapsed;
    }

    private void StatsTab_Click(object sender, RoutedEventArgs e)
    {
        TimerView.Visibility = Visibility.Collapsed;
        StatsView.Visibility = Visibility.Visible;
        SettingsView.Visibility = Visibility.Collapsed;
    }

    private void SettingsTab_Click(object sender, RoutedEventArgs e)
    {
        TimerView.Visibility = Visibility.Collapsed;
        StatsView.Visibility = Visibility.Collapsed;
        SettingsView.Visibility = Visibility.Visible;
    }
}
