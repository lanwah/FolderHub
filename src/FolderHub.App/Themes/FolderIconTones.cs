using System.Windows.Media;

namespace FolderHub.App.Themes;

/// <summary>对齐 design-demo 样例卡片的 icon / tone。</summary>
public static class FolderIconTones
{
    /// <summary>design-demo 新建文件夹默认图标。</summary>
    public const string DefaultIcon = "📁";

    /// <summary>design-demo 新建文件夹默认 tone（#edf3ff）。</summary>
    private static readonly Color DefaultToneColor = Color.FromRgb(0xed, 0xf3, 0xff);

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
        DefaultToneColor,
    ];

    public static SolidColorBrush Default { get; } = CreateBrush(DefaultToneColor);

    public static SolidColorBrush For(Guid folderId)
    {
        var index = (int)(unchecked((uint)folderId.GetHashCode()) % (uint)Palette.Length);
        return CreateBrush(Palette[index]);
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
