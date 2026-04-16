namespace ShinyWonderland.UITests.PlatformTests;

[InheritsTests]
public class IosStartupPageTests : StartupPageTests
{
    [ClassDataSource<IosAppFixture>(Shared = SharedType.PerTestSession)]
    public required IosAppFixture IosFixture { get; init; }

    protected override PlatformFixture Fixture => IosFixture;
}

[InheritsTests]
public class IosNavigationTests : NavigationTests
{
    [ClassDataSource<IosAppFixture>(Shared = SharedType.PerTestSession)]
    public required IosAppFixture IosFixture { get; init; }

    protected override PlatformFixture Fixture => IosFixture;
}

[InheritsTests]
public class IosRideTimesPageTests : RideTimesPageTests
{
    [ClassDataSource<IosAppFixture>(Shared = SharedType.PerTestSession)]
    public required IosAppFixture IosFixture { get; init; }

    protected override PlatformFixture Fixture => IosFixture;
}

[InheritsTests]
public class IosMapRideTimesPageTests : MapRideTimesPageTests
{
    [ClassDataSource<IosAppFixture>(Shared = SharedType.PerTestSession)]
    public required IosAppFixture IosFixture { get; init; }

    protected override PlatformFixture Fixture => IosFixture;
}

[InheritsTests]
public class IosSettingsPageTests : SettingsPageTests
{
    [ClassDataSource<IosAppFixture>(Shared = SharedType.PerTestSession)]
    public required IosAppFixture IosFixture { get; init; }

    protected override PlatformFixture Fixture => IosFixture;
}

[InheritsTests]
public class IosParkingPageTests : ParkingPageTests
{
    [ClassDataSource<IosAppFixture>(Shared = SharedType.PerTestSession)]
    public required IosAppFixture IosFixture { get; init; }

    protected override PlatformFixture Fixture => IosFixture;
}

[InheritsTests]
public class IosMealTimePageTests : MealTimePageTests
{
    [ClassDataSource<IosAppFixture>(Shared = SharedType.PerTestSession)]
    public required IosAppFixture IosFixture { get; init; }

    protected override PlatformFixture Fixture => IosFixture;
}

[InheritsTests]
public class IosHoursPageTests : HoursPageTests
{
    [ClassDataSource<IosAppFixture>(Shared = SharedType.PerTestSession)]
    public required IosAppFixture IosFixture { get; init; }

    protected override PlatformFixture Fixture => IosFixture;
}

[InheritsTests]
public class IosRideHistoryPageTests : RideHistoryPageTests
{
    [ClassDataSource<IosAppFixture>(Shared = SharedType.PerTestSession)]
    public required IosAppFixture IosFixture { get; init; }

    protected override PlatformFixture Fixture => IosFixture;
}
