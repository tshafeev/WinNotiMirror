using System.Windows;
using Windows.UI.Notifications.Management;
using Application = System.Windows.Application;

namespace WinNotiMirror;

public partial class App : Application
{
    private TrayIconManager? _tray;
    private NotificationListenerService? _listener;
    private OverlayManager? _overlayManager;
    private SettingsWindow? _settingsWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!SparsePackageRegistrar.HasIdentity())
        {
            await BootstrapIdentityAndRelaunchAsync();
            return;
        }

        _tray = new TrayIconManager();
        _tray.ExitRequested += () => Shutdown();
        _overlayManager = new OverlayManager();
        _tray.SettingsRequested += ShowSettings;

        _listener = new NotificationListenerService();
        UserNotificationListenerAccessStatus access;
        try
        {
            access = await _listener.InitializeAsync();
        }
        catch (Exception ex)
        {
            _tray.ShowBalloon("Startup error", ex.Message);
            return;
        }

        if (access != UserNotificationListenerAccessStatus.Allowed)
        {
            _tray.ShowBalloon(
                "Notification access needed",
                "Grant notification access in the prompt (or Settings > Notifications), then restart WinNotiMirror.");
            return;
        }

        _listener.NotificationReceived += OnNotificationReceived;
        _tray.ShowBalloon("Windows Notification Mirror", "Mirroring is active.");
    }

    /// <summary>
    /// A raw-launched .exe has no package identity. Deploy the sparse package if needed,
    /// then relaunch through shell:AppsFolder activation, which is the only way to get an
    /// identity-bearing process, and exit this one.
    /// </summary>
    private async Task BootstrapIdentityAndRelaunchAsync()
    {
        var msixPath = SparsePackageRegistrar.FindSparseMsixPath();
        var familyName = await SparsePackageRegistrar.EnsureDeployedAndGetFamilyNameAsync(msixPath);

        if (familyName is null)
        {
            var tray = new TrayIconManager();
            tray.ExitRequested += () => Shutdown();
            tray.ShowBalloon(
                "Setup incomplete",
                "The sparse package isn't built yet. Run the packaging scripts (see README), then restart WinNotiMirror.");
            _tray = tray;
            return;
        }

        SparsePackageRegistrar.RelaunchViaAppActivation(familyName);
        Shutdown();
    }

    private void ShowSettings()
    {
        if (_overlayManager is null)
        {
            return;
        }

        if (_settingsWindow is null)
        {
            _settingsWindow = new SettingsWindow(_overlayManager);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        }

        if (_settingsWindow.WindowState == WindowState.Minimized)
        {
            _settingsWindow.WindowState = WindowState.Normal;
        }
        _settingsWindow.Activate();
    }

    private void OnNotificationReceived(Models.NotificationItem item)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (_tray?.IsPaused == true)
            {
                return;
            }

            _overlayManager?.ShowNotification(item);
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        base.OnExit(e);
    }
}
