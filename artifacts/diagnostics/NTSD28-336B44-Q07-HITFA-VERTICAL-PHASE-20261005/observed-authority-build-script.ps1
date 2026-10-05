[CmdletBinding()]
param(
    [ValidateSet('all', 'playable', 'parity_runner', 'scenario_gate', 'scenario_trace_encoding_tests', 'game_session_tests', 'selection_loop_tests', 'story_frontend_tests', 'war_frontend_tests', 'tournament_setup_tests', 'tournament_setup_stream_tests', 'tournament_roster_tests', 'tournament_roster_stream_tests', 'tournament_render_tests', 'tournament_page_render_tests', 'frontend_menu_render_tests', 'frontend_playback_tests', 'presentation_interpolation_tests', 'game_session_function_keys_tests', 'win32_input_bindings_tests', 'native_function_keys_tests', 'lfr_recorder_tests', 'game_session_lfr_tests', 'game_session_function_keys_lfr_tests', 'recording_application_tests', 'recording_fixture', 'recording_playback_ko_fixture', 'offscreen_gate', 'audio_backend_tests', 'audio_reference_corpus_tests')]
    [string]$Target = 'all',
    [string]$Compiler = '',
    [string]$OutputRoot = ''
)

$ErrorActionPreference = 'Stop'
$playableRoot = Split-Path -Parent $PSScriptRoot
$reverseRoot = Split-Path -Parent $playableRoot
$coreRoot = Join-Path $reverseRoot 'ntsd28_core'
$buildRoot = if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    Join-Path $playableRoot 'build'
} else {
    [System.IO.Path]::GetFullPath($OutputRoot)
}
$compiler = $Compiler
if ([string]::IsNullOrWhiteSpace($compiler)) {
    $compiler = $env:NTSD28_CXX
}
if ([string]::IsNullOrWhiteSpace($compiler)) {
    $pathCompiler = Get-Command 'g++.exe' -ErrorAction SilentlyContinue
    if ($pathCompiler) {
        $compiler = $pathCompiler.Source
    }
}
if ([string]::IsNullOrWhiteSpace($compiler)) {
    $legacyCompiler = 'G:\GoggleDownload\x86_64-15.1.0-release-win32-seh-msvcrt-rt_v12-rev0\mingw64\bin\g++.exe'
    if (Test-Path -LiteralPath $legacyCompiler -PathType Leaf) {
        $compiler = $legacyCompiler
    }
}
if ([string]::IsNullOrWhiteSpace($compiler) -or
    -not (Test-Path -LiteralPath $compiler -PathType Leaf)) {
    throw 'C++ compiler not found. Pass -Compiler, set NTSD28_CXX, or add g++.exe to PATH.'
}
$windres = Join-Path (Split-Path -Parent $compiler) 'windres.exe'
if (-not (Test-Path -LiteralPath $windres -PathType Leaf)) {
    throw "Windows resource compiler not found: $windres"
}
New-Item -ItemType Directory -Force -Path $buildRoot | Out-Null

$versionResource = Join-Path $buildRoot 'ntsd28_playable_version.o'
$resourceIncludeRoot = Join-Path $playableRoot 'resources'
$previousLocation = Get-Location
try {
    Set-Location -LiteralPath $resourceIncludeRoot
    & $windres 'ntsd28_playable_version.rc' `
        '-O' 'coff' '-o' $versionResource
} finally {
    Set-Location -LiteralPath $previousLocation
}
if ($LASTEXITCODE -ne 0) {
    throw "playable version resource build failed with exit code $LASTEXITCODE"
}

$coreSources = @(
    'src\data\dat_document.cpp',
    'src\data\dat_parser.cpp',
    'src\data\fusion_catalog.cpp',
    'src\data\kind_catalog.cpp',
    'src\data\minibar_catalog.cpp',
    'src\data\object_catalog.cpp',
    'src\simulation\frame_machine.cpp',
    'src\simulation\frame_motion.cpp',
    'src\simulation\native_random.cpp',
    'src\simulation\native_ai.cpp',
    'src\simulation\input_routing.cpp',
    'src\simulation\physics_integrator.cpp',
    'src\simulation\collision_geometry.cpp',
    'src\simulation\hit_candidates.cpp',
    'src\simulation\hit_response.cpp',
    'src\simulation\combat_records.cpp',
    'src\simulation\damage_resolution.cpp',
    'src\simulation\defense_resolution.cpp',
    'src\simulation\armor_resolution.cpp',
    'src\simulation\object_spawning.cpp',
    'src\simulation\battle_world.cpp',
    'src\simulation\battle_flow.cpp',
    'src\simulation\simulation_tick_driver.cpp',
    'src\rendering\render_snapshot.cpp',
    'src\rendering\background_definition.cpp',
    'src\rendering\native_resource_catalog.cpp',
    'src\rendering\native_frame_hud.cpp',
    'src\rendering\native_combo_hud.cpp'
) | ForEach-Object { Join-Path $coreRoot $_ }

if ($Target -eq 'tournament_setup_tests' -or $Target -eq 'all') {
    $tournamentSetupArguments = @(
        '-std=c++17', '-O2', '-Wall', '-Wextra', '-Wpedantic',
        '-finput-charset=UTF-8', '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include')),
        (Join-Path $playableRoot 'tests\tournament_setup_tests.cpp'),
        '-static-libgcc', '-static-libstdc++',
        '-o', (Join-Path $buildRoot 'tournament_setup_tests.exe')
    )
    Write-Host '[build] tournament_setup_tests.exe (pure native setup/shuffle validation)'
    & $compiler @tournamentSetupArguments
    if ($LASTEXITCODE -ne 0) { throw "tournament setup test build failed: $LASTEXITCODE" }
    if ($Target -eq 'tournament_setup_tests') {
        Write-Host "[build] focused complete: $buildRoot"
        return
    }
}

if ($Target -eq 'tournament_setup_stream_tests' -or $Target -eq 'all') {
    $tournamentStreamArguments = @(
        '-std=c++17', '-O2', '-Wall', '-Wextra', '-Wpedantic',
        '-finput-charset=UTF-8', '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include')),
        (Join-Path $coreRoot 'src\simulation\native_random.cpp'),
        (Join-Path $playableRoot 'tests\tournament_setup_stream_tests.cpp'),
        '-static-libgcc', '-static-libstdc++',
        '-o', (Join-Path $buildRoot 'tournament_setup_stream_tests.exe')
    )
    Write-Host '[build] tournament_setup_stream_tests.exe (shared native RNG validation)'
    & $compiler @tournamentStreamArguments
    if ($LASTEXITCODE -ne 0) { throw "tournament shared RNG test build failed: $LASTEXITCODE" }
    if ($Target -eq 'tournament_setup_stream_tests') {
        Write-Host "[build] focused complete: $buildRoot"
        return
    }
}

if ($Target -eq 'tournament_roster_tests' -or $Target -eq 'tournament_roster_stream_tests' -or $Target -eq 'all') {
    $rosterTargets = if ($Target -eq 'all') {
        @('tournament_roster_tests', 'tournament_roster_stream_tests')
    } else { @($Target) }
    foreach ($rosterTarget in $rosterTargets) {
        $rosterSources = if ($rosterTarget -eq 'tournament_roster_stream_tests') {
            @((Join-Path $coreRoot 'src\simulation\native_random.cpp'))
        } else { @() }
        $rosterArguments = @(
            '-std=c++17', '-O2', '-Wall', '-Wextra', '-Wpedantic',
            '-finput-charset=UTF-8', '-fexec-charset=UTF-8',
            ('-I' + (Join-Path $coreRoot 'include')),
            ('-I' + (Join-Path $playableRoot 'include'))
        ) + $rosterSources + @(
            (Join-Path $playableRoot ('tests\' + $rosterTarget + '.cpp')),
            '-static-libgcc', '-static-libstdc++',
            '-o', (Join-Path $buildRoot ($rosterTarget + '.exe'))
        )
        Write-Host "[build] $rosterTarget.exe (two-gate native tournament roster validation)"
        & $compiler @rosterArguments
        if ($LASTEXITCODE -ne 0) { throw "tournament roster test build failed: $LASTEXITCODE" }
    }
    if ($Target -ne 'all') {
        Write-Host "[build] focused complete: $buildRoot"
        return
    }
}

if ($Target -eq 'tournament_render_tests' -or $Target -eq 'all') {
    $tournamentRenderArguments = @(
        '-std=c++17', '-O2', '-Wall', '-Wextra', '-Wpedantic',
        '-finput-charset=UTF-8', '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include')),
        (Join-Path $playableRoot 'tests\tournament_render_tests.cpp'),
        '-static-libgcc', '-static-libstdc++',
        '-o', (Join-Path $buildRoot 'tournament_render_tests.exe')
    )
    Write-Host '[build] tournament_render_tests.exe (native crop/projection validation)'
    & $compiler @tournamentRenderArguments
    if ($LASTEXITCODE -ne 0) { throw "tournament render test build failed: $LASTEXITCODE" }
    if ($Target -eq 'tournament_render_tests') {
        Write-Host "[build] focused complete: $buildRoot"
        return
    }
}

if ($Target -eq 'frontend_menu_render_tests' -or $Target -eq 'tournament_page_render_tests' -or
    ($Target -eq 'all')) {
    $menuTargets = if ($Target -eq 'all') { @('tournament_page_render_tests') } else { @($Target) }
    foreach ($menuTarget in $menuTargets) {
    $menuArguments = @(
        '-std=c++17', '-O2', '-Wall', '-Wextra', '-Wpedantic',
        '-finput-charset=UTF-8', '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + @(
        (Join-Path $coreRoot 'src\data\dat_document.cpp'),
        (Join-Path $coreRoot 'src\data\dat_parser.cpp'),
        (Join-Path $playableRoot 'src\d3d11_renderer.cpp'),
        (Join-Path $playableRoot 'src\presentation_interpolation.cpp'),
        (Join-Path $coreRoot 'src\simulation\native_random.cpp'),
        (Join-Path $playableRoot ('tests\' + $menuTarget + '.cpp')),
        '-ld3d11', '-ldxgi', '-ld3dcompiler', '-lwindowscodecs', '-lole32',
        '-lshell32', '-lgdi32', '-luuid', '-static-libgcc', '-static-libstdc++',
        '-o', (Join-Path $buildRoot ($menuTarget + '.exe'))
    )
    Write-Host "[build] $menuTarget.exe (menu-resource WARP validation)"
    & $compiler @menuArguments
    if ($LASTEXITCODE -ne 0) { throw "menu render test build failed: $LASTEXITCODE" }
    }
    if ($Target -ne 'all') {
        Write-Host "[build] focused complete: $buildRoot"
        return
    }
}

if ($Target -eq 'audio_backend_tests' -or $Target -eq 'all') {
    $audioBackendTestArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include')),
        (Join-Path $playableRoot 'src\audio_backend.cpp'),
        (Join-Path $playableRoot 'tests\audio_backend_tests.cpp'),
        '-lole32',
        '-lxaudio2_9',
        '-lmfplat',
        '-lmfreadwrite',
        '-lmfuuid',
        '-lpropsys',
        '-luuid',
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'audio_backend_tests.exe')
    )
    Write-Host '[build] audio_backend_tests.exe (console-only decoder/mix validation)'
    & $compiler @audioBackendTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "audio_backend_tests build failed with exit code $LASTEXITCODE"
    }
    if ($Target -eq 'audio_backend_tests') {
        Write-Host "[build] focused complete: $buildRoot"
        return
    }
}

if ($Target -eq 'audio_reference_corpus_tests' -or $Target -eq 'all') {
    $audioCorpusTestArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include')),
        (Join-Path $playableRoot 'src\audio_backend.cpp'),
        (Join-Path $playableRoot 'tests\audio_reference_corpus_tests.cpp'),
        '-lole32',
        '-lxaudio2_9',
        '-lmfplat',
        '-lmfreadwrite',
        '-lmfuuid',
        '-lpropsys',
        '-luuid',
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'audio_reference_corpus_tests.exe')
    )
    Write-Host '[build] audio_reference_corpus_tests.exe (975 locked WAV paths)'
    & $compiler @audioCorpusTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "audio_reference_corpus_tests build failed with exit code $LASTEXITCODE"
    }
    if ($Target -eq 'audio_reference_corpus_tests') {
        Write-Host "[build] focused complete: $buildRoot"
        return
    }
}

if ($Target -eq 'game_session_tests') {
    $testArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'tests\game_session_tests.cpp'),
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'game_session_tests.exe')
    )
    Write-Host '[build] game_session_tests.exe (console-only validation)'
    & $compiler @testArguments
    if ($LASTEXITCODE -ne 0) {
        throw "game_session_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'game_session_function_keys_tests') {
    $functionKeySessionTestArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'tests\game_session_function_keys_tests.cpp'),
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'game_session_function_keys_tests.exe')
    )
    Write-Host '[build] game_session_function_keys_tests.exe (session state validation)'
    & $compiler @functionKeySessionTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "game_session_function_keys_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'selection_loop_tests' -or $Target -eq 'story_frontend_tests' -or $Target -eq 'war_frontend_tests') {
    $warRenderSources = if ($Target -eq 'war_frontend_tests') {
        @((Join-Path $playableRoot 'src\d3d11_renderer.cpp'),
          (Join-Path $playableRoot 'src\presentation_interpolation.cpp'))
    } else { @() }
    $warRenderLibraries = if ($Target -eq 'war_frontend_tests') {
        @('-ld3d11', '-ldxgi', '-ld3dcompiler', '-lwindowscodecs', '-lole32',
          '-lshell32', '-lgdi32', '-luuid')
    } else { @() }
    $selectionLoopTestArguments = @(
        '-std=c++17',
        '-g',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + $warRenderSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot ("tests\" + $Target + '.cpp'))
    ) + $warRenderLibraries + @(
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot ($Target + '.exe'))
    )
    Write-Host "[build] $Target.exe (frontend scene ownership validation)"
    & $compiler @selectionLoopTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "selection_loop_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'presentation_interpolation_tests') {
    $presentationTestArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include')),
        (Join-Path $playableRoot 'src\presentation_interpolation.cpp'),
        (Join-Path $playableRoot 'tests\presentation_interpolation_tests.cpp'),
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'presentation_interpolation_tests.exe')
    )
    Write-Host '[build] presentation_interpolation_tests.exe (presentation-only 30/120 contract)'
    & $compiler @presentationTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "presentation_interpolation_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'win32_input_bindings_tests') {
    $testArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include')),
        (Join-Path $playableRoot 'tests\win32_input_bindings_tests.cpp'),
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'win32_input_bindings_tests.exe')
    )
    Write-Host '[build] win32_input_bindings_tests.exe (console-only validation)'
    & $compiler @testArguments
    if ($LASTEXITCODE -ne 0) {
        throw "win32_input_bindings_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'native_function_keys_tests') {
    $nativeFunctionKeyTestArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $playableRoot 'include')),
        (Join-Path $playableRoot 'tests\native_function_keys_tests.cpp'),
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'native_function_keys_tests.exe')
    )
    Write-Host '[build] native_function_keys_tests.exe (pure routing validation)'
    & $compiler @nativeFunctionKeyTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "native_function_keys_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'lfr_recorder_tests') {
    $lfrRecorderTestArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $playableRoot 'include')),
        (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
        (Join-Path $playableRoot 'tests\lfr_recorder_tests.cpp'),
        '-lz',
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'lfr_recorder_tests.exe')
    )
    Write-Host '[build] lfr_recorder_tests.exe (offline static-zlib validation)'
    & $compiler @lfrRecorderTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "lfr_recorder_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'game_session_lfr_tests' -or $Target -eq 'frontend_playback_tests') {
    $gameSessionLfrTestArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-municode',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
        (Join-Path $playableRoot 'src\game_session_lfr.cpp'),
        (Join-Path $playableRoot ('tests\' + $Target + '.cpp')),
        '-lz',
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot ($Target + '.exe'))
    )
    Write-Host "[build] $Target.exe (ordinary recording/playback validation)"
    & $compiler @gameSessionLfrTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Target build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'game_session_function_keys_lfr_tests') {
    $functionKeyLfrTestArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
        (Join-Path $playableRoot 'src\game_session_lfr.cpp'),
        (Join-Path $playableRoot 'tests\game_session_function_keys_lfr_tests.cpp'),
        '-lz',
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'game_session_function_keys_lfr_tests.exe')
    )
    Write-Host '[build] game_session_function_keys_lfr_tests.exe (function-key round-trip validation)'
    & $compiler @functionKeyLfrTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "game_session_function_keys_lfr_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'recording_application_tests') {
    $recordingApplicationTestArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-municode',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
        (Join-Path $playableRoot 'src\game_session_lfr.cpp'),
        (Join-Path $playableRoot 'src\recording_application.cpp'),
        (Join-Path $playableRoot 'tests\recording_application_tests.cpp'),
        '-lz',
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'recording_application_tests.exe')
    )
    Write-Host '[build] recording_application_tests.exe (ordinary recording application state/file validation)'
    & $compiler @recordingApplicationTestArguments
    if ($LASTEXITCODE -ne 0) {
        throw "recording_application_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'recording_fixture') {
    $recordingFixtureArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-municode',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
        (Join-Path $playableRoot 'src\game_session_lfr.cpp'),
        (Join-Path $playableRoot 'src\recording_application.cpp'),
        (Join-Path $playableRoot 'src\recording_fixture_main.cpp'),
        '-lz',
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'ntsd28_recording_fixture.exe')
    )
    Write-Host '[build] ntsd28_recording_fixture.exe (explicit console fixture generator)'
    & $compiler @recordingFixtureArguments
    if ($LASTEXITCODE -ne 0) {
        throw "recording_fixture build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'recording_playback_ko_fixture') {
    $recordingPlaybackKoFixtureArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-municode',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
        (Join-Path $playableRoot 'src\game_session_lfr.cpp'),
        (Join-Path $playableRoot 'src\recording_application.cpp'),
        (Join-Path $playableRoot 'src\recording_playback_ko_fixture_main.cpp'),
        '-lz',
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'ntsd28_recording_playback_ko_fixture.exe')
    )
    Write-Host '[build] ntsd28_recording_playback_ko_fixture.exe (input-derived terminal native LFR)'
    & $compiler @recordingPlaybackKoFixtureArguments
    if ($LASTEXITCODE -ne 0) {
        throw "recording_playback_ko_fixture build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'scenario_gate') {
    $gateArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-municode',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'src\scenario28.cpp'),
        (Join-Path $playableRoot 'src\scenario_gate_main.cpp'),
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'ntsd28_scenario_gate.exe')
    )
    Write-Host '[build] ntsd28_scenario_gate.exe (console-only validation)'
    & $compiler @gateArguments
    if ($LASTEXITCODE -ne 0) {
        throw "ntsd28_scenario_gate build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'scenario_trace_encoding_tests') {
    $traceEncodingArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'src\scenario28.cpp'),
        (Join-Path $playableRoot 'tests\scenario_trace_encoding_tests.cpp'),
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'scenario_trace_encoding_tests.exe')
    )
    Write-Host '[build] scenario_trace_encoding_tests.exe (console-only UTF-8 JSON validation)'
    & $compiler @traceEncodingArguments
    if ($LASTEXITCODE -ne 0) {
        throw "scenario_trace_encoding_tests build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'offscreen_gate') {
    $offscreenArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-municode',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'src\d3d11_renderer.cpp'),
        (Join-Path $playableRoot 'src\offscreen_gate_main.cpp'),
        '-ld3d11',
        '-ldxgi',
        '-ld3dcompiler',
        '-lwindowscodecs',
        '-lole32',
        '-lshell32',
        '-lgdi32',
        '-luuid',
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'ntsd28_offscreen_gate.exe')
    )
    Write-Host '[build] ntsd28_offscreen_gate.exe (console-only WARP validation; no HWND/swap chain/Present)'
    & $compiler @offscreenArguments
    if ($LASTEXITCODE -ne 0) {
        throw "ntsd28_offscreen_gate build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

if ($Target -eq 'parity_runner') {
    $runnerArguments = @(
        '-std=c++17',
        '-O2',
        '-Wall',
        '-Wextra',
        '-Wpedantic',
        '-municode',
        '-finput-charset=UTF-8',
        '-fexec-charset=UTF-8',
        ('-I' + (Join-Path $coreRoot 'include')),
        ('-I' + (Join-Path $playableRoot 'include'))
    ) + $coreSources + @(
        (Join-Path $playableRoot 'src\game_session.cpp'),
        (Join-Path $playableRoot 'src\selection_flow.cpp'),
        (Join-Path $playableRoot 'src\scenario28.cpp'),
        (Join-Path $playableRoot 'src\parity_runner_main.cpp'),
        '-static-libgcc',
        '-static-libstdc++',
        '-o',
        (Join-Path $buildRoot 'ntsd28_parity_runner.exe')
    )
    Write-Host '[build] ntsd28_parity_runner.exe (explicit target only)'
    & $compiler @runnerArguments
    if ($LASTEXITCODE -ne 0) {
        throw "ntsd28_parity_runner build failed with exit code $LASTEXITCODE"
    }
    Write-Host "[build] focused complete: $buildRoot"
    return
}

$arguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-municode',
    '-mwindows',
    '-DUNICODE',
    '-D_UNICODE',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $coreSources + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'src\audio_backend.cpp'),
    (Join-Path $playableRoot 'src\d3d11_renderer.cpp'),
    (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
    (Join-Path $playableRoot 'src\game_session_lfr.cpp'),
    (Join-Path $playableRoot 'src\recording_application.cpp'),
    (Join-Path $playableRoot 'src\scenario28.cpp'),
    (Join-Path $playableRoot 'src\presentation_interpolation.cpp'),
    (Join-Path $playableRoot 'src\main.cpp'),
    $versionResource,
    '-ld3d11',
    '-ldxgi',
    '-ld3dcompiler',
    '-lwindowscodecs',
    '-lole32',
    '-lshell32',
    '-lgdi32',
    '-lcomdlg32',
    '-luuid',
    '-lwinmm',
    '-lxaudio2_9',
    '-lmfplat',
    '-lmfreadwrite',
    '-lmfuuid',
    '-lpropsys',
    '-lz',
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'Ntsd28Playable.exe')
)

Write-Host '[build] Ntsd28Playable.exe'
& $compiler @arguments
if ($LASTEXITCODE -ne 0) {
    throw "Ntsd28Playable build failed with exit code $LASTEXITCODE"
}
if ($Target -eq 'playable') {
    Write-Host "[build] focused complete: $buildRoot"
    return
}

$gateArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-municode',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $coreSources + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'src\scenario28.cpp'),
    (Join-Path $playableRoot 'src\scenario_gate_main.cpp'),
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'ntsd28_scenario_gate.exe')
)

Write-Host '[build] ntsd28_scenario_gate.exe (console-only validation)'
& $compiler @gateArguments
if ($LASTEXITCODE -ne 0) {
    throw "ntsd28_scenario_gate build failed with exit code $LASTEXITCODE"
}

$sessionTestArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $coreSources + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'tests\game_session_tests.cpp'),
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'game_session_tests.exe')
)

Write-Host '[build] game_session_tests.exe (console-only validation)'
& $compiler @sessionTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "game_session_tests build failed with exit code $LASTEXITCODE"
}

$selectionLoopTestArguments = @(
    '-std=c++17',
    '-g',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $coreSources + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'tests\selection_loop_tests.cpp'),
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'selection_loop_tests.exe')
)

Write-Host '[build] selection_loop_tests.exe (frontend scene ownership validation)'
& $compiler @selectionLoopTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "selection_loop_tests build failed with exit code $LASTEXITCODE"
}

$presentationTestArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include')),
    (Join-Path $playableRoot 'src\presentation_interpolation.cpp'),
    (Join-Path $playableRoot 'tests\presentation_interpolation_tests.cpp'),
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'presentation_interpolation_tests.exe')
)
Write-Host '[build] presentation_interpolation_tests.exe (presentation-only 30/120 contract)'
& $compiler @presentationTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "presentation_interpolation_tests build failed with exit code $LASTEXITCODE"
}

$functionKeySessionTestArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $coreSources + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'tests\game_session_function_keys_tests.cpp'),
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'game_session_function_keys_tests.exe')
)

Write-Host '[build] game_session_function_keys_tests.exe (session state validation)'
& $compiler @functionKeySessionTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "game_session_function_keys_tests build failed with exit code $LASTEXITCODE"
}

$controlBindingTestArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include')),
    (Join-Path $playableRoot 'tests\win32_input_bindings_tests.cpp'),
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'win32_input_bindings_tests.exe')
)

Write-Host '[build] win32_input_bindings_tests.exe (console-only validation)'
& $compiler @controlBindingTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "win32_input_bindings_tests build failed with exit code $LASTEXITCODE"
}

$nativeFunctionKeyTestArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $playableRoot 'include')),
    (Join-Path $playableRoot 'tests\native_function_keys_tests.cpp'),
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'native_function_keys_tests.exe')
)

Write-Host '[build] native_function_keys_tests.exe (pure routing validation)'
& $compiler @nativeFunctionKeyTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "native_function_keys_tests build failed with exit code $LASTEXITCODE"
}

$lfrRecorderTestArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $playableRoot 'include')),
    (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
    (Join-Path $playableRoot 'tests\lfr_recorder_tests.cpp'),
    '-lz',
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'lfr_recorder_tests.exe')
)

Write-Host '[build] lfr_recorder_tests.exe (offline static-zlib validation)'
& $compiler @lfrRecorderTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "lfr_recorder_tests build failed with exit code $LASTEXITCODE"
}

$gameSessionLfrTestArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-municode',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $coreSources + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
    (Join-Path $playableRoot 'src\game_session_lfr.cpp'),
    (Join-Path $playableRoot 'tests\game_session_lfr_tests.cpp'),
    '-lz',
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'game_session_lfr_tests.exe')
)

Write-Host '[build] game_session_lfr_tests.exe (ordinary in-memory producer validation)'
& $compiler @gameSessionLfrTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "game_session_lfr_tests build failed with exit code $LASTEXITCODE"
}

$functionKeyLfrTestArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $coreSources + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
    (Join-Path $playableRoot 'src\game_session_lfr.cpp'),
    (Join-Path $playableRoot 'tests\game_session_function_keys_lfr_tests.cpp'),
    '-lz',
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'game_session_function_keys_lfr_tests.exe')
)

Write-Host '[build] game_session_function_keys_lfr_tests.exe (function-key round-trip validation)'
& $compiler @functionKeyLfrTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "game_session_function_keys_lfr_tests build failed with exit code $LASTEXITCODE"
}

$recordingApplicationTestArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-municode',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $coreSources + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'src\lfr_recorder.cpp'),
    (Join-Path $playableRoot 'src\game_session_lfr.cpp'),
    (Join-Path $playableRoot 'src\recording_application.cpp'),
    (Join-Path $playableRoot 'tests\recording_application_tests.cpp'),
    '-lz',
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'recording_application_tests.exe')
)

Write-Host '[build] recording_application_tests.exe (ordinary recording application state/file validation)'
& $compiler @recordingApplicationTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "recording_application_tests build failed with exit code $LASTEXITCODE"
}

$offscreenArguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-municode',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $coreSources + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'src\d3d11_renderer.cpp'),
    (Join-Path $playableRoot 'src\offscreen_gate_main.cpp'),
    '-ld3d11',
    '-ldxgi',
    '-ld3dcompiler',
    '-lwindowscodecs',
    '-lole32',
    '-lshell32',
    '-lgdi32',
    '-luuid',
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    (Join-Path $buildRoot 'ntsd28_offscreen_gate.exe')
)

Write-Host '[build] ntsd28_offscreen_gate.exe (console-only WARP validation; no HWND/swap chain/Present)'
& $compiler @offscreenArguments
if ($LASTEXITCODE -ne 0) {
    throw "ntsd28_offscreen_gate build failed with exit code $LASTEXITCODE"
}
Write-Host "[build] complete: $buildRoot"
