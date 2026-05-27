using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Wpf_Tomato.Services;

public class HotKeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_CTRL = 0x0002;
    private const int MOD_ALT = 0x0001;
    private const int MOD_SHIFT = 0x0004;

    private const int HOTKEY_TOGGLE = 1;
    private const int HOTKEY_RESET = 2;
    private const int HOTKEY_SKIP = 3;

    private HwndSource? _source;
    private IntPtr _windowHandle;

    public event EventHandler? ToggleRequested;
    public event EventHandler? ResetRequested;
    public event EventHandler? SkipRequested;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public void Register(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _windowHandle = helper.Handle;
        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(HwndHook);

        // Ctrl+Alt+T: Toggle start/pause
        RegisterHotKey(_windowHandle, HOTKEY_TOGGLE, MOD_CTRL | MOD_ALT, 0x54); // T
        // Ctrl+Alt+R: Reset
        RegisterHotKey(_windowHandle, HOTKEY_RESET, MOD_CTRL | MOD_ALT, 0x52); // R
        // Ctrl+Alt+S: Skip
        RegisterHotKey(_windowHandle, HOTKEY_SKIP, MOD_CTRL | MOD_ALT, 0x53); // S
    }

    public void Unregister()
    {
        _source?.RemoveHook(HwndHook);
        if (_windowHandle != IntPtr.Zero)
        {
            UnregisterHotKey(_windowHandle, HOTKEY_TOGGLE);
            UnregisterHotKey(_windowHandle, HOTKEY_RESET);
            UnregisterHotKey(_windowHandle, HOTKEY_SKIP);
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            var id = wParam.ToInt32();
            switch (id)
            {
                case HOTKEY_TOGGLE:
                    ToggleRequested?.Invoke(this, EventArgs.Empty);
                    handled = true;
                    break;
                case HOTKEY_RESET:
                    ResetRequested?.Invoke(this, EventArgs.Empty);
                    handled = true;
                    break;
                case HOTKEY_SKIP:
                    SkipRequested?.Invoke(this, EventArgs.Empty);
                    handled = true;
                    break;
            }
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
        _source?.Dispose();
    }
}
