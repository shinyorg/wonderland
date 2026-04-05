namespace ShinyWonderland.Handlers;


public record AddMealPassCommand(MealTimeType Type) : ICommand;
public record RemoveMealPassCommand(int PassId) : ICommand;
public record UseMealPassCommand(MealTimeType Type) : ICommand;
public record MarkPassNotifiedCommand(int PassId) : ICommand;
public record GetMealPasses : IRequest<List<MealPass>>;

public record AddMealTime(MealTimeType Type) : ICommand;
public record GetMealTimeAvailability : IRequest<MealTimeAvailability>;
public record GetMealTimeHistory : IRequest<List<MealTimeHistoryRecord>>;

public record MealPassAvailability(int PassId, MealTimeType Type, DateTimeOffset? LastUsed, TimeSpan? AvailableIn, bool IsAvailable);

public record MealTimeAvailability(
    List<MealPassAvailability> DrinkPasses,
    List<MealPassAvailability> FoodPasses,
    int DrinkPassCount,
    int FoodPassCount,
    TimeSpan? NextDrinkAvailableIn,
    TimeSpan? NextFoodAvailableIn
)
{
    public bool IsDrinkAvailable => DrinkPasses.Any(x => x.IsAvailable);
    public bool IsFoodAvailable => FoodPasses.Any(x => x.IsAvailable);
    public bool HasDrinkPasses => DrinkPassCount > 0;
    public bool HasFoodPasses => FoodPassCount > 0;
}


[MediatorSingleton]
public class MealTimeHandlers(
    IDataService data,
    TimeProvider timeProvider,
    IOptions<MealTimeOptions> options
) :
    ICommandHandler<AddMealTime>,
    ICommandHandler<AddMealPassCommand>,
    ICommandHandler<RemoveMealPassCommand>,
    ICommandHandler<UseMealPassCommand>,
    ICommandHandler<MarkPassNotifiedCommand>,
    IRequestHandler<GetMealPasses, List<MealPass>>,
    IRequestHandler<GetMealTimeHistory, List<MealTimeHistoryRecord>>,
    IRequestHandler<GetMealTimeAvailability, MealTimeAvailability>
{
    public Task Handle(AddMealTime command, IMediatorContext context, CancellationToken cancellationToken)
        => data.AddMealTimeHistory(new MealTimeHistoryRecord
        {
            Timestamp = timeProvider.GetUtcNow(),
            Type = command.Type
        });

    public Task Handle(AddMealPassCommand command, IMediatorContext context, CancellationToken cancellationToken)
        => data.AddMealPass(new MealPass { Type = command.Type });

    public async Task Handle(RemoveMealPassCommand command, IMediatorContext context, CancellationToken cancellationToken)
    {
        await data.DeleteMealPass(command.PassId);
    }

    public async Task Handle(UseMealPassCommand command, IMediatorContext context, CancellationToken cancellationToken)
    {
        var passes = await data.GetMealPasses();
        var waitTime = command.Type == MealTimeType.Drink
            ? options.Value.DrinkTimeWait
            : options.Value.FoodTimeWait;
        var now = timeProvider.GetUtcNow();

        var available = passes
            .Where(x => x.Type == command.Type)
            .FirstOrDefault(x => x.LastUsed == null || x.LastUsed.Value.Add(waitTime) <= now);

        if (available != null)
        {
            available.LastUsed = now;
            available.NotificationSent = false;
            await data.UpdateMealPass(available);

            await data.AddMealTimeHistory(new MealTimeHistoryRecord
            {
                Timestamp = now,
                Type = command.Type
            });
        }
    }

    public async Task Handle(MarkPassNotifiedCommand command, IMediatorContext context, CancellationToken cancellationToken)
    {
        var passes = await data.GetMealPasses();
        var pass = passes.FirstOrDefault(x => x.Id == command.PassId);
        if (pass != null)
        {
            pass.NotificationSent = true;
            await data.UpdateMealPass(pass);
        }
    }

    public async Task<List<MealPass>> Handle(GetMealPasses request, IMediatorContext context, CancellationToken cancellationToken)
        => await data.GetMealPasses();

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
        var passes = await data.GetMealPasses();
        var now = timeProvider.GetUtcNow();

        var drinkPasses = BuildPassAvailability(passes, MealTimeType.Drink, options.Value.DrinkTimeWait, now);
        var foodPasses = BuildPassAvailability(passes, MealTimeType.Food, options.Value.FoodTimeWait, now);

        return new MealTimeAvailability(
            drinkPasses,
            foodPasses,
            drinkPasses.Count,
            foodPasses.Count,
            CalcNextAvailableIn(drinkPasses),
            CalcNextAvailableIn(foodPasses)
        );
    }

    static List<MealPassAvailability> BuildPassAvailability(List<MealPass> passes, MealTimeType type, TimeSpan waitTime, DateTimeOffset now)
    {
        return passes
            .Where(x => x.Type == type)
            .Select(x =>
            {
                var isAvailable = x.LastUsed == null || x.LastUsed.Value.Add(waitTime) <= now;
                TimeSpan? availableIn = null;
                if (!isAvailable && x.LastUsed != null)
                    availableIn = x.LastUsed.Value.Add(waitTime) - now;

                return new MealPassAvailability(x.Id, x.Type, x.LastUsed, availableIn, isAvailable);
            })
            .ToList();
    }

    static TimeSpan? CalcNextAvailableIn(List<MealPassAvailability> passes)
    {
        if (passes.Count == 0)
            return null;
        if (passes.Any(x => x.IsAvailable))
            return null;
        return passes.Where(x => x.AvailableIn != null).Min(x => x.AvailableIn);
    }
}
