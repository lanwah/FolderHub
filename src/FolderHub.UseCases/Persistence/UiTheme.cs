namespace FolderHub.UseCases.Persistence;

/// <summary>界面主题标识（与 design-demo data-theme 对齐）。</summary>
public static class UiTheme
{
    public const string Light = "light";
    public const string Dark = "dark";

    public static string Normalize(string? value)
        => string.Equals(value, Dark, StringComparison.OrdinalIgnoreCase) ? Dark : Light;

    public static bool IsDark(string? value)
        => Normalize(value) == Dark;
}
