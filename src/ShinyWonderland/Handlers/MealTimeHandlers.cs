namespace ShinyWonderland.Handlers;


[MediatorSingleton]
public class MealTimeHandlers(
    IDataService data,
    TimeProvider timeProvider,
    IOptions<MealTimeOptions> options
) : 
    ICommandHandler<AddMealTime>, 
    IRequestHandler<GetMealTimeHistory, List<MealTimeHistoryRecord>>,
    IRequestHandler<GetMealTimeAvailability, MealTimeAvailability>
{
    public Task Handle(AddMealTime command, IMediatorContext context, CancellationToken cancellationToken)
        => data.AddMealTimeHistory(new MealTimeHistoryRecord
        {
            Timestamp = timeProvider.GetUtcNow(),
            Type = command.Type
        });
    

    public async Task<List<MealTimeHistoryRecord>> Handle(GetMealTimeHistory request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var result = await data.GetMealTimeHistory();

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
        var results = await data.GetLatestMealTimes();
        
        var food = results.FirstOrDefault(x => x.Type == MealTimeType.Food)?.Timestamp;
        var foodNext = this.CalcNextTime(food, options.Value.FoodTimeWait);

        var drink = results.FirstOrDefault(x => x.Type == MealTimeType.Drink)?.Timestamp;
        var drinkNext = this.CalcNextTime(drink, options.Value.DrinkTimeWait);

        return new MealTimeAvailability(
            food, foodNext, drink, drinkNext
        );
    }


    TimeSpan? CalcNextTime(DateTimeOffset? last, TimeSpan waitTime)
    {
        TimeSpan? result = null;
        if (last != null)
        {
            var now = timeProvider.GetUtcNow();
            var dt = last.Value.Add(waitTime);
            if (dt > now)
                result = now.Subtract(dt);
        }

        return result;
    }
}


public record AddMealTime(MealTimeType Type) : ICommand;

public record GetMealTimeAvailability : IRequest<MealTimeAvailability>;

public record GetMealTimeHistory : IRequest<List<MealTimeHistoryRecord>>;
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
