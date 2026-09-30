<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/guren_cag_defense_lfr_probe.cpp
authority: selected formal 336B44 root EXE and playable GameSession28 step; formal Guren OID84/OID619 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001.md
-->

# NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001

Created before diagnostic script edits. Unity F01 mechanism is focused/self-check green, but no formal root same-state CAG contact has been observed. This script will record a natural OPoint-generated CAG from unchanged formal DAT, serialize source LFR, and replay it in the frozen root EXE. The script is a diagnostic carrier and does not define the rule. It cannot alter DAT, formal binaries, Unity runtime or scene. The Task defines bounded reachability, validation and rollback.

Actual changed code: only `Tools/NTSD28Q07Diagnostics/guren_cag_defense_lfr_probe.cpp`. Its responsibility is to initialize Guren action150/Lee action110, run at most 20 selected-source `GameSession28::step` calls, record natural OID619/ITR contact and serialize LFR. It does not implement a battle rule. Selected playable source-closure g++ compile exit0; source X600 output found OID619 tick11 and kind0 effect1/bdefend61 hit tick12. Frozen formal root EXE SHA was verified, exact LFR replay exited0, report passed, and declared tick1–20 fields matched 120/120; root hit event was applied with HP damage50. `nativeParityClaim=false`; root trace does not print bdefend or defense kind. Evidence and hashes: [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001/REPORT.md), [Comparison](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001/source-x600/comparison.json). Remaining parent risk: Unity original Battle Scene natural Play and same-state visual result have not been run. Rollback only this new diagnostic file after status/diff review; no DAT, Scene, Unity script, formal binary or nonbattle file changed by this subpackage.

Final checks: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` PASS (1050 records, 10 governed code files in the shared diff); `git -c core.safecrlf=false diff --check` PASS. Battle/Menu Scene, GameConfig and ProjectBattleModeConfig SHA-256 matched the protected pre-run values. An initial hash command used incorrect directory names and returned `NOT_FOUND`; the corrected `rg --files` paths were hashed successfully and are the basis of this conclusion. No Unity Editor/Play/test operation was performed by this diagnostic subpackage.
