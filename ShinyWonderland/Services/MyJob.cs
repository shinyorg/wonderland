using System.Text;
using Shiny.Jobs;
using Shiny.Notifications;
using ShinyWonderland.ThemeParksApi;
using Notification = Shiny.Notifications.Notification;

namespace ShinyWonderland.Services;


public class MyJob(
    ILogger<MyJob> logger,
    IOptions<ParkOptions> parkOptions,
    IGpsManager gpsManager,
    TimeProvider timeProvider,
    AppSettings appSettings,
    IMediator mediator,
    INotificationManager notifications
) : Job(logger)
{
    // how old though? - should be a max of 10 mins
    public EntityLiveDataResponse? LastSnapshot
    {
        get;
        set => this.Set(ref field, value);
    }
    

    public DateTimeOffset? LastSnapshotTime
    {
        get;
        set => this.Set(ref field, value);
    }
    
    
    protected override async Task Run(CancellationToken cancelToken)
    {
        // TODO: this should only run if inside the park
        
        this.MinimumTime = TimeSpan.FromMinutes(3); // this only matters when the GPS is running
        if (!appSettings.EnableNotifications)
            return;

        var within = await gpsManager.IsWithinPark(parkOptions.Value);
        if (!within)
        {
            logger.LogInformation("Outside Wonderland, background job will not run");
            return;
        }
        this.EnsureLastSnapshot();
        var current = await mediator.GetWonderlandData(true, cancelToken);
        await mediator.Publish(new JobDataRefreshEvent(), cancelToken);

        if (this.LastSnapshot != null)
            await IterateDiff(this.LastSnapshot, current.Result);

        this.LastSnapshot = current.Result;
        this.LastSnapshotTime = timeProvider.GetUtcNow();
    }


    void EnsureLastSnapshot()
    {
        if (this.LastSnapshotTime == null)
        {
            this.LastSnapshot = null;
        }
        else
        {
            var now = timeProvider.GetUtcNow();
            var diff = now.Subtract(this.LastSnapshotTime.Value);
            if (diff.TotalMinutes <= 30)
            {
                logger.LogDebug("Snapshot is good at {mins}", diff.TotalMinutes);
            }
            else
            {
                logger.LogDebug("Snapshot is too old at {mins}", diff.TotalMinutes);
                this.LastSnapshotTime = null;
                this.LastSnapshot = null;
            }
        }
    }

    
    async Task IterateDiff(EntityLiveDataResponse previous, EntityLiveDataResponse current)
    {
        foreach (var ride in previous.LiveData)
        {
            var currentRide = current.LiveData.FirstOrDefault(x => x.Id == ride.Id);
            if (currentRide is { Status: LiveStatusType.OPERATING } && currentRide.Queue.Standby.WaitTime < ride.Queue.Standby.WaitTime)
            {
                var currentWait = currentRide.Queue.Standby.WaitTime;
                var waitDiff = currentRide.Queue.Standby.WaitTime - ride.Queue.Standby.WaitTime;
                
                await notifications.Send(new Notification
                {
                    // TODO: would be nice if I could set the ID to the ride entity ID to prevent overlaps in notifications
                    // Id = ride.Id - w
                    Title = "Wonderland Ride Time",
                    Message = $"{ride.Name} is now a {currentWait} minute wait.  Down {waitDiff} minutes"
                });
            }
        }
    }
}