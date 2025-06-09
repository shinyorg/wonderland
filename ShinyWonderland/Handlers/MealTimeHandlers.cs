using Shiny.Notifications;
using SQLite;

namespace ShinyWonderland.Handlers;


[SingletonHandler]
public class MealTimeHandlers : ICommandHandler<AddMealTime>, IRequestHandler<GetMealTimeHistory, List<MealTimeHistoryRecord>>
{
    readonly SQLiteAsyncConnection data;
    readonly TimeProvider timeProvider;
    readonly INotificationManager notifications;

    readonly MealTimeOptions options;
    // TODO: save last meal time/drink time here - return calc later

    public MealTimeHandlers(
        SQLiteAsyncConnection data, 
        TimeProvider timeProvider,
        INotificationManager notifications,
        IOptions<MealTimeOptions> options
    )
    {
        this.data = data;
        this.timeProvider = timeProvider;
        this.notifications = notifications;
        this.options = options.Value;
        this.data.GetConnection().CreateTable<MealTimeHistoryRecord>();
    }
    

    public Task Handle(AddMealTime command, IMediatorContext context, CancellationToken cancellationToken)
    {
        return this.data.InsertAsync(new MealTimeHistoryRecord
        {
            Timestamp = this.timeProvider.GetUtcNow()
        });
    }
    

    public async Task<List<MealTimeHistoryRecord>> Handle(GetMealTimeHistory request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var result = await this.data
            .Table<MealTimeHistoryRecord>()
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
}


public record AddMealTime : ICommand; // TODO: drink or food

public record GetMealTimeHistory : IRequest<List<MealTimeHistoryRecord>>;

public class MealTimeHistoryRecord
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}