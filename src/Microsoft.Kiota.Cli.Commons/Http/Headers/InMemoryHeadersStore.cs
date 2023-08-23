using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Kiota.Cli.Commons.Http.Headers;

/// <summary>
/// An in memory headers store.
/// </summary>
/// 
/// <remarks>
/// <para>
/// Use <see cref="Instance"/> static property to get a default thread unsafe
/// header store and <see cref="ConcurrentInstance"/> static property to get a
/// thread-safe variant.
/// </para>
/// <para>
/// Use the <see cref="IsConcurrent"/> instance property to determine if an
/// instance of this class is thread-safe.
/// </para>
/// </remarks>
public sealed class InMemoryHeadersStore : BaseHeadersStore
{
    private readonly IDictionary<string, ICollection<string>> _headers;

    /// <summary>
    /// Returns a thread unsafe single instance of the
    /// <see cref="InMemoryHeadersStore"/> singleton.
    /// </summary>
    /// 
    /// <remarks>
    /// For cases where you might not want to use a single instance across your
    /// application e.g. when using an instance per thread, create a new
    /// instance and use that instead. In that case, make sure you have cached
    /// the instance when you need to access the stored headers.
    /// </remarks>
    public static InMemoryHeadersStore Instance { get; } = new();

    /// <summary>
    /// Returns a thread safe single instance of the
    /// <see cref="InMemoryHeadersStore"/> singleton.
    /// </summary>
    public static InMemoryHeadersStore ConcurrentInstance { get; } = new(true);

    /// <summary>
    /// A boolean indicating whether the store can be used in multi-threaded
    /// contexts safely.
    /// </summary>
    public bool IsConcurrent { get; }

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
    /// 
    /// <param name="concurrent">Whether to opt into thread safety.</param>
    /// 
    /// <remarks>
    /// When using the headers store from multiple threads, you can opt into
    /// thread synchronization by setting <paramref name="concurrent"/> to
    /// true. This option uses a thread safe backing store, but will incur
    /// some synchronization overhead.
    /// </remarks>
    public InMemoryHeadersStore(bool concurrent = false)
    {
        if (concurrent)
        {
            IsConcurrent = true;
            _headers = new ConcurrentDictionary<string, ICollection<string>>(StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            _headers = new Dictionary<string, ICollection<string>>(StringComparer.OrdinalIgnoreCase);
        }
    }
}