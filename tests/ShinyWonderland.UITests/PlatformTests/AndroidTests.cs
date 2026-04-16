namespace ShinyWonderland.UITests.PlatformTests;

[InheritsTests]
public class AndroidStartupPageTests : StartupPageTests
{
    [ClassDataSource<AndroidAppFixture>(Shared = SharedType.PerTestSession)]
    public required AndroidAppFixture AndroidFixture { get; init; }

    protected override PlatformFixture Fixture => AndroidFixture;
}

[InheritsTests]
public class AndroidNavigationTests : NavigationTests
{
    [ClassDataSource<AndroidAppFixture>(Shared = SharedType.PerTestSession)]
    public required AndroidAppFixture AndroidFixture { get; init; }

    protected override PlatformFixture Fixture => AndroidFixture;
}

[InheritsTests]
public class AndroidRideTimesPageTests : RideTimesPageTests
{
    [ClassDataSource<AndroidAppFixture>(Shared = SharedType.PerTestSession)]
    public required AndroidAppFixture AndroidFixture { get; init; }

    protected override PlatformFixture Fixture => AndroidFixture;
}

[InheritsTests]
public class AndroidMapRideTimesPageTests : MapRideTimesPageTests
{
    [ClassDataSource<AndroidAppFixture>(Shared = SharedType.PerTestSession)]
    public required AndroidAppFixture AndroidFixture { get; init; }

    protected override PlatformFixture Fixture => AndroidFixture;
}

[InheritsTests]
public class AndroidSettingsPageTests : SettingsPageTests
{
    [ClassDataSource<AndroidAppFixture>(Shared = SharedType.PerTestSession)]
    public required AndroidAppFixture AndroidFixture { get; init; }

    protected override PlatformFixture Fixture => AndroidFixture;
}

[InheritsTests]
public class AndroidParkingPageTests : ParkingPageTests
{
    [ClassDataSource<AndroidAppFixture>(Shared = SharedType.PerTestSession)]
    public required AndroidAppFixture AndroidFixture { get; init; }

    protected override PlatformFixture Fixture => AndroidFixture;
}

[InheritsTests]
public class AndroidMealTimePageTests : MealTimePageTests
{
    [ClassDataSource<AndroidAppFixture>(Shared = SharedType.PerTestSession)]
    public required AndroidAppFixture AndroidFixture { get; init; }

    protected override PlatformFixture Fixture => AndroidFixture;
}

[InheritsTests]
public class AndroidHoursPageTests : HoursPageTests
{
    [ClassDataSource<AndroidAppFixture>(Shared = SharedType.PerTestSession)]
    public required AndroidAppFixture AndroidFixture { get; init; }

    protected override PlatformFixture Fixture => AndroidFixture;
}

[InheritsTests]
public class AndroidRideHistoryPageTests : RideHistoryPageTests
{
    [ClassDataSource<AndroidAppFixture>(Shared = SharedType.PerTestSession)]
    public required AndroidAppFixture AndroidFixture { get; init; }

    protected override PlatformFixture Fixture => AndroidFixture;
}
