using System;
using System.IO;
using Microsoft.Kiota.Cli.Commons.IO;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.IO;

public class ConsoleTest {
    [Fact]
    public void TestWrite()
    {
        // Given
        var writer = new StringWriter();
        var console = new DefaultConsole(writer);
    
        // When
        console.Write("a test");
    
        // Then
        Assert.Equal("a test", writer.ToString());
    }

    [Fact]
    public void TestWriteLine()
    {
        // Given
        var writer = new StringWriter();
        var console = new DefaultConsole(writer);
    
        // When
        console.WriteLine("a test");
    
        // Then
        Assert.Equal($"a test{writer.NewLine}", writer.ToString());
    }
}
