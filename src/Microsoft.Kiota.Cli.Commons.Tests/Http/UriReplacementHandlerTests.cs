using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Kiota.Cli.Commons.Http;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.Http;

public class UriReplacementHandlerTests
{
    [Fact]
    public async Task Calls_Uri_ReplacementAsync()
    {
        var mockReplacement = new Mock<IUriReplacement>();
        mockReplacement.Setup(x => x.Replace(It.IsAny<Uri>())).Returns(new Uri("http://changed"));

        var handler = new UriReplacementHandler<IUriReplacement>(mockReplacement.Object)
        {
            InnerHandler = new TestingRequestHandler()
        };
        var msg = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
        var client = new HttpClient(handler);
        await client.SendAsync(msg);

        mockReplacement.Verify(x=> x.Replace(It.IsAny<Uri>()), Times.Once());
    }
}
