# NTSD28-Q07-HIDAN-NATURAL-RELATION-001

Status: `PLANNED`, BATCH-04/Q07. Source/root EXE natural Hidan catch and Unity 11-field 40-tick raw captures match; Unity raw schema omits the two reciprocal catch slots.

Final scoped status: `FOCUSED_TEST_PASS`; two independent original-Editor jobs each passed 1/1 after an initial MCP transport-log failure. See `ACCEPTANCE.md`. Physical Battle Play remains pending.

Authority: formal root EXE trace `NTSD28-Q07-HIDAN-NATURAL-INPUT-REACHABILITY-001/release-natural-x580|x1200-trace.jsonl`, paired playable source and Hidan OID24. Unity full-driver test should use the exact natural input JSON from `NTSD28-Q07-HIDAN-NATURAL-UNITY-DRIVER-001` and existing `NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests`, which builds the same formal-content world.

Declared scripts before edit: new `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HidanNaturalCatchRelationEditorTests.cs` and its Unity `.meta` only. No production, exporter schema, DAT, image/audio, Scene, config or nonbattle edit. Test X580/X1200 in Authority400 over 40 complete ticks, compare actor runtime CaughtSlotIndex and target CatchSourceSlot90 against the root trace each tick. Verify positive tick11 and far miss. Do not run blanket suites.

Acceptance: original Editor compile0 and two focused NUnit cases PASS; no later scene/asset mutation, Ledger/diff/protected hashes. Failure is preserved with exact tick/field first difference. This does not certify physical keyboard Battle Play, pixels or all Q07. Rollback only new test/meta and this evidence; preserve other work.
