using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using WinNotiMirror.Models;

namespace WinNotiMirror;

public sealed class OverlayManager
{
    private const int GapPx = 10;
    private static readonly TimeSpan AutoDismissAfter = TimeSpan.FromSeconds(6);

    private readonly Dictionary<string, List<OverlayWindow>> _bannersByScreen = new();

    public OverlaySettings Settings { get; set; } = OverlaySettings.Load();

    public void ShowNotification(NotificationItem item, OverlaySettings? previewSettings = null)
    {
        foreach (var screen in Screen.AllScreens)
        {
            SpawnBanner(item, screen, previewSettings ?? Settings);
        }
    }

    private void SpawnBanner(NotificationItem item, Screen screen, OverlaySettings settings)
    {
        var window = new OverlayWindow(item, settings)
        {
            Left = -32000,
            Top = -32000
        };

        if (!_bannersByScreen.TryGetValue(screen.DeviceName, out var list))
        {
            list = new List<OverlayWindow>();
            _bannersByScreen[screen.DeviceName] = list;
        }

        list.Add(window);

        window.Dismissed += w =>
        {
            list.Remove(w);
            ReflowScreen(screen, list);
        };

        window.ContentRendered += (_, _) => PositionOnScreen(window, screen, list);
        window.Show();

        var timer = new DispatcherTimer { Interval = AutoDismissAfter };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            window.Dismiss();
        };
        timer.Start();
    }

    private static void PositionOnScreen(OverlayWindow window, Screen screen, List<OverlayWindow> siblings)
    {
        var dpi = VisualTreeHelper.GetDpi(window);
        var widthPx = (int)Math.Round(window.ActualWidth * dpi.DpiScaleX);
        var heightPx = (int)Math.Round(window.ActualHeight * dpi.DpiScaleY);

        var stackOffsetPx = 0;
        foreach (var sibling in siblings)
        {
            if (ReferenceEquals(sibling, window))
            {
                break;
            }

            stackOffsetPx += (int)Math.Round(sibling.ActualHeight * dpi.DpiScaleY) + GapPx;
        }

        var position = window.Settings.GetPosition(screen.WorkingArea.Width, screen.WorkingArea.Height, widthPx, heightPx, stackOffsetPx);
        var x = screen.WorkingArea.X + position.X;
        var y = screen.WorkingArea.Y + position.Y;

        SetWindowPos(new WindowInteropHelper(window).Handle, HwndTopmost, x, y, 0, 0,
            SwpNoSize | SwpNoActivate);
    }

    private static void ReflowScreen(Screen screen, List<OverlayWindow> siblings)
    {
        foreach (var window in siblings)
        {
            PositionOnScreen(window, screen, siblings);
        }
    }

    private static readonly IntPtr HwndTopmost = new(-1);
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoActivate = 0x0010;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
}
