using Shiny.Jobs;
using Shiny.Notifications;
using ShinyWonderland.Contracts;
using ShinyWonderland.Features.MealTimes.Handlers;

namespace ShinyWonderland.Features.MealTimes.Delegates;


public class MealTimeJob(
    ILogger<MealTimeJob> logger,
    IMediator mediator,
    INotificationManager notificationManager,
    AppSettings appSettings,
    IOptions<MealTimeOptions> options,
    TimeProvider timeProvider
) : Job(logger)
{
    protected override async Task Run(CancellationToken cancelToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("MealTime feature is disabled");
            return;
        }

        var passes = (await mediator.Request(new GetMealPasses(), cancelToken)).Result;
        var now = timeProvider.GetUtcNow();

        foreach (var pass in passes)
        {
            if (pass.LastUsed == null || pass.NotificationSent)
                continue;

            var waitTime = pass.Type == MealTimeType.Drink
                ? options.Value.DrinkTimeWait
                : options.Value.FoodTimeWait;

            if (pass.LastUsed.Value.Add(waitTime) > now)
                continue;

            var shouldNotify = pass.Type == MealTimeType.Drink
                ? appSettings.EnableDrinkNotifications
                : appSettings.EnableMealNotifications;

            if (shouldNotify)
            {
                var typeLabel = pass.Type == MealTimeType.Drink ? "Drink" : "Food";
                await notificationManager.Send(new Notification
                {
                    Title = $"{typeLabel} Pass Available",
                    Message = $"Your {typeLabel.ToLower()} pass is ready to use!"
                });
            }

            await mediator.Send(new MarkPassNotifiedCommand(pass.Id), cancelToken);
        }
    }
}
