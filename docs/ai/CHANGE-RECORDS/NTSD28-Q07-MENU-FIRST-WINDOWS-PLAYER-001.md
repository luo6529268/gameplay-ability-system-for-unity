<!-- CHANGE-RECORD
id: NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001
status: VERIFIED
change-kind: BATTLE_PLAYER_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsBuildProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/NTSD28Q07MenuFirstWindowsPlayerProbe.cs
authority: user-confirmed Menu-first Battle-second Build Settings; Q07 formal-content callback contract
evidence: original Editor Menu callback PASS; prior Windows build probe only builds Battle Scene
-->

# NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001

The matching Task declares the exact pre-change state, script paths, invariants, acceptance and rollback. This package adds only opt-in diagnostic wiring and a Development Player-only callback probe. The existing Battle-only build request remains the default, and neither Menu production code nor Scene serialization is changed.

Actual code: existing build probe now accepts opt-in `menuFirst`, checks enabled Menu/Battle order, writes a separate build artifact and output path, and preserves Battle-only defaults. New Development Player-only runtime probe starts in Menu, invokes the same real component callbacks as the prior Editor probe, records formal-content/Battle/unload evidence, then exits.

Validation: original Editor Tundra compile PASS; menu-first Windows Mono Development build Succeeded with 0 errors and exact two-Scene order; postbuild sidecar 1371 files/46,883,057 bytes independently matched v2 manifest SHA/size with no extras. First `-batchmode -nographics` run failed formal prewarm at the shadow Material premultiplied-alpha contract and was stopped; no PASS is claimed for that launch. Two graphics-enabled hidden-window Player runs produced PASS, the second exit code 0. Both observed Menu -> formal prewarm -> Naruto OID2 -> Additive BattleRunning/World2 -> matching three content keys -> ordered unload/Menu, Stopped/World null/pool borrower0. Both Scene SHA and EditorBuildSettings SHA stayed unchanged. Scoped diff check and Change Ledger validator PASS. Evidence: `artifacts/diagnostics/NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001/REPORT.md`. This verifies only the Menu-first Player callback/content-bootstrap subgate; physical input, pixels, Android and formal EXE parity remain open.
