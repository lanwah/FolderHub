using FolderHub.UseCases.Ipc;
using FolderHub.UseCases.Persistence;

namespace FolderHub.CardHost.Services;

public sealed class CardModel
{
    public Guid CardId { get; init; }
    public Guid FolderId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public string? Description { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool IsPathUnavailable { get; set; }
    public double Left { get; set; }
    public double Top { get; set; }
    public bool Locked { get; set; }
    public long StateVersion { get; set; }

    /// <summary>恢复后需按 ADR-040 重新落盘归一化坐标。</summary>
    public bool NeedsPlacementPersist { get; set; }

    public string Title => string.IsNullOrWhiteSpace(Alias) ? DisplayName : Alias!;
    public string Hint => Locked ? "位置已锁定" : "拖动卡片可调整位置";

    public static CardModel FromUpsert(CardDefinitionUpsertPayload payload, CardPresentationStateSnapshot? state, int index)
    {
        // 位置由 CardHostController.ApplyRestoredPlacement 统一按 ADR-040 恢复。
        _ = index;
        return new CardModel
        {
            CardId = payload.CardId,
            FolderId = payload.FolderId,
            DisplayName = payload.DisplayName,
            Alias = payload.Alias,
            Description = payload.Description,
            Path = payload.Path,
            IsPathUnavailable = payload.IsPathUnavailable,
            Left = 0,
            Top = 0,
            Locked = state?.Locked ?? false,
            StateVersion = state?.StateVersion ?? 0
        };
    }
}
