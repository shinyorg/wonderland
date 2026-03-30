namespace ShinyWonderland.UITests;

public abstract class MealTimePageTests : PlatformTestBase
{
    protected MealTimePageTests(PlatformFixture fixture) : base(fixture) { }

    async Task NavigateToMealTimes()
    {
        await Driver.Navigate("//main/mealtimes");
        await Driver.WaitUntilExists("MealTimePage");
    }

    [Fact]
    public async Task MealTime_PageLoads()
    {
        await NavigateToMealTimes();

        var isVisible = await Driver.IsElementVisible("MealTimePage");
        isVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task MealTime_DrinkPassButtonExists()
    {
        await NavigateToMealTimes();

        var isVisible = await Driver.IsElementVisible("DrinkPassButton");
        isVisible.ShouldBeTrue("Drink pass button should be visible");
    }

    [Fact]
    public async Task MealTime_FoodPassButtonExists()
    {
        await NavigateToMealTimes();

        var isVisible = await Driver.IsElementVisible("FoodPassButton");
        isVisible.ShouldBeTrue("Food pass button should be visible");
    }

    [Fact]
    public async Task MealTime_CollectionViewExists()
    {
        await NavigateToMealTimes();

        var isVisible = await Driver.IsElementVisible("MealHistoryCollectionView");
        isVisible.ShouldBeTrue("Meal history collection view should be present");
    }

    [Fact]
    public async Task MealTime_Screenshot()
    {
        await NavigateToMealTimes();

        await Driver.Screenshot("meal-times.png");
    }
}
