[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$toolRoot = Split-Path -Parent $PSScriptRoot
$cppRoot = "J:\QQFile\NTSD2.4\ntsd_cpp"
$compiler = "G:\GoggleDownload\x86_64-15.1.0-release-win32-seh-msvcrt-rt_v12-rev0\mingw64\bin\g++.exe"
$source = Join-Path $toolRoot "native\dat_preview_cli.cpp"
$outputDirectory = Join-Path $toolRoot "native\bin"
$object = Join-Path $outputDirectory "dat_preview_cli.o"
$executable = Join-Path $outputDirectory "dat_preview_cli.exe"

foreach ($required in @(
    $compiler,
    $source,
    (Join-Path $cppRoot "src\core\dat_preview_cli.cpp"),
    (Join-Path $cppRoot "include\input_handler.h"),
    (Join-Path $cppRoot "lib\libSDL2.a")
)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Native preview prerequisite not found: $required"
    }
}

$linkObjects = Get-ChildItem -LiteralPath (Join-Path $cppRoot "src") -Recurse -File -Filter "*.dbg.o" |
    Where-Object {
        $_.Name -notlike "probe_*" -and
        $_.Name -notin @("main.dbg.o", "dat_preview_cli.dbg.o", "battle_logic.dbg.o")
    } |
    Sort-Object FullName

if ($linkObjects.Count -eq 0) {
    throw "No read-only Native debug objects were found under $cppRoot\src."
}

$latestInput = @(
    Get-Item -LiteralPath $source
    Get-Item -LiteralPath (Join-Path $cppRoot "src\core\dat_preview_cli.cpp")
    Get-Item -LiteralPath (Join-Path $cppRoot "include\input_handler.h")
    $linkObjects
) | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1

if (-not $Force -and
    (Test-Path -LiteralPath $executable -PathType Leaf) -and
    (Get-Item -LiteralPath $executable).LastWriteTimeUtc -ge $latestInput.LastWriteTimeUtc) {
    Write-Output "Native preview adapter is up to date: $executable"
    exit 0
}

New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

$compileArguments = @(
    "-std=c++17", "-O2", "-Wall", "-Wextra",
    "-DSDL_MAIN_HANDLED", "-DDEBUG_SKIP_CHARSEL",
    "-I", (Join-Path $cppRoot "include"),
    "-I", (Join-Path $cppRoot "include\SDL2"),
    "-I", (Join-Path $cppRoot "src\core"),
    "-c", $source,
    "-o", $object
)
& $compiler @compileArguments
if ($LASTEXITCODE -ne 0) {
    throw "Native preview adapter compilation failed with exit code $LASTEXITCODE."
}

$linkArguments = @(
    $object
) + @($linkObjects.FullName) + @(
    "-o", $executable,
    "-L", (Join-Path $cppRoot "lib"),
    "-lSDL2", "-lwinmm", "-lws2_32", "-lgdi32",
    "-static-libgcc", "-static-libstdc++"
)
& $compiler @linkArguments
if ($LASTEXITCODE -ne 0) {
    throw "Native preview adapter link failed with exit code $LASTEXITCODE."
}

Write-Output "Built Native preview adapter: $executable"
