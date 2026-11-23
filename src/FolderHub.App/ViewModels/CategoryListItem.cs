using System.Windows.Media;
using FolderHub.App.Services;
using FolderHub.UseCases.Icons;

namespace FolderHub.App.ViewModels;

public sealed class CategoryListItem : ObservableObject
{
    private bool _isSelected;
    private int _count;

    public CategoryListItem(Guid? categoryId, string name, bool isVirtualAll = false, string? iconFileName = null)
    {
        CategoryId = categoryId;
        Name = name;
        IsVirtualAll = isVirtualAll;
        IconFileName = AppIconCatalog.ResolveCategoryIcon(iconFileName, name, isVirtualAll);
        IconGlyph = PackagedIconCatalog.GetGlyph(IconFileName);
        IconImage = AppIconCatalog.TryLoadImage(IconFileName);
    }

    /// <summary>null 表示虚拟「全部文件夹」。</summary>
    public Guid? CategoryId { get; }

    public string Name { get; }

    public string IconFileName { get; }

    public string IconGlyph { get; }

    public ImageSource? IconImage { get; }

    public bool HasIconImage => IconImage is not null;

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
