# NTSD28-Q07-FUSION-UNIFIED-AI-SHUTDOWN-001

Status: `FOCUSED_TEST_PASS / CONTROLLED_WORLD_GATE_CLOSED`. Parent Q07 full-Driver diagnostic `NTSD28-Q07-FUSION-FULL-DRIVER-FIRST-DIFF-001` has 2/2 controlled-world focused PASS; Q07 and D-024 remain open.

Trigger: original Editor complete-Driver fusion row0 positive test produces tick0–3 battle state identical to paired formal Session, then fails at ordered shutdown stage `RenderersReturned`: `Unified AI row publisher observed a stale slot generation after commit`; World still has one active object and two claimed slots. The negative non-fusion case passes. Raw traces and test job id `8bd09de7b9a846f7a21b26ae7704ae23` are retained in the parent diagnostic.

Authority and boundary: the formal playable `GameSession28::step` / fusion result is the gameplay authority. The Unity `battle-runtime-ordered-shutdown-contract.md` stage 8 owns World logic cleanup after renderers return. The Unity unified AI row publisher is a derived view and must cease publication before cleanup mutates entity/slot generations. This repair must not alter Running pass order, combat outcomes, DAT, Scene, camera or nonbattle behavior.

Declared code path: `Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs`, method `ResetRegisteredObjects`. Move the existing `BattleAiUnifiedRowPublisher.EndPass()` from after entity release to before the first entity/slot mutation in the same reset transaction. Do not change top-level ordered shutdown stages, and do not mask failure by clearing World pointers. Existing Q07 full-Driver positive/negative test is the focused regression; run one existing ordered-shutdown test if necessary.

Acceptance: original Editor two focused cases pass with tick0–3 declared fields equal to formal traces, and helper reports zero World objects, claimed slots and logic-pool borrowers after shutdown. Verify compile, four protected Scene/config SHA unchanged, Change Ledger validator and diff check. Rollback is reversal of this exact method hunk after reviewing the worktree; no broad reset or cleanup.

Observed: original Editor fusion full-Driver job `f86e8f6c602842439f1b955aa2d84d26` 2/2 PASS with zero shutdown residue; ordered-shutdown neighbor job `cd79507d00054388b7a4b4e1862cd42b` 2/2 PASS. `git status` reports no modifications to Menu/Battle Scene, GameConfig or ProjectBattleModeConfig Asset. This is a focused controlled-world shutdown gate, not full natural Play exit/re-entry proof.
