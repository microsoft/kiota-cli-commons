using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// The JSON output formatter
/// </summary>
public class JsonOutputFormatter : IOutputFormatter
{
    private readonly IConsole console;

    private readonly bool prettify;

    /// <summary>
    /// Creates a new JSON output formatter with a default console
    /// </summary>
    public JsonOutputFormatter(IConsole console, bool prettify = false)
    {
        this.console = console;
        this.prettify = prettify;
    }

    /// <inheritdoc />
    public async Task WriteOutputAsync(Stream? content, CancellationToken cancellationToken = default)
    {
        if (content == null || content == Stream.Null)
        {
            return;
        }

        using var result = await ProcessJsonAsync(content, prettify, cancellationToken);
        using var r = new StreamReader(result);
        // Read char array from stream.
        const int BUFFER_LENGTH = 4096;
        var charsReceived = 0;
        var buffer = new char[BUFFER_LENGTH];
        do {
            charsReceived = await r.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (charsReceived == 0) {
                break;
            }
            console.Write(buffer.AsSpan(0, charsReceived));
        } while(charsReceived == BUFFER_LENGTH);

        console.WriteLine();
    }

    /// <summary>
    /// Given a JSON input stream, returns a processed JSON stream with optional indentation
    /// </summary>
    /// <param name="input">JSON input stream</param>
    /// <param name="indent">Whether to return indented output</param>
    /// <param name="cancellationToken">The cancellation token</param>
    private static async Task<Stream> ProcessJsonAsync(Stream input, bool indent = true, CancellationToken cancellationToken = default)
    {
        if (!indent) {
            return input;
        }

        Stream cache = new MemoryStream();
        if (!input.CanSeek) {
            // copy the stream
            await input.CopyToAsync(cache, cancellationToken).ConfigureAwait(false);
            cache.Position = 0;
        } else {
            cache = input;
        }

        try
        {
            var jsonDoc = await JsonDocument.ParseAsync(cache, default, cancellationToken).ConfigureAwait(false);
            var outputStream = new MemoryStream();
            var utf8Writer = new Utf8JsonWriter(outputStream, new JsonWriterOptions { Indented = indent, SkipValidation = true }); //not disposing to avoid disposing underlying stream
            jsonDoc.RootElement.WriteTo(utf8Writer);
            await utf8Writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            outputStream.Position = 0;
            return outputStream;
        }
        catch (JsonException)
        {
        }

        cache.Position = 0;
        return cache;
    }
}
