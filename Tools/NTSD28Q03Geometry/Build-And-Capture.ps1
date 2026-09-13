[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$authority = 'J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan'
$core = Join-Path $authority 'source/ntsd28_core'
$output = Join-Path $repo 'artifacts/diagnostics/NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001'
$temporary = Join-Path $repo 'Temp/NTSD28Q03Geometry'
$compiler = 'G:/GoggleDownload/x86_64-15.1.0-release-win32-seh-msvcrt-rt_v12-rev0/mingw64/bin/g++.exe'
New-Item -ItemType Directory -Force -Path $output, $temporary | Out-Null
$formalHash = (Get-FileHash -LiteralPath (Join-Path $authority 'NTSD2.8-Logan.exe') -Algorithm SHA256).Hash
if ($formalHash -ne 'B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033') {
    throw 'Formal authority identity mismatch.'
}
$sources = @(
    (Join-Path $PSScriptRoot 'AuthorityGeometryWitness.cpp'),
    (Join-Path $core 'src/data/dat_parser.cpp'),
    (Join-Path $core 'src/data/dat_document.cpp'),
    (Join-Path $core 'src/data/kind_catalog.cpp'),
    (Join-Path $core 'src/simulation/collision_geometry.cpp'),
    (Join-Path $core 'src/simulation/hit_candidates.cpp')
)
$identityPaths = @($sources) + @(Get-ChildItem -LiteralPath (Join-Path $core 'include/ntsd28') -Filter '*.h' | ForEach-Object FullName)
$before = @($identityPaths | ForEach-Object { Get-FileHash -LiteralPath $_ -Algorithm SHA256 })
$executable = Join-Path $temporary 'AuthorityGeometryWitness.exe'
$arguments = @('-std=c++17', '-O2', '-Wall', '-Wextra', '-ffunction-sections', '-fdata-sections', '-static-libgcc', '-static-libstdc++', '-Wl,--gc-sections', ('-I' + (Join-Path $core 'include'))) + $sources + @('-o', $executable)
& $compiler @arguments *> (Join-Path $output 'native-build.log')
if ($LASTEXITCODE -ne 0) { throw 'Native witness compilation failed; see native-build.log.' }
$after = @($identityPaths | ForEach-Object { Get-FileHash -LiteralPath $_ -Algorithm SHA256 })
if (Compare-Object ($before | ForEach-Object Hash) ($after | ForEach-Object Hash)) { throw 'Source changed during build.' }
@{ formalExeHash = $formalHash; sourcesAndHeaders = $before; compiler = (Get-FileHash -LiteralPath $compiler); executable = (Get-FileHash -LiteralPath $executable); arguments = $arguments } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $output 'native-build-identity.json') -Encoding utf8
& $executable (Join-Path $output 'fixtures') > (Join-Path $output 'native.tsv')
if ($LASTEXITCODE -ne 0) { throw 'Native witness capture failed.' }
Get-Content -LiteralPath (Join-Path $output 'native.tsv')
