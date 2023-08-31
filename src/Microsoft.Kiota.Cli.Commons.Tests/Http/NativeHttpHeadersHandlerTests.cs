using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Cli.Commons.Http;
using Microsoft.Kiota.Cli.Commons.Http.Headers;
using Microsoft.Kiota.Cli.Commons.Tests.Fakes;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.Http;

public class NativeHttpHeadersHandlerTests
{
    public class SendAsyncFunction
    {

        [Fact]
        public async Task AddsHeaderInStoreToMessageAsync()
        {
            var store = InMemoryHeadersStore.Instance;
            store.SetHeaders(new[] { "a=b", "c=d" });

            var handler = new NativeHttpHeadersHandler(() => store)
            {
                InnerHandler = new TestingRequestHandler()
            };
            var msg = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            var client = new HttpClient(handler);
            await client.SendAsync(msg);

            Assert.Equal("b", msg.Headers.NonValidated["a"].ToString());
            Assert.Equal("d", msg.Headers.NonValidated["c"].ToString());
        }

        [Fact]
        public async Task AddsContentHeaderInStoreToMessageAsync()
        {
            var store = InMemoryHeadersStore.Instance;
            store.SetHeaders(new[] { "Content-Type=application/text", "Content-Length=0" });
            var logger = new TestingLogger<NativeHttpHeadersHandler>();

            var handler = new NativeHttpHeadersHandler(() => store, logger)
            {
                InnerHandler = new TestingRequestHandler()
            };
            var msg = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            msg.Content = new StringContent(string.Empty);
            var client = new HttpClient(handler);
            await client.SendAsync(msg);

            Assert.Equal("application/text", msg?.Content?.Headers?.ContentType?.ToString());
            Assert.Equal("0", msg?.Content?.Headers?.ContentLength?.ToString());
            Assert.Single(logger.Messages);
            Assert.Equal(LogLevel.Warning, logger.Levels[0]);
            Assert.Equal("The header Content-Type will replace an existing header value with application/text.", logger.Messages[0]);
        }

        [Fact]
        public async Task LogsWarningWhenContentHeaderProvidedForNonContentMessage()
        {
            var store = InMemoryHeadersStore.Instance;
            store.SetHeaders(new[] { "Content-Type=application/text" });
            var logger = new TestingLogger<NativeHttpHeadersHandler>();

            var handler = new NativeHttpHeadersHandler(() => store, logger)
            {
                InnerHandler = new TestingRequestHandler()
            };
            var msg = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            var client = new HttpClient(handler);
            await client.SendAsync(msg);

            Assert.Single(logger.Levels);
            Assert.Single(logger.Messages);
            Assert.Equal(LogLevel.Warning, logger.Levels[0]);
            Assert.Equal("Could not add the content header Content-Type to the request headers", logger.Messages[0]);
        }

        [Fact]
        public async Task LogsWarningWhenInvalidHeaderValueProvided()
        {
            var store = InMemoryHeadersStore.Instance;
            store.SetHeaders(new[] { "invalid-header=x\nx" });
            var logger = new TestingLogger<NativeHttpHeadersHandler>();

            var handler = new NativeHttpHeadersHandler(() => store, logger)
            {
                InnerHandler = new TestingRequestHandler()
            };
            var msg = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            var client = new HttpClient(handler);
            await client.SendAsync(msg);

            Assert.Single(logger.Levels);
            Assert.Single(logger.Messages);
            Assert.Equal(LogLevel.Warning, logger.Levels[0]);
            Assert.Equal("Could not add the header invalid-header to the request headers", logger.Messages[0]);
        }
    }
}