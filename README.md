# LwXLabs.Mcp.Template.DotNet

A standalone C# MCP server template using .NET 10 and the official [MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk). Default stdio transport; optional local Streamable HTTP and ASP.NET Core MVC status page.

## Create your own MCP

```powershell
dotnet new install .
dotnet new lwx-mcp-dotnet -n MyCompany.Sample.Mcp -o MyCompany.Sample.Mcp
cd MyCompany.Sample.Mcp
dotnet test -c Release
```

Or clone the repository and build the included example directly.

## Run

```powershell
dotnet run --project src/LwXLabs.Mcp.Template.DotNet --no-launch-profile
dotnet run --project src/LwXLabs.Mcp.Template.DotNet --no-launch-profile -- --http
```

Stdio is started by the MCP client, not run in the background. HTTP mode exposes a local MVC status page at http://127.0.0.1:5188, health at `/health`, and MCP at `/mcp`. Override the port with `--http --port 5190`.

Example tools: `example.echo`, `example.add`, `example.status`.

## Windows installer

Requires the .NET 10 SDK and Inno Setup 6.

```powershell
./scripts/Build-Installer.ps1
```

Verify install, protocol and uninstall on a clean Windows test machine with `./scripts/Test-Installer.ps1`.

Setup is created in `artifacts/installer/`. Install per user without administrator privileges; no separate .NET runtime is required. The default install path is `%LOCALAPPDATA%\Programs\LwXLabs\LwXLabs.Mcp.Template.DotNet\LwXLabs.Mcp.Template.DotNet.exe`. Use the installed executable in your MCP client (see [example configuration](examples/mcp.example.json)).

For other platforms, run `./scripts/Publish.ps1 -Runtime linux-x64` (or another supported runtime).

## Customize

- `Tools/`: MCP tool definitions and descriptions.
- `Services/`: application logic and dependency injection.
- `Models/`: data contracts.
- `Controllers/` and `Views/`: optional MVC interface.
- `tests/`: unit and real MCP protocol tests.

Run `dotnet test -c Release`. To test an installed executable, set `MCP_TEMPLATE_TEST_EXECUTABLE` to its absolute path before running the tests.

HTTP mode is local development only. Do not expose it to the internet without authentication, authorization, TLS and a security review. No private libraries, credentials or updater services are required.

[MIT License](LICENSE).
