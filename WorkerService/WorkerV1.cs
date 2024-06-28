using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WorkerService;

public class WorkerV1
{
    private IServiceProvider services { set; get; }

    public WorkerV1(IServiceProvider services)
    {
        this.services = services;
        this.services.GetRequiredService<ILogger<WorkerV1>>();
    }
    
    
}