using Shiny.Jobs;
using Shiny.Notifications;

namespace ShinyWonderland.Delegates;


public class MyJob(
    ILogger<MyJob> logger,
    INotificationManager notifications
) : Job(logger)
{
    protected override Task Run(CancellationToken cancelToken)
    {
        // TODO: if I have a watch for changing times - pull and create notification is condition is met
        return Task.CompletedTask;
    }
}