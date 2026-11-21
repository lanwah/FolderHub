namespace FolderHub.UseCases.Display;

/// <summary>
/// 单台显示器工作区（DIP / 逻辑像素），供 ADR-040 位置捕获与恢复使用。
/// </summary>
public sealed record DisplayWorkArea(
    string MonitorId,
    double Left,
    double Top,
    double Width,
    double Height,
    double DpiScale)
{
    public bool ContainsPoint(double x, double y)
        => x >= Left && y >= Top && x < Left + Width && y < Top + Height;

    public bool FullyContains(double left, double top, double width, double height)
        => left >= Left
           && top >= Top
           && left + width <= Left + Width
           && top + height <= Top + Height;
}

/// <summary>
/// 持久化用：显示器身份 + 工作区归一化坐标 + DPI 参考（ADR-040）。
/// NormX/NormY 相对工作区左上角，通常约在 [0,1]。
/// </summary>
public sealed record NormalizedWindowPlacement(
    string MonitorId,
    double NormX,
    double NormY,
    double? DpiReference);

/// <summary>
/// 恢复结果：DIP 绝对坐标。
/// </summary>
public sealed record AbsoluteWindowPlacement(
    double Left,
    double Top,
    string MonitorId,
    double DpiScale,
    bool MigratedToPrimary);
