namespace FolderHub.Domain.Entities;

/// <summary>
/// 文件夹在某一分类内的归属与独立排序（FR-003/005）。
/// </summary>
public sealed class FolderCategoryLink
{
    public Guid FolderId { get; }
    public Guid CategoryId { get; }
    public int SortOrder { get; private set; }

    public FolderCategoryLink(Guid folderId, Guid categoryId, int sortOrder)
    {
        if (folderId == Guid.Empty) throw new ArgumentException("FolderId 无效。", nameof(folderId));
        if (categoryId == Guid.Empty) throw new ArgumentException("CategoryId 无效。", nameof(categoryId));

        FolderId = folderId;
        CategoryId = categoryId;
        SortOrder = sortOrder;
    }

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
