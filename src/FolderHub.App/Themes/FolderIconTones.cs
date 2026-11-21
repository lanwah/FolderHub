using System.Windows.Media;

namespace FolderHub.App.Themes;

/// <summary>对齐 design-demo 样例卡片的 icon tone 色板。</summary>
public static class FolderIconTones
{
    private static readonly Color[] Palette =
    [
        Color.FromRgb(0xea, 0xf2, 0xff),
        Color.FromRgb(0xe8, 0xf4, 0xef),
        Color.FromRgb(0xf5, 0xed, 0xff),
        Color.FromRgb(0xff, 0xf3, 0xe6),
        Color.FromRgb(0xff, 0xf0, 0xf0),
        Color.FromRgb(0xed, 0xf4, 0xff),
        Color.FromRgb(0xee, 0xf2, 0xf0),
        Color.FromRgb(0xf2, 0xf0, 0xe9),
    ];

    public static SolidColorBrush For(Guid folderId)
    {
        var index = (int)(unchecked((uint)folderId.GetHashCode()) % (uint)Palette.Length);
        var brush = new SolidColorBrush(Palette[index]);
        brush.Freeze();
        return brush;
    }
}
