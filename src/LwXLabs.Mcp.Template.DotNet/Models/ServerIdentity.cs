namespace LwXLabs.Mcp.Template.DotNet.Models;

public static class ServerIdentity
{
    public const string Name = "LwXLabs.Mcp.Template.DotNet";
    public static string Version => typeof(ServerIdentity).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
}

public sealed record ServerStatus(string Name, string Version, string Status, string Transport);
