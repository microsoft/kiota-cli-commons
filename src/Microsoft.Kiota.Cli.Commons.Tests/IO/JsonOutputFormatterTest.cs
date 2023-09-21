using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kiota.Cli.Commons.IO;
using Microsoft.Kiota.Cli.Commons.Tests.Fakes;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.IO;

public sealed class JsonOutputFormatterTest
{
    public class WriteOutputAsyncFunction_Should
    {
        private readonly string NewLine = Environment.NewLine;

        [Fact]
        public async Task Write_A_Line_With_Stream_Content()
        {
            var tc = new TestConsole();
            var formatter = new JsonOutputFormatter(tc);
            var content = "Test content";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(content));

            await formatter.WriteOutputAsync(stream, new JsonOutputFormatterOptions(true));

            Assert.Equal($"{content}{NewLine}", tc.Output);
        }

        [Fact]
        public async Task Write_Indented_Output_Given_A_Minified_Json_Stream()
        {
            var tc = new TestConsole();
            var formatter = new JsonOutputFormatter(tc);
            var content = "{\"a\": 1, \"b\": \"test\"}";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(content));
            var n = NewLine;

            await formatter.WriteOutputAsync(stream, new JsonOutputFormatterOptions(true));
            var expected = $"{{{n}  \"a\": 1,{n}  \"b\": \"test\"{n}}}";
            Console.Out.Flush();

            Assert.Equal($"{expected}{n}", tc.Output);
        }

        [Fact]
        public async Task Write_Minified_Output_Given_A_Minified_Json_Stream_If_Indentation_Disabled()
        {
            var tc = new TestConsole();
            var formatter = new JsonOutputFormatter(tc);
            var content = "{\"a\": 1, \"b\": \"test\"}";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(content));

            await formatter.WriteOutputAsync(stream, new JsonOutputFormatterOptions(false));
            var expected = $"{content}{NewLine}";
            Console.Out.Flush();

            Assert.Equal(expected, tc.Output);
        }

        [Fact]
        public async Task Write_Nothing_Given_A_Null_Stream()
        {
            var tc = new TestConsole();
            var formatter = new JsonOutputFormatter(tc);
            Stream? stream = null;

            await formatter.WriteOutputAsync(stream, new JsonOutputFormatterOptions(false));
            var expected = "";
            Console.Out.Flush();

            Assert.Equal(expected, tc.Output);
        }
    }
}
