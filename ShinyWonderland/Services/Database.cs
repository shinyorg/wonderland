using Shiny.SqliteDocumentDb;

namespace ShinyWonderland;

public enum MealTimeType
{
    Drink = 1,
    Food = 2
}

public class RideHistoryRecord
{
    public int Id { get; set; }
    public string RideId { get; set; }
    public string RideName { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class MealTimeHistoryRecord
{
    public int Id { get; set; }
    public MealTimeType Type { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class LastRideTime
{
    public string RideId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class MealTimeValue
{
    public MealTimeType Type { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public interface IDataService
{
    Task AddRideHistory(RideHistoryRecord record);
    Task AddMealTimeHistory(MealTimeHistoryRecord record);
    Task<List<MealTimeHistoryRecord>> GetMealTimeHistory();
    Task<List<RideHistoryRecord>> GetRideTimeHistory();
    Task<List<LastRideTime>> GetLastRideTimes();
    Task<List<MealTimeValue>> GetLatestMealTimes();
}

public class DataService(IDocumentStore store) : IDataService
{
    public Task AddRideHistory(RideHistoryRecord record)
    {
        store.Set(record);
        return Task.CompletedTask;
    }

    public Task AddMealTimeHistory(MealTimeHistoryRecord record)
    {
        store.Set(record);
        return Task.CompletedTask;
    }

    public async Task<List<RideHistoryRecord>> GetRideTimeHistory()
    {
        var results = await store.Query<RideHistoryRecord>()
            .OrderByDescending(x => x.Timestamp)
            .ToList();
        return results.ToList();
    }

    public async Task<List<MealTimeHistoryRecord>> GetMealTimeHistory()
    {
        var results = await store.Query<MealTimeHistoryRecord>()
            .OrderByDescending(x => x.Timestamp)
            .ToList();
        return results.ToList();
    }

    public async Task<List<LastRideTime>> GetLastRideTimes()
    {
        var all = await store.Query<RideHistoryRecord>().ToList();
        return all
            .GroupBy(x => x.RideId)
            .Select(g => new LastRideTime
            {
                RideId = g.Key,
                Timestamp = g.Max(x => x.Timestamp)
            })
            .ToList();
    }

    public async Task<List<MealTimeValue>> GetLatestMealTimes()
    {
        var all = await store.Query<MealTimeHistoryRecord>().ToList();
        return all
            .GroupBy(x => x.Type)
            .Select(g => new MealTimeValue
            {
                Type = g.Key,
                Timestamp = g.Max(x => x.Timestamp)
            })
            .ToList();
    }
}

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "wonderland.db"
        );
        services.AddSqliteDocumentStore($"Data Source={dbPath}");
        services.AddSingleton<IDataService, DataService>();
        return services;
    }
}
