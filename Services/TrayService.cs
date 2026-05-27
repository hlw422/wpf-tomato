using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Wpf_Tomato.Services;

public class TrayService : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private Window? _mainWindow;

    public event EventHandler? ShowWindowRequested;
    public event EventHandler? ExitRequested;

    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;

        _notifyIcon = new NotifyIcon
        {
            Icon = CreateDefaultIcon(),
            Visible = false,
            Text = "番茄时钟"
        };

        _notifyIcon.DoubleClick += (_, _) => ShowWindowRequested?.Invoke(this, EventArgs.Empty);

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("显示主窗口", null, (_, _) => ShowWindowRequested?.Invoke(this, EventArgs.Empty));
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("退出", null, (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty));
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    public void ShowTrayIcon()
    {
        if (_notifyIcon != null)
            _notifyIcon.Visible = true;
    }

    public void HideTrayIcon()
    {
        if (_notifyIcon != null)
            _notifyIcon.Visible = false;
    }

    public void ShowBalloonTip(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        _notifyIcon?.ShowBalloonTip(3000, title, text, icon);
    }

    public void UpdateTooltip(string text)
    {
        if (_notifyIcon != null)
            _notifyIcon.Text = text.Length > 63 ? text[..63] : text;
    }

    private static Icon CreateDefaultIcon()
    {
        var bitmap = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(System.Drawing.Color.Transparent);
        g.FillEllipse(System.Drawing.Brushes.Tomato, 2, 2, 28, 28);
        g.DrawString("🍅", new System.Drawing.Font("Segoe UI", 14), System.Drawing.Brushes.White, 2, 4);
        return Icon.FromHandle(bitmap.GetHicon());
    }

    public void Dispose()
    {
        _notifyIcon?.Dispose();
    }
}
