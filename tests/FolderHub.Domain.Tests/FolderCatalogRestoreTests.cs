using FolderHub.Domain.Catalog;
using FolderHub.Domain.Entities;

namespace FolderHub.Domain.Tests;

public class FolderCatalogRestoreTests
{
    [Fact]
    public void Restore_RequiresExactlyOneUncategorized()
    {
        Assert.Throws<InvalidOperationException>(() =>
            FolderCatalog.Restore(
                [new Category(Guid.NewGuid(), "工作", 0)],
                [],
                []));
    }

    [Fact]
    public void Restore_RebuildsLinksAndFolders()
    {
        var unc = Category.CreateUncategorized();
        var work = new Category(Guid.NewGuid(), "工作", 1);
        var folder = new FolderRecord(Guid.NewGuid(), @"D:\A", "A", "别名");
        var link = new FolderCategoryLink(folder.Id, work.Id, 0);

        var catalog = FolderCatalog.Restore([unc, work], [folder], [link]);

        Assert.Equal(2, catalog.Categories.Count);
        Assert.Equal("别名", catalog.GetFoldersInCategory(work.Id).Single().ListTitle);
    }
}
