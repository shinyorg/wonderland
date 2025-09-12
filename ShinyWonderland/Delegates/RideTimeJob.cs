using Shiny.Jobs;
using ShinyWonderland.Contracts;
using Notification = Shiny.Notifications.Notification;

namespace ShinyWonderland.Delegates;


public class RideTimeJob(
    ILogger<RideTimeJob> logger,
    IOptions<ParkOptions> parkOptions,
    RideTimeJobLocalized localized,
    CoreServices services
) : Job(logger)
{
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
        if (!services.AppSettings.EnableTimeRideNotifications)
        {
            logger.LogInformation("Job notifications is disabled");
            return;
        }
        
        if (!this.IsTimeToRun())
            return;

        if (!await this.IsInPark(cancelToken))
            return;

        this.EnsureLastSnapshot();
        var current = await GetCurrentData(cancelToken).ConfigureAwait(false);

        if (this.LastSnapshot != null)
            await IterateDiff(this.LastSnapshot, current);
        
        this.LastSnapshot = current;
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
                    Title = $"{parkOptions.Value.Name} {localized.RideTime}",
                    Message = localized.NotificationMessageFormatFormat(ride.Name, currentWait, waitDiff)
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


    internal async Task<List<RideTime>> GetCurrentData(CancellationToken cancellationToken)
    {
        var current = await services.Mediator.Request(
            new GetCurrentRideTimes(), 
            cancellationToken
        );
        var list = current.Result;
        var fireEvent = true;
        var cacheInfo = current.Context.Cache();

        if (cacheInfo == null)
        {
            logger.LogInformation("Data is fresh and not from cache");
        }
        else
        {
            fireEvent = false;
            
            var age = services.TimeProvider.GetUtcNow().Subtract(cacheInfo.Timestamp);
            if (age.TotalMinutes >= 5)
            {
                logger.LogInformation("Cache data is too old - {age}", age);
                current = await services.Mediator.Request(
                    new GetCurrentRideTimes(), 
                    cancellationToken,
                    ctx => ctx.ForceCacheRefresh()
                );
                list = current.Result;
            }
        }

        if (fireEvent)
        {
            logger.LogInformation("Firing data refresh event");
            await services.Mediator.Publish(new JobDataRefreshEvent(), cancellationToken);
        }

        return list;
    }
}