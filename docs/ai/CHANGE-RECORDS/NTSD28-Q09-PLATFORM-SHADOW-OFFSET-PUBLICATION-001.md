<!-- CHANGE-RECORD
id: NTSD28-Q09-PLATFORM-SHADOW-OFFSET-PUBLICATION-001
status: IN_PROGRESS
change-kind: BATTLE_PRESENTATION_BEHAVIOR
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
authority: paired playable render_snapshot.cpp shadow screen_top/depth_order and battle_world.cpp platform offset producer
evidence: PRE_EDIT_TASK_AND_RECORD_CREATED / TEST_FIRST_PENDING / UNITY_COMPILE_PENDING
-->

# NTSD28-Q09-PLATFORM-SHADOW-OFFSET-PUBLICATION-001

Before script modification, source producer and Unity runtime carrier are present, but Unity Legacy `UpdateShadow` uses `GetRenderZInt()` alone and central snapshot/command has no offset. Test-first scope is one shadow-only command displacement plus snapshot copy preservation and unaffected Z/order/foot anchor; production scope is exactly the three Task paths. Existing LF2Entity dirty boundary hunk near stage X must remain intact. No new manager, worker, pool, scene object or shutdown phase is introduced. Task contains authority, scope, acceptance, risk and rollback.

After the edits, record actual symbols, RED/GREEN focused results, compile/Play evidence, Scene/hash/governance checks, and remaining root EXE/visual limits. Do not infer nameplate or other Q09 item completion from shadow movement.
