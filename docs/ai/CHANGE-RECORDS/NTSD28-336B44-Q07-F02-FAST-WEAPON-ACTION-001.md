<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F02-FAST-WEAPON-ACTION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponFrameLogicResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4DerivedWeaponReferenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: selected formal 336B44 playable PhysicsIntegrator28 post-friction type4/6 state1000 high-speed action40 and BattleWorld28 action commit
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F02-FAST-WEAPON-ACTION-001.md
-->

# NTSD28-336B44-Q07-F02-FAST-WEAPON-ACTION-001

Created before scripts. The Unity ordinary-weapon physics path omits the formal action40 selection; the shared fallback tests speed before the one-unit ground friction; and the pre-frame resolver holds a duplicate selection that is not a reliable physics owner. Own only the declared physical paths, focused test class and the old self-check assertions that directly invoke this duplicate pre-frame branch. Preserve the original frame for landing rule inputs, select from post-friction velocity before landing resolution, and do not reset frame counter merely by selecting action40. Test-first RED and focused GREEN are required. Same-seed formal root and natural Battle Play are independent pending gates; package status cannot be `VERIFIED` from a unit test alone. Existing dirty work is protected. Rollback means review and reverse only this ID's exact hunks, without restoring whole files.

Original Editor job `9d122170aa6c4530a84107c1d2c9666d` compiled the new test file and ran 30 scoped cases: 11 failed. Four ordinary type4/6 airborne cases and formal staged OID600 had action0 instead of40; shared ground -10 had action40 instead of0; two shared landing cases chose70 instead of60. Four type4 X-distance failures were a test fixture expectation error: existing formal identity-X extra is 0.2*Vx, so X should be12 for Vx10; corrected only that assertion. The RED count describes the original job and is not silently replaced by the corrected expectation.

Production code now calls one `CharacterMechanics.SelectFastWeaponActionAfterFriction` from ordinary and shared noncharacter physics after common friction. Both retain the physics-entry frame state and let qualifying type4/6 landing override the pending action before writing the final raw frame; the old pre-frame duplicate selection was removed while hit_Fa4/12 remained. No DAT, scene, config, mode, UI, camera or framework files changed in this package. Unity compile, focused GREEN, adjacent self-check correction, formal-root same-state and natural Play are still pending.

The first post-code Unity compile failed on the corrected test expectation only: `NTSDGlobal` lacked its `NTSD.Simulation` using directive in the owned Editor test file. That using was added; a clean recompile and GREEN test are pending. No production compile diagnostic was reported in that pass.

The corrected original Editor job `3bbb667a5eb5461aab7e98c6f633e773` rebuilt and passed 30/30, including staged formal OID600 direct production physics. The old self-check pre-frame assertions were changed to expect no action40 until physics. A fresh full `BattleRuntimeSelfCheck` request ran but failed before its weapon check in `CheckBattleSpritePrewarmTransactionContracts` with the partial-sheet source-bound/pivot message; no claim of full self-check PASS. To verify this package's old weapon branch independently, the already-declared focused Editor test class now invokes `CheckFrameLifecycleWeaponFrameLogicContracts` by reflection; compile/run pending. The full self-check failure is retained in `Temp/NTSD_BattleRuntimeSelfCheck.result` and the preexisting September 29 PASS file was copied to this package's report folder before the request handler replaced it.

Final original Editor focused job `643237600e314a539880670c50aab69a` compiled and passed 50/50 across F02, B4 identity-X extras and source-coordinate physics classes, including the isolated `CheckFrameLifecycleWeaponFrameLogicContracts` invocation. The full self-check failure is a separate observed red at the earlier sprite-prewarm check, not an F02 PASS. Package status is `RUNTIME_PENDING`: formal root EXE same-state complete tick, Unity canonical complete tick with OID600, natural Battle Play and Q07 overall exit are still unverified. Detailed evidence is in the report.

The final Unity MCP job result and original RED/GREEN results were copied as JSON into the report directory, along with the full self-check failure text. Independent read-only review found no actionable defect in the new F02 logic, but did not promote it to full runtime parity. No nonbattle or protected asset changes were made.

2026-09-30 planned F02 self-check extension, before editing that assertion: a fresh full self-check now reaches `CheckStateTransformLandingMatrix` and fails its old high-Vx transformed type4 landing expectation. Formal 336B44 `PhysicsIntegrator28::step` checks action40 at state1000/Vx12, then hard-impact landing overrides to action0, Vy-7, Vx8.4, durability15. The synthetic test still expects the former pre-frame selection to suppress that bounce. Only this one existing self-check assertion may be rebaselined under the already-declared `BattleRuntimeSelfCheck.cs` path; preserve all production code. Its prior original Editor full-run failure is retained in the C017 diagnostics. Full self-check remains FAIL, formal root/natural Play still pending.

The transformed type4 assertion now expects final action0, Vy-7, Vx8.4 and durability15. Original Editor recompiled it and a fresh full self-check crossed this matrix, then failed later at `CheckAudit7HitConfirmCarrierTail`: the older test expects a type3 special-hit latch to persist after the producing tick's entity tail. Current C012 production and formal 336B44 tail rule clear it. This is a separate C012 test-contract issue, so F02 full self-check is still FAIL. The before/after failure files are in the F02 diagnostics folder; F02 formal-root same-state complete tick and natural Play remain pending.

After the separately governed C012 old latch assertion was corrected, the original Editor fresh full `BattleRuntimeSelfCheck` returned `PASS` at 2026-09-30 06:28:29 UTC; its result is copied into the F02 diagnostics folder. This supersedes only the earlier full-self-check FAIL gate. F02 original focused 50/50 and this whole self-check pass do **not** prove the outstanding formal root EXE same-state full tick, staged OID600 Unity complete Driver tick or natural Battle Play. The package remains `RUNTIME_PENDING`, and Q07 stays open.
