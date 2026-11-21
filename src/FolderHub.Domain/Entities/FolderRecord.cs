namespace FolderHub.Domain.Entities;

/// <summary>
/// 文件夹记录（FR-002/006/008/010/018）。同一规范化路径全局唯一。
/// </summary>
public sealed class FolderRecord
{
    public Guid Id { get; }
    public string Path { get; private set; }
    public string DisplayName { get; private set; }
    public string? Alias { get; private set; }
    public string? Description { get; private set; }
    public bool IsPathUnavailable { get; private set; }

    public FolderRecord(
        Guid id,
        string path,
        string displayName,
        string? alias = null,
        string? description = null,
        bool isPathUnavailable = false)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("文件夹路径不能为空。", nameof(path));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("文件夹显示名不能为空。", nameof(displayName));

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Path = path;
        DisplayName = displayName.Trim();
        Alias = NormalizeOptionalText(alias);
        Description = NormalizeOptionalText(description);
        IsPathUnavailable = isPathUnavailable;
    }

    public string ListTitle => string.IsNullOrEmpty(Alias) ? DisplayName : Alias!;

    public void SetAlias(string? alias) => Alias = NormalizeOptionalText(alias);

    public void SetDescription(string? description) => Description = NormalizeOptionalText(description);

    public void UpdatePath(string path, string displayName)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("文件夹路径不能为空。", nameof(path));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("文件夹显示名不能为空。", nameof(displayName));

        Path = path;
        DisplayName = displayName.Trim();
        IsPathUnavailable = false;
    }

    public void MarkPathUnavailable() => IsPathUnavailable = true;

    public void MarkPathAvailable() => IsPathUnavailable = false;

    private static string? NormalizeOptionalText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
