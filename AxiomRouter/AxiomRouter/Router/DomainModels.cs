public sealed class DomainConfig
{
    public List<ProjectDefinition> Projects { get; set; } = new();
}

public sealed class ProjectDefinition
{
    public string Name { get; set; } = "";
    public string DomainType { get; set; } = "general";
    public double Weight { get; set; } = 1.0;

    public List<string>? Hints { get; set; }
    public List<string>? NegativeHints { get; set; }

    public List<string>? RelatedDomains { get; set; }
    public string Root { get; set; } = "";

}

public sealed class ClassificationResult
{
    public string DomainType { get; set; } = "general";
    public List<ProjectDefinition> Projects { get; set; } = new();
    public List<ProjectScore> Scores { get; set; } = new();
    public string Explanation { get; set; } = "";
}

public sealed class ProjectScore
{
    public ProjectDefinition Project { get; set; } = null!;
    public double Score { get; set; }
}
