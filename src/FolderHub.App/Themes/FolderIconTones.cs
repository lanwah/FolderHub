using System.Windows.Media;
using FolderHub.UseCases.Icons;

namespace FolderHub.App.Themes;

/// <summary>对齐 design-demo 样例卡片的 icon / tone。</summary>
public static class FolderIconTones
{
    /// <summary>design-demo 新建文件夹默认图标（emoji）。</summary>
    public const string DefaultIcon = "📁";

    public static SolidColorBrush Default { get; } = FromHex(PackagedIconCatalog.GetToneHex(PackagedIconCatalog.FolderDefault));

    public static SolidColorBrush ForIcon(string? iconFileName)
        => FromHex(PackagedIconCatalog.GetToneHex(PackagedIconCatalog.NormalizeFolderIcon(iconFileName)));

    public static SolidColorBrush For(Guid folderId)
    {
        // 无自定义图标时按 Id 色散列，避免全部同色。
        var tones = new[]
        {
            "#eaf2ff", "#e8f4ef", "#f5edff", "#fff3e6",
            "#fff0f0", "#edf4ff", "#eef2f0", "#f2f0e9", "#edf3ff",
        };
        var index = (int)(unchecked((uint)folderId.GetHashCode()) % (uint)tones.Length);
        return FromHex(tones[index]);
    }

    public static SolidColorBrush FromHex(string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex)!;
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
