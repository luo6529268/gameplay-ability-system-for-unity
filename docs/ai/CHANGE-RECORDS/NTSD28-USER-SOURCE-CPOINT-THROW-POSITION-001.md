<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-CPOINT-THROW-POSITION-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_CPOINT_THROW_POSITION_WRITER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointThrowRawBindingEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 advance_catch_relations; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-CPOINT-THROW-POSITION-001.md
-->

# NTSD28-USER-SOURCE-CPOINT-THROW-POSITION-001

Created before script edits. The throw writer updates physical X/XInt only. This bounded change will mirror formal source-rule X from catcher source XInt plus raw frame-center/CPoint offset, leaving source Z and all existing physical outputs unchanged. Record actual test evidence and limits after implementation.

2026-09-24 result: `BattleCpointWriter.ApplyThrow` now writes victim source-rule precise and integer X from initialized catcher source XInt and native raw-local offset, gated on both source carriers. It leaves source Z, physical X/Y/Z, velocity, actions and DAT untouched. The two-view moved-catcher witness asserts raw source caught X159 in both views versus physical X159/184. Original Editor imported the declared scripts and class job `d28204f25ea24d21bb0471b7099263f3` passed 8/8, including existing immediate/full-tick/local snapshot cases. `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: source-facing/selection, WPoint follower, full battle Scene Play/EXE and source-domain consumer activation remain open.
