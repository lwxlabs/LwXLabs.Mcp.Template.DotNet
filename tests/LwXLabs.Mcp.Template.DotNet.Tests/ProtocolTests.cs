using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace LwXLabs.Mcp.Template.DotNet.Tests;

public sealed class ProtocolTests
{
    [Fact]
    public async Task Stdio_discovers_and_invokes_tools()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var (command, args) = ServerCommand();
        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "template-smoke",
            Command = command,
            Arguments = args
        }), cancellationToken: timeout.Token);
        await VerifyTools(client, timeout.Token);
    }

    [Fact]
    public async Task Http_serves_mvc_and_mcp_and_rejects_foreign_origin()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var ct = timeout.Token;
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var url = $"http://127.0.0.1:{port}";
        var (command, args) = ServerCommand();
        var start = new ProcessStartInfo(command) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (var arg in args.Concat(["--http", "--port", port.ToString()])) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start HTTP server.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            while (true)
            {
                if (process.HasExited)
                    throw new InvalidOperationException($"Server exited: {await stdout}\n{await stderr}");
                try
                {
                    using var response = await http.GetAsync(url + "/health", ct);
                    if (response.IsSuccessStatusCode) break;
                }
                catch (HttpRequestException) { }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested) { }
                await Task.Delay(150, ct);
            }
            Assert.Contains("Local MCP server", await http.GetStringAsync(url, ct));
            using var foreign = new HttpRequestMessage(HttpMethod.Get, url + "/health");
            foreign.Headers.TryAddWithoutValidation("Origin", "https://example.invalid");
            using var foreignResponse = await http.SendAsync(foreign, ct);
            Assert.Equal(HttpStatusCode.Forbidden, foreignResponse.StatusCode);
            using var host = new HttpRequestMessage(HttpMethod.Get, url + "/health");
            host.Headers.Host = $"example.invalid:{port}";
            using var hostResponse = await http.SendAsync(host, ct);
            Assert.Equal(HttpStatusCode.Forbidden, hostResponse.StatusCode);
            await using var client = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(url + "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            }), cancellationToken: ct);
            await VerifyTools(client, ct);
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }
    }

    private static async Task VerifyTools(McpClient client, CancellationToken ct)
    {
        var tools = await client.ListToolsAsync(cancellationToken: ct);
        Assert.Equal(new[] { "example.add", "example.echo", "example.status" }, tools.Select(t => t.Name).Order().ToArray());
        var echo = await client.CallToolAsync("example.echo", new Dictionary<string, object?> { ["text"] = "hello-mcp" }, cancellationToken: ct);
        Assert.NotEqual(true, echo.IsError);
        Assert.Contains(echo.Content.OfType<TextContentBlock>(), block => block.Text.Contains("hello-mcp", StringComparison.Ordinal));
        var add = await client.CallToolAsync("example.add", new Dictionary<string, object?> { ["a"] = 19, ["b"] = 23 }, cancellationToken: ct);
        Assert.NotEqual(true, add.IsError);
        Assert.Contains(add.Content.OfType<TextContentBlock>(), block => block.Text.Contains("42", StringComparison.Ordinal));
        var status = await client.CallToolAsync("example.status", cancellationToken: ct);
        Assert.NotEqual(true, status.IsError);
        Assert.Contains(status.Content.OfType<TextContentBlock>(), block => block.Text.Contains("ok", StringComparison.Ordinal));
    }

    private static (string Command, string[] Arguments) ServerCommand()
    {
        var executable = Environment.GetEnvironmentVariable("MCP_TEMPLATE_TEST_EXECUTABLE");
        if (!string.IsNullOrWhiteSpace(executable))
            return (Path.GetFullPath(executable), []);
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        var configuration = root.Parent?.Name ?? "Release";
        while (root != null && !File.Exists(Path.Combine(root.FullName, "LwXLabs.Mcp.Template.DotNet.slnx")))
            root = root.Parent;
        if (root == null) throw new InvalidOperationException("Solution root not found.");
        var dll = Path.Combine(root.FullName, "src", "LwXLabs.Mcp.Template.DotNet", "bin", configuration, "net10.0", "LwXLabs.Mcp.Template.DotNet.dll");
        if (!File.Exists(dll)) throw new FileNotFoundException("Build the server first.", dll);
        return ("dotnet", [dll]);
    }
}
