using System.Net.Http.Json;
using Protocols;

namespace Axiom.Arbiter;

public sealed class BridgeClient
{
    private readonly HttpClient _http;

    public BridgeClient(string baseUrl)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<string> BundleAsync(
        string targetRoot,
        string intentType,
        string domain,
        string runSelector,
        string[]? files = null)
    {
        var request = new
        {
            command = "bundle",
            targetRoot,
            intentType,
            domain,
            runSelector,
            files
        };

        var response = await _http.PostAsJsonAsync("/axiom", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BridgeResult>();
        return result?.stdout ?? "";
    }

    private sealed class BridgeResult
    {
        public bool ok { get; set; }
        public int exit_code { get; set; }
        public string stdout { get; set; } = "";
        public string stderr { get; set; } = "";
    }
}
