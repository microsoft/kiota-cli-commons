﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kiota.Cli.Commons.IO;
using Microsoft.Kiota.Cli.Commons.Tests.Fakes;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.IO;

public class TextOutputFormatterTest
{
    public class WriteOutputAsyncFunction_Should
    {
        private readonly string NewLine = Environment.NewLine;

        [Fact]
        public async Task Write_A_Line_With_Short_Stream_Content()
        {
            var tc = new TestConsole();
            var formatter = new TextOutputFormatter(tc);
            var content = "Test content";
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(content));

            await formatter.WriteOutputAsync(stream);

            Assert.Equal($"{content}{NewLine}", tc.Output);
        }

        [Fact]
        public async Task Write_A_Line_With_Long_Stream_Content()
        {
            var tc = new TestConsole();
            var formatter = new TextOutputFormatter(tc);
            using var fs = File.OpenRead("data/long_text_file.txt");

            await formatter.WriteOutputAsync(fs);

            Assert.StartsWith($"Lorem ipsum", tc.Output);
            Assert.EndsWith($"sed nisi lacus sed.{NewLine}", tc.Output);
        }

        [Fact]
        public async Task Write_A_Line_With_Empty_Stream_Content()
        {
            var tc = new TestConsole();
            var formatter = new TextOutputFormatter(tc);
            using var stream = Stream.Null;

            await formatter.WriteOutputAsync(stream);

            Assert.EndsWith($"{NewLine}", tc.Output);
        }
    }
}
