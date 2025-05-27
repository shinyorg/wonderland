using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ShinyWonderland.Tests;

public class Logger<T>(ITestOutputHelper output) : ILogger<T>
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        output.WriteLine(formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}