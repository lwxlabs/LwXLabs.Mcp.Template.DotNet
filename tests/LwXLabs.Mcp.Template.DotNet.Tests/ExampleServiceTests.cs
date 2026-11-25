using LwXLabs.Mcp.Template.DotNet.Services;
using Xunit;

namespace LwXLabs.Mcp.Template.DotNet.Tests;

public sealed class ExampleServiceTests
{
    private readonly ExampleService _service = new();

    [Fact]
    public void Echo_returns_original_text() => Assert.Equal("Hello, MCP!", _service.Echo("Hello, MCP!"));

    [Fact]
    public void Echo_rejects_oversized_input() => Assert.Throws<ArgumentException>(() => _service.Echo(new string('x', 4097)));

    [Fact]
    public void Add_avoids_integer_overflow() => Assert.Equal(4294967294L, _service.Add(int.MaxValue, int.MaxValue));
}
