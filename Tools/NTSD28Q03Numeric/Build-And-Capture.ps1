[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$authority = 'J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan'
$core = Join-Path $authority 'source/ntsd28_core'
$output = Join-Path $repo 'artifacts/diagnostics/NTSD28-Q03-NUMERIC-DECODE-WITNESS-001'
$temporary = Join-Path $repo 'Temp/NTSD28Q03Numeric'
$compiler = 'G:/GoggleDownload/x86_64-15.1.0-release-win32-seh-msvcrt-rt_v12-rev0/mingw64/bin/g++.exe'
New-Item -ItemType Directory -Force -Path $output, $temporary | Out-Null
$formalHash = (Get-FileHash -LiteralPath (Join-Path $authority 'NTSD2.8-Logan.exe') -Algorithm SHA256).Hash
if ($formalHash -ne 'B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033') { throw 'Authority identity mismatch.' }
$sources = @(
    (Join-Path $PSScriptRoot 'AuthorityNumericWitness.cpp'),
    (Join-Path $core 'src/data/dat_parser.cpp'),
    (Join-Path $core 'src/data/dat_document.cpp'),
    (Join-Path $core 'src/simulation/combat_records.cpp')
)
$identityPaths = @($sources) + @(Get-ChildItem -LiteralPath (Join-Path $core 'include/ntsd28') -Filter '*.h' | ForEach-Object FullName) + @(Get-ChildItem -LiteralPath (Join-Path $output 'fixtures') -Filter '*.dat' | ForEach-Object FullName)
$before = @($identityPaths | ForEach-Object { Get-FileHash -LiteralPath $_ -Algorithm SHA256 })
$executable = Join-Path $temporary 'AuthorityNumericWitness.exe'
$arguments = @('-std=c++17', '-O2', '-Wall', '-Wextra', '-ffunction-sections', '-fdata-sections', '-static-libgcc', '-static-libstdc++', '-Wl,--gc-sections', ('-I' + (Join-Path $core 'include'))) + $sources + @('-o', $executable)
& $compiler @arguments *> (Join-Path $output 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed; see build.log.' }
foreach ($run in @('native', 'native-rerun')) {
    & $executable (Join-Path $output 'fixtures') > (Join-Path $output ($run + '.tsv'))
    if ($LASTEXITCODE -ne 0) { throw 'Native capture failed.' }
}
$after = @($identityPaths | ForEach-Object { Get-FileHash -LiteralPath $_ -Algorithm SHA256 })
if (Compare-Object ($before | ForEach-Object Hash) ($after | ForEach-Object Hash)) { throw 'Source or fixture drift.' }
$firstHash = (Get-FileHash -LiteralPath (Join-Path $output 'native.tsv')).Hash
$secondHash = (Get-FileHash -LiteralPath (Join-Path $output 'native-rerun.tsv')).Hash
if ($firstHash -ne $secondHash) { throw 'Double-run mismatch.' }
@{ formalExeHash = $formalHash; inputs = $before; compiler = (Get-FileHash -LiteralPath $compiler); executable = (Get-FileHash -LiteralPath $executable); arguments = $arguments; outputHash = $firstHash; doubleRunStable = $true } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $output 'identity-and-stability.json') -Encoding utf8
Get-Content -LiteralPath (Join-Path $output 'native.tsv')
