# NTSD28-USER-SOURCE-CHARACTER-AI-NEAREST-WITNESS-001

Status: `FOCUSED_TEST_PASS / CONFIRMED_DEFECT`. D-024 test-only character AI source-distance witness; Q07 paused.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `SimulationTickDriver28::step` invokes `NativeAi28::step_main`, and `NativeAi28::select_local_character_target` ranks eligible characters by integer source X/Z Manhattan distance, strict `<`, slot order. Current Unity `GameConfig.asset` selects `DataOrientedCanonical` AI profile.

Unity pre-change candidate: SoA sensing snapshot, decision owned rows, refresh/fallback and legacy `world.X/Z` currently project physical runtime X/Z. Before production edits, create one source/physical ranking-inversion witness for production DataOriented and Legacy profile nearest-target selection.

Declared script scope: `Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs` only. No production AI, DAT, resource, Scene, ProjectSettings, camera or nonbattle edits. Test should assert source-nearest slot1 versus physical-nearest slot2 using initialized source-rule history on all participants. Reuse existing nearest capture helpers and exact profile configuration before entity registration. This witness establishes first difference, not full AI behavior parity.

Acceptance: original-project Editor focused test executes both profiles and returns the expected RED difference or disproves the candidate. Record actual values. Production correction requires a separate bounded Task/Change covering both profiles, snapshot/refresh/fallback and broadphase contracts.

Rollback: reverse only the added test hunk after review.

Result: formal-expected original Editor RED job `30d46b8a54c44c1a9de0ed8f33f873ff` failed both configured DataOriented and Legacy profiles, source-nearest slot1 versus actual physical-nearest slot2. The test was then converted into an explicit current-defect characterization asserting both distance orderings, and job `a2ba788ec3744f09a2be57da8175b8f7` passed 2/2. This pass confirms the defect; it does not close it or validate parity. No production AI change. Full position-domain contract and new production Task/Change precede implementation.
