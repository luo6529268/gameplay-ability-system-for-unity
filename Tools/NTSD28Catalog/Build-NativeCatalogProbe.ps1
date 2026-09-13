[CmdletBinding()]
param(
    [string]$Compiler = 'G:\GoggleDownload\x86_64-15.1.0-release-win32-seh-msvcrt-rt_v12-rev0\mingw64\bin\g++.exe',
    [string]$AuthorityRoot = 'J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan'
)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$output = Join-Path $repo 'artifacts/diagnostics/NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001'
$temporary = Join-Path $repo 'Temp/NTSD28Catalog'
$core = Join-Path $AuthorityRoot 'source/ntsd28_core'
$executable = Join-Path $temporary 'NativeCatalogProbe.exe'
New-Item -ItemType Directory -Force -Path $output, $temporary | Out-Null
if ((Get-FileHash -LiteralPath (Join-Path $AuthorityRoot 'NTSD2.8-Logan.exe')).Hash -ne 'B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033') {
    throw 'Formal EXE identity changed.'
}
$sources = @('data/dat_document.cpp','data/dat_parser.cpp','data/object_catalog.cpp','simulation/combat_records.cpp','simulation/object_spawning.cpp','simulation/collision_geometry.cpp','simulation/frame_motion.cpp','simulation/physics_integrator.cpp','simulation/frame_machine.cpp') | ForEach-Object { Join-Path $core "src/$_" }
$sources += Join-Path $PSScriptRoot 'NativeCatalogProbe.cpp'
$headers = Get-ChildItem -LiteralPath (Join-Path $core 'include') -Recurse -File | Where-Object { $_.Extension -in '.h','.hpp' } | ForEach-Object FullName
$identities = @($sources + $headers) | ForEach-Object { @{path=$_;sha256=(Get-FileHash -LiteralPath $_).Hash} }
$flags = @('-std=c++17','-O2','-municode','-ffunction-sections','-fdata-sections','-finput-charset=UTF-8','-fexec-charset=UTF-8','-static-libgcc','-static-libstdc++','-Wl,--gc-sections',('-I'+(Join-Path $core 'include')))
& $Compiler @flags @sources '-o' $executable *> (Join-Path $output 'compile.log')
if ($LASTEXITCODE -ne 0) { throw 'Native catalog probe compilation failed; inspect compile.log.' }
foreach ($identity in $identities) {
    if ((Get-FileHash -LiteralPath $identity.path).Hash -ne $identity.sha256) { throw 'Source changed during build.' }
}
@{sourceModel='SOURCE_MODEL_DIAGNOSTIC_ONLY';compiler=$Compiler;compilerHash=(Get-FileHash -LiteralPath $Compiler).Hash;sourcesAndHeaders=$identities;flags=$flags;executable=$executable;executableHash=(Get-FileHash -LiteralPath $executable).Hash} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $output 'build-manifest.json') -Encoding utf8
& $executable (Join-Path $AuthorityRoot 'resources/runtime') > (Join-Path $output 'native-formal-catalog.json')
if ($LASTEXITCODE -ne 0) { throw 'Native catalog probe failed.' }
$result = Get-Content (Join-Path $output 'native-formal-catalog.json') -Raw | ConvertFrom-Json
if (-not $result.success -or $result.entriesLoaded -ne 330) { throw 'Formal native catalog is not the expected 330-entry source.' }
Write-Output "Native catalog probe: $($result.entriesLoaded)/$($result.objectRows), source/header identity stable."
