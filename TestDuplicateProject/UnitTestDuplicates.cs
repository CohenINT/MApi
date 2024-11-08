
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DuplicateFinder.Service;
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

public static class Helpers
{
    public static string Cutoff(this string mainText, string textStart, string textEnd)
    {
        var i = mainText.IndexOf(textStart);
        var j = mainText.IndexOf(textEnd);
        var result = mainText.Substring(i+textStart.Length,j+textEnd.Length);
        return result;
    }
}
[TestClass]
public class UnitTestDuplicates
{
    private IServiceProvider _serviceProvider;
    private  DuplicateFinder.Service.DuplicateFinder svc;


    public UnitTestDuplicates()
    {                               
        this._serviceProvider = TestServiceProvider.BuildServiceProvider();
        this.svc = this._serviceProvider.GetRequiredService<DuplicateFinder.Service.DuplicateFinder>();
    }

   
    [TestMethod]                    
    public async Task TestGetFiles()
    {
       
        var path = "/Users/moshecohen/Documents/Unreal Projects";
        var destPath = "/Users/moshecohen/Documents/test/Folderindex.json";
        await svc.Init(path);
        // var data = await svc.ExportIndexToJSON();
        // await File.WriteAllTextAsync(destPath, data);
        // var count = svc.fileNames.Values.Count();       
        await svc.ExportIndexToJson(destPath);
        var temp = "";
    }
    
  
}       