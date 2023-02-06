using System;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Cli.Commons.IO;

namespace Microsoft.Kiota.Cli.Commons.Extensions;

/// <summary>
/// Extensions on <see cref="CommandLineBuilder"/>
/// </summary>
public static class CommandBuilderExtensions
{
    /// <summary>
    /// Registers an instance of IRequestAdapter.
    /// </summary>
    public static CommandLineBuilder UseRequestAdapter(this CommandLineBuilder builder, Func<InvocationContext, IRequestAdapter> builderFactory)
    {
        builder.AddMiddleware(async (context, next) =>
        {
            if (builderFactory.Invoke(context) is IRequestAdapter requestAdapter)
            {
                context.BindingContext.AddService(typeof(IRequestAdapter), p => requestAdapter);
            }

            // Log warning in case registration failed.
            await next(context);
        });
        return builder;
    }

    /// <summary>
    /// Registers an instance of IRequestAdapter.
    /// </summary>
    public static CommandLineBuilder UseRequestAdapter(this CommandLineBuilder builder, [NotNull] IRequestAdapter requestAdapter)
    {
        builder.AddMiddleware(async (context, next) =>
        {
            context.BindingContext.AddService(typeof(IRequestAdapter), p => requestAdapter);
            await next(context);
        });
        return builder;
    }

    /// <summary>
    /// Registers the default common implementations of
    /// <see cref="IOutputFormatterFactory"></see>,
    /// <see cref="IOutputFilter"/> and <see cref="IPagingService"/>
    /// with the <see cref="CommandLineBuilder"/>'s
    /// <see cref="BindingContext"/>.
    /// <example>
    /// Given an invocation context, one can get an output filter by writing:
    /// <code>
    ///     var outputFilter = invocationContext.BindingContext.GetService&lt;IAuthenticationCacheUtility&gt;();
    ///     var filteredStream = await outputFilter?.FilterOutputAsync(content, query, cancellationToken);
    /// </code>
    /// </example>
    /// </summary>
    public static CommandLineBuilder RegisterCommonServices(this CommandLineBuilder builder)
    {
        builder.AddMiddleware(async (context, next) =>
        {
            context.BindingContext.AddService<IOutputFormatterFactory>(_ => new OutputFormatterFactory());
            context.BindingContext.AddService<IOutputFilter>(_ => new JmesPathOutputFilter(new()));
            context.BindingContext.AddService<IPagingService>(_ => new ODataPagingService());
            await next(context);
        });
        return builder;
    }
}
