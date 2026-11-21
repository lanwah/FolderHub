namespace FolderHub.UseCases.Display;

/// <summary>
/// ADR-040 / ADR-033：显示器身份 + 工作区归一化坐标 + DPI 换算；
/// 原显示器不可用或窗口全部/部分移出可见桌面时，迁移到主显示器可见位置。
/// </summary>
public static class DisplayPlacement
{
    /// <summary>主显示器上默认边距（DIP），迁移时避免贴边。</summary>
    public const double PrimaryFallbackMargin = 24;

    public static NormalizedWindowPlacement Capture(
        double left,
        double top,
        double width,
        double height,
        IReadOnlyList<DisplayWorkArea> monitors)
    {
        ArgumentNullException.ThrowIfNull(monitors);
        if (monitors.Count == 0)
            throw new ArgumentException("至少需要一台显示器。", nameof(monitors));

        var anchorX = left + Math.Max(8, width * 0.1);
        var anchorY = top + Math.Max(8, height * 0.1);
        var monitor = FindContaining(monitors, anchorX, anchorY)
                      ?? FindNearest(monitors, left, top)
                      ?? monitors[0];

        var normX = monitor.Width > 0 ? (left - monitor.Left) / monitor.Width : 0;
        var normY = monitor.Height > 0 ? (top - monitor.Top) / monitor.Height : 0;
        return new NormalizedWindowPlacement(monitor.MonitorId, normX, normY, monitor.DpiScale);
    }

    /// <summary>
    /// 从归一化持久化态恢复。MonitorId 为空时按 legacy 绝对坐标解释 NormX/NormY。
    /// </summary>
    public static AbsoluteWindowPlacement Restore(
        string? monitorId,
        double storedLeftOrNormX,
        double storedTopOrNormY,
        double windowWidth,
        double windowHeight,
        double? dpiReference,
        IReadOnlyList<DisplayWorkArea> monitors,
        string? primaryMonitorId = null)
    {
        ArgumentNullException.ThrowIfNull(monitors);
        if (monitors.Count == 0)
            throw new ArgumentException("至少需要一台显示器。", nameof(monitors));

        var primary = ResolvePrimary(monitors, primaryMonitorId);

        if (string.IsNullOrWhiteSpace(monitorId))
        {
            // Legacy：Left/Top 为绝对 DIP。
            return RestoreAbsolute(
                storedLeftOrNormX,
                storedTopOrNormY,
                windowWidth,
                windowHeight,
                monitors,
                primary);
        }

        var target = monitors.FirstOrDefault(m =>
            string.Equals(m.MonitorId, monitorId, StringComparison.OrdinalIgnoreCase));

        if (target is null)
            return PlaceOnPrimary(primary, windowWidth, windowHeight, migrated: true);

        var left = target.Left + storedLeftOrNormX * target.Width;
        var top = target.Top + storedTopOrNormY * target.Height;

        // DPI 参考：归一化坐标已相对工作区；若 DPI 变化极大，仍以当前工作区为准。
        // 保留 dpiReference 供调用方记录，本算法不另做像素级缩放。
        _ = dpiReference;

        if (!IsFullyOnVirtualDesktop(left, top, windowWidth, windowHeight, monitors))
            return PlaceOnPrimary(primary, windowWidth, windowHeight, migrated: true);

        return new AbsoluteWindowPlacement(left, top, target.MonitorId, target.DpiScale, MigratedToPrimary: false);
    }

    public static AbsoluteWindowPlacement RestoreAbsolute(
        double left,
        double top,
        double windowWidth,
        double windowHeight,
        IReadOnlyList<DisplayWorkArea> monitors,
        DisplayWorkArea? primary = null)
    {
        ArgumentNullException.ThrowIfNull(monitors);
        if (monitors.Count == 0)
            throw new ArgumentException("至少需要一台显示器。", nameof(monitors));

        primary ??= ResolvePrimary(monitors, primaryMonitorId: null);

        if (!IsFullyOnVirtualDesktop(left, top, windowWidth, windowHeight, monitors))
            return PlaceOnPrimary(primary, windowWidth, windowHeight, migrated: true);

        var monitor = FindContaining(monitors, left + 8, top + 8)
                      ?? FindNearest(monitors, left, top)
                      ?? primary;

        return new AbsoluteWindowPlacement(left, top, monitor.MonitorId, monitor.DpiScale, MigratedToPrimary: false);
    }

    public static bool IsFullyOnVirtualDesktop(
        double left,
        double top,
        double width,
        double height,
        IReadOnlyList<DisplayWorkArea> monitors)
    {
        if (monitors.Count == 0 || width <= 0 || height <= 0)
            return false;

        // 四角均落在某一工作区内 → 视为完全可见（允许跨屏但每角都在某屏上）。
        // ADR-033：部分移出可见桌面也要迁移；因此要求四角都在并集内。
        return PointOnAny(left, top, monitors)
               && PointOnAny(left + width, top, monitors)
               && PointOnAny(left, top + height, monitors)
               && PointOnAny(left + width, top + height, monitors);
    }

    private static AbsoluteWindowPlacement PlaceOnPrimary(
        DisplayWorkArea primary,
        double windowWidth,
        double windowHeight,
        bool migrated)
    {
        var left = primary.Left + PrimaryFallbackMargin;
        var top = primary.Top + PrimaryFallbackMargin;
        var maxLeft = primary.Left + Math.Max(PrimaryFallbackMargin, primary.Width - windowWidth - PrimaryFallbackMargin);
        var maxTop = primary.Top + Math.Max(PrimaryFallbackMargin, primary.Height - windowHeight - PrimaryFallbackMargin);
        left = Math.Clamp(left, primary.Left + PrimaryFallbackMargin, maxLeft);
        top = Math.Clamp(top, primary.Top + PrimaryFallbackMargin, maxTop);
        return new AbsoluteWindowPlacement(left, top, primary.MonitorId, primary.DpiScale, migrated);
    }

    private static DisplayWorkArea ResolvePrimary(IReadOnlyList<DisplayWorkArea> monitors, string? primaryMonitorId)
    {
        if (!string.IsNullOrWhiteSpace(primaryMonitorId))
        {
            var match = monitors.FirstOrDefault(m =>
                string.Equals(m.MonitorId, primaryMonitorId, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                return match;
        }

        return monitors[0];
    }

    private static bool PointOnAny(double x, double y, IReadOnlyList<DisplayWorkArea> monitors)
        => monitors.Any(m => m.ContainsPoint(x, y));

    private static DisplayWorkArea? FindContaining(IReadOnlyList<DisplayWorkArea> monitors, double x, double y)
        => monitors.FirstOrDefault(m => m.ContainsPoint(x, y));

    private static DisplayWorkArea? FindNearest(IReadOnlyList<DisplayWorkArea> monitors, double x, double y)
    {
        DisplayWorkArea? best = null;
        var bestDist = double.MaxValue;
        foreach (var m in monitors)
        {
            var cx = Math.Clamp(x, m.Left, m.Left + m.Width);
            var cy = Math.Clamp(y, m.Top, m.Top + m.Height);
            var dx = x - cx;
            var dy = y - cy;
            var dist = dx * dx + dy * dy;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = m;
            }
        }

        return best;
    }
}
