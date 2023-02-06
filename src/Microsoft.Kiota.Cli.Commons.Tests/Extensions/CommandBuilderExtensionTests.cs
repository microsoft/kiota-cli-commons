using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Cli.Commons.Extensions;
using Microsoft.Kiota.Cli.Commons.IO;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.Extensions;

public class CommandBuilderExtensionTests {
    [Fact]
    public async Task UseRequestAdapterFuncRegistersTheGivenRequestAdapterAsync()
    {
        var adapterMock = new Mock<IRequestAdapter>();
        IRequestAdapter? foundAdapter = null;
        var command = new Command("test");
        command.SetHandler(ctx => foundAdapter = ctx.GetRequestAdapter());
        var parser = new CommandLineBuilder(command)
            .UseRequestAdapter(ctx => adapterMock.Object)
            .Build();

        var result = parser.Parse("test");
        await result.InvokeAsync();

        Assert.NotNull(foundAdapter);
        Assert.Equal(adapterMock.Object, foundAdapter);
    }

    [Fact]
    public async Task UseRequestAdapterLiteralRegistersTheGivenRequestAdapterAsync()
    {
        var adapterMock = new Mock<IRequestAdapter>();
        IRequestAdapter? foundAdapter = null;
        var command = new Command("test");
        command.SetHandler(ctx => foundAdapter = ctx.GetRequestAdapter());
        var parser = new CommandLineBuilder(command)
            .UseRequestAdapter(adapterMock.Object)
            .Build();

        var result = parser.Parse("test");
        await result.InvokeAsync();

        Assert.NotNull(foundAdapter);
        Assert.Equal(adapterMock.Object, foundAdapter);
    }

    [Fact]
    public async Task RegisterCommonServicesRegistersServicesAsync()
    {
        object? foundFmtFactory = null;
        object? foundFilter = null;
        object? foundPagingSvc = null;
        var command = new Command("test");
        command.SetHandler(ctx => {
            foundFmtFactory = ctx.BindingContext.GetService(typeof(IOutputFormatterFactory));
            foundFilter = ctx.BindingContext.GetService(typeof(IOutputFilter));
            foundPagingSvc = ctx.BindingContext.GetService(typeof(IPagingService));
        });
        var parser = new CommandLineBuilder(command)
            .RegisterCommonServices()
            .Build();

        var result = parser.Parse("test");
        await result.InvokeAsync();

        Assert.NotNull(foundFmtFactory);
        Assert.IsType<OutputFormatterFactory>(foundFmtFactory);
        Assert.NotNull(foundFilter);
        Assert.IsType<JmesPathOutputFilter>(foundFilter);
        Assert.NotNull(foundPagingSvc);
        Assert.IsType<ODataPagingService>(foundPagingSvc);
    }
}
