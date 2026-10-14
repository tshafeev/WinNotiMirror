using System.IO;
using System.Windows;
using WinNotiMirror.Models;

namespace WinNotiMirror;

public partial class SettingsWindow : Window
{
    private readonly OverlayManager _overlayManager;

    public SettingsWindow(OverlayManager overlayManager)
    {
        InitializeComponent();
        _overlayManager = overlayManager;
        HorizontalSlider.Value = overlayManager.Settings.HorizontalPercent;
        VerticalSlider.Value = overlayManager.Settings.VerticalPercent;
        TransparencySlider.Value = overlayManager.Settings.TransparencyPercent;
    }

    private OverlaySettings ReadSettings() => new()
    {
        HorizontalPercent = HorizontalSlider.Value,
        VerticalPercent = VerticalSlider.Value,
        TransparencyPercent = TransparencySlider.Value
    };

    private void OnTestClick(object sender, RoutedEventArgs e)
    {
        _overlayManager.ShowNotification(new NotificationItem
        {
            AppDisplayName = "WinNotiMirror",
            Title = "Test notification",
            Body = "This is how notifications will look with these settings. Click the banner to dismiss it."
        }, ReadSettings());
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var settings = ReadSettings();
        try
        {
            settings.Save();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            System.Windows.MessageBox.Show(this, "Could not save settings.\n" + ex.Message,
                Title, MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _overlayManager.Settings = settings;
        Close();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
