using Shiny.Jobs;
using Shiny.Notifications;

namespace ShinyWonderland.Delegates;

public class MealTimeJob(
    ILogger<MealTimeJob> logger,
    IMediator mediator,
    INotificationManager notificationManager,
    AppSettings appSettings
) : Job(logger)
{
    protected override async Task Run(CancellationToken cancelToken)
    {
        
    }
}