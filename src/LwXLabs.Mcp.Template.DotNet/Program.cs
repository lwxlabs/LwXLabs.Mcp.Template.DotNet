using System.Net;
using System.Text.Json;
using LwXLabs.Mcp.Template.DotNet.Models;
using LwXLabs.Mcp.Template.DotNet.Services;
using LwXLabs.Mcp.Template.DotNet.Tools;
using Microsoft.Extensions.Logging;

if (args is ["--health"])
{
    Console.WriteLine(JsonSerializer.Serialize(new { status = "ok", name = ServerIdentity.Name, version = ServerIdentity.Version }));
    return 0;
}
if (args is ["--version"])
{
    Console.WriteLine(ServerIdentity.Version);
    return 0;
}
if (args is ["--help"])
{
    Console.WriteLine("Usage: LwXLabs.Mcp.Template.DotNet [--http [--port 5188] | --health | --version | --help]");
    return 0;
}

if (args.Length == 0)
{
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
        Args = [],
        ContentRootPath = AppContext.BaseDirectory
    });
    // MCP stdio uses stdout exclusively for JSON-RPC.
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
    builder.Services.AddSingleton<ExampleService>();
    builder.Services.AddMcpServer().WithStdioServerTransport().WithTools<ExampleTools>();
    using var host = builder.Build();
    await host.RunAsync();
    return 0;
}

if (args[0] != "--http" || (args.Length != 1 && args.Length != 3))
{
    Console.Error.WriteLine("Invalid arguments. Use --help.");
    return 2;
}
var port = 5188;
if (args.Length == 3 && (args[1] != "--port" || !int.TryParse(args[2], out port) || port is < 1 or > 65535))
{
    Console.Error.WriteLine("Invalid port. Use --help.");
    return 2;
}

var web = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = [],
    ContentRootPath = AppContext.BaseDirectory
});
web.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port));
web.Services.AddControllersWithViews();
web.Services.AddSingleton<ExampleService>();
web.Services.AddMcpServer().WithHttpTransport().WithTools<ExampleTools>();

var app = web.Build();
// Local development only. Reject non-loopback peers, untrusted Host headers
// (DNS rebinding), and foreign browser origins. No CORS or public listener.
app.Use(async (context, next) =>
{
    var host = context.Request.Host;
    var validHost = host.Port == port && (host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host.Host == "127.0.0.1");
    var origin = context.Request.Headers.Origin;
    var validOrigin = origin.Count == 0 || (origin.Count == 1 &&
        (origin[0] == $"http://127.0.0.1:{port}" || origin[0] == $"http://localhost:{port}"));
    if (context.Connection.RemoteIpAddress is not { } peer || !IPAddress.IsLoopback(peer) || !validHost || !validOrigin)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Cache-Control"] = "no-store";
    await next();
});
app.MapControllers();
app.MapMcp("/mcp");
await app.RunAsync();
return 0;
