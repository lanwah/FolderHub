using FolderHub.Infrastructure.Shell;

namespace FolderHub.Infrastructure.Tests;

public class ShellFolderServiceTests
{
    [Fact]
    public void IsDirectoryAccessible_True_ForExistingTempDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "FolderHubShellTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var shell = new WindowsShellFolderService();
            Assert.True(shell.IsDirectoryAccessible(root));
            Assert.True(shell.IsDirectoryAccessible(root + Path.DirectorySeparatorChar));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void IsDirectoryAccessible_False_ForMissingOrEmptyPath()
    {
        var shell = new WindowsShellFolderService();
        Assert.False(shell.IsDirectoryAccessible(string.Empty));
        Assert.False(shell.IsDirectoryAccessible("   "));
        Assert.False(shell.IsDirectoryAccessible(
            Path.Combine(Path.GetTempPath(), "FolderHub_Missing_" + Guid.NewGuid().ToString("N"))));
    }

    [Fact]
    public void IsDirectoryAccessible_False_ForFilePath()
    {
        var file = Path.Combine(Path.GetTempPath(), "FolderHubShellFile_" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(file, "x");
        try
        {
            var shell = new WindowsShellFolderService();
            Assert.False(shell.IsDirectoryAccessible(file));
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void TryOpenInExplorer_Fails_WhenPathUnavailable()
    {
        var shell = new WindowsShellFolderService();
        var missing = Path.Combine(Path.GetTempPath(), "FolderHub_MissingOpen_" + Guid.NewGuid().ToString("N"));
        Assert.False(shell.TryOpenInExplorer(missing, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}
