using System.Windows.Media;
using System.Windows.Media.Imaging;
using FolderHub.UseCases.Icons;

namespace FolderHub.App.Services;

/// <summary>WPF 侧加载 <see cref="PackagedIconCatalog"/> 图像。</summary>
public static class AppIconCatalog
{
    public static string FolderDefault => PackagedIconCatalog.FolderDefault;
    public static string CategoryDefault => PackagedIconCatalog.CategoryDefault;

    public static IReadOnlyList<string> FolderIcons => PackagedIconCatalog.FolderIcons;
    public static IReadOnlyList<string> CategoryIcons => PackagedIconCatalog.CategoryIcons;

    public static string NormalizeFolderIcon(string? iconFileName)
        => PackagedIconCatalog.NormalizeFolderIcon(iconFileName);

    public static string NormalizeCategoryIcon(string? iconFileName)
        => PackagedIconCatalog.NormalizeCategoryIcon(iconFileName);

    public static string ResolveCategoryIcon(string? iconFileName, string categoryName, bool isVirtualAll)
        => PackagedIconCatalog.ResolveCategoryIcon(iconFileName, categoryName, isVirtualAll);

    public static ImageSource? TryLoadImage(string? iconFileName)
    {
        var full = PackagedIconCatalog.TryGetFullPath(iconFileName);
        if (full is null)
            return null;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(full, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}
