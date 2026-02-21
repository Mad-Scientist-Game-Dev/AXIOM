namespace Protocols;

public sealed class ModelDecision
{
    public string Kind { get; init; } = "";           // "response" | "tool_request"
    public string? Response { get; init; }
    public ToolRequest? ToolRequest { get; init; }
}
