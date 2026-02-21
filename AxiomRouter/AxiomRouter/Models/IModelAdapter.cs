using Protocols;

namespace Models;

public interface IModelAdapter
{
    string Name { get; }

    bool CanHandle(IntentEnvelope intent);

    ModelResponse Execute(IntentEnvelope intent);
}
