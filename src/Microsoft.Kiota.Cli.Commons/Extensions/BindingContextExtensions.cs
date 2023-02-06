using System;
using System.CommandLine.Binding;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Cli.Commons.Extensions;

public static class BindingContextExtensions
{
    /// <summary>
    /// Returns an instance of a registered IRequestAdapter.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Throws an <c>InvalidOperationException</c> if there's no request adapter registered.
    /// </exception>
    public static IRequestAdapter GetRequestAdapter(this BindingContext context) => context.GetService(typeof(IRequestAdapter)) as IRequestAdapter ??
                        throw new InvalidOperationException("IRequest adapter not found. Register a request adapter instance");
}
