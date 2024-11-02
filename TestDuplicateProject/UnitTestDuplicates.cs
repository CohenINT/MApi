
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
        var path = "/Users/moshecohen/Documents/projects";
        var destPath = "/Users/moshecohen/Documents/projects/Folderindex.json";
        await svc.Init(path);
        // var data = await svc.ExportIndexToJSON();
        // await File.WriteAllTextAsync(destPath, data);
        // var count = svc.fileNames.Values.Count();       
        await svc.ExportIndexToJson(destPath);
        var temp = "";
    }
}       