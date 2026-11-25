using System.ComponentModel;
using LwXLabs.Mcp.Template.DotNet.Services;
using ModelContextProtocol.Server;

namespace LwXLabs.Mcp.Template.DotNet.Tools;

[McpServerToolType]
public sealed class ExampleTools(ExampleService service)
{
    [McpServerTool(Name = "example.echo", ReadOnly = true, Destructive = false)]
    [Description("Returns the supplied text unchanged (maximum 4096 characters).")]
    public string Echo([Description("Text to echo.")] string text) => service.Echo(text);

    [McpServerTool(Name = "example.add", ReadOnly = true, Destructive = false)]
    [Description("Adds two signed 32-bit integers and returns their sum as a 64-bit integer.")]
    public long Add([Description("First integer.")] int a, [Description("Second integer.")] int b) => service.Add(a, b);

    [McpServerTool(Name = "example.status", ReadOnly = true, Destructive = false)]
    [Description("Returns the server name, version and health status.")]
    public object Status() => new { name = Models.ServerIdentity.Name, version = Models.ServerIdentity.Version, status = "ok" };
}
