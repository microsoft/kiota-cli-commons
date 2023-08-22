using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Kiota.Cli.Commons.Http.Headers;

/// <summary>
/// An in memory headers store.
/// </summary>
/// <remarks>
/// This class is not thread safe.
/// </remarks>
public sealed class InMemoryHeadersStore : BaseHeadersStore
{
    private readonly Dictionary<string, ICollection<string>> _headers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a single instance of the <see cref="InMemoryHeadersStore"/>
    /// singleton.
    /// </summary>
    /// <remarks>
    /// For cases where you might not want to use a single instance across your
    /// application e.g. when using an instance per thread, create a new
    /// instance and use that instead. In that case, make sure you have cached
    /// the instance when you need to access the stored headers.
    /// </remarks>
    public static InMemoryHeadersStore Instance { get; } = new();

    /// <inheritdoc />
    public override IEnumerable<KeyValuePair<string, ICollection<string>>> GetHeaders() => _headers.AsEnumerable();

    /// <inheritdoc />
    public override IEnumerable<KeyValuePair<string, ICollection<string>>> Drain()
    {
        var existing = _headers.ToList();
        _headers.Clear();
        return existing;
    }

    /// <inheritdoc />
    protected override void DoAddHeader(string name, string value)
    {
        if (!_headers.TryAdd(name, new List<string> { value }))
        {
            _headers[name].Add(value);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="InMemoryHeadersStore"/>
    /// </summary>
    public InMemoryHeadersStore()
    {
    }
}