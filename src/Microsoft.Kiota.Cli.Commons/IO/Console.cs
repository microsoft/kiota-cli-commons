using System;
using System.IO;

namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// Console abstraction to help unit test.
/// </summary>
public interface IConsole
{
    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void Write(string? value);

    /// <summary>
    /// Writes the specified character span to the standard output stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void Write(ReadOnlySpan<char> value);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void WriteLine(string? value);

    /// <summary>
    /// Writes the specified character span, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void WriteLine(ReadOnlySpan<char> value);

    /// <summary>
    /// Writes the current line terminator to the standard output stream.
    /// </summary>
    void WriteLine();
}

/// <summary>
/// Default console. Delegates to the provided System.IO.TextWriter.* methods.
/// </summary>
public class DefaultConsole : IConsole
{
    private readonly TextWriter writer;

    /// <summary>
    /// Create a new console
    /// </summary>
    /// <param name="writer">A writer to delegate to for the created instance.</param>
    public DefaultConsole(TextWriter writer)
    {
        this.writer = writer;
    }

    /// <inheritdoc/>
    public void Write(string? value)
    {
        writer.Write(value);
    }

    /// <inheritdoc/>
    public void Write(ReadOnlySpan<char> value)
    {
        writer.Write(value);
    }

    /// <inheritdoc/>
    public void WriteLine(string? value)
    {
        writer.WriteLine(value);
    }

    /// <inheritdoc/>
    public void WriteLine(ReadOnlySpan<char> value)
    {
        writer.WriteLine(value);
    }

    /// <inheritdoc/>
    public void WriteLine()
    {
        writer.WriteLine();
    }
}
