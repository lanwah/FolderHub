namespace FolderHub.UseCases.Icons;

/// <summary>
/// design-demo 图标包：文件位于程序目录 <c>images/</c>，持久化存文件名。
/// 文件夹图标为 Twemoji 彩色 PNG（透明底）；ToneHex 由 UI 圆角色块提供，对齐 demo .folder-icon。
/// </summary>
public static class PackagedIconCatalog
{
    public const string FolderDefault = "folder.png";
    public const string CategoryDefault = "category-default.png";
    public const string CategoryAll = "category-all.png";
    public const string CategoryUncategorized = "category-uncategorized.png";

    private static readonly IconSpec[] FolderSpecs =
    [
        new("folder.png", "📁", "#edf3ff"),
        new("code.png", "💻", "#e8f4ef"),
        new("design.png", "🎨", "#f5edff"),
        new("docs.png", "🗂️", "#fff3e6"),
        new("photos.png", "🖼️", "#fff0f0"),
        new("learning.png", "📚", "#edf4ff"),
        new("package.png", "📦", "#eef2f0"),
        new("archive.png", "🗄️", "#f2f0e9"),
    ];

    private static readonly IconSpec[] CategorySpecs =
    [
        new("category-default.png", "◇", "#edf3ff"),
        new("category-work.png", "▣", "#e8f4ef"),
        new("category-project.png", "◈", "#f5edff"),
        new("category-personal.png", "♡", "#fff0f0"),
        new("category-uncategorized.png", "□", "#eef2f0"),
        new("category-all.png", "▦", "#eaf2ff"),
    ];

    public static string ImagesDirectory
        => Path.Combine(AppContext.BaseDirectory, "images");

    public static IReadOnlyList<string> FolderIcons
        => FolderSpecs.Select(s => s.FileName).ToArray();

    public static IReadOnlyList<string> CategoryIcons
        => CategorySpecs.Select(s => s.FileName).ToArray();

    public static string NormalizeFolderIcon(string? iconFileName)
        => Normalize(iconFileName, FolderSpecs, FolderDefault);

    public static string NormalizeCategoryIcon(string? iconFileName)
        => Normalize(iconFileName, CategorySpecs, CategoryDefault);

    public static string ResolveCategoryIcon(string? iconFileName, string categoryName, bool isVirtualAll)
    {
        if (isVirtualAll)
            return CategoryAll;

        if (!string.IsNullOrWhiteSpace(iconFileName))
            return NormalizeCategoryIcon(iconFileName);

        return categoryName switch
        {
            "工作" => "category-work.png",
            "项目" => "category-project.png",
            "个人" => "category-personal.png",
            "未分类" => CategoryUncategorized,
            _ => CategoryDefault,
        };
    }

    public static string GetGlyph(string? iconFileName)
    {
        var spec = FindSpec(iconFileName);
        return spec?.Glyph ?? FolderSpecs[0].Glyph;
    }

    public static string GetToneHex(string? iconFileName)
    {
        var spec = FindSpec(iconFileName);
        return spec?.ToneHex ?? FolderSpecs[0].ToneHex;
    }

    public static string? TryGetFullPath(string? iconFileName)
    {
        if (string.IsNullOrWhiteSpace(iconFileName))
            return null;

        var name = Path.GetFileName(iconFileName.Trim());
        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return null;

        var full = Path.Combine(ImagesDirectory, name);
        return File.Exists(full) ? full : null;
    }

    private static IconSpec? FindSpec(string? iconFileName)
    {
        if (string.IsNullOrWhiteSpace(iconFileName))
            return null;

        var name = Path.GetFileName(iconFileName.Trim());
        return FolderSpecs.FirstOrDefault(s => string.Equals(s.FileName, name, StringComparison.OrdinalIgnoreCase))
            ?? CategorySpecs.FirstOrDefault(s => string.Equals(s.FileName, name, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string? iconFileName, IReadOnlyList<IconSpec> allowed, string fallback)
    {
        if (string.IsNullOrWhiteSpace(iconFileName))
            return fallback;

        var name = Path.GetFileName(iconFileName.Trim());
        foreach (var spec in allowed)
        {
            if (string.Equals(spec.FileName, name, StringComparison.OrdinalIgnoreCase))
                return spec.FileName;
        }

        return fallback;
    }

    private sealed record IconSpec(string FileName, string Glyph, string ToneHex);
}
