<!-- CHANGE-RECORD
id: NTSD28-Q07-CPOINT-RESOURCE-TRANSACTION-001
status: FOCUSED_TEST_PASS
change-kind: Q07_CPOINT_RESOURCE_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryAccountingCoverProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointThrowAtomicProductionEditorTests.cs
authority: formal root NTSD2.8-Logan.exe paired playable battle_world.cpp CPoint transfer and measured source-world Unity first difference
evidence: diagnostic NTSD28-Q07-CPOINT-RESOURCE-FIRST-DIFF-001 source real-DAT five cases and original Editor job 82a1b42c6b824a7db6dce675b58095d3 RED 2/5 positive
-->

# NTSD28-Q07-CPOINT-RESOURCE-TRANSACTION-001

Pre-change: paired formal source `settle_catch_relations` and `advance_catch_relations` call the generic native resource transaction with `cpoint.drain/gain/injury` or `throwinjury` before HP/environment writes. Unity `BattleCpointWriter` resolves the resource attacker and updates display but does not call that transaction. With source-real Hidan/Reaper injury30, paired source PP100→122; the original Editor resource-equivalent synthetic held and throw cases remain PP100; local-mode and counter controls pass. This measured first difference is limited to resource operands/branch gates and is not formal-EXE natural reachability.

Declared script paths and symbols: `BattleCpointWriter.cs::SyncCaughtByCpoint`, `ApplyHeldInjury`, `ApplyThrow`, `ApplyThrowInjuryDisplayLead` or renamed equivalent, one common local resource helper; the two existing focused test files only if a distinct drain/gain gate is added. Existing generic resource core is reused without editing `BattleDamageWriter.cs`. The expected side effect is current PP and consumed-MP accounting on eligible CPoint branches; display, HP, throw environment and relation state must preserve their order and values. Nonbattle, scene, DAT, images and config are excluded. The Task Contract records acceptance, rollback and evidence limits.

Actual code change: only `BattleCpointWriter.cs` was edited. `SyncCaughtByCpoint` passes the immutable CPoint to `ApplyHeldInjury`; that method calls a single `ApplyCpointResourceTransfer` before its existing HP/credit branch. `ApplyThrow` calls the same helper before its existing environment write. The helper preserves display-on-resolved-resource-attacker ordering, derives resource injury from the attack effect source and active mode, and passes CPoint drain/gain, suppression, local gate, current PP and PPMax into the existing generic transaction. The old throw display-only helper was replaced. No test assertion, DAT, scene, config, image, nonbattle or other production script was changed by this production ID.

Current status: `CODE_WRITTEN`. Validation pending: current original Editor compile, RED→GREEN five cases, adjacent held/throw suite and warmed allocation, Change Ledger validator, diff and protected asset hashes. Do not elevate scoped tests to formal EXE/Play or Q07 completion. Append actual outcomes before closing.

Before the next declared test-file edit: current original Editor produced GREEN 5/5 (`4d49360d1a484ae7b17171ff89373662`) and both adjacent classes 33/33 (`2b8b7fc22c9c4d47b81d9301a3030370`), including the warmed zero-allocation test. Add one `drain=10/gain=-5` forwarding case to `NTSD28B6HeldInjuryAccountingCoverProductionEditorTests.cs` using its existing `CreatePair` helper, with local-gate control and current-PP/consumed-MP assertions. This is a distinct gate for the two DAT parameters passed by the new common helper; no production code or other test method is to change in that edit.

Actual test addition under this ID: only the declared held test file gained `HeldInjury_ForwardsDrainAndGainThroughSharedTransaction` and optional `drain/gain` inputs on its existing pair constructor. The final test covers both eligible and local-mode-off accounting without changing any preexisting assertion. No throw test file was changed by the production ID; its earlier RED fixture belongs to the first-difference diagnostic ID.

Validation: original Editor compiled with zero Console errors; RED positive 2/5→GREEN 5/5, then both adjacent classes 33/33 before the drain/gain addition and 35/35 after it, including zero-allocation. The final run is `e6f2e543e9c149f1bf729ea11e5f5820`. Final `Tools/Validate-ChangeLedger.ps1` exit0, 871 records/five current code-diff files covered; the fifth `BattleControlsView.cs` was a concurrent unrelated diff and historical declared-path warnings remain. `git diff --check` exit0. Protected Menu/Battle Scene and two config hashes stayed unchanged; three v3 recovered progress docs NUL count0. Exact commands, result IDs and boundaries are in `ACCEPTANCE.md`.

Result: `FOCUSED_TEST_PASS / NATURAL_PLAY_AND_FORMAL_EXE_PENDING`. Q07/BATCH-04/total-goal remain active. Rollback is exact writer and added drain/gain test hunks only, preserving the diagnostic RED evidence and preexisting user changes.
