using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Cli.Commons.Extensions;
using Microsoft.Kiota.Cli.Commons.Http.Headers;
using Microsoft.Kiota.Cli.Commons.IO;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.Extensions;

public class CommandBuilderExtensionTests
{
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
        command.SetHandler(ctx =>
        {
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

    public class RegisterHeadersOptionTests
    {
        [Fact]
        public async Task RegistersDefaultNamedHeadersOption()
        {
            var storeMock = new Mock<IHeadersStore>();
            ICollection<string>? givenHeaders = null;
            storeMock.Setup(m => m.SetHeaders(It.IsAny<ICollection<string>>()))
                .Returns((ICollection<string> headers) =>
                {
                    givenHeaders = headers;
                    return Enumerable.Empty<KeyValuePair<string, ICollection<string>>>();
                });
            var command = new Command("test");
            var subcmd = new Command("sub");
            command.Add(subcmd);
            subcmd.SetHandler(_ => { });
            var parser = new CommandLineBuilder(command)
                .RegisterHeadersOption(() => storeMock.Object)
                .Build();

            var result = parser.Parse("test sub --headers a=b");
            await result.InvokeAsync();

            Assert.NotNull(givenHeaders);
            Assert.Equal("a=b", givenHeaders.FirstOrDefault());
            Assert.Empty(command.Options);
            Assert.Single(subcmd.Options);
        }
        
        [Fact]
        public async Task RegistersDefaultNamedHeadersOptionWhenNameIsEmpty()
        {
            var storeMock = new Mock<IHeadersStore>();
            ICollection<string>? givenHeaders = null;
            storeMock.Setup(m => m.SetHeaders(It.IsAny<ICollection<string>>()))
                .Returns((ICollection<string> headers) =>
                {
                    givenHeaders = headers;
                    return Enumerable.Empty<KeyValuePair<string, ICollection<string>>>();
                });
            var command = new Command("test");
            var subcmd = new Command("sub");
            command.Add(subcmd);
            subcmd.SetHandler(_ => { });
            var parser = new CommandLineBuilder(command)
                .RegisterHeadersOption(() => storeMock.Object, name: string.Empty)
                .Build();

            var result = parser.Parse("test sub --headers a=b");
            await result.InvokeAsync();

            Assert.NotNull(givenHeaders);
            Assert.Equal("a=b", givenHeaders.FirstOrDefault());
            Assert.Empty(command.Options);
            Assert.Single(subcmd.Options);
        }
        [Fact]
        public async Task RegistersNamedHeadersOptionWhenNameIsProvided()
        {
            var storeMock = new Mock<IHeadersStore>();
            ICollection<string>? givenHeaders = null;
            storeMock.Setup(m => m.SetHeaders(It.IsAny<ICollection<string>>()))
                .Returns((ICollection<string> headers) =>
                {
                    givenHeaders = headers;
                    return Enumerable.Empty<KeyValuePair<string, ICollection<string>>>();
                });
            var command = new Command("test");
            var subcmd = new Command("sub");
            command.Add(subcmd);
            subcmd.SetHandler(_ => { });
            var parser = new CommandLineBuilder(command)
                .RegisterHeadersOption(() => storeMock.Object, name: "--custom-headers")
                .Build();

            var result = parser.Parse("test sub --custom-headers a=b");
            await result.InvokeAsync();

            Assert.NotNull(givenHeaders);
            Assert.Equal("a=b", givenHeaders.FirstOrDefault());
            Assert.Empty(command.Options);
            Assert.Single(subcmd.Options);
        }
    }
}