#requires -Version 7.0
[CmdletBinding()]
param(
    [string]$DepotDownloader = "E:\DepotDownloader\DepotDownloader.exe",
    [string]$ModDll,
    [string]$VersionsFile = "$PSScriptRoot\versions.json",
    [string]$AppId = "294420",
    [string]$TargetAssembly = "Assembly-CSharp",
    [string[]]$Only,
    [string]$Username,
    [switch]$ForceDownload
)

$ErrorActionPreference = "Stop"
$repo = Split-Path $PSScriptRoot -Parent
$ci = $env:GITHUB_ACTIONS -eq "true"

function Group-Start($name) { if ($ci) { Write-Host "::group::$name" } }
function Group-End { if ($ci) { Write-Host "::endgroup::" } }

if (-not $ModDll) {
    $release = Join-Path $repo "Build\Release\Mod\WalkerSimMod.dll"
    $debug = Join-Path $repo "Build\Debug\Mod\WalkerSimMod.dll"
    if (Test-Path $release) { $ModDll = $release }
    elseif (Test-Path $debug) { $ModDll = $debug }
    else { throw "Could not find WalkerSimMod.dll. Build the mod first or pass -ModDll." }
}
if (-not (Test-Path $ModDll)) { throw "Mod assembly not found: $ModDll" }
if (-not (Test-Path $DepotDownloader)) { throw "DepotDownloader not found: $DepotDownloader" }
if (-not (Test-Path $VersionsFile)) { throw "Versions file not found: $VersionsFile" }

$fileList = Join-Path $repo "Binaries\required_files.txt"
if (-not (Test-Path $fileList)) { throw "required_files.txt not found: $fileList" }

$cacheRoot = Join-Path $PSScriptRoot ".gamebins"
$checkerProj = Join-Path $PSScriptRoot "CheckApiCompat\CheckApiCompat.csproj"
$checkerDll = Join-Path $PSScriptRoot "CheckApiCompat\bin\Release\net8.0\CheckApiCompat.dll"

Group-Start "Build checker"
dotnet build $checkerProj -c Release -v quiet
if ($LASTEXITCODE -ne 0) { throw "Failed to build CheckApiCompat." }
if (-not (Test-Path $checkerDll)) { throw "Checker build output missing: $checkerDll" }
Group-End

$versions = Get-Content $VersionsFile -Raw | ConvertFrom-Json
if ($Only) { $versions = $versions | Where-Object { $Only -contains $_.name } }
if (-not $versions) { throw "No versions selected." }

$results = @()

foreach ($v in $versions) {
    $name = $v.name
    $dir = Join-Path $cacheRoot $name
    $managed = Join-Path $dir "7DaysToDieServer_Data\Managed"
    $missing = @()

    if ($v.manifest -and ($v.manifest -like "REPLACE_*")) {
        $results += [pscustomobject]@{ Name = $name; Status = "skipped"; Missing = $missing }
        continue
    }

    $needDownload = $ForceDownload -or -not (Test-Path (Join-Path $managed "$TargetAssembly.dll"))
    if ($needDownload) {
        if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
        New-Item -ItemType Directory -Path $dir -Force | Out-Null

        $ddArgs = @("-app", $AppId, "-depot", $v.depot, "-filelist", $fileList, "-dir", $dir)
        if ($v.manifest) { $ddArgs += @("-manifest", $v.manifest) }
        if ($v.branch) { $ddArgs += @("-branch", $v.branch) }
        if ($v.branchpassword) { $ddArgs += @("-branchpassword", $v.branchpassword) }
        if ($Username) { $ddArgs += @("-username", $Username, "-remember-password") }

        if ($ci) {
            $ddLog = & $DepotDownloader @ddArgs 2>&1
            $ddCode = $LASTEXITCODE
        }
        else {
            Write-Host "downloading $name (depot $($v.depot))..." -ForegroundColor DarkGray
            & $DepotDownloader @ddArgs
            $ddCode = $LASTEXITCODE
        }

        if ($ddCode -ne 0) {
            if ($ci) {
                Group-Start "download $name (failed)"
                $ddLog | ForEach-Object { Write-Host $_ }
                Group-End
                Write-Host "::error title=API compat ($name)::DepotDownloader exited $ddCode"
            }
            else {
                Write-Host "  FAIL: DepotDownloader exited $ddCode" -ForegroundColor Red
            }
            $results += [pscustomobject]@{ Name = $name; Status = "download-failed"; Missing = $missing }
            continue
        }
    }

    if (-not (Test-Path (Join-Path $managed "$TargetAssembly.dll"))) {
        Write-Host "::error title=API compat ($name)::$TargetAssembly.dll not present after download"
        $results += [pscustomobject]@{ Name = $name; Status = "missing-assembly"; Missing = $missing }
        continue
    }

    $checkOut = & dotnet $checkerDll $ModDll $managed $TargetAssembly 2>&1
    $code = $LASTEXITCODE
    $missing = @($checkOut | Where-Object { $_ -match '^\s*MISSING\s' } | ForEach-Object { (($_ -replace '^\s*MISSING\s+', '') -replace '\s+', ' ').Trim() })

    Group-Start $name
    $checkOut | ForEach-Object { Write-Host $_ }
    Group-End

    $status = if ($code -eq 0) { "pass" } else { "FAIL" }
    if ($ci -and $status -eq "FAIL") {
        foreach ($m in $missing) { Write-Host "::error title=API incompatible with $name::$m" }
    }
    $results += [pscustomobject]@{ Name = $name; Status = $status; Missing = $missing }
}

Write-Host ""
Write-Host "API compatibility:" -ForegroundColor Cyan
foreach ($r in $results) {
    $color = switch ($r.Status) { "pass" { "Green" } "FAIL" { "Red" } default { "DarkYellow" } }
    $note = if ($r.Missing.Count -gt 0) { " ($($r.Missing.Count) missing)" } else { "" }
    Write-Host ("  {0,-10} {1}{2}" -f $r.Name, $r.Status, $note) -ForegroundColor $color
}

if ($ci -and $env:GITHUB_STEP_SUMMARY) {
    $lines = @("## API compatibility", "", "| Version | Result | Missing |", "| --- | --- | --- |")
    foreach ($r in $results) {
        $icon = switch ($r.Status) { "pass" { "pass" } "FAIL" { "**FAIL**" } default { $r.Status } }
        $detail = if ($r.Missing.Count -gt 0) { ($r.Missing -join "<br>") } else { "-" }
        $lines += "| $($r.Name) | $icon | $detail |"
    }
    Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value ($lines -join "`n")
}

if ($results | Where-Object { $_.Status -ne "pass" }) { exit 1 }
exit 0
