namespace FolderHub.UseCases.Persistence;

/// <summary>
/// CardHost 唯一写入的卡片展示态（ADR-035）。Main Process 不得通过业务路径改写。
/// </summary>
public interface ICardPresentationStateStore
{
    IReadOnlyList<CardPresentationStateSnapshot> LoadAll();

    CardPresentationStateSnapshot? Load(Guid cardId);

    void Upsert(CardPresentationStateSnapshot state);

    void Delete(Guid cardId);
}

public sealed record CardPresentationStateSnapshot(
    Guid CardId,
    double Left,
    double Top,
    bool Locked,
    string? MonitorReference,
    double? DpiReference,
    long StateVersion,
    DateTimeOffset UpdatedAtUtc);
