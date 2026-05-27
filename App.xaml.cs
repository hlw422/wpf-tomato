using System.Windows;

namespace Wpf_Tomato;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            System.Windows.MessageBox.Show($"发生未处理的异常: {args.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
