using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Media.Imaging;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using WinNotiMirror.Models;

namespace WinNotiMirror;

public sealed class NotificationListenerService
{
    private readonly UserNotificationListener _listener = UserNotificationListener.Current;

    public event Action<NotificationItem>? NotificationReceived;

    public async Task<UserNotificationListenerAccessStatus> InitializeAsync()
    {
        var accessStatus = await _listener.RequestAccessAsync();
        if (accessStatus == UserNotificationListenerAccessStatus.Allowed)
        {
            _listener.NotificationChanged += OnNotificationChanged;
        }

        return accessStatus;
    }

    private async void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
    {
        if (args.ChangeKind != UserNotificationChangedKind.Added)
        {
            return;
        }

        UserNotification? notification;
        try
        {
            notification = sender.GetNotification(args.UserNotificationId);
        }
        catch (Exception)
        {
            return;
        }

        if (notification is null)
        {
            return;
        }

        var item = BuildItem(notification);
        if (item is not null)
        {
            NotificationReceived?.Invoke(item);
        }
    }

    private static NotificationItem? BuildItem(UserNotification notification)
    {
        try
        {
            var binding = notification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            if (binding is null)
            {
                return null;
            }

            var texts = binding.GetTextElements()
                .Select(t => t.Text)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            var title = texts.FirstOrDefault();
            var body = texts.Count > 1 ? string.Join(Environment.NewLine, texts.Skip(1)) : null;
            var appDisplayName = notification.AppInfo?.DisplayInfo?.DisplayName ?? "Notification";

            BitmapSource? icon = TryLoadIcon(notification);

            return new NotificationItem
            {
                AppDisplayName = appDisplayName,
                Title = title,
                Body = body,
                AppIcon = icon
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static BitmapSource? TryLoadIcon(UserNotification notification)
    {
        try
        {
            var logoStreamRef = notification.AppInfo?.DisplayInfo?.GetLogo(new Windows.Foundation.Size(48, 48));
            if (logoStreamRef is null)
            {
                return null;
            }

            var streamTask = logoStreamRef.OpenReadAsync().AsTask();
            streamTask.Wait();
            using var stream = streamTask.Result;

            var reader = new Windows.Storage.Streams.DataReader(stream);
            var loadTask = reader.LoadAsync((uint)stream.Size).AsTask();
            loadTask.Wait();

            var bytes = new byte[stream.Size];
            reader.ReadBytes(bytes);

            using var memoryStream = new MemoryStream(bytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
