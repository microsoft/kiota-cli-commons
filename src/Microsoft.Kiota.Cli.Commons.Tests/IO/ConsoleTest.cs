using System;
using System.IO;
using System.Text;
using Microsoft.Kiota.Cli.Commons.IO;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.IO;

public class ConsoleTest {
    [Fact]
    public void TestWrite()
    {
        var sb = new StringBuilder();
        var writer = new StringWriter(sb);
        var console = new DefaultConsole(writer);

        console.Write("a test");
        Assert.Equal("a test", writer.ToString());

        sb.Clear();

        console.Write(new ReadOnlySpan<char>(new [] {'t', 'e', 's', 't'}));
        Assert.Equal("test", writer.ToString());
    }

    [Fact]
    public void TestWriteLine()
    {
        var sb = new StringBuilder();
        var writer = new StringWriter(sb);
        var console = new DefaultConsole(writer);
    
        console.WriteLine("a test");
        Assert.Equal($"a test{writer.NewLine}", writer.ToString());

        sb.Clear();

        console.WriteLine(new ReadOnlySpan<char>(new [] {'t', 'e', 's', 't'}));
        Assert.Equal($"test{writer.NewLine}", writer.ToString());

        sb.Clear();

        console.WriteLine();
        Assert.Equal(writer.NewLine, writer.ToString());
    }
}
