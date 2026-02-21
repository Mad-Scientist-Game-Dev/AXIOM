using System;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Axiom.Host
{
    internal static class CoreSupervisor
    {
        private static readonly string CoreBaseUrl =
            Environment.GetEnvironmentVariable("AXIOM_CORE_BASEURL")
            ?? "https://192.168.1.101:7071";

        private static readonly string CoreHealthUrl = $"{CoreBaseUrl.TrimEnd('/')}/health";

        private const string SshHost = "192.168.1.101";
        private const string SshUser = "axiom";

        // Secrets (DO NOT hardcode)
        private static readonly string? SshPassword =
            Environment.GetEnvironmentVariable("AXIOM_SSH_PASSWORD");

        private static readonly string? CoreToken =
            Environment.GetEnvironmentVariable("AXIOM_CORE_TOKEN");

        // Cert pin (SHA256 fingerprint). Example format:
        // "A1B2C3...FF" (no colons). Case-insensitive.
        private static readonly string? CoreCertSha256 =
            Environment.GetEnvironmentVariable("AXIOM_CORE_CERT_SHA256");

        // Start Core.Service (adjust path to where you actually keep it)
        // NOTE: This assumes Core.Service is already configured to serve HTTPS with its cert.
        private const string StartCommand =
            "cd /home/axiom/axiom/Axiom.Core.Service && " +
            "nohup dotnet run --urls https://0.0.0.0:7071 > core.log 2>&1 &";

        public static async Task EnsureRunning()
        {
            if (await IsRunning())
            {
                Console.WriteLine("[Host] Core.Service online.");
                return;
            }

            Console.WriteLine("[Host] Core.Service offline - starting via SSH.");
            StartViaSsh();
            await WaitForStartup();
        }

        private static async Task<bool> IsRunning()
        {
            try
            {
                if (CoreToken is null)
                    throw new InvalidOperationException("AXIOM_CORE_TOKEN env var not set.");

                if (CoreCertSha256 is null)
                    throw new InvalidOperationException("AXIOM_CORE_CERT_SHA256 env var not set.");

                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = ValidatePinnedCertificate
                };

                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(2)
                };

                client.DefaultRequestHeaders.Remove("X-Axiom-Token");
                client.DefaultRequestHeaders.Add("X-Axiom-Token", CoreToken);

                var response = await client.GetAsync(CoreHealthUrl);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Host] Core health check failed: {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        private static bool ValidatePinnedCertificate(
            HttpRequestMessage req,
            X509Certificate2? cert,
            X509Chain? chain,
            SslPolicyErrors errors)
        {
            // We are explicitly pinning, so we don't care about trust-chain errors on LAN
            // as long as we match the expected fingerprint.
            if (cert is null || CoreCertSha256 is null)
                return false;

            var expected = NormalizeFingerprint(CoreCertSha256);
            var actual = NormalizeFingerprint(GetSha256Fingerprint(cert));

            var ok = string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);

            if (!ok)
            {
                Console.WriteLine("[Host] Core cert pin mismatch.");
                Console.WriteLine($"[Host] Expected: {expected}");
                Console.WriteLine($"[Host] Actual:   {actual}");
            }

            return ok;
        }

        private static string GetSha256Fingerprint(X509Certificate2 cert)
        {
            // SHA256 over DER cert
            var hash = cert.GetCertHash(HashAlgorithmName.SHA256);
            return Convert.ToHexString(hash);
        }

        private static string NormalizeFingerprint(string fp)
        {
            return new string(fp.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        }

        private static void StartViaSsh()
        {
            if (SshPassword is null)
                throw new InvalidOperationException("AXIOM_SSH_PASSWORD env var not set.");

            using var ssh = new SshClient(SshHost, SshUser, SshPassword);
            ssh.Connect();
            ssh.RunCommand(StartCommand);
            ssh.Disconnect();
        }

        private static async Task WaitForStartup()
        {
            const int maxAttempts = 25;

            for (int i = 0; i < maxAttempts; i++)
            {
                Console.WriteLine("[Host] Negotiating with Core.Service...");
                if (await IsRunning())
                    return;

                await Task.Delay(1000);
            }

            throw new Exception("[Host] Core.Service start failed - timeout.");
        }
    }
}
