using RoomSharp.Abstraction;
using RoomSharp.Attributes;
using RoomSharp.Converters;
using RoomSharp.Core;
using RoomSharp.DependencyInjection;

namespace ShinyWonderland.Services;

public static class Database
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddRoomSharpDatabase<AppDatabaseImpl>(ctx =>
        {
            ctx.UseSqlite("app.db");
    
            // ctx.Builder.SetVersion(1).AddMigrations(new InitialMigration());
        });
        services.AddRoomSharpDao<AppDatabaseImpl, IDataService>(db => db.Data);
        return services;
    }
}


[Database(
    Version = 1, 
    Entities = [
        typeof(MealTimeHistoryRecord),
        typeof(RideHistoryRecord)
    ]
)]
public abstract class AppDatabase(
    IDatabaseProvider provider, 
    ILogger? logger = null
) : RoomDatabase(provider, logger) 
{
    public abstract IDataService Data { get; }
}


[Dao]
public interface IDataService
{
    [Insert]
    Task AddRideHistory(RideHistoryRecord record);

    [Insert]
    Task AddMealTimeHistory(MealTimeHistoryRecord record);

    [Query("SELECT * FROM MealTimeHistoryRecord ORDER BY Timestamp DESC")]
    Task<List<MealTimeHistoryRecord>> GetMealTimeHistory();
    
    [Query("SELECT * FROM RideHistoryRecord ORDER BY Timestamp DESC")]
    Task<List<RideHistoryRecord>> GetRideTimeHistory();
    
    [Query(
        """
        SELECT 
            RideId, 
            MAX(Timestamp) AS Timestamp
        FROM 
            RideHistoryRecord
        GROUP 
            BY RideId;
        """
    )]
    Task<List<LastRideTime>> GetLastRideTimes();


    [Query(
        """
        SELECT
            Type,
            MAX(Timestamp)
        FROM 
            MealTimeHistoryRecord
        GROUP BY
            Type
        """
    )]
    Task<List<MealTimeValue>> GetLatestMealTimes();
}

[Entity]
public class MealTimeHistoryRecord
{
    [PrimaryKey(AutoGenerate = true)]
    public int Id { get; set; }
    public MealTimeType Type { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

[Entity]
public class RideHistoryRecord
{
    [PrimaryKey(AutoGenerate = true)]
    public int Id { get; set; }
    public string RideId { get; set; }
    public string RideName { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

[DatabaseView]
public class LastRideTime
{
    public string RideId { get; set; }
    
    [RoomSharp.Attributes.TypeConverter(ConverterType = typeof(DateTimeTicksConverter))]
    public DateTimeOffset Timestamp { get; set; }
}

public class DateTimeTicksConverter : TypeConverter<DateTimeOffset, long>
{
    public override long FromProvider(DateTimeOffset provider)
        => provider.Ticks;

    public override DateTimeOffset ToProvider(long model)
        => new(model, TimeSpan.Zero);
}

public class EnumIntTypeConverter<TEnum> : TypeConverter<TEnum, int>
    where TEnum : Enum
{
    public override int FromProvider(TEnum provider)
        => Convert.ToInt32(provider);

    public override TEnum ToProvider(int model)
        => (TEnum)Enum.ToObject(typeof(TEnum), model);
}

[DatabaseView]
public class MealTimeValue
{
    [RoomSharp.Attributes.TypeConverter(ConverterType = typeof(EnumIntTypeConverter<MealTimeType>))]
    public MealTimeType Type { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public enum MealTimeType
{
    Drink = 1,
    Food = 2
}