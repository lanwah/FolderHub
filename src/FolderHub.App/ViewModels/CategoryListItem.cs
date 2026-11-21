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
        Icon = ResolveIcon(name, isVirtualAll);
    }

    /// <summary>null 表示虚拟「全部文件夹」。</summary>
    public Guid? CategoryId { get; }

    public string Name { get; }

    public string Icon { get; }

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

    private static string ResolveIcon(string name, bool isVirtualAll)
    {
        if (isVirtualAll || name == "全部文件夹")
            return "▦";
        return name switch
        {
            "工作" => "▣",
            "项目" => "◈",
            "个人" => "♡",
            "未分类" => "□",
            _ => "◇"
        };
    }
}
