<!-- CHANGE-RECORD
id: NTSD28-Q06-STATE2000-FACING-ORACLE-001
status: VERIFIED
change-kind: TEST_ORACLE_AUDIT
code-path: Tools/NTSD28AuthorityTrace/state2000_facing_witness.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/FrameAdvanceRuntimeSnapshotEditorTests.cs
authority: Current formal playable battle_world.cpp step_frame_slot and frame_machine.cpp.
evidence: Source-first audit; prior e8431e2c failure preserved.
-->

# State2000 late facing oracle

IN_PROGRESS / SOURCE_FIRST. Prior e8431e2c failure preserved. Current source FrameMachine28::step held path preserves facing; only negative next flips it. BattleWorld28::step_frame_slot routes through actual step_frames_range. Old Unity compatibility frame body assigns facing from Vx, while current native C25 transaction does not. Do not restore old behavior without authority.

Exact initial code scope: Tools/NTSD28AuthorityTrace/state2000_facing_witness.cpp. Build against current formal playable closure; run twice; actual World slot21 type0 state2000 wait100 next0, positive/zero/negative velocity and opposite initial facing, plus negative-next flip control. Verify facing/action/counter. Diagnostic is source API evidence, not EXE recording. Unity test changes must be declared after source evidence. No production, Scene, resource or nonbattle change; no new owner/schema. Reversible local diagnostic addition, no deletion/revert of user work. Exit: source authority then accurate test semantics and focused test, no broad Q06 completion claim.

Source actual4 cases PASS, double-run SHAF4846CA2C06F20712A59E6A48699AD972DF5EFBF15CCFFD9271E9E5F7A2F5BB4; formal/closure identities match. Pre-edit extend exact scope to FrameAdvanceRuntimeSnapshotEditorTests.cs old CharacterFrameTick_DataOrientedState2000FacesFinalHorizontalVelocity method only: rename to PreservesFacingUnlessNegativeNext, retain three velocity representatives and assert source-preserved directions, add fourth negative-next flip control and action/counter assertions. Exact count becomes4, fallback0 remains. No production fix. Run this method and unchanged transition/counter neighbor only.

Closed VERIFIED, diagnostic/test-only scope. Native4 expected rows/double identity PASS; Unity focused492179a3 actual2/2PASS; compileCS0/LedgerPASS. See artifacts/diagnostics/NTSD28-Q06-STATE2000-FACING-ORACLE-001/ACCEPTANCE.md. No production edit; no whole Q06 claim.
