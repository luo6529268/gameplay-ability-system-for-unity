<!-- CHANGE-RECORD
id: NTSD28-Q06-RECOVERY-NOOP-FIXTURE-001
status: VERIFIED
change-kind: TEST_FIXTURE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/FrameAdvanceRuntimeSnapshotEditorTests.cs
authority: Formal playable battle_world.cpp resource phase clock and resource pass; current Unity native clock contract.
evidence: Prior e8431e2c failure preserved; pre-change phase ownership audit; focused b24b523e2of2 PASS; artifacts/diagnostics/NTSD28-Q06-RECOVERY-NOOP-FIXTURE-001/ACCEPTANCE.md.
-->

# Recovery no-op fixture correction

IN_PROGRESS. Existing failure e8431e2c: expectedHP400 actual401 at LateEntityUpdateAll(1). Current formal playable battle_world.cpp begin_native_resource_tick increments independent phases; advance_native_resources uses phase12==0, not caller tickIndex. Unity BattleEcsCharacterRecoveryPass.Execute reads NativeResourcePhase12/3. New World starts phases0. Therefore current fixture has not established its named nonperiodic precondition.

Exact sole code scope: Assets/NTSD/Scripts/Test/Editor/FrameAdvanceRuntimeSnapshotEditorTests.cs, CharacterRecovery_NonPeriodicTickUsesProvenNoOp method only. Set phases12/3 to1 before partial late call, preserve all original HP/PP/no-op assertions and assert partial call did not advance phases. No production edits, no change to timing/rules, no new lifecycle owner or data schema. Existing phase-zero recovery test retained as opposite case. Run these two recovery cases, fresh compile, Change Ledger. Preserve dirty work; rollback only this method addition with user authorization. Does not close state2000 failure, platform full transaction or Q06.

## Actual completion
Test-only exact scope VERIFIED: original assertions preserved, explicit nonperiodic clock setup and unchanged-phase assertions added; actual2/2PASS, CS0, LedgerPASS. See ACCEPTANCE.md. No production, Scene, resources or other tests changed.
