# Recovery no-op fixture correction

IN_PROGRESS. Existing failure e8431e2c: expectedHP400 actual401 at LateEntityUpdateAll(1). Current formal playable battle_world.cpp begin_native_resource_tick increments independent phases; advance_native_resources uses phase12==0, not caller tickIndex. Unity BattleEcsCharacterRecoveryPass.Execute reads NativeResourcePhase12/3. New World starts phases0. Therefore current fixture has not established its named nonperiodic precondition.

Exact sole code scope: Assets/NTSD/Scripts/Test/Editor/FrameAdvanceRuntimeSnapshotEditorTests.cs, CharacterRecovery_NonPeriodicTickUsesProvenNoOp method only. Set phases12/3 to1 before partial late call, preserve all original HP/PP/no-op assertions and assert partial call did not advance phases. No production edits, no change to timing/rules, no new lifecycle owner or data schema. Existing phase-zero recovery test retained as opposite case. Run these two recovery cases, fresh compile, Change Ledger. Preserve dirty work; rollback only this method addition with user authorization. Does not close state2000 failure, platform full transaction or Q06.

Closed VERIFIED for exact test-only scope, b24b523e2/2; see ACCEPTANCE.md.
