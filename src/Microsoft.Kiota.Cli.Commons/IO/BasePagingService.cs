using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;

#if !SIGNED
[assembly: InternalsVisibleTo("Microsoft.Kiota.Cli.Commons.Tests")]
#else
[assembly: InternalsVisibleTo("Microsoft.Kiota.Cli.Commons.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#endif
namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// Paging service that supports the x-ms-pageable extension
/// </summary>
public abstract class BasePagingService : IPagingService
{
    /// <inheritdoc />
    public abstract IPagingResponseHandler CreateResponseHandler();

    /// <inheritdoc />
    public abstract Task<Uri?> GetNextPageLinkAsync(PageLinkData pageLinkData, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual async Task<PageResponse?> GetPagedDataAsync(Func<RequestInformation, CancellationToken, Task> requestExecutorAsync, PageLinkData pageLinkData, bool fetchAllPages = false, CancellationToken cancellationToken = default)
    {
        if (!OnBeforeGetPagedData(pageLinkData, fetchAllPages))
        {
            return null;
        }

        var requestInfo = pageLinkData.RequestInformation;
        Uri? nextLink;
        Stream? response = null;
        int? statusCode;
        do
        {
            var responseHandler = CreateResponseHandler();
            requestInfo.SetResponseHandler(responseHandler);
            await requestExecutorAsync(requestInfo, cancellationToken);
            var pageData = await responseHandler.GetResponseStreamAsync(cancellationToken);
            statusCode = responseHandler.GetStatusCode();
            var headers = responseHandler.GetResponseHeaders();
            var contentHeaders = responseHandler.GetResponseContentHeaders();
            pageLinkData = new PageLinkData(requestInfo, pageData, headers, contentHeaders, pageLinkData.ItemName, pageLinkData.NextLinkName);
            if (fetchAllPages)
            {
                nextLink = await GetNextPageLinkAsync(pageLinkData, cancellationToken);
                if (nextLink != null) pageLinkData.RequestInformation.URI = nextLink;
            }
            else
            {
                nextLink = null;
            }

            response = await MergePageAsync(response, pageLinkData, cancellationToken);
        } while (nextLink != null);

        return new PageResponse(statusCode ?? 0, response);
    }

    /// <inheritdoc />
    public abstract Task<Stream?> MergePageAsync(Stream? currentResult, PageLinkData newPageData, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual bool OnBeforeGetPagedData(PageLinkData pageLinkData, bool fetchAllPages = false)
    {
        return true;
    }
}
