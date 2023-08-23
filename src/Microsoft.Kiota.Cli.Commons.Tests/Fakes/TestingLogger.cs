using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Kiota.Cli.Commons.Tests.Fakes;

internal class TestingLogger<T> : ILogger<T>
{
    public List<string> Messages { get; init; } = new();

    public List<LogLevel> Levels { get; init; } = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Levels.Add(logLevel);
        Messages.Add(state?.ToString() ?? string.Empty);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}