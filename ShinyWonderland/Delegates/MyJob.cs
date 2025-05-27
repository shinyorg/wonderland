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
        if (!services.AppSettings.EnableNotifications)
        {
            logger.LogInformation("Job notifications is disabled");
            return;
        }
        
        if (!this.IsTimeToRun())
            return;

        if (!await this.IsInPark(cancelToken))
            return;

        // if (this.LastRunTime)
        // TODO: if the last snapshot is null, we'll let the mainpage pick it up first
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


    internal void EnsureLastSnapshot()
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

    
    internal async Task IterateDiff(List<RideTime> previous, List<RideTime> current)
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


    internal bool IsTimeToRun()
    {
        if (this.LastSnapshotTime == null)
            return true;

        var ts = this.LastSnapshotTime.Value.Subtract(services.TimeProvider.GetUtcNow());
        logger.LogInformation("Job last ran {mins} mins ago", ts.TotalMinutes);
        return ts.TotalMinutes >= 5;
    }

    
    internal async Task<bool> IsInPark(CancellationToken cancellationToken)
    {
        try
        {
            var within = await services.IsUserWithinPark(cancellationToken);
            logger.LogInformation("User is near/within park: {flag}", within);
            if (!within)
            {
                logger.LogInformation("Outside Wonderland, background job will not run");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not get GPS coordinates in job");
            return false;
        }
    }
}