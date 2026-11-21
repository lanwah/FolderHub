namespace FolderHub.UseCases.Persistence;

/// <summary>
/// Main Process 权威卡片定义（ADR-035；CardHost 不得写入）。
/// </summary>
public interface ICardDefinitionStore
{
    IReadOnlyList<CardDefinitionSnapshot> LoadAll();

    void ReplaceAll(IEnumerable<CardDefinitionSnapshot> definitions);
}

public sealed record CardDefinitionSnapshot(
    Guid CardId,
    Guid FolderId,
    bool Enabled);
