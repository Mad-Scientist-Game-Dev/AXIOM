namespace Protocols;

public sealed class AxiomQuery
{
    public string ProjectRoot { get; init; } = "";
    public string Run { get; init; } = "current";
    public string[] Files { get; init; } = Array.Empty<string>();
}
