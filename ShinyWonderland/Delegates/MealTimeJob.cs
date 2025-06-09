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
        // TODO: if in park
        // gpsManager.IsWithinPark()
        
        // TODO: only send notification for last time and then reset with new drink time
        // could use IDs of meal time historical records
    }
}