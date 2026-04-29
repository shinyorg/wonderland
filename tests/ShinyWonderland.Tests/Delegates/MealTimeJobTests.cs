using Shiny.Notifications;
using ShinyWonderland.Contracts;
using ShinyWonderland.Features.MealTimes.Handlers;

namespace ShinyWonderland.Tests.Delegates;

public class TestableMealTimeJob(
    ILogger<MealTimeJob> logger,
    IMediator mediator,
    INotificationManager notificationManager,
    AppSettings appSettings,
    IOptions<MealTimeOptions> options,
    TimeProvider timeProvider
) : MealTimeJob(logger, mediator, notificationManager, appSettings, options, timeProvider)
{
    public Task RunJob(CancellationToken ct) => this.Run(ct);
}

public class MealTimeJobTests
{
    readonly TestMediator mediator;
    readonly INotificationManagerImposter notifications;
    readonly AppSettings appSettings;
    readonly IOptions<MealTimeOptions> options;
    readonly FakeTimeProvider timeProvider;
    readonly TestableMealTimeJob job;

    public MealTimeJobTests()
    {
        mediator = new TestMediator();
        notifications = new INotificationManagerImposter();
        appSettings = new AppSettings
        {
            EnableDrinkNotifications = true,
            EnableMealNotifications = true
        };
        options = Options.Create(new MealTimeOptions
        {
            Enabled = true,
            DrinkTimeWait = TimeSpan.FromMinutes(15),
            FoodTimeWait = TimeSpan.FromMinutes(90)
        });
        timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        job = new TestableMealTimeJob(
            new ILoggerImposter<MealTimeJob>().Instance(),
            mediator,
            notifications.Instance(),
            appSettings,
            options,
            timeProvider
        );
    }

    [Test]
    public async Task Run_ExpiredPass_ShouldSendNotificationAndMarkNotified()
    {
        var now = timeProvider.GetUtcNow();
        var passes = new List<MealPass>
        {
            new() { Id = 1, Type = MealTimeType.Drink, LastUsed = now.AddMinutes(-20), NotificationSent = false }
        };
        mediator.SetupRequest<GetMealPasses, List<MealPass>>(passes);

        await job.RunJob(CancellationToken.None);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Once());
        await Assert.That(mediator.SentCommands.OfType<MarkPassNotifiedCommand>())
            .Contains(x => x.PassId == 1);
    }

    [Test]
    public async Task Run_PassOnCooldown_ShouldNotSendNotification()
    {
        var now = timeProvider.GetUtcNow();
        var passes = new List<MealPass>
        {
            new() { Id = 1, Type = MealTimeType.Drink, LastUsed = now.AddMinutes(-5), NotificationSent = false }
        };
        mediator.SetupRequest<GetMealPasses, List<MealPass>>(passes);

        await job.RunJob(CancellationToken.None);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Never());
        await Assert.That(mediator.SentCommands.OfType<MarkPassNotifiedCommand>()).IsEmpty();
    }

    [Test]
    public async Task Run_AlreadyNotified_ShouldNotSendDuplicate()
    {
        var now = timeProvider.GetUtcNow();
        var passes = new List<MealPass>
        {
            new() { Id = 1, Type = MealTimeType.Drink, LastUsed = now.AddMinutes(-20), NotificationSent = true }
        };
        mediator.SetupRequest<GetMealPasses, List<MealPass>>(passes);

        await job.RunJob(CancellationToken.None);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Never());
        await Assert.That(mediator.SentCommands.OfType<MarkPassNotifiedCommand>()).IsEmpty();
    }

    [Test]
    public async Task Run_Disabled_ShouldSkip()
    {
        var disabledOptions = Options.Create(new MealTimeOptions
        {
            Enabled = false,
            DrinkTimeWait = TimeSpan.FromMinutes(15),
            FoodTimeWait = TimeSpan.FromMinutes(90)
        });
        var jobDisabled = new TestableMealTimeJob(
            new ILoggerImposter<MealTimeJob>().Instance(),
            mediator,
            notifications.Instance(),
            appSettings,
            disabledOptions,
            timeProvider
        );

        await jobDisabled.RunJob(CancellationToken.None);

        await Assert.That(mediator.Requests.OfType<GetMealPasses>()).IsEmpty();
    }

    [Test]
    public async Task Run_NotificationsOff_ShouldStillMarkNotifiedButNotSend()
    {
        appSettings.EnableDrinkNotifications = false;
        var now = timeProvider.GetUtcNow();
        var passes = new List<MealPass>
        {
            new() { Id = 1, Type = MealTimeType.Drink, LastUsed = now.AddMinutes(-20), NotificationSent = false }
        };
        mediator.SetupRequest<GetMealPasses, List<MealPass>>(passes);

        await job.RunJob(CancellationToken.None);

        notifications.Send(Arg<Notification>.Any()).Called(Count.Never());
        await Assert.That(mediator.SentCommands.OfType<MarkPassNotifiedCommand>())
            .Contains(x => x.PassId == 1);
    }
}
