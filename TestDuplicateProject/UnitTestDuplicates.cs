
using Microsoft.Extensions.DependencyInjection;
 
namespace TestDuplicateProject;

public static class TestServiceProvider
{
    public static IServiceProvider BuildServiceProvider(Action<IServiceCollection> configureServices = null)
    {
        var services = new ServiceCollection();

        services.AddSingleton<DuplicateFinder.Service.DuplicateFinder>();
        // Allow custom configuration
        configureServices?.Invoke(services);

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
        var path = "";
        await svc.GetFilesAsync(path);
    }
}