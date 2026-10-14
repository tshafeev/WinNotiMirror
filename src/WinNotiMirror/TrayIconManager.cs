using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinNotiMirror;

public sealed class TrayIconManager : IDisposable
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "WinNotiMirror";

    private readonly Icon _trayIcon = new(typeof(TrayIconManager), "Assets.Notification.ico");
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly ToolStripMenuItem _startupItem;

    public event Action? PauseToggled;
    public event Action? ExitRequested;
    public event Action? SettingsRequested;

    public bool IsPaused { get; private set; }

    public TrayIconManager()
    {
        _pauseItem = new ToolStripMenuItem("Pause mirroring", null, (_, _) => TogglePause());
        _startupItem = new ToolStripMenuItem("Start with Windows", null, (_, _) => ToggleStartup())
        {
            Checked = IsStartupEnabled()
        };
        var exitItem = new ToolStripMenuItem("Exit", null, (_, _) => ExitRequested?.Invoke());

        var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Settings...", null, (_, _) => SettingsRequested?.Invoke()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_pauseItem);
        menu.Items.Add(_startupItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = _trayIcon,
            Text = "Windows Notification Mirror",
            Visible = true,
            ContextMenuStrip = menu
        };
    }

    public void ShowBalloon(string title, string text) =>
        _notifyIcon.ShowBalloonTip(4000, title, text, ToolTipIcon.Info);

    private void TogglePause()
    {
        IsPaused = !IsPaused;
        _pauseItem.Text = IsPaused ? "Resume mirroring" : "Pause mirroring";
        PauseToggled?.Invoke();
    }

    private static bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return key?.GetValue(RunValueName) is not null;
    }

    private void ToggleStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                         ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);

        if (IsStartupEnabled())
        {
            key!.DeleteValue(RunValueName, throwOnMissingValue: false);
            _startupItem.Checked = false;
        }
        else
        {
            var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
            if (exePath is not null)
            {
                key!.SetValue(RunValueName, $"\"{exePath}\"");
                _startupItem.Checked = true;
            }
        }
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _trayIcon.Dispose();
    }
}
