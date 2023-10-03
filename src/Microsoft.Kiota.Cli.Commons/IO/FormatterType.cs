namespace Microsoft.Kiota.Cli.Commons.IO;

/// <summary>
/// The formatter type
/// </summary>
public enum FormatterType
{
    /// <summary>
    /// Raw JSON format.
    /// </summary>
    RAW_JSON,
    /// <summary>
    /// Prettified JSON format.
    /// </summary>
    PRETTY_JSON,
    /// <summary>
    /// Table format
    /// </summary>
    TABLE,
    /// <summary>
    /// Text formatting
    /// </summary>
    TEXT,
    /// <summary>
    /// No output
    /// </summary>
    NONE
}
