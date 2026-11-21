namespace FolderHub.UseCases.Persistence;

/// <summary>
/// Main Process 权威全局/主组件设置（ADR-021 / ADR-035）。
/// </summary>
public interface IMainSettingsStore
{
    MainSettingsSnapshot Load();

    void Save(MainSettingsSnapshot settings);
}

public sealed record MainSettingsSnapshot(
    bool KeepCardsAfterMainExit,
    bool MainPinned,
    double MainLeft,
    double MainTop,
    double MainWidth,
    double MainHeight,
    bool MainLocked,
    double MainOpacity,
    bool StartupEnabled,
    string? MainMonitorReference = null,
    double? MainDpiReference = null);
