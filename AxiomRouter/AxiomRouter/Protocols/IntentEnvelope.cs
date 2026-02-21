namespace Protocols;

public sealed class IntentEnvelope
{
    public string Source { get; init; } = "";        // human, model, tool
    public string IntentType { get; init; } = "";    // ask, design, modify, explain
    public string Domain { get; init; } = "";        // code, system, game, data
    public string Payload { get; init; } = "";       // raw text or JSON
    public DateTime ReceivedUtc { get; init; } = DateTime.UtcNow;
}
