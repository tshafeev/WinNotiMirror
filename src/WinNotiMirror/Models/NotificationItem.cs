using System.Windows.Media.Imaging;

namespace WinNotiMirror.Models;

public sealed class NotificationItem
{
    public required string AppDisplayName { get; init; }
    public string? Title { get; init; }
    public string? Body { get; init; }
    public BitmapSource? AppIcon { get; init; }
}
