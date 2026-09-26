<!-- CHANGE-RECORD
id: NTSD28-Q07-FUSION-UNIFIED-AI-SHUTDOWN-001
status: FOCUSED_TEST_PASS
change-kind: Q07_FUSION_WORLD_SHUTDOWN_AI_ROW_INVALIDATION
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs
authority: formal paired playable fusion result and Unity battle-runtime-ordered-shutdown-contract.md stage 8
evidence: docs/ai/TASKS/NTSD28-Q07-FUSION-UNIFIED-AI-SHUTDOWN-001.md
-->

# NTSD28-Q07-FUSION-UNIFIED-AI-SHUTDOWN-001

Pre-change: formal and Unity paired complete-Driver positive/negative fusion traces match all declared tick0–3 fields. Positive test shutdown fails at `RenderersReturned` with a stale unified AI row publisher slot generation; negative case passes. `ResetRegisteredObjects` currently calls `BattleAiUnifiedRowPublisher.EndPass()` only after releasing registered entities, so field mutations during release can address a stale derived row. This is an implementation diagnosis pending focused verification, not a claim about source gameplay rules.

Declared modification: move the existing EndPass call to the start of World registered-object reset, before any entity or slot mutation. Stage 8 position remains unchanged; top-level shutdown ordering and Running logic remain unchanged. No DAT/Scene/ProjectSettings or other script is in scope.

Expected side effects: shutdown cleanup cannot publish into rows whose slot generations are changing; all active World objects, claimed slots and pool borrowers reach zero. Risk is a different reset caller depending on a live publisher during object release; inspect callers and run focused original-Editor regression. Rollback is review and reversal of only the declared hunk, with no deletion.

Post-change code/compile/targeted runtime results, protected hashes, validator and limitations will be appended after execution.

Code written: in `SimulationRegistryModule.ResetRegisteredObjects`, the existing `world.AiUnifiedRowPublisherForServices.EndPass()` now runs immediately after the spatial reset and before `registeredObjects.Clear()`/entity `OnRemoved`/pool release/slot reset; its former call after `ObjectBuckets.Clear()` was removed. This is a move within the same World reset transaction, with no new cleanup stage or Running-path mutation. The original Editor recompiled `Assembly-CSharp.dll` after `Assets/Refresh`; the exact two-case focused job is running. `git diff` confirms only the declared two-line move in this production file.

Focused validation: original Editor fusion full-Driver job `f86e8f6c602842439f1b955aa2d84d26` 2/2 PASS, including positive merge shutdown zero objects/slots/borrowers and negative no-merge. Adjacent ordered-shutdown job `cd79507d00054388b7a4b4e1862cd42b` ran exactly `DriverShutdown_ClosesRuntimeStagesBeforeStoppedAndIsIdempotent` and `StructuralShutdown_RejectsRegisterButAllowsExistingWorldReset`, 2/2 PASS. An initial adjacent job `f8e5186176734c568594a2c51bde1016` used a wrong namespace and matched zero tests; it is not counted as passing evidence. Protected Scene/config files are clean in Git and current SHA values are recorded in the parent acceptance work; no edits to them in this package. Full Play enter/exit after a natural fusion is not in this controlled-world focused gate.
