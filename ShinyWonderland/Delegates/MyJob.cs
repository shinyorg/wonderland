using Shiny.Jobs;
using Shiny.Notifications;

namespace ShinyWonderland.Delegates;


public class MyJob(
    ILogger<MyJob> logger,
    IMediator mediator,
    INotificationManager notifications
) : Job(logger)
{
    protected override async Task Run(CancellationToken cancelToken)
    {
        // keep offline up to date? 
        await mediator.GetWonderlandData(cancelToken);    
    }
}