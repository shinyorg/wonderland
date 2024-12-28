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
        // TODO: compare deltas of data... if there is a ride that is now short, trigger a notification?
            // TODO: may want some configuration on this and may want to check cache context date
        
        // force cache to be up-to-date
        await mediator.GetWonderlandData(true, cancelToken);    
    }
}