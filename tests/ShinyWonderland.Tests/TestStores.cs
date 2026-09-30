using System.Runtime.CompilerServices;
using Shiny.Extensions.Stores;

// [Bind] properties (AppSettings, RideTimeJob) read and write through the static Shiny.Stores.Default.
// On plain net10.0 that is a persistent settings.json under LocalApplicationData, so values leaked
// between tests and across test runs. Every test now shares one in-memory store, which is why the
// tests can't run in parallel.
[assembly: NotInParallel]

namespace ShinyWonderland.Tests;


public static class TestStores
{
    // module initializer rather than a hook - test class constructors (which write settings)
    // can run before TUnit's session/test hooks
    [ModuleInitializer]
    internal static void Initialize()
    {
        Stores.Register(StoreKeys.Default, new MemoryKeyValueStore());
        Stores.Register(StoreKeys.Secure, new MemoryKeyValueStore());
    }


    [AfterEvery(Test)]
    public static void Clear(TestContext context)
    {
        Stores.Default.Clear();
        Stores.Secure.Clear();
    }
}
