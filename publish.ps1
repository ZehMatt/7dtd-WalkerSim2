param(
    [ValidateSet("all", "mod", "editor")]
    [string]$Target = "all",

    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$RootDir = $PSScriptRoot
$PublishDir = Join-Path $RootDir "Publish"

function Get-CurrentRid {
    if ($IsWindows -or ($env:OS -eq "Windows_NT")) {
        return "win-x64"
    } elseif ($IsMacOS) {
        $arch = uname -m
        if ($arch -eq "arm64") { return "osx-arm64" }
        return "osx-x64"
    } elseif ($IsLinux) {
        return "linux-x64"
    }
    throw "Unsupported OS"
}

function Get-PlatformName([string]$RuntimeId) {
    switch ($RuntimeId) {
        "win-x64"   { "Windows" }
        "linux-x64" { "Linux" }
        "osx-x64"   { "Mac" }
        "osx-arm64" { "Mac" }
    }
}

function Find-MSBuild {
    # Try vswhere first
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $installPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -property installationPath 2>$null
        if ($installPath) {
            $msbuild = Join-Path $installPath "MSBuild\Current\Bin\MSBuild.exe"
            if (Test-Path $msbuild) { return $msbuild }
        }
    }

    # Try PATH
    $msbuild = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($msbuild) { return $msbuild.Path }

    throw "MSBuild not found. Install Visual Studio or Build Tools."
}

function Publish-Mod {
    Write-Host "=== Publishing Mod ===" -ForegroundColor Cyan

    $outputDir = Join-Path $PublishDir "Mod"

    if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

    $msbuild = Find-MSBuild
    Write-Host "Using MSBuild: $msbuild"

    & $msbuild (Join-Path $RootDir "Mod\Mod.csproj") `
        /p:Configuration=$Configuration `
        /p:Platform=x64 `
        /t:Build `
        /restore `
        /v:minimal

    if ($LASTEXITCODE -ne 0) { throw "Mod build failed" }

    # Copy build output to publish directory
    $buildOutput = Join-Path $RootDir "Build\$Configuration\Mod"
    Copy-Item (Join-Path $buildOutput "*") $outputDir -Recurse -Force

    Write-Host "Mod published to: $outputDir" -ForegroundColor Green
}

function Publish-Editor {
    param([string]$RuntimeId)

    $platformName = Get-PlatformName $RuntimeId

    Write-Host "=== Publishing Editor for $platformName ($RuntimeId) ===" -ForegroundColor Cyan

    $outputDir = Join-Path $PublishDir "Editor\$platformName"

    if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }

    dotnet publish (Join-Path $RootDir "Editor\Editor.csproj") `
        -c $Configuration `
        -r $RuntimeId `
        -o $outputDir `
        -p:Platform=x64 `
        --self-contained true

    if ($LASTEXITCODE -ne 0) { throw "Editor publish failed for $RuntimeId" }

    Write-Host "Editor published to: $outputDir" -ForegroundColor Green
}

# --- Main ---

$rid = Get-CurrentRid
$platformName = Get-PlatformName $rid

Write-Host "Publish Target: $Target" -ForegroundColor Yellow
Write-Host "Detected OS:    $platformName ($rid)" -ForegroundColor Yellow
Write-Host ""

# Publish Mod (Windows only — requires MSBuild + .NET Framework)
if ($Target -eq "all" -or $Target -eq "mod") {
    if ($rid -like "win-*") {
        Publish-Mod
        Write-Host ""
    } else {
        Write-Host "Skipping Mod — requires Windows with MSBuild." -ForegroundColor DarkYellow
        Write-Host ""
    }
}

# Publish Editor (AOT for current OS)
if ($Target -eq "all" -or $Target -eq "editor") {
    Publish-Editor -RuntimeId $rid
    Write-Host ""
}

Write-Host "Publish complete!" -ForegroundColor Green
