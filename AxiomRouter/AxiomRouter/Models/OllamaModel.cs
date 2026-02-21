using System.Net.Http.Json;
using System.Text.Json;
using Protocols;

namespace Models;

public sealed class OllamaModel : IModelAdapter
{
    public string Name => "ollama:qwen2.5-coder";

    private static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri("http://localhost:11434")
    };

    public bool CanHandle(IntentEnvelope intent)
        => intent.IntentType == "ask"
        || intent.IntentType == "design"
        || intent.IntentType == "explain";

    public ModelResponse Execute(IntentEnvelope intent)
    {
        var payload = new
        {
            model = "qwen2.5-coder:7b-instruct",
            prompt = BuildPrompt(intent),
            stream = false
        };

        var response = Http.PostAsJsonAsync("/api/generate", payload).Result;
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(
            response.Content.ReadAsStringAsync().Result
        );

        var output = doc.RootElement
            .GetProperty("response")
            .GetString();

        return new ModelResponse
        {
            ModelName = Name,
            Output = output ?? ""
        };
    }

    private static string BuildPrompt(IntentEnvelope intent)
    {
        return intent.Payload;
    }
}
