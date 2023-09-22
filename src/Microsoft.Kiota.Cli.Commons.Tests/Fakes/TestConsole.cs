using System.IO;
using System.Text;
using Microsoft.Kiota.Cli.Commons.IO;

namespace Microsoft.Kiota.Cli.Commons.Tests.Fakes;

internal class TestConsole : IConsole
{
    private readonly StringBuilder sb;

    private readonly TextWriter tw;

    public string? Output => tw.ToString();

    public TestConsole()
    {
        sb = new StringBuilder();
        tw = new StringWriter(sb);
    }

    public void Write(string? value)
    {
        tw.Write(value);
    }

    public void WriteLine(string? value)
    {
        tw.WriteLine(value);
    }

    public void WriteLine()
    {
        tw.WriteLine();
    }
}
