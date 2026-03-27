namespace ShinyWonderland.UITests;

public class AppFixture : IAsyncLifetime
{
    public MauiDevFlowDriver Driver { get; } = new();

    public async Task InitializeAsync()
    {
        // Assumes app is already running and agent is registered.
        // Run: dotnet build -f net10.0-ios -t:Run (or equivalent) before tests.
        await Driver.WaitForAgent(timeoutSeconds: 30);

        // Wait for startup page to finish and navigate to main tab bar
        await Driver.WaitUntilExists("RideTimesPage", timeoutSeconds: 30);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("App")]
public class AppCollection : ICollectionFixture<AppFixture>;
