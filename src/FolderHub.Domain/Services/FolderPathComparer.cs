namespace FolderHub.Domain.Services;

/// <summary>
/// 首版路径比较：trim、统一分隔符、去掉末尾分隔符、Windows 不区分大小写。
/// 不处理 junction / \\?\ / 盘符替代等高级规范化（可后续增强）。
/// </summary>
public static class FolderPathComparer
{
    public static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("路径不能为空。", nameof(path));

        var trimmed = path.Trim().Replace('/', '\\');
        while (trimmed.Length > 3 && trimmed.EndsWith('\\'))
            trimmed = trimmed[..^1];

        return trimmed;
    }

    public static bool Equals(string left, string right)
        => string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
}
