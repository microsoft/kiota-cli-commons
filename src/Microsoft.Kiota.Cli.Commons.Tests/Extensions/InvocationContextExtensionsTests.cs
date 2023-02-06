using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Cli.Commons.Extensions;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.Extensions;

public class InvocationContextExtensionsTests {
    [Fact]
    public void GetRequestAdapterThrowsOnNoAdapterRegistered()
    {
        var parser = new Parser(new Command("test"));
        var invocationContext = new InvocationContext(parser.Parse("test"));

        Assert.Throws<InvalidOperationException>(() => invocationContext.GetRequestAdapter());
    }

    [Fact]
    public void GetRequestAdapterReturnsAdapterIfOneIsRegistered()
    {
        var parser = new Parser(new Command("test"));
        var invocationContext = new InvocationContext(parser.Parse("test"));
        invocationContext.BindingContext.AddService<IRequestAdapter>(_ => new Mock<IRequestAdapter>().Object);

        var adapter = invocationContext.GetRequestAdapter();

        Assert.NotNull(adapter);
    }
}
