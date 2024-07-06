using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DuplicateIndexer;

public class MainClass
{
    private IServiceProvider Services { get; set; }
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
        Services = BuildServiceProvider();
    }
    public static void Main()
    {
        
    }
}