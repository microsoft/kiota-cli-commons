using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Cli.Commons.Http;

/// <summary>
/// Interface for making URI replacements.
/// </summary>
public interface IUriReplacement
{
    /// <summary>
    /// Accepts a URI and returns a new URI with all replacements applied.
    /// </summary>
    /// <param name="original">The URI to apply replacements to</param>
    /// <returns>A new URI with all replacements applied.</returns>
    Uri? Replace(Uri? original);
}

/// <summary>
/// Replaces a portion of the URL.
/// </summary>
public class UriReplacementHandler<TUriReplacement> : DelegatingHandler where TUriReplacement : IUriReplacement
{
    private readonly TUriReplacement uriReplacement;

    /// <summary>
    /// Creates a new UriReplacementHandler.
    /// </summary>
    public UriReplacementHandler(TUriReplacement uriReplacement)
    {
        this.uriReplacement = uriReplacement;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        request.RequestUri = uriReplacement.Replace(request.RequestUri);
        return await base.SendAsync(request, cancellationToken);
    }
}
