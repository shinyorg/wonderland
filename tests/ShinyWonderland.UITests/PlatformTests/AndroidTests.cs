namespace ShinyWonderland.UITests.PlatformTests;

[Collection("Android")]
public class AndroidStartupPageTests(AndroidAppFixture fixture) : StartupPageTests(fixture);

[Collection("Android")]
public class AndroidNavigationTests(AndroidAppFixture fixture) : NavigationTests(fixture);

[Collection("Android")]
public class AndroidRideTimesPageTests(AndroidAppFixture fixture) : RideTimesPageTests(fixture);

[Collection("Android")]
public class AndroidMapRideTimesPageTests(AndroidAppFixture fixture) : MapRideTimesPageTests(fixture);

[Collection("Android")]
public class AndroidSettingsPageTests(AndroidAppFixture fixture) : SettingsPageTests(fixture);

[Collection("Android")]
public class AndroidParkingPageTests(AndroidAppFixture fixture) : ParkingPageTests(fixture);

[Collection("Android")]
public class AndroidMealTimePageTests(AndroidAppFixture fixture) : MealTimePageTests(fixture);

[Collection("Android")]
public class AndroidHoursPageTests(AndroidAppFixture fixture) : HoursPageTests(fixture);

[Collection("Android")]
public class AndroidRideHistoryPageTests(AndroidAppFixture fixture) : RideHistoryPageTests(fixture);
