param([string] $Installer)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$identity = '{B3D38780-5F23-4FD5-9CA2-5A76B3C1AE6E}'
$uninstallKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\${identity}_is1"
if (Test-Path $uninstallKey) { throw 'An installation already exists. Uninstall it first or test on a clean machine.' }
if (-not $Installer) {
    $Installer = Get-ChildItem (Join-Path $root 'artifacts/installer') -Filter 'LwXLabs.Mcp.Template.DotNet-*-Setup.exe' -File |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $Installer -or -not (Test-Path -LiteralPath $Installer -PathType Leaf)) { throw 'Build the installer first.' }
$Installer = (Resolve-Path -LiteralPath $Installer).Path
$workspace = Join-Path ([IO.Path]::GetTempPath()) ('mcp-installer-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $workspace | Out-Null
$destination = Join-Path $workspace 'installed'
$setupLog = Join-Path $workspace 'setup.log'
$uninstallLog = Join-Path $workspace 'uninstall.log'
$oldTestPath = $env:MCP_TEMPLATE_TEST_EXECUTABLE
try {
    $arguments = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/DIR=`"$destination`"", "/LOG=`"$setupLog`"")
    $setup = Start-Process -FilePath $Installer -ArgumentList $arguments -PassThru -Wait
    if ($setup.ExitCode -ne 0) { throw "Setup failed: $($setup.ExitCode). Log: $setupLog" }
    $exe = Join-Path $destination 'LwXLabs.Mcp.Template.DotNet.exe'
    if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw 'Installed executable missing.' }
    if (-not (Test-Path $uninstallKey)) { throw 'Uninstall registration missing.' }
    $health = & $exe --health | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $health.status -ne 'ok' -or $health.name -ne 'LwXLabs.Mcp.Template.DotNet') { throw 'Installed health probe failed.' }
    Write-Host "Installed executable healthy: $($health.name) $($health.version)"
    $env:MCP_TEMPLATE_TEST_EXECUTABLE = $exe
    dotnet test (Join-Path $root 'LwXLabs.Mcp.Template.DotNet.slnx') -c Release --logger 'console;verbosity=normal'
    if ($LASTEXITCODE) { throw 'Installed MCP protocol tests failed.' }
}
finally {
    $env:MCP_TEMPLATE_TEST_EXECUTABLE = $oldTestPath
    $uninstaller = Join-Path $destination 'unins000.exe'
    if (Test-Path -LiteralPath $uninstaller -PathType Leaf) {
        $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/LOG=`"$uninstallLog`"") -PassThru -Wait
        if ($uninstall.ExitCode -ne 0 -or (Test-Path $uninstallKey) -or (Test-Path (Join-Path $destination 'LwXLabs.Mcp.Template.DotNet.exe'))) {
            throw "Test uninstall failed. Logs: $workspace"
        }
        Write-Host 'Temporary installation removed successfully.'
    }
    Write-Host "Installer test logs: $workspace"
}
