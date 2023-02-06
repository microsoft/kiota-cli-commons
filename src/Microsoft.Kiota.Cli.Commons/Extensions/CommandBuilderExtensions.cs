using System;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Cli.Commons.IO;

namespace Microsoft.Kiota.Cli.Commons.Extensions;

/// <summary>
/// Extensions on a CommandBuilder
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
            var requestAdapter = builderFactory.Invoke(context);
            if (requestAdapter != null)
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
    public static CommandLineBuilder UseRequestAdapter(this CommandLineBuilder builder, IRequestAdapter requestAdapter)
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
    /// <see cref="IOutputFilter"></see> and <see cref="IPagingService"></see>
    /// with the <see cref="CommandLineBuilder"></see>'s
    /// <see cref="BindingContext"></see>.
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
