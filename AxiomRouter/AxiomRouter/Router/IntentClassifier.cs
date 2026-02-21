using Protocols;

namespace Router;

public static class IntentClassifier
{
    private static readonly DomainRegistry _registry =
        new DomainRegistry("domains.json");

    public static IntentEnvelope Classify(IntentEnvelope input)
    {
        var classification = _registry.Classify(
            input.Payload,
            input.IntentType
        );

        // Log scoring for observability
        Console.WriteLine("[Classifier] Scores:");
        foreach (var score in classification.Scores)
        {
            Console.WriteLine(
                $"  {score.Project.Name} => {score.Score}"
            );
        }

        Console.WriteLine(
            $"[Classifier] Resolved Domain: {classification.DomainType}"
        );

        return new IntentEnvelope
        {
            Source = input.Source,
            IntentType = input.IntentType,
            Domain = classification.DomainType,
            Payload = input.Payload
        };
    }
}
