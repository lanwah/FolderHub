namespace FolderHub.Domain.Entities;

/// <summary>
/// 平级分类（FR-001 / BR-001）。系统保证存在“未分类”内建分类。
/// </summary>
public sealed class Category
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsUncategorized { get; }

    public Category(Guid id, string name, int sortOrder, bool isUncategorized = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("分类名称不能为空。", nameof(name));

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name.Trim();
        SortOrder = sortOrder;
        IsUncategorized = isUncategorized;
    }

    public static Category CreateUncategorized(int sortOrder = 0)
        => new(Guid.Parse("00000000-0000-0000-0000-000000000001"), "未分类", sortOrder, isUncategorized: true);

    public void Rename(string name)
    {
        if (IsUncategorized)
            throw new InvalidOperationException("内建“未分类”不可重命名。");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("分类名称不能为空。", nameof(name));
        Name = name.Trim();
    }

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
