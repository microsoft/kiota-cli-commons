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
    private const string AddHeaderWarningTpl = "Could not add the {Kind}header {HeaderName} to the request headers";
    private const string ContentKind = "content ";
    private const string ContentEncodingHeader = "Content-Encoding";
    private const string ContentLanguageHeader = "Content-Language";
    private const string ContentLengthHeader = "Content-Length";
    private const string ContentLocationHeader = "Content-Location";
    private const string ContentMd5Header = "Content-MD5";
    private const string ContentRangeHeader = "Content-Range";
    private const string ContentTypeHeader = "Content-Type";

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
    /// <para>
    /// Adds headers to the <paramref name="request"/> before sending it.
    ///</para>
    /// <para>
    /// If cancellation is requested, the headers in the store will have
    /// been cleared.
    /// </para>
    /// </remarks>
    /// <exception cref="FormatException">
    /// The header name format is invalid. -or- New line characters in header values must be followed by a white-space
    /// character.
    /// </exception>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        AddHeadersFromStore(request, cancellationToken);
        return base.SendAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Adds headers to the <paramref name="request"/> before sending it.
    ///</para>
    /// <para>
    /// If cancellation is requested, the headers in the store will have
    /// been cleared.
    /// </para>
    /// </remarks>
    /// <exception cref="FormatException">
    /// The header name format is invalid. -or- New line characters in header values must be followed by a white-space
    /// character.
    /// </exception>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        AddHeadersFromStore(request, cancellationToken);
        return base.Send(request, cancellationToken);
    }

    private void AddHeadersFromStore(HttpRequestMessage request, CancellationToken token = default)
    {
        foreach (var headerItem in _headersStoreGetter().Drain())
        {
            if (token.IsCancellationRequested)
            {
                // Cancellation doesn't support returning the drained headers back into the store.
                return;
            }

            try
            {
                switch (IsContentHeader(headerItem.Key))
                {
                    case true when request.Content is { } content:
                        // These headers don't support multiple values.
                        // First remove the existing header, but log a warning
                        // so the user is aware a replacement will happen
                        if ((ContentTypeHeader.Equals(headerItem.Key) || ContentLengthHeader.Equals(headerItem.Key)) &&
                            content.Headers.Remove(headerItem.Key))
                        {
                            _logger?.LogWarning(
                                "The header {HeaderName} will replace an existing header value with {NewHeaderValue}.",
                                headerItem.Key, headerItem.Value);
                        }

                        content.Headers.Add(headerItem.Key, headerItem.Value);
                        break;
                    case false:
                        request.Headers.Add(headerItem.Key, headerItem.Value);
                        break;
                    default:
                        _logger?.LogWarning(AddHeaderWarningTpl, ContentKind, headerItem.Key);
                        break;
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or FormatException)
            {
                _logger?.LogWarning(ex, AddHeaderWarningTpl, string.Empty, headerItem.Key);
            }
        }
    }

    private static bool IsContentHeader(string value)
    {
        // content headers defined in: https://www.rfc-editor.org/rfc/rfc2616
        return ContentEncodingHeader.Equals(value, StringComparison.OrdinalIgnoreCase) ||
               ContentLanguageHeader.Equals(value, StringComparison.OrdinalIgnoreCase) ||
               ContentLengthHeader.Equals(value, StringComparison.OrdinalIgnoreCase) ||
               ContentLocationHeader.Equals(value, StringComparison.OrdinalIgnoreCase) ||
               ContentMd5Header.Equals(value, StringComparison.OrdinalIgnoreCase) ||
               ContentRangeHeader.Equals(value, StringComparison.OrdinalIgnoreCase) ||
               ContentTypeHeader.Equals(value, StringComparison.OrdinalIgnoreCase);
    }
}