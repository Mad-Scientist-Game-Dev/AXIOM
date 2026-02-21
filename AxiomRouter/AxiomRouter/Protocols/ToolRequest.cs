using System.Text.Json;

namespace Protocols;

public sealed class ToolRequest
{
    public string Tool { get; init; } = "";           // e.g. "axiom.query"
    public string Purpose { get; init; } = "";        // why the tool is needed
    public JsonElement Arguments { get; init; }       // tool-specific payload
}
