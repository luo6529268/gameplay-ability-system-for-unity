<!-- CHANGE-RECORD
id: NTSD28-USER-D024-PROTECTED-OBJECT-X-CLAMP-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_PROTECTED_OBJECT_STAGE_BOUNDARY
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28D024NonCharacterXBoundaryWitnessEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable BattleWorld28::settle_ordinary_stage_bounds
evidence: artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/D024-CURRENT-GATE.md
-->

# NTSD28-USER-D024-PROTECTED-OBJECT-X-CLAMP-001

Before: protected OID122/123 positive participant-class X clamp uses 10-pixel margins in Unity versus formal 100-pixel margins. The existing focused diagnostic passed only as a characterization of this first difference. D-025 separately replaced immediate noncharacter offstage culling and must remain untouched. SelfCheck expects stale 10/790 positions. Exact old writer, users, authority, invariants, validation and rollback are in the same-ID Task.

Intended after: correct only the protected-object branch and its independent source-rule position mirror at the existing PreFrame outlet. Preserve D-025 timing, all noneligible paths, DAT and nonbattle systems. Mode1 selected-stage exception is not silently implemented without a current Unity carrier contract. Record exact focused/compile/SelfCheck/Play results after implementation. Current status `IN_PROGRESS`; no script change yet at creation.

2026-09-25 code written: the original Editor ran the new exact protected-clamp cases RED 0/4 (job `8f21a2c844d041458bae8b3f55625946`); all four failures were the old 10-pixel behavior, including width150 resolving to X140 instead of the formal collapsed 100 endpoint. `LF2Entity.ApplyPreFrameXBounds` now clamps eligible physical X and initialized source-rule X independently to `[100,max(100,width-100)]`, synchronizing only the source X integer. The existing D-025 walkable timeout and noneligible branches are unchanged. The focused test now uses formal type6 OID122/123 and checks both coordinates; only the two stale protected-object SelfCheck expectations changed. Unity recompile, GREEN, SelfCheck and Scene check remain pending. The prior IN_PROGRESS paragraph above describes the pre-edit state.

2026-09-25 focused result: original Editor refreshed/recompiled and exact job `cbe836eedf9d4fe68f80a1ff8bb2481e` passed 7/7 (protected4 plus D-025 neighboring3). Fresh existing-request `BattleRuntimeSelfCheck` returned `PASS` at `07:30:31.971947Z`. Menu/Battle/GameConfig disk SHAs held. Actual script paths are the three declared metadata paths; `LF2Entity` changes only the protected branch, the focused test changes its old characterization and adds source/negative/width150 assertions, and SelfCheck changes two expectations in a pre-existing dirty file. No DAT, image, Scene, camera, Menu or production TTL edit. Targeted Play, formal EXE same-world trace and Q08 mode1 stage gate are unverified; status is `FOCUSED_TEST_PASS`, not full alignment. Full evidence in the same-ID acceptance report.

Final governance check after this update: `Tools/Validate-ChangeLedger.ps1` PASS (814 Records, 17 governed code files in current diff); `git -c core.safecrlf=false diff --check` exit0. No broadened test suite was run.
