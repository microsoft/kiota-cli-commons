using System.Collections.Generic;

namespace Microsoft.Kiota.Cli.Commons.Http.Headers;

/// <summary>
/// The interface for header storage.
/// </summary>
public interface IHeadersStore
{
    /// <summary>
    /// Get the currently existing headers.
    /// </summary>
    /// <returns>The currently existing headers.</returns>
    public IEnumerable<KeyValuePair<string, ICollection<string>>> GetHeaders();
    
    /// <summary>
    /// Receives a collection of strings with header information, parses the
    /// header information, then replaces all the existing headers in the store
    /// with the parsed headers.
    /// </summary>
    /// <param name="headers">A collection of strings with header information.</param>
    /// <returns>The previously existing headers.</returns>
    IEnumerable<KeyValuePair<string, ICollection<string>>> SetHeaders(ICollection<string>? headers);

    /// <summary>
    /// Receives a collection of strings with header information, parses the
    /// header information, then adds the parsed headers to the store.
    /// </summary>
    /// <param name="headers">A collection of strings with header information.</param>
    /// <returns>A boolean indicating whether any values were added to the header store.</returns>
    bool AddHeaders(ICollection<string>? headers);

    /// <summary>
    /// Clears the store and returns a collection of the previously existing headers
    /// </summary>
    /// <returns>A collection of the previously existing headers.</returns>
    IEnumerable<KeyValuePair<string, ICollection<string>>> Drain();
}
