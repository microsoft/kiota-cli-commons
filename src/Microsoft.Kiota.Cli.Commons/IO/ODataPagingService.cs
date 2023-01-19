using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

#if !SIGNED
[assembly: InternalsVisibleTo("Microsoft.Kiota.Cli.Commons.Tests")]
#else
[assembly: InternalsVisibleTo("Microsoft.Kiota.Cli.Commons.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#endif
namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// Paging service that supports the x-ms-pageable extension
/// </summary>
public class ODataPagingService : BasePagingService
{
    /// <inheritdoc />
    public override IPagingResponseHandler CreateResponseHandler()
    {
        return new NativePagingResponseHandler();
    }

    /// <inheritdoc />
    public override async Task<Uri?> GetNextPageLinkAsync(PageLinkData pageLinkData, CancellationToken cancellationToken = default)
    {
        if (IsJson(pageLinkData) && pageLinkData.Response != null)
        {
            try
            {
                using var doc = await JsonDocument.ParseAsync(pageLinkData.Response, cancellationToken: cancellationToken);
                var hasNextLink = doc.RootElement.TryGetProperty(pageLinkData.NextLinkName, out var nextLink);
                if (hasNextLink && nextLink.ValueKind == JsonValueKind.String)
                {
                    string? link = nextLink.GetString();
                    if (!string.IsNullOrWhiteSpace(link)) return new Uri(link);
                }
            }
            catch (JsonException)
            {
                // If the response isn't valid JSON, there will be no next link.
                // TODO: Log warning once logging story is defined
            }
        }

        return null;
    }

    /// <inheritdoc />
    public override async Task<Stream?> MergePageAsync(Stream? currentResult, PageLinkData newPageData, CancellationToken cancellationToken = default)
    {
        if (IsJson(newPageData))
        {
            return await MergeJsonStreamsAsync(currentResult, newPageData.Response, newPageData.ItemName, newPageData.NextLinkName, cancellationToken);
        }

        return null;
    }

    /// <inheritdoc />
    public override bool OnBeforeGetPagedData(PageLinkData pageLinkData, bool fetchAllPages = false)
    {
        return true;
    }

    private bool IsJson(PageLinkData pageLinkData)
    {
        return pageLinkData.ResponseContentHeaders.TryGetValue("Content-Type", out var contentType) && contentType.Any(c => c.Contains("json"));
    }

    /// <summary>
    /// Merges 2 streams of JSON on the property defined by <code>itemName</code>. The property should be a JSON array
    /// </summary>
    /// <param name="left">The first stream.</param>
    /// <param name="right">The second stream.</param>
    /// <param name="itemName">The name of the array property to merge on.</param>
    /// <param name="nextLinkName">The name of the property containing the next link name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    internal async Task<Stream?> MergeJsonStreamsAsync(Stream? left, Stream? right, string itemName = "value", string nextLinkName = "nextLink", CancellationToken cancellationToken = default)
    {
        if (left?.CanSeek == true) left?.Seek(0, SeekOrigin.Begin);
        if (right?.CanSeek == true) right?.Seek(0, SeekOrigin.Begin);
        if (left == null || right == null)
        {
            return left ?? right;
        }

        JsonNode? nodeLeft = JsonNode.Parse(left);
        if (left.CanSeek == true) left.Seek(0, SeekOrigin.Begin);
        JsonNode? nodeRight = JsonNode.Parse(right);
        if (right.CanSeek == true) right.Seek(0, SeekOrigin.Begin);

        JsonArray? leftArray = null;
        JsonArray? rightArray = null;
        if (!string.IsNullOrWhiteSpace(itemName))
        {
            if (nodeLeft?[itemName] == null)
            {
                return right;
            }
            else if (nodeRight?[itemName] == null)
            {
                return left;
            }

            leftArray = nodeLeft[itemName]?.AsArray();
            rightArray = nodeRight[itemName]?.AsArray();
        }
        else
        {
            leftArray = nodeLeft?.AsArray();
            rightArray = nodeRight?.AsArray();
        }


        if (leftArray != null && rightArray != null)
        {
            var elements = rightArray.Where(i => i != null);
            var item = elements.FirstOrDefault();
            while (item != null)
            {
                rightArray.Remove(item);
                leftArray.Add(item);
                item = elements.FirstOrDefault();
            }
        }
        if (!string.IsNullOrWhiteSpace(itemName) && nodeLeft != null)
        {
            nodeLeft[itemName] = leftArray ?? rightArray;
        }
        else
        {
            nodeLeft = leftArray ?? rightArray;
        }

        // Replace next link with new page's next link
        if (!string.IsNullOrWhiteSpace(nextLinkName))
        {
            var obj1 = nodeLeft as JsonObject;
            if (obj1?[nextLinkName] != null)
                obj1.Remove(nextLinkName);
            if (nodeRight is JsonObject obj2 && obj2?[nextLinkName] != null)
            {
                var nextLink = obj2[nextLinkName];
                obj2.Remove(nextLinkName);
                obj1?.Add(nextLinkName, nextLink);
            }
        }
        var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        nodeLeft?.WriteTo(writer);
        await writer.FlushAsync(cancellationToken);
        stream.Position = 0;

        return stream;
    }
}
