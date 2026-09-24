# NTSD28-USER-SOURCE-OBJECT-AI-TARGET-001

Status: `FOCUSED_TEST_PASS / PLAY_PENDING`. D-024 non-perceptual check; Q07 paused.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `SimulationTickDriver28::step` calls `NativeAi28::step_non_character_hit_fa`, whose `native_ai.cpp` nearest-target scan uses source integer X/Z Manhattan distance, strict `<10000`, slot order. Unity production `LF2Entity.RunFrameLogicBeforeAdvance` reaches `ResolveObjectAiTargetForFrameLogic` and currently ranks from physical `GetRuntimeXInt` and target-Z helper.

Declared scripts: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`; `Assets/NTSD/Scripts/Test/Editor/NTSD28B0ObjectAiTarget3F8DeconflictionEditorTests.cs`. Add divergent source/physical target ranking RED on the existing object hit_Fa scan. Use source integer X/Z only when source and all eligible scanned candidates have initialized source history; otherwise retain physical ranking, without mixing domains. Preserve eligibility, strict comparator, slot order, stale-target behavior, existing velocity and DAT values. No Scene, resource, camera, ProjectSettings or nonbattle edits.

Acceptance: original-project Editor RED before and GREEN after plus existing B0 class. Full Driver, natural Battle Play and formal EXE parity remain separate gates.

Rollback: reverse only declared script hunks after review.

Result: original Editor RED `88b377f5ec1f4460871e43ca02d9e2c2` selected slot1 rather than source-nearest slot0. Dual-ranking correction and incomplete-history physical fallback passed final B0 9/9 job `c87c40594fa942c1b530668e2c2405c0`. Full Driver/natural Play/formal EXE and character AI snapshot remain open.
