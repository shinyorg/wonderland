using SQLite;

namespace ShinyWonderland.Handlers;


[SingletonHandler]
public class RideHistoryHandlers : ICommandHandler<DidRideCommand>, IRequestHandler<GetRideHistory, List<RideHistoryRecord>>
{
    readonly SQLiteAsyncConnection data;
    readonly TimeProvider timeProvider;
    
    
    public RideHistoryHandlers(SQLiteAsyncConnection data, TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
        this.data = data;
        this.data.GetConnection().CreateTable<RideHistoryRecord>();
    }

    
    public Task Handle(DidRideCommand command, IMediatorContext context, CancellationToken cancellationToken)
        => this.data.InsertAsync(new RideHistoryRecord
        {
            RideName = command.RideName,
            Timestamp = this.timeProvider.GetUtcNow()
        });

    
    public Task<List<RideHistoryRecord>> Handle(GetRideHistory request, IMediatorContext context, CancellationToken cancellationToken)
        => this.data
            .Table<RideHistoryRecord>()
            .OrderBy(x => x.Timestamp)
            .ToListAsync();
}

public record DidRideCommand(string RideName) : ICommand;
public record GetRideHistory : IRequest<List<RideHistoryRecord>>;

public class RideHistoryRecord
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public string RideName { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}