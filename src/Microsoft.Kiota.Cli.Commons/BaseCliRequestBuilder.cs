using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Cli.Commons;

/// <summary>
/// Base class for CLI request builders
/// </summary>
public class BaseCliRequestBuilder
{
    internal const string RawUrlKey = "request-raw-url";

    /// <summary>Path parameters for the request</summary>
    protected Dictionary<string, object> PathParameters { get; set; }
    /// <summary>Url template to use to build the URL for the current request builder</summary>
    protected string UrlTemplate { get; set; }

    /// <summary>
    /// Instantiates a new <see cref="BaseCliRequestBuilder"/> class
    /// </summary>
    /// <param name="pathParameters">Path parameters for the request</param>
    /// <param name="urlTemplate">Url template to use to build the URL for the current request builder</param>
    protected BaseCliRequestBuilder(string urlTemplate, Dictionary<string, object> pathParameters)
    {
        _ = pathParameters ?? throw new ArgumentNullException(nameof(pathParameters));
        _ = urlTemplate ?? throw new ArgumentNullException(nameof(urlTemplate)); // empty is fine
        PathParameters = new Dictionary<string, object>(pathParameters);
        UrlTemplate = urlTemplate;
    }

    /// <summary>
    /// Instantiates a new <see cref="BaseCliRequestBuilder"/> class
    /// </summary>
    /// <param name="urlTemplate">Url template to use to build the URL for the current request builder</param>
    /// <param name="rawUrl">The raw URL to use for the current request builder</param>
    protected BaseCliRequestBuilder(string urlTemplate, string rawUrl)
        : this(urlTemplate, new Dictionary<string, object> { { RawUrlKey, rawUrl } })
    {
        if (string.IsNullOrEmpty(rawUrl))
        {
            throw new ArgumentNullException(nameof(rawUrl));
        }
    }
}
