namespace ShinyWonderland.UITests;

public abstract class MealTimePageTests : PlatformTestBase
{
    async Task NavigateToMealTimes()
    {
        await Driver.Navigate("//main/mealtimes");
        await Driver.WaitUntilExists("MealTimePage");
    }

    [Test]
    public async Task MealTime_PageLoads()
    {
        await NavigateToMealTimes();

        var isVisible = await Driver.IsElementVisible("MealTimePage");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task MealTime_DrinkPassButtonExists()
    {
        await NavigateToMealTimes();

        var isVisible = await Driver.IsElementVisible("DrinkPassButton");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task MealTime_FoodPassButtonExists()
    {
        await NavigateToMealTimes();

        var isVisible = await Driver.IsElementVisible("FoodPassButton");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task MealTime_CollectionViewExists()
    {
        await NavigateToMealTimes();

        var isVisible = await Driver.IsElementVisible("MealHistoryCollectionView");
        await Assert.That(isVisible).IsTrue();
    }

    [Test]
    public async Task MealTime_Screenshot()
    {
        await NavigateToMealTimes();

        await Driver.Screenshot("meal-times.png");
    }
}
