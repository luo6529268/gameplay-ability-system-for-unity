<!-- CHANGE-RECORD
id: NTSD28-Q07-WINDOWS-PLAYER-RUNTIME-001
status: VERIFIED
change-kind: TEST_ONLY_PLAYER
code-path: Assets/NTSD/Scripts/Test/NTSD28Q07WindowsPlayerRuntimeProbe.cs
authority: D-023 formal content and Q07 Windows Player build-output certificate
evidence: q07-player-runtime-build-1 succeeded; q07-player-runtime-run-3 terminal PASS and exit code 0; audio portability return open
-->

# NTSD28-Q07-WINDOWS-PLAYER-RUNTIME-001

Before: Editor full formal-content publication and actual App/menu Play passed; Windows Mono Player BuildPipeline output contains 1,343 exact formal files and SPARK. Yet production GameConfig root is still empty and no built Player has loaded the formal source. File presence cannot prove runtime source selection, publication or shutdown.

Planned exact file/symbol: add one Development Player-only `RuntimeInitializeOnLoadMethod(AfterSceneLoad)` probe gated by `-ntsd-q07-player-probe`. This callback runs after serialized scene objects are loaded and before their `Start` methods, allowing it to select the formal sidecar root on the in-memory GameConfig used by the test bootstrap. It creates a transient monitor, waits for the real battle bootstrap, checks identity/owners/World, then performs ordered shutdown/unload and resource/pool checks, writes JSON and exits. No change to AppManager, BattleTestBootstrap, GameConfig, Scene, old resources or nonbattle code is authorized by this Record. A missing config before scene `Start` requires evidence and a contract amendment before broadening scope.

Expected side effects: a new unique Development Player build/output, a Player JSON/log, transient in-memory config value and battle objects. The probe is dormant in Editor/release or when the flag is absent. Rollback and validation in Task Contract. Build and runtime outcomes to append; no Q07 production switch until proven.

Written: added the declared probe script with formal-root selection, loaded-config lookup, candidate fingerprint/key assertions, World and object checks, publication resource tracking, ordered shutdown, pool/quiescence and scene-unload assertions, and terminal JSON/exit code. A short-circuit `out` read was separated into an explicit call before compiling. Player build and runtime validation remain pending.

Validation: actual Windows Mono Development `BuildPipeline.BuildPlayer` run `q07-player-runtime-build-1` succeeded with 0 errors, reported size 130,680,494 bytes, 1,343 formal sidecar files and SPARK present. A fresh copy of its exact binaries/content (`q07-player-runtime-run-3`) was launched with `-ntsd-q07-player-probe` in a hidden window and `Start-Process -Wait -PassThru` captured exit code 0. Its terminal JSON reports PASS, formal root and expected fingerprint, equal manager/data/UI keys, World4, 29,400 owned resources, shutdown `RuntimeMapCleared`, two frames Stopped, 0 pool borrowers and 0 tracked resource survivors. Scene SHA remained `BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6`; GameConfig and ProjectSettings have no Git diff. The Player log contains 13 missing legacy audio-directory reports under `NTSD/Sound/SFX_*`; this package does not claim audio or natural-skill parity. The first non-waiting run PASS had an uncaptured process exit code, which the independent run-3 resolved. No old asset or Scene change.
