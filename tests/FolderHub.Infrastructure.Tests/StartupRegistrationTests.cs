using FolderHub.Infrastructure.Shell;
using Microsoft.Win32;

namespace FolderHub.Infrastructure.Tests;

public sealed class StartupRegistrationTests
{
    [Fact]
    public void Apply_EnableThenDisable_RoundTripsUnderHkcuTestKey()
    {
        var testRoot = $@"Software\FolderHub\TestStartup\{Guid.NewGuid():N}";
        var runKeyPath = $@"{testRoot}\Run";
        var valueName = "FolderHubTest";
        var command = "\"C:\\Tools\\FolderHub.App.exe\"";

        try
        {
            var service = new WindowsStartupRegistrationService(
                commandLineFactory: () => command,
                valueName: valueName,
                runKeyPath: runKeyPath);

            Assert.False(service.IsRegistered());

            service.Apply(enabled: true);
            Assert.True(service.IsRegistered());

            using (var key = Registry.CurrentUser.OpenSubKey(runKeyPath, writable: false))
            {
                Assert.NotNull(key);
                Assert.Equal(command, key.GetValue(valueName) as string);
            }

            service.Apply(enabled: false);
            Assert.False(service.IsRegistered());
        }
        finally
        {
            Registry.CurrentUser.DeleteSubKeyTree(testRoot, throwOnMissingSubKey: false);
        }
    }

    [Fact]
    public void Apply_Enable_IsIdempotent()
    {
        var testRoot = $@"Software\FolderHub\TestStartup\{Guid.NewGuid():N}";
        var runKeyPath = $@"{testRoot}\Run";
        var valueName = "FolderHubTest";
        var command = "\"D:\\Apps\\FolderHub.exe\"";

        try
        {
            var service = new WindowsStartupRegistrationService(
                () => command,
                valueName,
                runKeyPath);

            service.Apply(true);
            service.Apply(true);
            Assert.True(service.IsRegistered());

            service.Apply(false);
            service.Apply(false);
            Assert.False(service.IsRegistered());
        }
        finally
        {
            Registry.CurrentUser.DeleteSubKeyTree(testRoot, throwOnMissingSubKey: false);
        }
    }
}
