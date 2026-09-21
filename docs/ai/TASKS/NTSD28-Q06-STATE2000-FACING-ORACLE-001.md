# State2000 late facing oracle

IN_PROGRESS / SOURCE_FIRST. Prior e8431e2c failure preserved. Current source FrameMachine28::step held path preserves facing; only negative next flips it. BattleWorld28::step_frame_slot routes through actual step_frames_range. Old Unity compatibility frame body assigns facing from Vx, while current native C25 transaction does not. Do not restore old behavior without authority.

Exact initial code scope: Tools/NTSD28AuthorityTrace/state2000_facing_witness.cpp. Build against current formal playable closure; run twice; actual World slot21 type0 state2000 wait100 next0, positive/zero/negative velocity and opposite initial facing, plus negative-next flip control. Verify facing/action/counter. Diagnostic is source API evidence, not EXE recording. Unity test changes must be declared after source evidence. No production, Scene, resource or nonbattle change; no new owner/schema. Reversible local diagnostic addition, no deletion/revert of user work. Exit: source authority then accurate test semantics and focused test, no broad Q06 completion claim.

Source4 confirmed current rules. Unity exact scope now one test method in Assets/NTSD/Scripts/Test/Editor/FrameAdvanceRuntimeSnapshotEditorTests.cs, rename and correct facing expectation with actual-source four-case controls; no helper/production changes.

Closed VERIFIED for exact source diagnostic/test correction. See ACCEPTANCE.md.
