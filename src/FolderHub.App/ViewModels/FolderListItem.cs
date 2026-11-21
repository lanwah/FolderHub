namespace FolderHub.App.ViewModels;

public sealed class FolderListItem : ObservableObject
{
    private bool _isSelected;

    public FolderListItem(
        Guid id,
        string title,
        string subtitle,
        string description,
        string path,
        string categoriesLabel,
        bool isPathUnavailable = false)
    {
        Id = id;
        Title = title;
        Subtitle = subtitle;
        Description = description;
        Path = path;
        CategoriesLabel = categoriesLabel;
        IsPathUnavailable = isPathUnavailable;
    }

    public Guid Id { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string Description { get; }
    public string Path { get; }
    public string CategoriesLabel { get; }
    public bool IsPathUnavailable { get; }
    public string PathStatusLabel => IsPathUnavailable ? "路径异常 · 请重新指定" : Path;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
