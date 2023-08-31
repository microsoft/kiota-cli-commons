using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Cli.Commons.Http.Headers;
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
    public static CommandLineBuilder UseRequestAdapter(this CommandLineBuilder builder,
        Func<InvocationContext, IRequestAdapter> builderFactory)
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

    /// <summary>
    /// Registers a headers option to all executable commands in the builder
    /// and a middleware that reads options from the parsed result and adds
    /// them to the store instance returned after calling
    /// <paramref name="headersStoreGetter"/>.
    /// </summary>
    /// 
    /// <param name="builder">A command line builder</param>
    /// <param name="headersStoreGetter">
    /// A delegate that will be called to get an instance of
    /// <see cref="IHeadersStore"/>.
    /// </param>
    /// <param name="name">
    /// A custom name for the registered option.
    /// Defaults to <c>--headers</c>
    /// </param>
    /// <param name="customDescription">
    /// An optional description for the registered headers option.
    /// </param>
    /// 
    /// <returns>
    /// The same instance of <see cref="CommandLineBuilder"/>.
    /// </returns>
    /// 
    /// <remarks>
    /// <para>
    /// If the <paramref name="name"/> has no alphanumeric characters, the
    /// default name will be used.
    /// </para>
    /// <para>
    /// This function must be called after the root command has been set in
    /// the <see cref="CommandLineBuilder"/>.
    /// </para>
    /// <para>
    /// This function does nothing to add the headers to a request. You would
    /// need to implement a way to read from the updated
    /// <see cref="IHeadersStore"/> and update the request. For an example that
    /// works with <see cref="System.Net.Http.HttpClient"/>,
    /// see the
    /// <see cref="Http.NativeHttpHeadersHandler"/>
    /// class.
    /// </para>
    /// </remarks>
    public static CommandLineBuilder RegisterHeadersOption(this CommandLineBuilder builder,
        Func<IHeadersStore> headersStoreGetter, string name = "--headers", string? customDescription = null)
    {
        // System.CommandLine library doesn't validate empty option names. Handle that scenario.
        if (string.IsNullOrWhiteSpace(name))
        {
            // Use the default name instead of throwing an exception.
            name = "--headers";
        }

        var headersOption = new Option<string[]>(name,
            customDescription ??
            $"Allows adding custom headers to the request. The option can be used multiple times to add multiple headers. e.g. --{name} key1=value1 --{name} key2=value2")
        {
            Arity = ArgumentArity.ZeroOrMore
        };

        // Recursively adds the headers option to the commands with handlers starting with the root
        AddOptionToCommandIf(builder.Command, headersOption, cmd => cmd.Handler is not null);

        builder.AddMiddleware(async (ic, next) =>
        {
            // Add headers to the headers store.
            headersStoreGetter().SetHeaders(ic.ParseResult.GetValueForOption(headersOption));
            await next(ic);
        });
        return builder;
    }

    private static void AddOptionToCommandIf(Command command, in Option option, Func<Command, bool> predicate)
    {
        if (predicate(command))
        {
            command.AddOption(option);
        }

        foreach (var cmd in command.Subcommands)
        {
            AddOptionToCommandIf(cmd, option, predicate);
        }
    }
}