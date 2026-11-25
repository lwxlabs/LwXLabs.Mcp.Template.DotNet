using LwXLabs.Mcp.Template.DotNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace LwXLabs.Mcp.Template.DotNet.Controllers;

public sealed class HomeController(ExampleService service) : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View(service.Status("Streamable HTTP"));

    [HttpGet("/health")]
    public IActionResult Health() => Ok(service.Status("Streamable HTTP"));
}
