using Shiny.Mediator.Caching;

namespace ShinyWonderland.Tests;

/// <summary>
/// Helper class to create mock mediator results for testing
/// </summary>
public static class MediatorTestHelpers
{
    /// <summary>
    /// Creates a mock mediator result tuple for use in tests
    /// </summary>
    public static (IMediatorContext Context, T Result) CreateResult<T>(T result)
    {
        var context = Substitute.For<IMediatorContext>();
        return (context, result);
    }

    /// <summary>
    /// Creates a mock mediator result tuple with cache context for use in tests
    /// </summary>
    public static (IMediatorContext Context, T Result) CreateResultWithCache<T>(T result, DateTimeOffset? cacheTimestamp)
    {
        var context = Substitute.For<IMediatorContext>();
        if (cacheTimestamp.HasValue)
        {
            var cacheContext = Substitute.For<CacheContext>();
            cacheContext.Timestamp.Returns(cacheTimestamp.Value);
            context.Cache().Returns(cacheContext);
        }
        return (context, result);
    }
}