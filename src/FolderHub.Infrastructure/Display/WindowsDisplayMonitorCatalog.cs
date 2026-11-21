using System.Runtime.InteropServices;
using FolderHub.UseCases.Display;

namespace FolderHub.Infrastructure.Display;

/// <summary>
/// 基于 EnumDisplayMonitors / GetMonitorInfo / GetDpiForMonitor 的工作区目录（DIP）。
/// </summary>
public sealed class WindowsDisplayMonitorCatalog : IDisplayMonitorCatalog
{
    private const int MdTDefaultToNearest = 2;

    public string? PrimaryMonitorId { get; private set; }

    public IReadOnlyList<DisplayWorkArea> GetWorkAreas()
    {
        var list = new List<DisplayWorkArea>();
        string? primaryId = null;

        bool Callback(IntPtr hMonitor, IntPtr hdc, ref NativeRect rect, IntPtr data)
        {
            var info = new MonitorInfoEx
            {
                cbSize = (uint)Marshal.SizeOf<MonitorInfoEx>()
            };
            if (!GetMonitorInfo(hMonitor, ref info))
                return true;

            var dpi = TryGetDpi(hMonitor);
            var scale = dpi / 96.0;
            if (scale <= 0)
                scale = 1.0;

            var work = info.rcWork;
            var area = new DisplayWorkArea(
                MonitorId: info.szDevice.TrimEnd('\0'),
                Left: work.Left / scale,
                Top: work.Top / scale,
                Width: (work.Right - work.Left) / scale,
                Height: (work.Bottom - work.Top) / scale,
                DpiScale: scale);

            if ((info.dwFlags & 1) != 0) // MONITORINFOF_PRIMARY
            {
                primaryId = area.MonitorId;
                list.Insert(0, area);
            }
            else
            {
                list.Add(area);
            }

            return true;
        }

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callback, IntPtr.Zero);
        PrimaryMonitorId = primaryId ?? list.FirstOrDefault()?.MonitorId;

        if (list.Count == 0)
        {
            // 极端回退：单屏虚拟桌面。
            list.Add(new DisplayWorkArea(
                MonitorId: "FALLBACK",
                Left: 0,
                Top: 0,
                Width: 1920,
                Height: 1080,
                DpiScale: 1.0));
            PrimaryMonitorId = "FALLBACK";
        }

        return list;
    }

    private static double TryGetDpi(IntPtr hMonitor)
    {
        try
        {
            if (GetDpiForMonitor(hMonitor, MdTDefaultToNearest, out var dpiX, out _) == 0)
                return dpiX;
        }
        catch (EntryPointNotFoundException)
        {
            // Server 2012 / 旧系统可能无 shcore GetDpiForMonitor。
        }
        catch (DllNotFoundException)
        {
        }

        return 96;
    }

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref NativeRect lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hMonitor, int dpiType, out uint dpiX, out uint dpiY);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MonitorInfoEx
    {
        public uint cbSize;
        public NativeRect rcMonitor;
        public NativeRect rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }
}
