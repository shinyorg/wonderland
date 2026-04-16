using Microsoft.Extensions.DependencyInjection;
using Shiny.DocumentDb;
using Shiny.DocumentDb.Sqlite;

namespace ShinyWonderland.Tests;

public class DataServiceTests
{
    readonly IDataService dataService;

    public DataServiceTests()
    {
        var services = new ServiceCollection();
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var fullPath = Path.Combine(appData, $"{Guid.NewGuid()}.db");
        services.AddDocumentStore(opts =>
        {
            opts.DatabaseProvider = new SqliteDatabaseProvider($"Data Source={fullPath}");
        });
        services.AddSingleton<IDataService, DataService>();
        var sp = services.BuildServiceProvider();
        this.dataService = sp.GetRequiredService<IDataService>();
    }

    [Test]
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
        await Assert.That(history).Contains(x => x.RideId == "ride-1" && x.RideName == "Space Mountain");
    }

    [Test]
    public async Task GetRideTimeHistory_ShouldReturnRecordsInDescendingOrder()
    {
        var now = DateTimeOffset.UtcNow;
        var record1 = new RideHistoryRecord { RideId = "ride-1", RideName = "Space Mountain", Timestamp = now.AddHours(-2) };
        var record2 = new RideHistoryRecord { RideId = "ride-2", RideName = "Thunder Mountain", Timestamp = now.AddHours(-1) };
        var record3 = new RideHistoryRecord { RideId = "ride-3", RideName = "Pirates", Timestamp = now };

        await this.dataService.AddRideHistory(record1);
        await this.dataService.AddRideHistory(record2);
        await this.dataService.AddRideHistory(record3);

        var history = await this.dataService.GetRideTimeHistory();

        await Assert.That(history.Count).IsGreaterThanOrEqualTo(3);
        var myRecords = history.Where(x => x.RideId.StartsWith("ride-")).ToList();
        await Assert.That(myRecords[0].RideId).IsEqualTo("ride-3");
        await Assert.That(myRecords[1].RideId).IsEqualTo("ride-2");
        await Assert.That(myRecords[2].RideId).IsEqualTo("ride-1");
    }

    [Test]
    public async Task GetLastRideTimes_ShouldReturnLatestTimestampPerRide()
    {
        var now = DateTimeOffset.UtcNow;
        var record1 = new RideHistoryRecord { RideId = "unique-ride-a", RideName = "Space Mountain", Timestamp = now.AddHours(-2) };
        var record2 = new RideHistoryRecord { RideId = "unique-ride-a", RideName = "Space Mountain", Timestamp = now };
        var record3 = new RideHistoryRecord { RideId = "unique-ride-b", RideName = "Thunder Mountain", Timestamp = now.AddHours(-1) };

        await this.dataService.AddRideHistory(record1);
        await this.dataService.AddRideHistory(record2);
        await this.dataService.AddRideHistory(record3);

        var lastTimes = await this.dataService.GetLastRideTimes();

        var rideA = lastTimes.FirstOrDefault(x => x.RideId == "unique-ride-a");
        var rideB = lastTimes.FirstOrDefault(x => x.RideId == "unique-ride-b");

        await Assert.That(rideA).IsNotNull();
        await Assert.That(rideA!.Timestamp).IsEqualTo(now).Within(TimeSpan.FromSeconds(1));

        await Assert.That(rideB).IsNotNull();
        await Assert.That(rideB!.Timestamp).IsEqualTo(now.AddHours(-1)).Within(TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task AddMealTimeHistory_ShouldInsertRecord()
    {
        var record = new MealTimeHistoryRecord
        {
            Type = MealTimeType.Food,
            Timestamp = DateTimeOffset.UtcNow
        };

        await this.dataService.AddMealTimeHistory(record);

        var history = await this.dataService.GetMealTimeHistory();
        await Assert.That(history).Contains(x => x.Type == MealTimeType.Food);
    }

    [Test]
    public async Task GetMealTimeHistory_ShouldReturnRecordsInDescendingOrder()
    {
        var now = DateTimeOffset.UtcNow;
        var record1 = new MealTimeHistoryRecord { Type = MealTimeType.Food, Timestamp = now.AddHours(-2) };
        var record2 = new MealTimeHistoryRecord { Type = MealTimeType.Drink, Timestamp = now.AddHours(-1) };
        var record3 = new MealTimeHistoryRecord { Type = MealTimeType.Food, Timestamp = now };

        await this.dataService.AddMealTimeHistory(record1);
        await this.dataService.AddMealTimeHistory(record2);
        await this.dataService.AddMealTimeHistory(record3);

        var history = await this.dataService.GetMealTimeHistory();

        await Assert.That(history.Count).IsGreaterThanOrEqualTo(3);
        await Assert.That(history[0].Timestamp).IsGreaterThanOrEqualTo(history[1].Timestamp);
        await Assert.That(history[1].Timestamp).IsGreaterThanOrEqualTo(history[2].Timestamp);
    }

    [Test]
    public async Task GetLatestMealTimes_ShouldReturnLatestTimestampPerType()
    {
        var now = DateTimeOffset.UtcNow;
        var foodOld = new MealTimeHistoryRecord { Type = MealTimeType.Food, Timestamp = now.AddHours(-3) };
        var foodNew = new MealTimeHistoryRecord { Type = MealTimeType.Food, Timestamp = now.AddHours(-1) };
        var drinkOld = new MealTimeHistoryRecord { Type = MealTimeType.Drink, Timestamp = now.AddHours(-2) };
        var drinkNew = new MealTimeHistoryRecord { Type = MealTimeType.Drink, Timestamp = now };

        await this.dataService.AddMealTimeHistory(foodOld);
        await this.dataService.AddMealTimeHistory(foodNew);
        await this.dataService.AddMealTimeHistory(drinkOld);
        await this.dataService.AddMealTimeHistory(drinkNew);

        var latestTimes = await this.dataService.GetLatestMealTimes();

        var food = latestTimes.FirstOrDefault(x => x.Type == MealTimeType.Food);
        var drink = latestTimes.FirstOrDefault(x => x.Type == MealTimeType.Drink);

        await Assert.That(food).IsNotNull();
        await Assert.That(food!.Timestamp).IsNotNull();
        await Assert.That(food.Timestamp!.Value).IsEqualTo(now.AddHours(-1)).Within(TimeSpan.FromSeconds(1));

        await Assert.That(drink).IsNotNull();
        await Assert.That(drink!.Timestamp).IsNotNull();
        await Assert.That(drink.Timestamp!.Value).IsEqualTo(now).Within(TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task AddMealPass_ShouldInsertPass()
    {
        var pass = new MealPass { Type = MealTimeType.Drink };

        await this.dataService.AddMealPass(pass);

        var passes = await this.dataService.GetMealPasses();
        await Assert.That(passes).Contains(x => x.Type == MealTimeType.Drink);
    }

    [Test]
    public async Task GetMealPasses_ShouldReturnAllPasses()
    {
        await this.dataService.AddMealPass(new MealPass { Type = MealTimeType.Drink });
        await this.dataService.AddMealPass(new MealPass { Type = MealTimeType.Food });
        await this.dataService.AddMealPass(new MealPass { Type = MealTimeType.Drink });

        var passes = await this.dataService.GetMealPasses();
        await Assert.That(passes.Count).IsEqualTo(3);
    }

    [Test]
    public async Task UpdateMealPass_ShouldPersistChanges()
    {
        var pass = new MealPass { Type = MealTimeType.Drink };
        await this.dataService.AddMealPass(pass);

        var passes = await this.dataService.GetMealPasses();
        var inserted = passes.First(x => x.Type == MealTimeType.Drink);
        inserted.LastUsed = DateTimeOffset.UtcNow;
        inserted.NotificationSent = true;
        await this.dataService.UpdateMealPass(inserted);

        var updated = (await this.dataService.GetMealPasses()).First(x => x.Id == inserted.Id);
        await Assert.That(updated.LastUsed).IsNotNull();
        await Assert.That(updated.NotificationSent).IsTrue();
    }

    [Test]
    public async Task DeleteMealPass_ShouldRemovePass()
    {
        await this.dataService.AddMealPass(new MealPass { Type = MealTimeType.Drink });

        var passes = await this.dataService.GetMealPasses();
        await Assert.That(passes.Count).IsEqualTo(1);

        await this.dataService.DeleteMealPass(passes[0].Id);

        var remaining = await this.dataService.GetMealPasses();
        await Assert.That(remaining.Count).IsEqualTo(0);
    }
}
