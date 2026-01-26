using RoomSharp.DependencyInjection;
using Xunit.Abstractions;

namespace ShinyWonderland.Tests;

public class DataServiceTests
{
    readonly IDataService dataService;

    public DataServiceTests()
    {
        var services = new ServiceCollection();
        services.AddRoomSharpDatabase<AppDatabaseImpl>(ctx =>
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var fullPath = Path.Combine(appData, $"{Guid.NewGuid()}.db");
            ctx.UseSqlite(fullPath);
        });
        services.AddRoomSharpDao<AppDatabaseImpl, IDataService>(db => db.Data);
        var sp = services.BuildServiceProvider();
        this.dataService = sp.GetRequiredService<IDataService>();
    }

    [Fact]
    public async Task AddRideHistory_ShouldInsertRecord()
    {
        var record = new RideHistoryRecord
        {
            RideId = "ride-1",
            RideName = "Space Mountain",
            Timestamp = DateTimeOffset.UtcNow
        };

        await this.dataService.AddRideHistory(record);

        var history = await this.dataService.GetRideTimeHistory();
        history.ShouldContain(x => x.RideId == "ride-1" && x.RideName == "Space Mountain");
    }

    [Fact]
    public async Task GetRideTimeHistory_ShouldReturnRecordsInDescendingOrder()
    {
        var now = DateTimeOffset.UtcNow;
        var record1 = new RideHistoryRecord
        {
            RideId = "ride-1",
            RideName = "Space Mountain",
            Timestamp = now.AddHours(-2)
        };
        var record2 = new RideHistoryRecord
        {
            RideId = "ride-2",
            RideName = "Thunder Mountain",
            Timestamp = now.AddHours(-1)
        };
        var record3 = new RideHistoryRecord
        {
            RideId = "ride-3",
            RideName = "Pirates",
            Timestamp = now
        };

        await this.dataService.AddRideHistory(record1);
        await this.dataService.AddRideHistory(record2);
        await this.dataService.AddRideHistory(record3);

        var history = await this.dataService.GetRideTimeHistory();

        history.Count.ShouldBeGreaterThanOrEqualTo(3);
        var myRecords = history.Where(x => x.RideId.StartsWith("ride-")).ToList();
        myRecords[0].RideId.ShouldBe("ride-3");
        myRecords[1].RideId.ShouldBe("ride-2");
        myRecords[2].RideId.ShouldBe("ride-1");
    }

    [Fact]
    public async Task GetLastRideTimes_ShouldReturnLatestTimestampPerRide()
    {
        var now = DateTimeOffset.UtcNow;
        var record1 = new RideHistoryRecord
        {
            RideId = "unique-ride-a",
            RideName = "Space Mountain",
            Timestamp = now.AddHours(-2)
        };
        var record2 = new RideHistoryRecord
        {
            RideId = "unique-ride-a",
            RideName = "Space Mountain",
            Timestamp = now
        };
        var record3 = new RideHistoryRecord
        {
            RideId = "unique-ride-b",
            RideName = "Thunder Mountain",
            Timestamp = now.AddHours(-1)
        };

        await this.dataService.AddRideHistory(record1);
        await this.dataService.AddRideHistory(record2);
        await this.dataService.AddRideHistory(record3);

        var lastTimes = await this.dataService.GetLastRideTimes();

        var rideA = lastTimes.FirstOrDefault(x => x.RideId == "unique-ride-a");
        var rideB = lastTimes.FirstOrDefault(x => x.RideId == "unique-ride-b");

        rideA.ShouldNotBeNull();
        rideA.Timestamp.ShouldBe(now, TimeSpan.FromSeconds(1));

        rideB.ShouldNotBeNull();
        rideB.Timestamp.ShouldBe(now.AddHours(-1), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AddMealTimeHistory_ShouldInsertRecord()
    {
        var record = new MealTimeHistoryRecord
        {
            Type = MealTimeType.Food,
            Timestamp = DateTimeOffset.UtcNow
        };

        await this.dataService.AddMealTimeHistory(record);

        var history = await this.dataService.GetMealTimeHistory();
        history.ShouldContain(x => x.Type == MealTimeType.Food);
    }

    [Fact]
    public async Task GetMealTimeHistory_ShouldReturnRecordsInDescendingOrder()
    {
        var now = DateTimeOffset.UtcNow;
        var record1 = new MealTimeHistoryRecord
        {
            Type = MealTimeType.Food,
            Timestamp = now.AddHours(-2)
        };
        var record2 = new MealTimeHistoryRecord
        {
            Type = MealTimeType.Drink,
            Timestamp = now.AddHours(-1)
        };
        var record3 = new MealTimeHistoryRecord
        {
            Type = MealTimeType.Food,
            Timestamp = now
        };

        await this.dataService.AddMealTimeHistory(record1);
        await this.dataService.AddMealTimeHistory(record2);
        await this.dataService.AddMealTimeHistory(record3);

        var history = await this.dataService.GetMealTimeHistory();

        history.Count.ShouldBeGreaterThanOrEqualTo(3);
        history[0].Timestamp.ShouldBeGreaterThanOrEqualTo(history[1].Timestamp);
        history[1].Timestamp.ShouldBeGreaterThanOrEqualTo(history[2].Timestamp);
    }

    [Fact]
    public async Task GetLatestMealTimes_ShouldReturnLatestTimestampPerType()
    {
        var now = DateTimeOffset.UtcNow;
        var foodOld = new MealTimeHistoryRecord
        {
            Type = MealTimeType.Food,
            Timestamp = now.AddHours(-3)
        };
        var foodNew = new MealTimeHistoryRecord
        {
            Type = MealTimeType.Food,
            Timestamp = now.AddHours(-1)
        };
        var drinkOld = new MealTimeHistoryRecord
        {
            Type = MealTimeType.Drink,
            Timestamp = now.AddHours(-2)
        };
        var drinkNew = new MealTimeHistoryRecord
        {
            Type = MealTimeType.Drink,
            Timestamp = now
        };

        await this.dataService.AddMealTimeHistory(foodOld);
        await this.dataService.AddMealTimeHistory(foodNew);
        await this.dataService.AddMealTimeHistory(drinkOld);
        await this.dataService.AddMealTimeHistory(drinkNew);

        var latestTimes = await this.dataService.GetLatestMealTimes();

        var food = latestTimes.FirstOrDefault(x => x.Type == MealTimeType.Food);
        var drink = latestTimes.FirstOrDefault(x => x.Type == MealTimeType.Drink);

        food.ShouldNotBeNull();
        food.Timestamp.ShouldNotBeNull();
        food.Timestamp!.Value.ShouldBe(now.AddHours(-1), TimeSpan.FromSeconds(1));

        drink.ShouldNotBeNull();
        drink.Timestamp.ShouldNotBeNull();
        drink.Timestamp!.Value.ShouldBe(now, TimeSpan.FromSeconds(1));
    }
}