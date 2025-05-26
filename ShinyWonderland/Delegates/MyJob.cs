using Shiny.Jobs;
using ShinyWonderland.Contracts;
using Notification = Shiny.Notifications.Notification;

namespace ShinyWonderland.Delegates;


public class MyJob(
    ILogger<MyJob> logger,
    CoreServices services
) : Job(logger)
{
    // how old though? - should be a max of 10 mins
    public List<RideTime>? LastSnapshot
    {
        get;
        set
        {
            field = value;
            this.RaisePropertyChanged();
        }
    }
    

    public DateTimeOffset? LastSnapshotTime
    {
        get;
        set => this.Set(ref field, value);
    }
    
    
    protected override async Task Run(CancellationToken cancelToken)
    {
        // TODO: shiny foreground services execute immediately on startup - we don't want that
        this.MinimumTime = TimeSpan.FromMinutes(5); 

        if (!services.AppSettings.EnableNotifications)
        {
            logger.LogInformation("Job notifications is disabled");
            return;
        }
        
        var within = await services.IsUserWithinPark(cancelToken);
        if (!within)
        {
            logger.LogInformation("Outside Wonderland, background job will not run");
            return;
        }
        this.EnsureLastSnapshot();
        var current = await services.Mediator.Request(
            new GetCurrentRideTimes(), 
            cancelToken, 
            ctx => ctx.ForceCacheRefresh()
        );
        await services.Mediator.Publish(new JobDataRefreshEvent(), cancelToken);

        if (this.LastSnapshot != null)
            await IterateDiff(this.LastSnapshot, current.Result);
        
        this.LastSnapshot = current.Result;
        this.LastSnapshotTime = services.TimeProvider.GetUtcNow();
    }


    void EnsureLastSnapshot()
    {
        if (this.LastSnapshotTime == null)
        {
            this.LastSnapshot = null;
        }
        else
        {
            var now = services.TimeProvider.GetUtcNow();
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

    
    async Task IterateDiff(List<RideTime> previous, List<RideTime> current)
    {
        foreach (var ride in previous)
        {
            var currentRide = current.FirstOrDefault(x => x.Id == ride.Id);
            
            if (currentRide is { IsOpen: true } && currentRide.WaitTimeMinutes < ride.WaitTimeMinutes)
            {
                var currentWait = currentRide.WaitTimeMinutes;
                var waitDiff = currentRide.WaitTimeMinutes! - ride.WaitTimeMinutes!;
                
                await services.Notifications.Send(new Notification
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