using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Kiota.Cli.Commons.Http.Headers;

/// <summary>
/// Base class for the <see cref="IHeadersStore"/> interface.
/// This type exists so all child classes can share the headers parsing logic.
/// </summary>
public abstract class BaseHeadersStore : IHeadersStore
{
    /// <inheritdoc />
    public abstract IEnumerable<KeyValuePair<string, ICollection<string>>> GetHeaders();

    /// <inheritdoc />
    /// <remarks> 
    /// Passing a null or empty collection of headers to this function has the
    /// same effect as calling <see cref="Drain"/>
    /// </remarks>
    public virtual IEnumerable<KeyValuePair<string, ICollection<string>>> SetHeaders(ICollection<string>? headers)
    {
        var existing = Drain();
        AddHeaders(headers);
        return existing;
    }

    /// <inheritdoc />
    /// <remarks>
    /// If <paramref name="headers"/> collection is null, this function will
    /// return false
    /// The parsing logic for this type uses the <see cref="ParseHeaders"/>
    /// function to parse the headers. Override ParseHeaders in sub-classes
    /// to change the parsing behavior.
    /// </remarks>
    public virtual bool AddHeaders(ICollection<string>? headers)
    {
        if (headers is null || headers.Count < 1)
        {
            return false;
        }

        var m = ParseHeaders(headers);

        foreach (var (key, value) in m)
        {
            DoAddHeader(key, value);
        }

        return true;
    }

    /// <inheritdoc />
    public abstract IEnumerable<KeyValuePair<string, ICollection<string>>> Drain();

    /// <summary>
    /// Parses a collection of headers into a collection of key-value pairs
    /// with the header name and value.
    /// This function expects each header item to be in the format
    /// <code>header-name=header-value</code>
    /// This function does not do anything about duplicates, so if you passed
    /// in <code>["a=1", "b=2"]</code> the result will be
    /// <code>[{ "a": "1" }, { "b": "2" }]</code>
    /// </summary>
    /// 
    /// <param name="headers">
    /// A collection of strings with header information. This collection MUST
    /// be non-null.
    /// </param>
    /// 
    /// <returns>
    /// A collection of key-value pairs with parsed header information.
    /// </returns>
    /// 
    /// <remarks>
    /// Override this function to customize the parsing logic.
    /// </remarks>
    protected virtual IEnumerable<KeyValuePair<string, string>> ParseHeaders(IEnumerable<string> headers)
    {
        // This function is called by AddHeaders which checks for null. If
        // headers is null, then something went wrong
        ArgumentNullException.ThrowIfNull(headers);
        return ParseHeadersIterator(headers);
    }

    private IEnumerable<KeyValuePair<string, string>> ParseHeadersIterator(IEnumerable<string> headers)
    {
        foreach (var headerLine in headers)
        {
            var idx = headerLine.IndexOf('=', StringComparison.Ordinal);
            if (idx < 0 || idx + 1 >= headerLine.Length)
            {
                continue;
            }

            yield return new KeyValuePair<string, string>(headerLine[..idx], headerLine[(idx + 1)..]);
        }
    }
    
    /// <summary>
    /// Implementation of logic to add headers to a headers store.
    /// </summary>
    /// <param name="name">The header name</param>
    /// <param name="value">The header value</param>
    /// <remarks>
    /// This function exists to facilitate customizing the header storage
    /// location.
    /// For example, one might want to customize the data structure used to
    /// store the headers for performance optimizations.
    /// </remarks>
    protected abstract void DoAddHeader(string name, string value);
}