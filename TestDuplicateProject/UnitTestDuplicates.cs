
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace TestDuplicateProject;

public static class TestServiceProvider
{
    public static IServiceProvider BuildServiceProvider(Action<IServiceCollection> configureServices = null)
    {
        var services = new ServiceCollection();
         
        services.AddSingleton<DuplicateFinder.Service.DuplicateFinder>();
        services.AddLogging(b =>
        {
            b.AddConsole();
            b.AddDebug();
        });

        return services.BuildServiceProvider();
    }
}

[TestClass]
public class UnitTestDuplicates
{
    private IServiceProvider _serviceProvider;

    public UnitTestDuplicates()
    {
        this._serviceProvider = TestServiceProvider.BuildServiceProvider();
    }
    [TestMethod]
    public async Task TestGetFiles()
    {
        var svc = new DuplicateFinder.Service.DuplicateFinder(_serviceProvider);
        var path = "/Users/moshecohen/Documents/Unreal Projects/BotArena";//"/Users/moshecohen/test";
        var destPath = "/Users/moshecohen/Documents/Unreal Projects/BotArena/BotArenaIndex.json";
        await svc.IndexAllFilesAsync(path);
        var data = svc.ExportIndexToJSON();
        await File.WriteAllTextAsync(destPath, data);
        var temp = "";
    }
}