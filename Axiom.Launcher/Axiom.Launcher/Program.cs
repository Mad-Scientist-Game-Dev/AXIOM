using Axiom.Arbiter;
using Axiom.Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------
// KESTREL CONFIG (LAN BIND)
// ------------------------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // HTTP on LAN
    // Later: add HTTPS with cert
});

// ------------------------------------
// SERVICES
// ------------------------------------
builder.Services.AddSingleton<ArbiterHost>();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<AxiomMcpTools>();

var app = builder.Build();

// ------------------------------------
// CORE SUPERVISION
// ------------------------------------
CoreSupervisor
    .EnsureRunning()
    .GetAwaiter()
    .GetResult();

Console.WriteLine("AXIOM HOST ONLINE (LAN MODE)");

app.Run();
