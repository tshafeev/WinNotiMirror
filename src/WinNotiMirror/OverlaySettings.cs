using System.IO;
using System.Text.Json;

namespace WinNotiMirror;

public sealed class OverlaySettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinNotiMirror", "settings.json");

    public double HorizontalPercent { get; set; } = 50;
    public double VerticalPercent { get; set; } = 15;
    public double TransparencyPercent { get; set; } = 40;

    public bool IsValid() =>
        IsPercent(HorizontalPercent) && IsPercent(VerticalPercent) && IsPercent(TransparencyPercent);

    private static bool IsPercent(double value) => double.IsFinite(value) && value >= 0 && value <= 100;

    public (int X, int Y) GetPosition(int screenWidth, int screenHeight, int bannerWidth, int bannerHeight, int stackOffset)
    {
        var maxX = Math.Max(0, screenWidth - bannerWidth);
        var maxY = Math.Max(0, screenHeight - bannerHeight);
        var x = (int)Math.Round(maxX * HorizontalPercent / 100);
        var y = Math.Clamp((int)Math.Round(screenHeight * VerticalPercent / 100), 0, maxY);
        // Stack upward in the lower half so the newest banner stays inside the screen.
        y += VerticalPercent > 50 ? -stackOffset : stackOffset;
        return (Math.Clamp(x, 0, maxX), Math.Clamp(y, 0, maxY));
    }

    public static OverlaySettings Load()
    {
        try
        {
            var settings = JsonSerializer.Deserialize<OverlaySettings>(File.ReadAllText(SettingsPath));
            return settings?.IsValid() == true ? settings : new OverlaySettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new OverlaySettings();
        }
    }

    public void Save()
    {
        if (!IsValid())
        {
            throw new InvalidDataException("Settings must be percentages between 0 and 100.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        var temporaryPath = SettingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(this));
        File.Move(temporaryPath, SettingsPath, overwrite: true);
    }
}
