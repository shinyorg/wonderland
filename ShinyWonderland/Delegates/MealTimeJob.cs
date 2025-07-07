using Shiny.Jobs;
using Shiny.Notifications;
using ShinyWonderland.Contracts;

namespace ShinyWonderland.Delegates;


public class MealTimeJob(
    ILogger<MealTimeJob> logger,
    IMediator mediator,
    INotificationManager notificationManager,
    AppSettings appSettings
) : Job(logger), IEventHandler<GpsEvent>
{
    protected override async Task Run(CancellationToken cancelToken)
    {
        // TODO: only send notification for last time and then reset with new drink time
        // could use IDs of meal time historical records
    }

    public async Task Handle(GpsEvent @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        // TODO: if in park
    }
}