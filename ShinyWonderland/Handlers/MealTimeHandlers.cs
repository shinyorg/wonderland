using SQLite;

namespace ShinyWonderland.Handlers;


[MediatorSingleton]
public class MealTimeHandlers : 
    ICommandHandler<AddMealTime>, 
    IRequestHandler<GetMealTimeHistory, List<MealTimeHistoryRecord>>,
    IRequestHandler<GetMealTimeAvailability, MealTimeAvailability>
{
    readonly SQLiteAsyncConnection data;
    readonly TimeProvider timeProvider;
    readonly MealTimeOptions options;

    public MealTimeHandlers(
        SQLiteAsyncConnection data, 
        TimeProvider timeProvider,
        IOptions<MealTimeOptions> options
    )
    {
        this.data = data;
        this.timeProvider = timeProvider;
        this.options = options.Value;
        this.data.GetConnection().CreateTable<MealTimeHistoryRecord>();
    }
    
    
    public Task Handle(AddMealTime command, IMediatorContext context, CancellationToken cancellationToken)
    {
        return this.data.InsertAsync(new MealTimeHistoryRecord
        {
            Timestamp = this.timeProvider.GetUtcNow(),
            Type = command.Type
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

    public async Task<MealTimeAvailability> Handle(GetMealTimeAvailability request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var results = await this.data.QueryAsync<MealTimeValue>(
            """
            SELECT
                Type,
                MAX(Timestamp)
            FROM 
                MealTimeHistoryRecord
            GROUP BY
                Type
            """
        );
        
        var food = results.FirstOrDefault(x => x.Type == MealTimeType.Food)?.Timestamp;
        var foodNext = this.CalcNextTime(food, this.options.FoodTimeWait);

        var drink = results.FirstOrDefault(x => x.Type == MealTimeType.Drink)?.Timestamp;
        var drinkNext = this.CalcNextTime(drink, this.options.DrinkTimeWait);

        return new MealTimeAvailability(
            food, foodNext, drink, drinkNext
        );
    }


    TimeSpan? CalcNextTime(DateTimeOffset? last, TimeSpan waitTime)
    {
        TimeSpan? result = null;
        if (last != null)
        {
            var now = this.timeProvider.GetUtcNow();
            var dt = last.Value.Add(waitTime);
            if (dt > now)
                result = now.Subtract(dt);
        }

        return result;
    }
}


public record AddMealTime(MealTimeType Type) : ICommand;

public record GetMealTimeAvailability : IRequest<MealTimeAvailability>;

class MealTimeValue
{
    public MealTimeType Type { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}
public record MealTimeAvailability(
    DateTimeOffset? LastDrink,
    TimeSpan? DrinkAvailableIn,
    DateTimeOffset? LastFood,
    TimeSpan? FoodAvailableIn
)
{
    public bool IsFoodAvailable => this.FoodAvailableIn == null;
    public bool IsDrinkAvailable => this.DrinkAvailableIn == null;
}

public enum MealTimeType
{
    Drink = 1,
    Food = 2
}

public record GetMealTimeHistory : IRequest<List<MealTimeHistoryRecord>>;

public class MealTimeHistoryRecord
{
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }
    public MealTimeType Type { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}