using Axiom.Arbiter;
using Protocols;

[McpServerToolType]
public sealed class AxiomMcpTools
{
    private readonly ArbiterHost _arbiter;

    public AxiomMcpTools(ArbiterHost arbiter)
    {
        _arbiter = arbiter;
    }

    [McpServerTool]
    public async Task<string> Ask(string input)
    {
        var intent = new IntentEnvelope
        {
            Source = "mcp",
            IntentType = "ask",
            Domain = "general",
            Payload = input
        };

        var result = await _arbiter.HandleIntentAsync(intent);

        return result ?? string.Empty;
    }
}
