using FolderHub.Domain.Entities;
using FolderHub.Domain.Services;

namespace FolderHub.Domain.Catalog;

/// <summary>
/// 内存目录聚合：分类、文件夹记录、多归属与排序规则。
/// </summary>
public sealed class FolderCatalog
{
    private readonly Dictionary<Guid, Category> _categories = new();
    private readonly Dictionary<Guid, FolderRecord> _folders = new();
    private readonly List<FolderCategoryLink> _links = new();

    public IReadOnlyCollection<Category> Categories => _categories.Values.OrderBy(c => c.SortOrder).ToList();
    public IReadOnlyCollection<FolderRecord> Folders => _folders.Values.ToList();
    public IReadOnlyCollection<FolderCategoryLink> Links => _links.AsReadOnly();

    public Category Uncategorized =>
        _categories.Values.Single(c => c.IsUncategorized);

    public static FolderCatalog CreateEmpty()
    {
        var catalog = new FolderCatalog();
        var uncategorized = Category.CreateUncategorized();
        catalog._categories.Add(uncategorized.Id, uncategorized);
        return catalog;
    }

    /// <summary>
    /// 从持久化快照恢复聚合；必须包含且仅包含一个内建“未分类”。
    /// </summary>
    public static FolderCatalog Restore(
        IEnumerable<Category> categories,
        IEnumerable<FolderRecord> folders,
        IEnumerable<FolderCategoryLink> links)
    {
        var catalog = new FolderCatalog();
        foreach (var category in categories)
        {
            if (catalog._categories.ContainsKey(category.Id))
                throw new InvalidOperationException($"重复的分类 Id：{category.Id}");
            catalog._categories.Add(category.Id, category);
        }

        if (catalog._categories.Values.Count(c => c.IsUncategorized) != 1)
            throw new InvalidOperationException("必须恰好包含一个内建“未分类”分类。");

        foreach (var folder in folders)
        {
            if (catalog._folders.ContainsKey(folder.Id))
                throw new InvalidOperationException($"重复的文件夹 Id：{folder.Id}");
            if (catalog._folders.Values.Any(f => FolderPathComparer.Equals(f.Path, folder.Path)))
                throw new InvalidOperationException($"重复的文件夹路径：{folder.Path}");
            catalog._folders.Add(folder.Id, folder);
        }

        foreach (var link in links)
        {
            if (!catalog._folders.ContainsKey(link.FolderId))
                throw new InvalidOperationException($"链接引用了不存在的文件夹：{link.FolderId}");
            if (!catalog._categories.ContainsKey(link.CategoryId))
                throw new InvalidOperationException($"链接引用了不存在的分类：{link.CategoryId}");
            if (catalog._links.Any(l => l.FolderId == link.FolderId && l.CategoryId == link.CategoryId))
                throw new InvalidOperationException("重复的分类归属链接。");
            catalog._links.Add(link);
        }

        return catalog;
    }

    public Category AddCategory(string name)
    {
        if (_categories.Values.Any(c => string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"分类“{name.Trim()}”已存在。");

        var category = new Category(Guid.NewGuid(), name, NextCategorySortOrder());
        _categories.Add(category.Id, category);
        return category;
    }

    public void RenameCategory(Guid categoryId, string name)
    {
        var category = GetCategory(categoryId);
        if (!category.IsUncategorized
            && _categories.Values.Any(c => c.Id != categoryId
                && string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"分类“{name.Trim()}”已存在。");

        category.Rename(name);
    }

    public void DeleteCategory(Guid categoryId)
    {
        var category = GetCategory(categoryId);
        if (category.IsUncategorized)
            throw new InvalidOperationException("内建“未分类”不可删除。");

        var uncategorizedId = Uncategorized.Id;
        var affected = _links.Where(l => l.CategoryId == categoryId).ToList();
        foreach (var link in affected)
        {
            _links.Remove(link);
            if (!_links.Any(l => l.FolderId == link.FolderId && l.CategoryId == uncategorizedId))
            {
                _links.Add(new FolderCategoryLink(
                    link.FolderId,
                    uncategorizedId,
                    NextFolderSortOrder(uncategorizedId)));
            }
        }

        _categories.Remove(categoryId);
        ReindexCategorySortOrders();
    }

    public void ReorderCategories(IReadOnlyList<Guid> orderedIds)
    {
        if (orderedIds.Count != _categories.Count
            || orderedIds.Any(id => !_categories.ContainsKey(id))
            || orderedIds.Distinct().Count() != orderedIds.Count)
            throw new ArgumentException("分类排序列表必须覆盖全部且仅一次。", nameof(orderedIds));

        for (var i = 0; i < orderedIds.Count; i++)
            _categories[orderedIds[i]].SetSortOrder(i);
    }

    public AddFolderResult AddFolder(
        string path,
        string displayName,
        Guid targetCategoryId,
        string? alias = null,
        string? description = null)
    {
        var category = GetCategory(targetCategoryId);
        var normalized = FolderPathComparer.Normalize(path);

        var existing = _folders.Values.FirstOrDefault(f => FolderPathComparer.Equals(f.Path, normalized));
        if (existing is not null)
        {
            if (_links.Any(l => l.FolderId == existing.Id && l.CategoryId == category.Id))
                return AddFolderResult.AlreadyLinked(existing);

            _links.Add(new FolderCategoryLink(existing.Id, category.Id, NextFolderSortOrder(category.Id)));
            return AddFolderResult.LinkedExisting(existing);
        }

        var folder = new FolderRecord(Guid.NewGuid(), normalized, displayName, alias, description);
        _folders.Add(folder.Id, folder);
        _links.Add(new FolderCategoryLink(folder.Id, category.Id, NextFolderSortOrder(category.Id)));
        return AddFolderResult.Created(folder);
    }

    public void UpdateFolder(Guid folderId, string? alias, string? description)
    {
        var folder = GetFolder(folderId);
        folder.SetAlias(alias);
        folder.SetDescription(description);
    }

    public void RetargetFolderPath(Guid folderId, string newPath, string displayName)
    {
        var folder = GetFolder(folderId);
        var normalized = FolderPathComparer.Normalize(newPath);
        if (_folders.Values.Any(f => f.Id != folderId && FolderPathComparer.Equals(f.Path, normalized)))
            throw new InvalidOperationException("目标路径已被其他文件夹记录占用。");

        // 路径未变时只更新显示名，保留异常标记（避免仅改别名/说明/同名路径编辑误清 FR-010 状态）。
        if (FolderPathComparer.Equals(folder.Path, normalized))
        {
            folder.SetDisplayName(displayName);
            return;
        }

        folder.UpdatePath(normalized, displayName);
    }

    /// <summary>
    /// 标记路径不可用（FR-010 / BR-009）。不删除记录或磁盘目录。
    /// </summary>
    public void MarkFolderPathUnavailable(Guid folderId)
        => GetFolder(folderId).MarkPathUnavailable();

    /// <summary>
    /// 清除路径异常标记（路径恢复可访问后，BR-010）。
    /// </summary>
    public void MarkFolderPathAvailable(Guid folderId)
        => GetFolder(folderId).MarkPathAvailable();

    public void DeleteFolder(Guid folderId)
    {
        if (!_folders.ContainsKey(folderId))
            throw new KeyNotFoundException("文件夹记录不存在。");

        _links.RemoveAll(l => l.FolderId == folderId);
        _folders.Remove(folderId);
    }

    /// <summary>
    /// 设置文件夹的完整分类归属（多归属）。空列表时回落到「未分类」。
    /// </summary>
    public void SetFolderCategories(Guid folderId, IReadOnlyList<Guid> categoryIds)
    {
        _ = GetFolder(folderId);
        var distinct = (categoryIds ?? Array.Empty<Guid>()).Distinct().ToList();
        if (distinct.Count == 0)
            distinct.Add(Uncategorized.Id);

        foreach (var categoryId in distinct)
            _ = GetCategory(categoryId);

        _links.RemoveAll(l => l.FolderId == folderId && !distinct.Contains(l.CategoryId));

        foreach (var categoryId in distinct)
        {
            if (_links.Any(l => l.FolderId == folderId && l.CategoryId == categoryId))
                continue;
            _links.Add(new FolderCategoryLink(folderId, categoryId, NextFolderSortOrder(categoryId)));
        }
    }

    public void ReorderFoldersInCategory(Guid categoryId, IReadOnlyList<Guid> orderedFolderIds)
    {
        _ = GetCategory(categoryId);
        var links = _links.Where(l => l.CategoryId == categoryId).ToList();
        if (orderedFolderIds.Count != links.Count
            || orderedFolderIds.Any(id => links.All(l => l.FolderId != id))
            || orderedFolderIds.Distinct().Count() != orderedFolderIds.Count)
            throw new ArgumentException("文件夹排序列表必须覆盖该分类全部且仅一次。", nameof(orderedFolderIds));

        for (var i = 0; i < orderedFolderIds.Count; i++)
            links.Single(l => l.FolderId == orderedFolderIds[i]).SetSortOrder(i);
    }

    public IReadOnlyList<FolderRecord> GetFoldersInCategory(Guid categoryId)
    {
        _ = GetCategory(categoryId);
        return _links
            .Where(l => l.CategoryId == categoryId)
            .OrderBy(l => l.SortOrder)
            .Select(l => _folders[l.FolderId])
            .ToList();
    }

    public IReadOnlyList<Category> GetCategoriesForFolder(Guid folderId)
    {
        _ = GetFolder(folderId);
        return _links
            .Where(l => l.FolderId == folderId)
            .Select(l => _categories[l.CategoryId])
            .OrderBy(c => c.SortOrder)
            .ToList();
    }

    public IReadOnlyList<FolderRecord> Search(string? keyword)
        => FolderSearch.Search(_folders.Values, keyword);

    private Category GetCategory(Guid categoryId)
        => _categories.TryGetValue(categoryId, out var c)
            ? c
            : throw new KeyNotFoundException("分类不存在。");

    private FolderRecord GetFolder(Guid folderId)
        => _folders.TryGetValue(folderId, out var f)
            ? f
            : throw new KeyNotFoundException("文件夹记录不存在。");

    private int NextCategorySortOrder()
        => _categories.Count == 0 ? 0 : _categories.Values.Max(c => c.SortOrder) + 1;

    private int NextFolderSortOrder(Guid categoryId)
    {
        var links = _links.Where(l => l.CategoryId == categoryId).ToList();
        return links.Count == 0 ? 0 : links.Max(l => l.SortOrder) + 1;
    }

    private void ReindexCategorySortOrders()
    {
        var ordered = _categories.Values.OrderBy(c => c.SortOrder).ToList();
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SetSortOrder(i);
    }
}

public sealed class AddFolderResult
{
    public enum Kind
    {
        Created,
        LinkedExisting,
        AlreadyLinked
    }

    public Kind ResultKind { get; }
    public FolderRecord Folder { get; }

    private AddFolderResult(Kind kind, FolderRecord folder)
    {
        ResultKind = kind;
        Folder = folder;
    }

    public static AddFolderResult Created(FolderRecord folder) => new(Kind.Created, folder);
    public static AddFolderResult LinkedExisting(FolderRecord folder) => new(Kind.LinkedExisting, folder);
    public static AddFolderResult AlreadyLinked(FolderRecord folder) => new(Kind.AlreadyLinked, folder);
}
