<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F02-OID600-FULL-TICK-PAIR-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/fast_weapon_oid600_full_tick_source_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4DerivedWeaponReferenceEditorTests.cs
authority: selected formal 336B44 playable GameSession28::step, SimulationTickDriver28::step, PhysicsIntegrator28::step and formal OID600 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F02-OID600-FULL-TICK-PAIR-001.md
-->

# NTSD28-336B44-Q07-F02-OID600-FULL-TICK-PAIR-001

Created before diagnostic/test script edits. The existing F02 focused tests prove direct Unity physics but not the complete production Driver pass. A bounded source probe will call the selected playable host with a formal-resource OID600 at slot50 and exact high/boundary X velocities, while a focused Editor test will run the same staged OID600 through Unity's production release tick. Record all intermediate/final values and first difference. The probe is diagnostic code, not rule authority; only the unchanged selected playable live call path and formal EXE can define the rule. No production Unity code or asset may change in this package.

Validation, rollback and higher formal-root/natural exits are defined in the Task. Do not promote a source-build candidate to the formal root EXE or a synthetic complete tick to natural Play evidence.

Actual changes: added only `Tools/NTSD28Q07Diagnostics/fast_weapon_oid600_full_tick_source_probe.cpp` and one parameterized exact test in `NTSD28B4DerivedWeaponReferenceEditorTests.cs`. No production C#, formal source, DAT, resource, Scene or config changed. The probe compiled with the selected playable source and ran four Vx cases; output records from/to frame, final action, integer/precise X, Y and speed. Original Editor refreshed and compiled the new test; first full-tick run 4/4 PASS, final four controls plus 2048-wide physical projection 5/5 PASS. The source-rule X matches formal local results while projected physical X follows the shared 2048/1333 factor. [Raw evidence and report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F02-OID600-FULL-TICK-PAIR-001/REPORT.md). Fresh full SelfCheck was already PASS before this test-only package and was not rerun.

Unverified: formal root EXE same-state tick, natural weapon generation/throw, original Battle Scene Play, whole-world/whole-match equivalence and other entity projection. The standalone compiled source probe has no authority to replace the formal root EXE. Rollback is limited to the new diagnostic file and one test hunk after inspecting intervening work; do not restore shared dirty files.

Final checks: original Editor was ready and out of Play after the focused job; `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` returned `Change ledger validation PASSED` (the validator's pre-existing warnings about records outside the current diff remain); `git -c core.safecrlf=false diff --check` passed. The formal root EXE, Battle/Menu Scenes, GameConfig and ProjectBattleModeConfig retain their recorded SHA-256 values. No broad EditMode sweep was run for this test-only package.
