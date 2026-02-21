using Protocols;
using Router;

namespace Axiom.Arbiter;

public sealed class ArbiterHost
{
    public async Task<string?> HandleHumanInputAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var intent = new IntentEnvelope
        {
            Source = "human",
            IntentType = "ask",
            Domain = "general",
            Payload = input
        };

        return await RouterHost.HandleAsync(intent);
    }

    public async Task<string> HandleIntentAsync(IntentEnvelope intent)
    {
        return await RouterHost.HandleAsync(intent);
    }
}
