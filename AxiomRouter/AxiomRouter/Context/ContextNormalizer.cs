using System.Text;
using System.Text.Json;

namespace Context;

public static class ContextNormalizer
{
    public static NormalizedContext Normalize(
        string bundleJson,
        string userInput)
    {
        Console.WriteLine("=== RAW BUNDLE JSON (first 1000 chars) ===");
        Console.WriteLine(bundleJson.Substring(0, Math.Min(1000, bundleJson.Length)));
        Console.WriteLine("==========================================");

        var bundle = JsonSerializer.Deserialize<ContextBundle>(
            bundleJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException("Invalid bundle JSON");

        // ------------------------------------------------------------
        // SYSTEM DOMAIN
        // ------------------------------------------------------------
        if (bundle.system != null)
        {
            var sys = bundle.system;

            return new NormalizedContext
            {
                SystemPrompt = BaseSystemPrompt(),
                UserPrompt =
$"""
User asked:
{userInput}

=== SYSTEM SNAPSHOT ===
Machine: {sys.machine_name}
OS Version: {sys.os_version}
Process ID: {sys.process_id}
.NET Version: {sys.dotnet_version}
CPU Cores: {sys.processor_count}
Working Set (bytes): {sys.working_set_bytes}

Answer strictly using the SYSTEM SNAPSHOT above.
If the information is insufficient, say so explicitly.
"""
            };
        }

        // ------------------------------------------------------------
        // CODE DOMAIN
        // ------------------------------------------------------------
        if (bundle.files != null && bundle.files.Length > 0)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== AXIOM CONTEXT BUNDLE ===");
            sb.AppendLine($"Run ID: {bundle.meta.run_id}");
            sb.AppendLine($"Project Root: {bundle.meta.project_root}");
            sb.AppendLine();

            foreach (var f in bundle.files)
            {
                sb.AppendLine($"File: {f.path}");
                sb.AppendLine($"  SHA256: {f.sha256}");
                sb.AppendLine($"  Size: {f.size_bytes}");
                sb.AppendLine($"  Last Modified: {f.last_modified_utc}");
                sb.AppendLine();
            }

            return new NormalizedContext
            {
                SystemPrompt = BaseSystemPrompt() + "\n\n" + sb.ToString(),
                UserPrompt =
$"""
User asked:
{userInput}

Answer strictly using the AXIOM CONTEXT BUNDLE above.
If insufficient information is present, say so explicitly.
"""
            };
        }

        // ------------------------------------------------------------
        // NO CONTEXT
        // ------------------------------------------------------------
        return new NormalizedContext
        {
            SystemPrompt = BaseSystemPrompt(),
            UserPrompt = userInput
        };
    }

    private static string BaseSystemPrompt() =>
"""
You are Axiom.
You must not invent facts.
You may reason, but only from provided context.
If context is missing, say so.
""";
}
