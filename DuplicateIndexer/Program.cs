using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DuplicateIndexer;

public class MainClass
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

    public MainClass()
    {
    }

    // /Users/moshecohen/Projects/MApi/DuplicateIndexer/bin/Release/net7.0/publish
    public static async Task Main(string[] args)
    {
        var services = BuildServiceProvider();
        var duplicateFinderSvc = services.GetRequiredService<DuplicateFinder.Service.DuplicateFinder>();
        var readpath = args.FirstOrDefault("");
        var savejsonpath = args.LastOrDefault("");
        if (readpath == savejsonpath)
            savejsonpath = "";

        Console.WriteLine($"Readpath: {readpath}");
        Console.WriteLine($"SaveJsonpPaTH: {savejsonpath}");
        var data = await duplicateFinderSvc.ExportIndexToJson();
        if (!string.IsNullOrEmpty(savejsonpath))
            await File.WriteAllTextAsync(savejsonpath, data);
    }
}