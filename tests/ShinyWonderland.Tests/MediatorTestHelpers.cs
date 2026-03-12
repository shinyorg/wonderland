using Microsoft.Extensions.Localization;

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

/// <summary>
/// In-memory IStringLocalizer for testing. Returns the key name as
/// the value by default; specific overrides can be provided.
/// </summary>
public class TestStringLocalizer<T> : IStringLocalizer<T>
{
    readonly Dictionary<string, string> strings;

    public TestStringLocalizer(Dictionary<string, string>? strings = null)
        => this.strings = strings ?? new();

    public LocalizedString this[string name]
        => new(name, strings.GetValueOrDefault(name, name));

    public LocalizedString this[string name, params object[] arguments]
        => new(name, string.Format(strings.GetValueOrDefault(name, name), arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        => strings.Select(kvp => new LocalizedString(kvp.Key, kvp.Value));
}

/// <summary>
/// Factory for creating StringsLocalized instances for testing
/// </summary>
public static class TestLocalization
{
    public static StringsLocalized Create(Dictionary<string, string>? overrides = null)
        => new(new TestStringLocalizer<Strings>(overrides));
}