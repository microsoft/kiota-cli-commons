using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Cli.Commons.Http.Headers;

namespace Microsoft.Kiota.Cli.Commons.Http;

/// <summary>
/// A <see cref="DelegatingHandler"/> that calls a function which should return
/// a store (<see cref="IHeadersStore"/>) instance and adds the stored headers
/// to any request before sending it.
/// </summary>
public class NativeHttpHeadersHandler : DelegatingHandler
{
    private const string AddHeaderWarningTpl = "Could not add the {Kind}header {HeaderName}";

    private readonly Func<IHeadersStore> _headersStoreGetter;
    private readonly ILogger<NativeHttpHeadersHandler>? _logger;

    /// <summary>
    /// Creates a new instance of the <see cref="NativeHttpHeadersHandler"/>
    /// that will call the provided <paramref name="headersStoreGetter"/> function
    /// and attempt to add all the headers in the returned
    /// <see cref="IHeadersStore"/> instance. This handler calls the
    /// <see cref="IHeadersStore.Drain"/> function on the instance.
    /// </summary>
    /// <param name="headersStoreGetter">
    /// A function that when called, should return a <see cref="IHeadersStore"/>
    /// instance containing headers to send.
    /// </param>
    /// <param name="logger">
    /// An optional logger which defaults to null.
    /// </param>
    /// <remarks>
    /// The class will not log any message if a logger is not provided.
    /// </remarks>
    public NativeHttpHeadersHandler(Func<IHeadersStore> headersStoreGetter,
        ILogger<NativeHttpHeadersHandler>? logger = null)
    {
        _headersStoreGetter = headersStoreGetter;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Adds headers to the <paramref name="request"/> before sending it.
    /// </remarks>
    /// <exception cref="FormatException">
    /// The header name format is invalid. -or- New line characters in header values must be followed by a white-space
    /// character.
    /// </exception>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        foreach (var headerItem in _headersStoreGetter().Drain())
        {
            try
            {
                // content headers defined in: https://www.rfc-editor.org/rfc/rfc2616
                if (headerItem.Key is "Content-Encoding" or "Content-Language" or "Content-Length" or "Content-Location"
                    or "Content-MD5" or "Content-Range" or "Content-Type")
                {
                    if (request.Content is { } content)
                    {
                        content.Headers.Add(headerItem.Key, headerItem.Value);
                    }
                    else
                    {
                        _logger?.LogWarning(AddHeaderWarningTpl, "Content ", headerItem.Key);
                    }
                }
                else
                {
                    request.Headers.Add(headerItem.Key, headerItem.Value);
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or FormatException)
            {
                _logger?.LogWarning(ex, AddHeaderWarningTpl, string.Empty, headerItem.Key);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}