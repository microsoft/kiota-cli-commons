using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Kiota.Cli.Commons.Http.Headers;

/// <summary>
/// An in memory headers store for use in multi-threaded code.
/// </summary>
/// <remarks>
/// This class is thread safe.
/// </remarks>
public sealed class ConcurrentInMemoryHeadersStore : BaseHeadersStore
{
    private readonly ConcurrentDictionary<string, ICollection<string>> _headers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a single instance of the <see cref="ConcurrentInMemoryHeadersStore"/>
    /// singleton.
    /// </summary>
    public static ConcurrentInMemoryHeadersStore Instance { get; } = new();

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
    /// Creates a new instance of <see cref="ConcurrentInMemoryHeadersStore"/>
    /// </summary>
    public ConcurrentInMemoryHeadersStore()
    {
    }
}