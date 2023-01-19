using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Rendering;

#if !SIGNED
[assembly: InternalsVisibleTo("Microsoft.Kiota.Cli.Commons.Tests")]
#else
[assembly: InternalsVisibleTo("Microsoft.Kiota.Cli.Commons.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#endif
namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// The table output formatter
/// </summary>
public class TableOutputFormatter : IOutputFormatter
{
    private readonly IAnsiConsole _ansiConsole;

    /// <summary>
    /// Creates a new table output formatter with a default console
    /// </summary>
    public TableOutputFormatter() : this(AnsiConsole.Console)
    {
    }

    /// <summary>
    /// Creates a new table output formatter with the provided console
    /// </summary>
    /// <param name="console">The console to use</param>
    public TableOutputFormatter(IAnsiConsole console)
    {
        _ansiConsole = console;
    }

    /// <inheritdoc />
    public async Task WriteOutputAsync(Stream? content, IOutputFormatterOptions? options = null, CancellationToken cancellationToken = default) {
        if (content == null)
        {
            return;
        }
        using var doc = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        var table = ConstructTable(doc);
        _ansiConsole.Write(table);
    }

    /// <summary>
    /// Construct a table given a JSON document
    /// </summary>
    /// <param name="document">The parsed json document</param>
    /// <returns>A table</returns>
    internal Table ConstructTable(JsonDocument document) {
        var root = GetRootElement(document.RootElement);
        var firstElement = GetFirstElement(root);

        IEnumerable<string> propertyNames = GetPropertyNames(firstElement);
        var table = new Table();
        table.Expand();

        foreach (var propertyName in propertyNames)
        {
            table.AddColumn(propertyName, column =>
            {
                if (firstElement.ValueKind == JsonValueKind.Object)
                {
                    var hasProp = firstElement.TryGetProperty(propertyName, out var property);
                    if (property.ValueKind == JsonValueKind.Number)
                        column.RightAligned().PadLeft(10);
                }
            });
        }

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var row in root.EnumerateArray())
            {
                var rowCols = GetRowColumns(propertyNames, row);
                table.AddRow(rowCols);
            }
        }
        else if (root.ValueKind == JsonValueKind.Object)
        {
            var rowCols = GetRowColumns(propertyNames, root);
            table.AddRow(rowCols);
        }
        else
        {
            table.AddRow(GetPropertyValue(root));
        }

        return table;
    }

    private static JsonElement GetRootElement(JsonElement input) {
        var root = input;
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("value", out var value))
            root = value;
        
        return root;
    }

    private static JsonElement GetFirstElement(JsonElement root) {
        var firstElement = root;
        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
        {
            var enumerated = root.EnumerateArray();
            firstElement = enumerated.FirstOrDefault();
        }
        
        return firstElement;
    }

    private static IEnumerable<string> GetPropertyNames(JsonElement firstElement) {
        IEnumerable<string> propertyNames;
        if (firstElement.ValueKind != JsonValueKind.Object)
        {
            propertyNames = new List<string> { "Value" };
        }
        else
        {
            var restrictedValueKinds = new JsonValueKind[] {
                    JsonValueKind.Array,
                    JsonValueKind.Object
                };
            var objectEnumerator = firstElement.EnumerateObject();
            var buffer = new List<string>();
            foreach (var property in objectEnumerator)
            {
                if (restrictedValueKinds.Contains(property.Value.ValueKind)) {
                    continue;
                }

                buffer.Add(property.Name);
            }
            propertyNames = buffer;
        }
        
        return propertyNames;
    }

    private static IEnumerable<IRenderable> GetRowColumns(IEnumerable<string> propertyNames, JsonElement row)
    {
        return propertyNames.Select(p =>
        {
            var propertyName = p;
            if (row.ValueKind == JsonValueKind.Object)
            {
                var hasProp = row.TryGetProperty(propertyName, out var property);
                if (hasProp)
                    return GetPropertyValue(property);
                else
                    return new Markup("-");
            }

            return GetPropertyValue(row);
        });
    }

    private static IRenderable GetPropertyValue(JsonElement property)
    {
        var valueKind = property.ValueKind;
        object? value = null;
        switch (valueKind)
        {
            case JsonValueKind.String:
                value = property.GetString();
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                value = property.GetBoolean();
                break;
            case JsonValueKind.Number:
                value = property.GetDecimal();
                break;
        }
        return new Markup(value?.ToString() ?? "-");
    }
}
