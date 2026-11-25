using LwXLabs.Mcp.Template.DotNet.Models;

namespace LwXLabs.Mcp.Template.DotNet.Services;

// Replace this service with the domain logic for your own MCP.
public sealed class ExampleService
{
    public string Echo(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length > 4096)
            throw new ArgumentException("Text must be at most 4096 characters.", nameof(text));
        return text;
    }

    public long Add(int a, int b) => (long)a + b;

    public ServerStatus Status(string transport) => new(ServerIdentity.Name, ServerIdentity.Version, "ok", transport);
}
