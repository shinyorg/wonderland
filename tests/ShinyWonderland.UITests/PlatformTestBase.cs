using TUnit.Core.Exceptions;

namespace ShinyWonderland.UITests;

public abstract class PlatformTestBase
{
    protected abstract PlatformFixture Fixture { get; }

    protected MauiDevFlowDriver Driver => Fixture.Driver;

    [Before(Test)]
    public void SkipIfOSMismatch()
    {
        if (!Fixture.CanRunOnCurrentOS())
            throw new SkipTestException($"{Fixture.Platform} tests are not supported on this OS.");
    }
}
