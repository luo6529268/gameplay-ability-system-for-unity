<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-CHARACTER-AI-NEAREST-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_CHARACTER_AI_NEAREST_SOURCE_WITNESS_TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable NativeAi28 select_local_character_target; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-CHARACTER-AI-NEAREST-WITNESS-001.md
-->

# NTSD28-USER-SOURCE-CHARACTER-AI-NEAREST-WITNESS-001

Created before test-script edits. Test-only source/physical character AI target-rank witness in configured DataOriented and Legacy execution profiles. Production AI remains unchanged until source contract and first difference are established.

2026-09-24 result: original-project Editor formal-expected RED job `30d46b8a54c44c1a9de0ed8f33f873ff` failed both DataOrientedCanonical and LegacyCanonical cases: expected source-nearest slot1, actual physical-nearest slot2. The same test now explicitly characterizes source distances 20/40 and physical distances 100/30, and asserts the current defect as a labelled witness; focused job `a2ba788ec3744f09a2be57da8175b8f7` passed 2/2. No production AI scripts changed. This is a confirmed non-perceptual first difference, not parity acceptance. Next production package must include sensing rows, owned decision rows, refresh/fallback, unified snapshots and spatial broadphase under both execution profiles, with source-complete/incomplete contracts; full Driver/Play/EXE pending.
