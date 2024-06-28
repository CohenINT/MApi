using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DuplicateFinder.Service;

public class DuplicateFinder
{
    public ILogger<DuplicateFinder> log { set; get; }
    public IServiceProvider services { get; set; }

    public DuplicateFinder(IServiceProvider svc)
    {
        this.services = svc;
        this.log = this.services.GetRequiredService<ILogger<DuplicateFinder>>();
        
    }

    public Task<bool> IndexAllFilesAsync(params string[] pathToFolder)
    {
        
    }
}