param([ValidateSet('win-x64','win-arm64','linux-x64','linux-arm64','osx-x64','osx-arm64')][string] $Runtime = 'win-x64')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src/LwXLabs.Mcp.Template.DotNet/LwXLabs.Mcp.Template.DotNet.csproj'
$destination = Join-Path $root "artifacts/publish/$Runtime"
if (Test-Path $destination) { Remove-Item -LiteralPath $destination -Recurse -Force }
dotnet restore $project --configfile (Join-Path $root 'NuGet.config') -r $Runtime
if ($LASTEXITCODE) { throw 'Restore failed.' }
dotnet publish $project -c Release -r $Runtime --self-contained true --no-restore -o $destination
if ($LASTEXITCODE) { throw 'Publish failed.' }
Copy-Item (Join-Path $root 'LICENSE') $destination
$exe = 'LwXLabs.Mcp.Template.DotNet' + $(if ($Runtime.StartsWith('win-')) { '.exe' } else { '' })
if (-not (Test-Path (Join-Path $destination $exe))) { throw 'Missing published executable.' }
Write-Host "Published: $destination"
