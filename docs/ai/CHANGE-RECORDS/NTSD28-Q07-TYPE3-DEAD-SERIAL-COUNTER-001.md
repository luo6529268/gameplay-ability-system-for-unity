<!-- CHANGE-RECORD
id: NTSD28-Q07-TYPE3-DEAD-SERIAL-COUNTER-001
status: FOCUSED_TEST_PASS
change-kind: Q07_TYPE3_DEAD_SERIAL_COUNTER
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
authority: formal root NTSD2.8-Logan.exe paired playable BattleWorld28::step_frame_slot apply_native_type3_frame_hp_drain and FrameMachine28::step with indexed w/e.dat
evidence: original Editor complete Driver positive child completed tick1 latch formal1 Unity1 and counter formal1 Unity0; paired source run-02 counter ticks1-3 are 1 2 0
-->

# NTSD28-Q07-TYPE3-DEAD-SERIAL-COUNTER-001

Before change, `LF2SpecialAttack.RunPostNativePhysicsSerialForWorldPass` invokes `DieEvent()` whenever a native type-3 object's HP is nonpositive, after the native frame step. `Generic_Die` calls `SetFrameDirect(hit_d)` and resets `AttackingCounter` even if the action is already `hit_d`. The formal ordinary type-3 frame path has its death redirect in the pre-frame HP-drain block, gated by positive `hit_a`; only state 3007 has an unconditional dead redirect and counter reset there. Formal tick1 counter1 versus Unity0 is measured in the original Editor; the late Unity reset is the narrow candidate.

The Task Contract declares the exact production path, preserved behavior, acceptance and rollback. Before script write, this Record is `PLANNED`; no production implementation or post-edit validation is claimed. No new manager, queue, worker, renderer, pool or shutdown stage. Keep DAT values, scene, config and nonbattle logic unchanged.

2026-09-26 implementation: removed only the `if (Health.HP <= 0) DieEvent()` block from `LF2SpecialAttack.RunPostNativePhysicsSerialForWorldPass`. The native C25 type-3 HP drain/frame transition still runs before frame-counter advance; state-entry handling and the `DieEvent`/`Generic_Die` definitions remain for their other callers. No DAT, scene, config or nonbattle script was edited under this Task. Original Editor recompile, paired eight-tick comparison, neighboring type-3 frame test and protection checks are pending. Status `CODE_WRITTEN`; no parity claim yet.

Scoped acceptance: after original Editor refresh/reload, paired complete-Driver job `cad966d5789d44b7bfc19b0c590680df` passed both positive and negative eight-tick cases (2/2). Native type-3 frame transaction job `68947de82a6e43ff9a39fb797e975698` passed (1/1). This fixes the measured completed-tick1 counter 1/0 difference and the consequent tick3 action divergence in the selected case. `Tools/Validate-ChangeLedger.ps1` and `git diff --check` exit0; four protected scene/config hashes unchanged. A post-test whitespace-only blank line was removed from the same method. Full SelfCheck was not repeated after the preceding unrelated odd-dvx assertion failure; natural Battle Play, root EXE same-world and Q07/D-024 overall closure remain pending. Evidence and limitations: `artifacts/diagnostics/NTSD28-Q07-TYPE3-DEAD-SERIAL-COUNTER-001/ACCEPTANCE.md`. Status `FOCUSED_TEST_PASS` is scoped, not Q07 completion.
