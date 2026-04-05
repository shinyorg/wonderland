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
    readonly ILogger<MealTimeJob> logger;
    readonly IMediator mediator;
    readonly INotificationManager notificationManager;
    readonly AppSettings appSettings;
    readonly IOptions<MealTimeOptions> options;
    readonly FakeTimeProvider timeProvider;
    readonly TestableMealTimeJob job;

    public MealTimeJobTests()
    {
        logger = Substitute.For<ILogger<MealTimeJob>>();
        mediator = Substitute.For<IMediator>();
        notificationManager = Substitute.For<INotificationManager>();
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

        job = new TestableMealTimeJob(logger, mediator, notificationManager, appSettings, options, timeProvider);
    }

    [Fact]
    public async Task Handle_GpsEvent_ShouldNotThrow()
    {
        // Arrange
        var gpsEvent = new GpsEvent(new Position(33.8121, -117.9190));
        var context = Substitute.For<IMediatorContext>();

        // Act & Assert
        await Should.NotThrowAsync(() => job.Handle(gpsEvent, context, CancellationToken.None));
    }

    [Fact]
    public void MealTimeJob_ShouldImplementIEventHandler()
    {
        // Assert
        job.ShouldBeAssignableTo<IEventHandler<GpsEvent>>();
    }

    [Fact]
    public async Task Run_ExpiredPass_ShouldSendNotificationAndMarkNotified()
    {
        // Arrange
        var now = timeProvider.GetUtcNow();
        var passes = new List<MealPass>
        {
            new() { Id = 1, Type = MealTimeType.Drink, LastUsed = now.AddMinutes(-20), NotificationSent = false }
        };
        mediator.Request(Arg.Any<GetMealPasses>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(passes));

        // Act
        await job.RunJob(CancellationToken.None);

        // Assert
        await notificationManager.Received(1).Send(Arg.Any<Notification>());
        await mediator.Received(1).Send(
            Arg.Is<MarkPassNotifiedCommand>(x => x.PassId == 1),
            Arg.Any<CancellationToken>(),
            Arg.Any<Action<IMediatorContext>>()
        );
    }

    [Fact]
    public async Task Run_PassOnCooldown_ShouldNotSendNotification()
    {
        // Arrange
        var now = timeProvider.GetUtcNow();
        var passes = new List<MealPass>
        {
            new() { Id = 1, Type = MealTimeType.Drink, LastUsed = now.AddMinutes(-5), NotificationSent = false }
        };
        mediator.Request(Arg.Any<GetMealPasses>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(passes));

        // Act
        await job.RunJob(CancellationToken.None);

        // Assert
        await notificationManager.DidNotReceive().Send(Arg.Any<Notification>());
        await mediator.DidNotReceive().Send(
            Arg.Any<MarkPassNotifiedCommand>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<Action<IMediatorContext>>()
        );
    }

    [Fact]
    public async Task Run_AlreadyNotified_ShouldNotSendDuplicate()
    {
        // Arrange
        var now = timeProvider.GetUtcNow();
        var passes = new List<MealPass>
        {
            new() { Id = 1, Type = MealTimeType.Drink, LastUsed = now.AddMinutes(-20), NotificationSent = true }
        };
        mediator.Request(Arg.Any<GetMealPasses>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(passes));

        // Act
        await job.RunJob(CancellationToken.None);

        // Assert
        await notificationManager.DidNotReceive().Send(Arg.Any<Notification>());
        await mediator.DidNotReceive().Send(
            Arg.Any<MarkPassNotifiedCommand>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<Action<IMediatorContext>>()
        );
    }

    [Fact]
    public async Task Run_Disabled_ShouldSkip()
    {
        // Arrange
        var disabledOptions = Options.Create(new MealTimeOptions
        {
            Enabled = false,
            DrinkTimeWait = TimeSpan.FromMinutes(15),
            FoodTimeWait = TimeSpan.FromMinutes(90)
        });
        var jobDisabled = new TestableMealTimeJob(logger, mediator, notificationManager, appSettings, disabledOptions, timeProvider);

        // Act
        await jobDisabled.RunJob(CancellationToken.None);

        // Assert
        await mediator.DidNotReceive().Request(
            Arg.Any<GetMealPasses>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<Action<IMediatorContext>>()
        );
    }

    [Fact]
    public async Task Run_NotificationsOff_ShouldStillMarkNotifiedButNotSend()
    {
        // Arrange
        appSettings.EnableDrinkNotifications = false;
        var now = timeProvider.GetUtcNow();
        var passes = new List<MealPass>
        {
            new() { Id = 1, Type = MealTimeType.Drink, LastUsed = now.AddMinutes(-20), NotificationSent = false }
        };
        mediator.Request(Arg.Any<GetMealPasses>(), Arg.Any<CancellationToken>(), Arg.Any<Action<IMediatorContext>>())
            .Returns(MediatorTestHelpers.CreateResult(passes));

        // Act
        await job.RunJob(CancellationToken.None);

        // Assert
        await notificationManager.DidNotReceive().Send(Arg.Any<Notification>());
        await mediator.Received(1).Send(
            Arg.Is<MarkPassNotifiedCommand>(x => x.PassId == 1),
            Arg.Any<CancellationToken>(),
            Arg.Any<Action<IMediatorContext>>()
        );
    }
}
