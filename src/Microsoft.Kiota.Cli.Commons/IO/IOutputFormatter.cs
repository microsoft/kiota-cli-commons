using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// Output formatter contract.
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Format and write stream content
    /// </summary>
    /// <param name="content">The stream content to format and write out</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task WriteOutputAsync(Stream? content, CancellationToken cancellationToken = default);
}
