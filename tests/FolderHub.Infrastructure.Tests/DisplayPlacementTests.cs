using FolderHub.UseCases.Display;

namespace FolderHub.Infrastructure.Tests;

public sealed class DisplayPlacementTests
{
    private static readonly DisplayWorkArea Primary = new(
        @"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1040, DpiScale: 1.0);

    private static readonly DisplayWorkArea Secondary = new(
        @"\\.\DISPLAY2", Left: 1920, Top: 0, Width: 1600, Height: 900, DpiScale: 1.25);

    private static IReadOnlyList<DisplayWorkArea> Dual => [Primary, Secondary];

    [Fact]
    public void Capture_UsesContainingMonitor_AndNormalizes()
    {
        var captured = DisplayPlacement.Capture(
            left: 1920 + 160,
            top: 90,
            width: 200,
            height: 120,
            Dual);

        Assert.Equal(Secondary.MonitorId, captured.MonitorId);
        Assert.Equal(160 / 1600.0, captured.NormX, 5);
        Assert.Equal(90 / 900.0, captured.NormY, 5);
        Assert.Equal(1.25, captured.DpiReference);
    }

    [Fact]
    public void Restore_SameMonitor_KeepsRelativePosition()
    {
        var captured = DisplayPlacement.Capture(200, 100, 400, 300, Dual);
        var restored = DisplayPlacement.Restore(
            captured.MonitorId,
            captured.NormX,
            captured.NormY,
            windowWidth: 400,
            windowHeight: 300,
            captured.DpiReference,
            Dual,
            Primary.MonitorId);

        Assert.False(restored.MigratedToPrimary);
        Assert.Equal(Primary.MonitorId, restored.MonitorId);
        Assert.Equal(200, restored.Left, 3);
        Assert.Equal(100, restored.Top, 3);
    }

    [Fact]
    public void Restore_MissingMonitor_MigratesToPrimary()
    {
        var restored = DisplayPlacement.Restore(
            @"\\.\DISPLAY_GONE",
            storedLeftOrNormX: 0.5,
            storedTopOrNormY: 0.5,
            windowWidth: 200,
            windowHeight: 100,
            dpiReference: 1.0,
            monitors: [Primary],
            primaryMonitorId: Primary.MonitorId);

        Assert.True(restored.MigratedToPrimary);
        Assert.Equal(Primary.MonitorId, restored.MonitorId);
        Assert.Equal(DisplayPlacement.PrimaryFallbackMargin, restored.Left);
        Assert.Equal(DisplayPlacement.PrimaryFallbackMargin, restored.Top);
    }

    [Fact]
    public void Restore_PartiallyOffDesktop_MigratesToPrimary()
    {
        // 归一化会把窗口放到工作区外（极大 Norm）。
        var restored = DisplayPlacement.Restore(
            Primary.MonitorId,
            storedLeftOrNormX: 1.5,
            storedTopOrNormY: 1.5,
            windowWidth: 400,
            windowHeight: 300,
            dpiReference: 1.0,
            Dual,
            Primary.MonitorId);

        Assert.True(restored.MigratedToPrimary);
        Assert.Equal(Primary.MonitorId, restored.MonitorId);
    }

    [Fact]
    public void Restore_LegacyAbsolute_NullMonitor_UsesAbsoluteCoords()
    {
        var restored = DisplayPlacement.Restore(
            monitorId: null,
            storedLeftOrNormX: 2000,
            storedTopOrNormY: 40,
            windowWidth: 180,
            windowHeight: 100,
            dpiReference: null,
            Dual,
            Primary.MonitorId);

        Assert.False(restored.MigratedToPrimary);
        Assert.Equal(Secondary.MonitorId, restored.MonitorId);
        Assert.Equal(2000, restored.Left);
        Assert.Equal(40, restored.Top);
    }

    [Fact]
    public void Restore_LegacyAbsolute_Offscreen_Migrates()
    {
        var restored = DisplayPlacement.RestoreAbsolute(
            left: -800,
            top: -600,
            windowWidth: 200,
            windowHeight: 100,
            Dual,
            Primary);

        Assert.True(restored.MigratedToPrimary);
        Assert.Equal(Primary.MonitorId, restored.MonitorId);
    }

    [Fact]
    public void IsFullyOnVirtualDesktop_RequiresAllCorners()
    {
        Assert.True(DisplayPlacement.IsFullyOnVirtualDesktop(10, 10, 100, 80, Dual));
        Assert.False(DisplayPlacement.IsFullyOnVirtualDesktop(-50, 10, 100, 80, Dual));
        Assert.False(DisplayPlacement.IsFullyOnVirtualDesktop(4000, 10, 100, 80, Dual));
    }
}
