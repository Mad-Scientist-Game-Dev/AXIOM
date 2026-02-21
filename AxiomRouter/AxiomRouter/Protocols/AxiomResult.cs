namespace Protocols;

public sealed class AxiomResult
{
    public string File { get; init; } = "";
    public string Sha256 { get; init; } = "";
    public long SizeBytes { get; init; }
    public string LastModifiedUtc { get; init; } = "";
    public string Content { get; init; } = "";
}
