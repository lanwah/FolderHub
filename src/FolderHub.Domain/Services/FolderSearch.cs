using FolderHub.Domain.Entities;

namespace FolderHub.Domain.Services;

/// <summary>
/// 跨分类名称/别名包含搜索（FR-007 / BR-008）：不区分大小写；结果按 FolderId 去重。
/// </summary>
public static class FolderSearch
{
    public static IReadOnlyList<FolderRecord> Search(IEnumerable<FolderRecord> folders, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Array.Empty<FolderRecord>();

        var needle = keyword.Trim();
        return folders
            .Where(f => ContainsIgnoreCase(f.DisplayName, needle) || ContainsIgnoreCase(f.Alias, needle))
            .GroupBy(f => f.Id)
            .Select(g => g.First())
            .ToList();
    }

    private static bool ContainsIgnoreCase(string? haystack, string needle)
        => !string.IsNullOrEmpty(haystack)
           && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
