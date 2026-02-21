using Protocols;

namespace Models;

public sealed class EchoModel : IModelAdapter
{
    public string Name => "echo";

    public bool CanHandle(IntentEnvelope intent)
    => intent.Domain == "debug";

    public ModelResponse Execute(IntentEnvelope intent)
    {
        return new ModelResponse
        {
            ModelName = Name,
            Output = $"[echo] {intent.Payload}"
        };
    }
}
