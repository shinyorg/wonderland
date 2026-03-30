namespace ShinyWonderland.UITests;

public abstract class PlatformTestBase : IAsyncLifetime
{
    readonly PlatformFixture fixture;

    protected PlatformTestBase(PlatformFixture fixture)
    {
        this.fixture = fixture;
    }

    protected MauiDevFlowDriver Driver => fixture.Driver;

    public Task InitializeAsync()
    {
        if (!fixture.CanRunOnCurrentOS())
            throw new Exception($"$XunitDynamicSkip${fixture.Platform} tests are not supported on this OS.");

        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
