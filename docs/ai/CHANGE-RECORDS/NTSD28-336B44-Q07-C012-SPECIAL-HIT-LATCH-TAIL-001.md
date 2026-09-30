<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C012-SPECIAL-HIT-LATCH-TAIL-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsCharacterPostFrameTailPassEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: selected 336B44 formal NTSD 2.8-Logan release playable C012 active-entity special-hit latch tail
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C012-SPECIAL-HIT-LATCH-TAIL-001.md
-->

# NTSD28-336B44-Q07-C012-SPECIAL-HIT-LATCH-TAIL-001

Created before script edits. The [Task](../TASKS/NTSD28-336B44-Q07-C012-SPECIAL-HIT-LATCH-TAIL-001.md) records source authority, Unity first-difference candidate, exact scope, invariants, RED/GREEN sequence, runtime gates and rollback. No C012 script has yet been changed. Existing hit producers, snapshot carriers and pre-tail `ClearHitCandidateCarriers` semantics must be preserved.

Test-first edit: added two parameterized tail cases to the existing `BattleEcsCharacterPostFrameTailPassEditorTests` class. Each checks an exact character and a healthless OID815/type3 object with the latch set before `EntityPostFrameTailAll`, then requires both latches cleared and ordinary character `HitConfirm2` still cleared. Legacy and data-oriented modes are selected independently. No production code changed yet. Original-Editor RED pending.

Actual original-Editor exact RED job `39687e8b17e64464b4203f1c1bbcc55e` ran both selected parameterized cases and failed 2/2 at the first latch assertion (`expected false, actual true`). No production script had changed before this job. The shared active-entity tail edit is now eligible.

Production code written: `SimulationWorld.EntityPostFrameTailAll` now clears `entity.Runtime.SpecialHitLatch0EB` once at the shared active-slot tail, immediately before the existing `Health == null` branch. This covers exact characters in both post-frame pass modes and healthless type3 objects without changing hit producers, same-tick carrier clearing, serializer, or lifecycle reset. Original-Editor compile and GREEN remain pending.

Original-Editor exact GREEN job `783c720d94d54c80a3e531b06a6d21c5` passed 4/4 after compile/domain reload: new legacy and data-oriented tail cases, old post-frame maintenance parity, and old pre-tail latch-preservation control. Status is `RUNTIME_PENDING`: the native source test demonstrates next-tick candidate eligibility, but Unity has not yet run the complete hit-candidate tick or matched the formal root EXE's same state. Q07 remains open. [C012 report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C012-SPECIAL-HIT-LATCH-TAIL-001/REPORT.md).

Post-edit `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` passed (1035 records; 82 governed code files in the existing dirty diff). Scoped `git -c core.safecrlf=false diff --check` passed for all three current packages' owned scripts and current status docs. These checks do not fill the complete-hit or formal-root gates.

Next bounded validation before further script edits: extend the already-declared Editor test file with a synthetic type3 ordinary ITR and character BDY pair. Require a real collected candidate, preserve the latch through candidate consumption on the first complete `NTSDBattleTickSystem` tick, observe the shared active-entity tail clear, and require the next complete tick to consume a newly collected candidate. Keep test data local; do not change DAT or production hit logic. If the synthetic setup cannot reproduce the native test's preconditions, retain the RED and report the limit rather than promoting C012.

First complete-tick probe job `577ca5628240445fb49d72c6b32e04dc` failed at next-tick HP (500 unchanged) after first-tick latch clear. After adding a candidate precondition, job `239e202ee5cb45ce9ac9a0ad34da70d8` proved one real geometric candidate and unchanged action/position, but HP still remained 500. Fixture diagnosis: `LF2OtherObject` is the type5 effect shell and does not implement `IBattleHitCandidateConsumer`; a manually assigned ObjType 3 does not turn its object-hit behavior into type3. The test now uses `LF2SpecialAttack`, the production type3 shell. This is a test-fixture correction only; production code is unchanged and the corrected test has not yet run.

Corrected original-Editor job `bcfc0fb61f17468bbe8ba6c106304322` passed 1/1, 0 failed/skipped. A synthetic type3 `LF2SpecialAttack` with an ordinary ITR and type0 character with BDY produced exactly one geometric candidate. With the latch set, the first full `NTSDBattleTickSystem.RunReleaseTick` left target HP at 500 and cleared the attacker latch at the active tail. The second full tick damaged the same target. This establishes the Unity production candidate/consumer/tail sequence across two complete ticks; the two preceding failed jobs remain fixture-diagnosis evidence, not production RED. Status remains `RUNTIME_PENDING / UNITY_FULL_TICK_PASS` because the synthetic data is not a formal-root EXE same-state run or natural original Battle Scene Play.

Post-run fixture audit before another test edit: the earlier `ActiveEntityTail_ClearsSpecialHitLatchForCharacterAndHealthlessType3` instantiated `LF2OtherObject`, whose default `Health` is non-null and whose object enum is type5; setting only `Runtime.ObjType=3` did not establish the claimed healthless type3 path. Replace it in the already-owned test file with a test-only subclass whose object enum is type3 and whose `Health` is set null after the base constructor. Assert those preconditions, then rerun the two parameterized tail cases in the original Editor. No production edit is needed.

The corrected original-Editor tail job `b90ee5f82a6a491bb06ddca77e88b7fa` passed both Legacy and DataOriented cases 2/2, 0 failed/skipped. `HealthlessType3Entity` now has object enum type3 and `Health == null` asserted before calling the shared tail, and both its latch and the character latch clear. Earlier 4/4 includes the same two cases before fixture correction; cite this final 2/2 for the healthless claim. The complete type3 hit test job `bcfc0fb61f17468bbe8ba6c106304322` used the actual `LF2SpecialAttack` consumer and remains 1/1. No further production changes.

2026-09-30 self-check extension declared before editing: a fresh full `BattleRuntimeSelfCheck` crossed F02's transformed-landing matrix, then failed at `CheckAudit7HitConfirmCarrierTail` because this older assertion still requires the type3 `SpecialHitLatch0EB` to persist after `EntityPostFrameTailAll`. Current selected formal `simulation_tick_driver.cpp` active-slot tail explicitly writes `special_hit_latch_0eb=false` after hit consumers, and this C012 package's shared Unity tail already does the same. Add only `BattleRuntimeSelfCheck.cs` to this ID's test ownership; change the one post-tail type3 latch predicate and message to expect cleared, retaining the pre-tail positive latch and independent weapon/MP controls. Do not change production, DAT or assets. Re-run the relevant exact tail tests and fresh full self-check; any later failure remains a failure.

Original Editor result: the stale post-tail type3 assertion alone now expects latch false. The exact Legacy/DataOriented tail and two-complete-hit-tick job `212acf233e684324b761b350e972f727` passed 3/3. A fresh `BattleRuntimeSelfCheck` request then returned `PASS` at 2026-09-30 06:28:29 UTC; raw files are in this package's diagnostics folder. The initial test command timed out after 30 seconds but the Editor reported a live job id, which was polled to terminal success; it was not restarted. This changes only a historical self-check expectation, not production C012 or formal-root/natural Play status. Package remains `RUNTIME_PENDING` for those higher gates.


2026-09-30 C012限定关闭：正式根两自然Pur222反射案例和未反射控制各40tick/560声明字段，合计1680/1680；原Battle唯一special-hit-latch-scene-01 PASS/DONE，40tick/560字段与源码严格相同、首差0，tick6自然OPoint出生、tick8 owner1/group2/action30、最终尾flagfalse/NarutoHP500。原Editoridle/nonPlay、Scene clean且四保护SHA稳定。结合已有尾部/前尾4/4、真实healthless type3 2/2、两tick命中资格1/1和SelfCheck，关闭共享特殊命中尾清门；Pur30自然销毁链不证明下一tick角色再命中，整World/物理键/整场与Q07仍未关闭。本轮补证未改生产/DAT/Scene/非战斗。

Final governance checkpoint: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path PASS/exit0,1058 records/18 governed code files; git -c core.safecrlf=false diff --check exit0. OriginalEditor idle/nonPlay, no compile/reloadpending; fourprotectedSHAs unchanged. These checks do not extend the declared behavior scope.
