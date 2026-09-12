# Build Doom Builder 64 Enhanced (Release | x86)
# Called from build.bat at the repo root.

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

function Test-HasCsharpTargets($msbuild) {
    if (-not $msbuild) { return $false }
    $roslyn = Join-Path (Split-Path $msbuild) "Roslyn\Microsoft.CSharp.Core.targets"
    return (Test-Path $roslyn)
}

function Find-MSBuild {
    # Prefer a VS MSBuild that actually ships Roslyn CSharp.Core.targets.
    # Incomplete VS 2022 installs fail with MSB4019; Framework 4.0 MSBuild then
    # fails with MSB3091 if Windows SDK resgen.exe is missing.
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild `
            -find "MSBuild\**\Bin\MSBuild.exe" 2>$null
        if ($found) {
            $first = ($found | Select-Object -First 1).ToString().Trim()
            if ((Test-Path $first) -and (Test-HasCsharpTargets $first)) { return $first }
        }
    }
    $vs = @(
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )
    foreach ($c in $vs) {
        if ((Test-Path $c) -and (Test-HasCsharpTargets $c)) { return $c }
    }
    $framework = @(
        "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe",
        "$env:WINDIR\Microsoft.NET\Framework\v3.5\MSBuild.exe",
        "${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe"
    )
    foreach ($c in $framework) {
        if (Test-Path $c) { return $c }
    }
    return $null
}

$msbuild = Find-MSBuild
if (-not $msbuild) {
    Write-Error "MSBuild not found. Install Visual Studio 2022 (or Build Tools) with the .NET desktop workload."
}

$buildDir = Join-Path $RepoRoot "Build"
$slimdx = Join-Path $buildDir "SlimDX.dll"
$sharpzip = Join-Path $buildDir "Sharpzip.dll"
if (-not (Test-Path $slimdx)) {
    Write-Error "Missing Build\SlimDX.dll. The Build\ runtime tree must ship with SlimDX and Sharpzip."
}
if (-not (Test-Path $sharpzip)) {
    Write-Error "Missing Build\Sharpzip.dll. The Build\ runtime tree must ship with SlimDX and Sharpzip."
}

$ndp35 = Test-Path "$env:WINDIR\Microsoft.NET\Framework\v3.5\csc.exe"
if (-not $ndp35) {
    Write-Warning ".NET Framework 3.5 compiler not found at $env:WINDIR\Microsoft.NET\Framework\v3.5. Enable the Windows optional feature 'Microsoft .NET Framework 3.5' if the build fails."
}

$sln = Join-Path $RepoRoot "Builder.sln"
Write-Host "MSBuild: $msbuild"
Write-Host "Solution: $sln"
Write-Host "Configuration: Release | x86"
& $msbuild $sln /p:Configuration=Release /p:Platform=x86 /m /v:minimal /nologo
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$exe = Join-Path $buildDir "Builder.exe"
if (-not (Test-Path $exe)) {
    Write-Error "Build reported success but Build\Builder.exe is missing."
}

# Runtime deps must sit next to Builder.exe — never in Plugins\. PluginManager
# loads every *.dll there as a plug; Sharpzip/SlimDX (and a copied Builder.exe)
# produce "not supposed to be in the Plugins subdirectory" errors on launch.
$pluginsDir = Join-Path $buildDir "Plugins"
if (Test-Path $pluginsDir) {
    foreach ($name in @("Sharpzip.dll", "SlimDX.dll", "Builder.exe", "Builder.pdb")) {
        $stray = Join-Path $pluginsDir $name
        if (Test-Path $stray) {
            # Prefer keeping a root copy if somehow only Plugins had it.
            $rootCopy = Join-Path $buildDir $name
            if (($name -like "*.dll") -and -not (Test-Path $rootCopy)) {
                Copy-Item -Force $stray $rootCopy
                Write-Host "Restored $name to Build\"
            }
            Remove-Item -Force $stray
            Write-Host "Removed Plugins\$name (must live next to Builder.exe)"
        }
    }
}

foreach ($name in @("Sharpzip.dll", "SlimDX.dll")) {
    if (-not (Test-Path (Join-Path $buildDir $name))) {
        Write-Error "Build finished but Build\$name is missing next to Builder.exe."
    }
}

# Carcosa HTML help (browser) — no CHM rebuild required
$helpSrc = Join-Path $RepoRoot "Help"
$helpDst = Join-Path $buildDir "Help"
New-Item -ItemType Directory -Force -Path $helpDst | Out-Null
foreach ($name in @("carcosa_multimap.html", "default.css")) {
    $src = Join-Path $helpSrc $name
    if (Test-Path $src) {
        Copy-Item -Force $src (Join-Path $helpDst $name)
    }
}

Write-Host "Output: $exe"
exit 0
