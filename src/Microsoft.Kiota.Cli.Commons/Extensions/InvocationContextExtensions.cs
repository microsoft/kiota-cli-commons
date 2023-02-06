using System;
using System.CommandLine.Invocation;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Cli.Commons.Extensions;

/// <summary>
/// Extensions on <see cref="InvocationContext"/>.
/// </summary>
public static class InvocationContextExtensions
{
    /// <summary>
    /// Returns an instance of a registered IRequestAdapter.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Throws an <c>InvalidOperationException</c> if there's no request adapter registered.
    /// </exception>
    public static IRequestAdapter GetRequestAdapter(this InvocationContext context) => context.BindingContext.GetService(typeof(IRequestAdapter)) as IRequestAdapter ??
                        throw new InvalidOperationException("IRequest adapter not found. Register a request adapter instance");
}
