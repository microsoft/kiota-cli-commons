using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// A no-op output formatter
/// </summary>
public class NoneOutputFormatter : IOutputFormatter
{
    /// <inheritdoc />
    public Task WriteOutputAsync(Stream? content, IOutputFormatterOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
