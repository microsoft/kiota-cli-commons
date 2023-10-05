using System;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// The JSON output formatter
/// </summary>
public class TextOutputFormatter : IOutputFormatter
{
    private readonly IConsole console;


    /// <summary>
    /// Creates a new JSON output formatter with a default console
    /// </summary>
    public TextOutputFormatter(IConsole console)
    {
        this.console = console;
    }

    /// <inheritdoc />
    public async Task WriteOutputAsync(Stream? content, CancellationToken cancellationToken = default)
    {
        if (content == null)
        {
            return;
        }

        using var reader = new StreamReader(content);
        const int BUFFER_LENGTH = 4096;
        var charsReceived = 0;
        do {
            var buffer = new char[BUFFER_LENGTH];
            charsReceived = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (charsReceived == 0) {
                break;
            }
            console.Write(new string(buffer, 0, charsReceived));
        } while(charsReceived == BUFFER_LENGTH);
        console.WriteLine();
    }
}
