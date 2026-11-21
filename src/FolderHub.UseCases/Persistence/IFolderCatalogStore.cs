using FolderHub.Domain.Catalog;

namespace FolderHub.UseCases.Persistence;

/// <summary>
/// Main Process 权威目录聚合的持久化端口（ADR-035 / ADR-039）。
/// </summary>
public interface IFolderCatalogStore
{
    FolderCatalog Load();

    void Save(FolderCatalog catalog);
}
