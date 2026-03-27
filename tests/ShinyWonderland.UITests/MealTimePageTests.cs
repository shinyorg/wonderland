namespace ShinyWonderland.UITests;

[Collection("App")]
public class MealTimePageTests
{
    readonly MauiDevFlowDriver driver;

    public MealTimePageTests(AppFixture fixture)
    {
        driver = fixture.Driver;
    }

    async Task NavigateToMealTimes()
    {
        await driver.Navigate("//main/mealtimes");
        await driver.WaitUntilExists("MealTimePage");
    }

    [Fact]
    public async Task MealTime_PageLoads()
    {
        await NavigateToMealTimes();

        var isVisible = await driver.IsElementVisible("MealTimePage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task MealTime_DrinkPassButtonExists()
    {
        await NavigateToMealTimes();

        var isVisible = await driver.IsElementVisible("DrinkPassButton");
        isVisible.ShouldBeTrue("Drink pass button should be visible");
    }

    [Fact]
    public async Task MealTime_FoodPassButtonExists()
    {
        await NavigateToMealTimes();

        var isVisible = await driver.IsElementVisible("FoodPassButton");
        isVisible.ShouldBeTrue("Food pass button should be visible");
    }

    [Fact]
    public async Task MealTime_CollectionViewExists()
    {
        await NavigateToMealTimes();

        var isVisible = await driver.IsElementVisible("MealHistoryCollectionView");
        isVisible.ShouldBeTrue("Meal history collection view should be present");
    }

    [Fact]
    public async Task MealTime_Screenshot()
    {
        await NavigateToMealTimes();

        await driver.Screenshot("meal-times.png");
    }
}
