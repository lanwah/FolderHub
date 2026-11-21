namespace FolderHub.UseCases.Display;

/// <summary>
/// 当前会话可见显示器工作区目录（平台适配实现）。
/// </summary>
public interface IDisplayMonitorCatalog
{
    /// <summary>按稳定优先顺序返回工作区；索引 0 视为主显示器。</summary>
    IReadOnlyList<DisplayWorkArea> GetWorkAreas();

    string? PrimaryMonitorId { get; }
}
