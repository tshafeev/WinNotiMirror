using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WinNotiMirror.Models;

namespace WinNotiMirror;

public partial class OverlayWindow : Window
{
    private bool _dismissed;

    public event Action<OverlayWindow>? Dismissed;

    public OverlaySettings Settings { get; }

    public OverlayWindow(NotificationItem item, OverlaySettings settings)
    {
        InitializeComponent();
        Settings = settings;
        BannerBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
            (byte)Math.Round(255 * (1 - settings.TransparencyPercent / 100)), 245, 245, 245));

        AppNameText.Text = item.AppDisplayName;

        TitleText.Text = item.Title ?? string.Empty;
        TitleText.Visibility = string.IsNullOrEmpty(item.Title) ? Visibility.Collapsed : Visibility.Visible;

        BodyText.Text = item.Body ?? string.Empty;
        BodyText.Visibility = string.IsNullOrEmpty(item.Body) ? Visibility.Collapsed : Visibility.Visible;

        if (item.AppIcon is not null)
        {
            IconImage.Source = item.AppIcon;
        }
        else
        {
            IconBorder.Visibility = Visibility.Collapsed;
        }

        Opacity = 0;
        Loaded += (_, _) => FadeIn();
    }

    private void FadeIn()
    {
        var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
        BeginAnimation(OpacityProperty, animation);
    }

    public void Dismiss()
    {
        if (_dismissed)
        {
            return;
        }

        _dismissed = true;

        var animation = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(200));
        animation.Completed += (_, _) =>
        {
            Dismissed?.Invoke(this);
            Close();
        };
        BeginAnimation(OpacityProperty, animation);
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Dismiss();
    }
}
