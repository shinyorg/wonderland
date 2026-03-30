namespace ShinyWonderland.UITests.PlatformTests;

[Collection("iOS")]
public class IosStartupPageTests(IosAppFixture fixture) : StartupPageTests(fixture);

[Collection("iOS")]
public class IosNavigationTests(IosAppFixture fixture) : NavigationTests(fixture);

[Collection("iOS")]
public class IosRideTimesPageTests(IosAppFixture fixture) : RideTimesPageTests(fixture);

[Collection("iOS")]
public class IosMapRideTimesPageTests(IosAppFixture fixture) : MapRideTimesPageTests(fixture);

[Collection("iOS")]
public class IosSettingsPageTests(IosAppFixture fixture) : SettingsPageTests(fixture);

[Collection("iOS")]
public class IosParkingPageTests(IosAppFixture fixture) : ParkingPageTests(fixture);

[Collection("iOS")]
public class IosMealTimePageTests(IosAppFixture fixture) : MealTimePageTests(fixture);

[Collection("iOS")]
public class IosHoursPageTests(IosAppFixture fixture) : HoursPageTests(fixture);

[Collection("iOS")]
public class IosRideHistoryPageTests(IosAppFixture fixture) : RideHistoryPageTests(fixture);
