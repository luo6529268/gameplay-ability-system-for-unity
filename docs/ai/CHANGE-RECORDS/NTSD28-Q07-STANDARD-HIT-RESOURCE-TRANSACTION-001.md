<!-- CHANGE-RECORD
id: NTSD28-Q07-STANDARD-HIT-RESOURCE-TRANSACTION-001
status: FOCUSED_TEST_PASS
change-kind: SHARED_STANDARD_CHARACTER_HIT_RESOURCE_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ResourceTransactionPureCoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5UnarmoredHpConsumptionProductionEditorTests.cs
authority: formal root NTSD2.8-Logan.exe plus paired battle_world.cpp native hit resource transfer on standard unarmored type-0 target branch
evidence: NTSD28-Q07-SASUKE-NEEDLE-TARGET-HIT-FIRST-DIFF-001 tick16 attacker current MP500 formal versus 400 Unity, target HP and OID440 lifecycle equal
-->

# NTSD28-Q07-STANDARD-HIT-RESOURCE-TRANSACTION-001

2026-09-26 implementation/acceptance: `BattleDamageWriter.ApplyNativeStandardHitResourceTransfer` is now called once after standard unarmored type-0 HP/credit writes. It resolves the existing resource owner, computes injury from the physical source and current mode, then uses the native-order transaction in current `PP`/`PPMax`. The pure tests now assert `PP` while preserving a separate legacy `MP` sentinel. Original Editor compiled 0 errors; RED 2/2 became GREEN 2/2, pure class 8/8. The preserved X550/X1200 26-tick formal EXE traces vs fresh post-fix Unity raw compare 1428/1428 and 2100/2100 common fields; pre/post raw only changes X550 attacker current MP at ticks16–26. The adjacent 24-test selection had 23 pass and one pre-existing-scope reflection `TargetParameterCountException` in the class's `HitPlan_CharacterProjectionAccumulatesEffectiveHpDamage`; its baseline was not separately measured and is not claimed green. Ledger validator PASS, diff check exit0, four protected hashes and three NUL-free docs verified. Exact evidence: `artifacts/diagnostics/NTSD28-Q07-SASUKE-NEEDLE-TARGET-HIT-FIRST-DIFF-001/RESOURCE-FIX-ACCEPTANCE-20260926.md`. Scope remains focused; natural Play and other hit routes pending.

2026-09-26 RED before production edit: original Editor compiled the two new focused tests. Job `9e33d33e04c24d0287244d43321c00db` failed exactly 2/2 as intended: pure helper expected current `PP=0` but retained 40; shared type3-child→type0 target production route expected root `PP=426` but retained 400. Target HP assertion reached and passed. The failing tests were written after this Record and before changing `BattleDamageWriter.cs`.

Pre-change: the original Editor raw X550 trace matches formal entity fields except Sasuke current MP after accepted hits. The production standard unarmored character route omits native resource transaction; the existing standalone helper is only called by pure tests and writes `MP`, while native current MP maps to `PP`. Unrelated dirty work in the same repository is protected.

Expected change: adjust only the three declared script paths. Test the current-PP bank, local/suppression/owner gates and ordinary shared hit route. Invoke the existing resource-injury formula and transaction once at the same position as formal standard unarmored branch; use physical attack-effect source for definition/suppression and up-to-two-hop resolved resource attacker for reward. The caller must supply the existing active-mode percentages and max-current-MP field. Keep existing reduced/armor/noncharacter hit branches untouched pending independent evidence.

Expected side effects: type-0 attacker and type-0 target can receive native local-mode MP rewards after an accepted unarmored hit, with DAT drain/gain order; input MP consumption counters follow the existing transaction. No DAT, image, Scene, camera, ProjectSettings, GameConfig/ProjectBattleModeConfig, Menu or nonbattle changes. This can change checksum/snapshot values only because the previously omitted gameplay MP changes now occur.

Validation and rollback: preserve pre-fix X550/X1200 traces; run focused RED→GREEN and original Editor compile; rerun only these paired scenarios to new files, compare all common fields, validate Ledger/diff and protected hashes. Runtime/Play claim only if actually run. Revert only exact new hunks after review; do not clean unrelated work or delete historic outputs.
