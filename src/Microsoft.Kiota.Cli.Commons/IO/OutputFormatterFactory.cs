using System;

namespace Microsoft.Kiota.Cli.Commons.IO;

/// <inheritdoc />
public sealed class OutputFormatterFactory : IOutputFormatterFactory
{
    private const string INVALID_FORMATTER_ERROR = "The formatter type specified is not valid. Ensure any new formatters are registered with the OutputFormatterFactory";

    /// <inheritdoc />
    public IOutputFormatter GetFormatter(FormatterType formatterType)
    {
        return formatterType switch
        {
            FormatterType.JSON => new JsonOutputFormatter(new SystemConsole()),
            FormatterType.TABLE => new TableOutputFormatter(),
            FormatterType.TEXT => new TextOutputFormatter(new SystemConsole()),
            FormatterType.NONE => new NoneOutputFormatter(),
            _ => throw new ArgumentOutOfRangeException(nameof(formatterType), formatterType, INVALID_FORMATTER_ERROR),
        };
    }

    /// <inheritdoc />
    public IOutputFormatter GetFormatter(string format)
    {
        var success = Enum.TryParse(format, true, out FormatterType type);
        if (!success)
        {
            throw new ArgumentOutOfRangeException(nameof(format), format, INVALID_FORMATTER_ERROR);
        }
        return GetFormatter(type);
    }
}
