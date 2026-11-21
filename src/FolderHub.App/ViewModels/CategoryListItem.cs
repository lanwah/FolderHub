namespace FolderHub.App.ViewModels;

public sealed class CategoryListItem : ObservableObject
{
    private bool _isSelected;
    private int _count;

    public CategoryListItem(Guid? categoryId, string name, bool isVirtualAll = false)
    {
        CategoryId = categoryId;
        Name = name;
        IsVirtualAll = isVirtualAll;
    }

    /// <summary>null 表示虚拟「全部文件夹」。</summary>
    public Guid? CategoryId { get; }

    public string Name { get; }

    public bool IsVirtualAll { get; }

    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
