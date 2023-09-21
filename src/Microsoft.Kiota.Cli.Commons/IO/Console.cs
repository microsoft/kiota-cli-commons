using System;

namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// Console abstraction to help unit test.
/// </summary>
public interface IConsole {
    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void Write (string? value);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void WriteLine(string? value);

    /// <summary>
    /// Writes the current line terminator to the standard output stream.
    /// </summary>
    void WriteLine();
}

/// <summary>
/// System console. Uses System.Console.* methods.
/// </summary>
public class SystemConsole : IConsole
{
    /// <inheritdoc/>
    public void Write(string? value)
    {
        Console.Write(value);
    }

    /// <inheritdoc/>
    public void WriteLine(string? value)
    {
        Console.WriteLine(value);
    }

    /// <inheritdoc/>
    public void WriteLine()
    {
        Console.WriteLine();
    }
}
