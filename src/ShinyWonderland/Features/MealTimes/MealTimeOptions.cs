namespace ShinyWonderland.Features.MealTimes;


public class MealTimeOptions
{
    public bool Enabled { get; set; }
    public TimeSpan DrinkTimeWait { get; set; }
    public TimeSpan FoodTimeWait { get; set; }
}
