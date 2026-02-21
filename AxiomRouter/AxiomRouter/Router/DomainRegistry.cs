using System.Text.Json;

namespace Router;

public sealed class DomainRegistry
{
    private readonly DomainConfig _config;

    public DomainRegistry(string configPath)
    {
        if (!File.Exists(configPath))
            throw new FileNotFoundException($"domains.json not found at {configPath}");

        var json = File.ReadAllText(configPath);

        _config = JsonSerializer.Deserialize<DomainConfig>(
     json,
     new JsonSerializerOptions
     {
         PropertyNameCaseInsensitive = true
     })
     ?? throw new InvalidOperationException("Failed to parse domains.json");
    }

    public ClassificationResult Classify(string text, string intentType)
    {
        text = text.ToLowerInvariant();

        var scoredProjects = new List<ProjectScore>();

        foreach (var project in _config.Projects)
        {
            var score = ScoreProject(project, text);

            if (score > 0)
            {
                scoredProjects.Add(new ProjectScore
                {
                    Project = project,
                    Score = score
                });
            }
        }

        scoredProjects = scoredProjects
            .OrderByDescending(p => p.Score)
            .ToList();

        return new ClassificationResult
        {
            DomainType = ResolveDomainType(scoredProjects),
            Projects = scoredProjects.Select(p => p.Project).ToList(),
            Scores = scoredProjects,
            Explanation = BuildExplanation(scoredProjects)
        };
    }

    // ---------------------------------------------------------
    // SCORING
    // ---------------------------------------------------------
    private static double ScoreProject(ProjectDefinition project, string text)
    {
        double score = 0;

        // Positive hints
        if (project.Hints != null)
        {
            foreach (var hint in project.Hints)
            {
                if (!string.IsNullOrWhiteSpace(hint) &&
                    text.Contains(hint.ToLowerInvariant()))
                {
                    score += 1;
                }
            }
        }

        // Negative hints
        if (project.NegativeHints != null)
        {
            foreach (var negative in project.NegativeHints)
            {
                if (!string.IsNullOrWhiteSpace(negative) &&
                    text.Contains(negative.ToLowerInvariant()))
                {
                    score -= 1.5;
                }
            }
        }

        return score * project.Weight;
    }

    // ---------------------------------------------------------
    // DOMAIN RESOLUTION
    // ---------------------------------------------------------
    private static string ResolveDomainType(List<ProjectScore> scores)
    {
        if (scores.Count == 0)
            return "general";

        return scores.First().Project.DomainType;
    }

    // ---------------------------------------------------------
    // EXPLANATION
    // ---------------------------------------------------------
    private static string BuildExplanation(List<ProjectScore> scores)
    {
        if (scores.Count == 0)
            return "No domain hints matched.";

        return string.Join(
            Environment.NewLine,
            scores.Select(s =>
                $"Matched project '{s.Project.Name}' with score {s.Score}")
        );
    }
}
