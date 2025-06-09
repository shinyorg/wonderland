using SQLite;

namespace ShinyWonderland.Handlers;


[SingletonHandler]
public class RideHistoryHandlers : 
    ICommandHandler<AddRideCommand>, 
    IRequestHandler<GetRideHistory, List<RideHistoryRecord>>, 
    IRequestHandler<GetParkLastRiddenTimes, List<LastRideTime>>
{
    readonly SQLiteAsyncConnection data;
    readonly TimeProvider timeProvider;
    
    
    public RideHistoryHandlers(SQLiteAsyncConnection data, TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
        this.data = data;
        this.data.GetConnection().CreateTable<RideHistoryRecord>();
    }

    
    public Task Handle(AddRideCommand command, IMediatorContext context, CancellationToken cancellationToken)
        => this.data.InsertAsync(new RideHistoryRecord
        {
            RideName = command.RideName,
            Timestamp = this.timeProvider.GetUtcNow()
        });


    public async Task<List<RideHistoryRecord>> Handle(GetRideHistory request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var result = await this.data
            .Table<RideHistoryRecord>()
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

        return result
            .Select(x =>
            {
                x.Timestamp = x.Timestamp.LocalDateTime;
                return x;
            })
            .ToList();
    }

    
    public Task<List<LastRideTime>> Handle(GetParkLastRiddenTimes request, IMediatorContext context, CancellationToken cancellationToken)
        => this.data.QueryAsync<LastRideTime>(
            """
            SELECT 
                RideId, 
                MAX(Timestamp) AS LatestTimestamp
            FROM 
                RideHistoryRecord
            GROUP 
                BY RideId;
            """
        );
}

public record AddRideCommand(string RideId, string RideName) : ICommand;
public record GetRideHistory : IRequest<List<RideHistoryRecord>>;
public record GetParkLastRiddenTimes : IRequest<List<LastRideTime>>;

public class LastRideTime
{
    public string RideId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    
}

public class RideHistoryRecord
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public string RideId { get; set; }
    public string RideName { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}