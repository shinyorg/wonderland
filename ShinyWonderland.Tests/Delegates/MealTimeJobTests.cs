namespace ShinyWonderland.Tests.Delegates;

public class MealTimeJobTests
{
    readonly ILogger<MealTimeJob> logger;
    readonly MealTimeJob job;

    public MealTimeJobTests()
    {
        logger = Substitute.For<ILogger<MealTimeJob>>();
        job = new MealTimeJob(logger);
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
}