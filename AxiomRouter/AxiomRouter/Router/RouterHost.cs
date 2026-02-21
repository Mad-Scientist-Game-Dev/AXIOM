using Axiom.Arbiter;
using Context;
using Models;
using Protocols;
using Router.Memory;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Router;

public static class RouterHost
{
    private static readonly HttpClient _http = new();

    private const string CoreBundleUrl = "https://192.168.1.101:7071/bundle";

    private static readonly IModelAdapter[] _models =
    {
        new EchoModel(),
        new OllamaModel()
    };

    private static readonly ConversationalMemory _memory =
        new ConversationalMemory("memory.json");

    private static readonly DomainRegistry _registry =
        new DomainRegistry("domains.json");

    // --------------------------------------------------------
    // ENTRY POINT
    // --------------------------------------------------------
    public static async Task<string> HandleAsync(IntentEnvelope intent)
    {
        Console.WriteLine("=== ROUTER BUILD ID ===");
        Console.WriteLine(typeof(RouterHost).Assembly.FullName);
        Console.WriteLine(typeof(RouterHost).Assembly.Location);
        Console.WriteLine("=======================");
        Console.WriteLine($"[Router] {intent.IntentType} :: {intent.Payload}");

        // --------------------------------------------------------
        // DOMAIN REGISTRY CLASSIFICATION
        // --------------------------------------------------------
        var classification = _registry.Classify(intent.Payload, intent.IntentType);

        Console.WriteLine("[Classifier] Scores:");
        foreach (var score in classification.Scores)
            Console.WriteLine($"  {score.Project.Name} -> {score.Score}");

        Console.WriteLine($"[Classifier] Resolved Domain: {classification.DomainType}");
        Console.WriteLine($"[Classifier] Explanation:\n{classification.Explanation}");

        // Build routed intent from classification result
        var routedIntent = new IntentEnvelope
        {
            Source = intent.Source,
            IntentType = intent.IntentType,
            Domain = classification.DomainType,
            Payload = intent.Payload
        };

        // --------------------------------------------------------
        // DEPENDENCY RESOLUTION
        // --------------------------------------------------------
        var decision = ResolveDependencies(classification.DomainType);

        Console.WriteLine("[Router] Dependency decision:");
        Console.WriteLine($"  Reason: {decision.Reason}");
        Console.WriteLine($"  BundleAll: {decision.BundleAll}");
        Console.WriteLine(
            $"  Files: {(decision.Files.Length == 0 ? "(none)" : string.Join(", ", decision.Files))}"
        );

        string bridgeBody = "{}";

        // --------------------------------------------------------
        // BRIDGE CALL
        // --------------------------------------------------------
        if (!IsBundleFreeIntent(routedIntent))
        {
            var payload = new
            {
                command = "bundle",
                targetRoot = "/home/axiom/axiom/Axiom.Bridge",
                intentType = routedIntent.IntentType,
                domain = routedIntent.Domain,
                runSelector = "latest",
                files = decision.Files,
                bundleAll = decision.BundleAll
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsync(CoreBundleUrl, content);
            }
            catch (Exception ex)
            {
                var msg = $"[Router] Bridge call failed: {ex.GetType().Name}: {ex.Message}";
                Console.WriteLine(msg);
                return msg;
            }

            bridgeBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[Router] Bridge payload length: {bridgeBody.Length}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Router] Bridge error:");
                Console.WriteLine(bridgeBody);
                return bridgeBody;
            }
        }
        else
        {
            Console.WriteLine("[Router] Explicit bundle-free intent.");
        }

        // --------------------------------------------------------
        // MEMORY CONTEXT
        // --------------------------------------------------------
        var memoryContext = BuildMemoryContext();

        // --------------------------------------------------------
        // CONTEXT NORMALIZATION
        // --------------------------------------------------------
        var normalized = ContextNormalizer.Normalize(
            bridgeBody,
            routedIntent.Payload
        );

        if (!string.IsNullOrWhiteSpace(memoryContext))
        {
            normalized = new NormalizedContext
            {
                SystemPrompt = normalized.SystemPrompt
                    + "\n\n=== CONVERSATIONAL MEMORY ===\n"
                    + memoryContext,
                UserPrompt = normalized.UserPrompt
            };
        }

        Console.WriteLine("[Router] SYSTEM PROMPT:");
        Console.WriteLine(normalized.SystemPrompt);

        Console.WriteLine("[Router] USER PROMPT:");
        Console.WriteLine(normalized.UserPrompt);

        // --------------------------------------------------------
        // MODEL SELECTION
        // --------------------------------------------------------
        var adapter = _models.FirstOrDefault(m => m.CanHandle(routedIntent));

        if (adapter == null)
        {
            const string msg = "[Router] No model adapter can handle this intent.";
            Console.WriteLine(msg);
            return msg;
        }

        // --------------------------------------------------------
        // MODEL EXECUTION
        // --------------------------------------------------------
        var modelResponse = adapter.Execute(
            new IntentEnvelope
            {
                Source = routedIntent.Source,
                IntentType = routedIntent.IntentType,
                Domain = routedIntent.Domain,
                Payload = normalized.UserPrompt
            }
        );

        Console.WriteLine($"[Router] MODEL RESPONSE ({adapter.Name}):");
        Console.WriteLine(modelResponse.Output);

        return modelResponse.Output ?? "";
    }

    // --------------------------------------------------------
    // MEMORY HELPER
    // --------------------------------------------------------
    private static string BuildMemoryContext()
    {
        var snapshot = _memory.Snapshot();
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(snapshot.User.Name))
            sb.AppendLine($"User Name: {snapshot.User.Name}");

        foreach (var pref in snapshot.User.Preferences)
            sb.AppendLine($"{pref.Key}: {pref.Value}");

        return sb.ToString();
    }

    // --------------------------------------------------------
    // DEPENDENCY RESOLUTION (TEMPORARY)
    // --------------------------------------------------------
    private static DependencyDecision ResolveDependencies(string domainType)
    {
        return domainType switch
        {
            "system" => new DependencyDecision(
                "System domain",
                Array.Empty<string>(),
                BundleAll: false
            ),

            "code_project" => new DependencyDecision(
                "Project code domain",
                Array.Empty<string>(),
                BundleAll: true
            ),

            _ => new DependencyDecision(
                "General domain",
                Array.Empty<string>(),
                BundleAll: false
            )
        };
    }

    private static bool IsBundleFreeIntent(IntentEnvelope intent)
    {
        return intent.IntentType is "health" or "ping";
    }
}
