using FolderHub.Domain.Catalog;
using FolderHub.Domain.Services;

namespace FolderHub.Domain.Tests;

public class FolderCatalogTests
{
    [Fact]
    public void CreateEmpty_HasUncategorized()
    {
        var catalog = FolderCatalog.CreateEmpty();
        Assert.Single(catalog.Categories);
        Assert.True(catalog.Uncategorized.IsUncategorized);
        Assert.Equal("未分类", catalog.Uncategorized.Name);
    }

    [Fact]
    public void AddFolder_CreatesRecord_AndLinksCategory()
    {
        var catalog = FolderCatalog.CreateEmpty();
        var work = catalog.AddCategory("工作");

        var result = catalog.AddFolder(@"D:\Projects\A", "A", work.Id, alias: "项目A");

        Assert.Equal(AddFolderResult.Kind.Created, result.ResultKind);
        Assert.Equal("项目A", result.Folder.ListTitle);
        Assert.Single(catalog.GetFoldersInCategory(work.Id));
    }

    [Fact]
    public void AddFolder_SamePath_LinksExisting_WithoutDuplicate()
    {
        var catalog = FolderCatalog.CreateEmpty();
        var work = catalog.AddCategory("工作");
        var personal = catalog.AddCategory("个人");

        catalog.AddFolder(@"D:\Projects\A\", "A", work.Id);
        var second = catalog.AddFolder(@"d:\projects\a", "A", personal.Id);

        Assert.Equal(AddFolderResult.Kind.LinkedExisting, second.ResultKind);
        Assert.Single(catalog.Folders);
        Assert.Equal(2, catalog.GetCategoriesForFolder(second.Folder.Id).Count);
    }

    [Fact]
    public void AddFolder_AlreadyLinked_DoesNotDuplicateLink()
    {
        var catalog = FolderCatalog.CreateEmpty();
        var work = catalog.AddCategory("工作");
        catalog.AddFolder(@"D:\Projects\A", "A", work.Id);

        var again = catalog.AddFolder(@"D:\Projects\A", "A", work.Id);

        Assert.Equal(AddFolderResult.Kind.AlreadyLinked, again.ResultKind);
        Assert.Single(catalog.GetFoldersInCategory(work.Id));
    }

    [Fact]
    public void DeleteCategory_MovesOrphansToUncategorized()
    {
        var catalog = FolderCatalog.CreateEmpty();
        var work = catalog.AddCategory("工作");
        var created = catalog.AddFolder(@"D:\Projects\A", "A", work.Id).Folder;

        catalog.DeleteCategory(work.Id);

        Assert.DoesNotContain(catalog.Categories, c => c.Id == work.Id);
        Assert.Contains(catalog.GetFoldersInCategory(catalog.Uncategorized.Id), f => f.Id == created.Id);
    }

    [Fact]
    public void ReorderFoldersInCategory_IsIndependentPerCategory()
    {
        var catalog = FolderCatalog.CreateEmpty();
        var work = catalog.AddCategory("工作");
        var personal = catalog.AddCategory("个人");
        var a = catalog.AddFolder(@"D:\A", "A", work.Id).Folder;
        var b = catalog.AddFolder(@"D:\B", "B", work.Id).Folder;
        catalog.AddFolder(@"D:\A", "A", personal.Id);
        catalog.AddFolder(@"D:\B", "B", personal.Id);

        catalog.ReorderFoldersInCategory(work.Id, [b.Id, a.Id]);

        Assert.Equal(["B", "A"], catalog.GetFoldersInCategory(work.Id).Select(f => f.DisplayName));
        Assert.Equal(["A", "B"], catalog.GetFoldersInCategory(personal.Id).Select(f => f.DisplayName));
    }

    [Fact]
    public void Search_MatchesAlias_AndDedupesAcrossCategories()
    {
        var catalog = FolderCatalog.CreateEmpty();
        var work = catalog.AddCategory("工作");
        var personal = catalog.AddCategory("个人");
        var folder = catalog.AddFolder(@"D:\Projects\A", "Alpha", work.Id, alias: "代号").Folder;
        catalog.AddFolder(@"D:\Projects\A", "Alpha", personal.Id);

        var hits = catalog.Search("代号");

        Assert.Single(hits);
        Assert.Equal(folder.Id, hits[0].Id);
    }

    [Fact]
    public void FolderPathComparer_NormalizesTrailingSlash_AndCase()
    {
        Assert.True(FolderPathComparer.Equals(@"D:\Projects\A\", @"d:\projects\a"));
        Assert.Equal(@"D:\Projects\A", FolderPathComparer.Normalize(@"D:\Projects\A\"));
    }

    [Fact]
    public void SetFolderCategories_ReplacesMembership_EmptyFallsBackToUncategorized()
    {
        var catalog = FolderCatalog.CreateEmpty();
        var work = catalog.AddCategory("工作");
        var personal = catalog.AddCategory("个人");
        var folder = catalog.AddFolder(@"D:\Projects\A", "A", work.Id).Folder;

        catalog.SetFolderCategories(folder.Id, [personal.Id]);
        Assert.Equal([personal.Id], catalog.GetCategoriesForFolder(folder.Id).Select(c => c.Id));

        catalog.SetFolderCategories(folder.Id, []);
        Assert.Equal([catalog.Uncategorized.Id], catalog.GetCategoriesForFolder(folder.Id).Select(c => c.Id));
    }
}
