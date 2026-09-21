# Destroy original-pool owner focused result

Status FOCUSED_TEST_PASS / LOGIC_ONLY_OWNER_RETURN; Renderer/runtime closure remains pending.

Before: the LF2Entity/LF2LivingObject/LF2Character Destroy implementations resolved the logic pool after Unregister or renderer return detached the original World. Isolated World factory tests0/1/5 each removed slot20 but original pool stayed ActiveCount1. RED job9b993b58 actual3/3 failures archived.
After: each exact override captures BattleLogicReferencePool before DestroyEvent, retains its existing event/Destroy/renderer/unregister ordering, and releases to that captured owner at its existing tail. Three production files, each3 added/1 replaced lines; no service/schema/resource/Scene/nonbattle edits. Existing Free unchanged.

Validation:
- Actual Unity reload and jobe7476287:3/3PASS. Each test constructs two independent Worlds via actual LogicEntityFactory, asserts originalpool1->0/source slot removal, controlpool1/controlslot intact, repeatedDestroy stays0/control1, both Worlds shutdown successfully.
- RED cleanup explicitly reclaimed fixture-only leaked borrow in finally, after failed ownership assertion and saved output; no production compensation.
- git diff --check PASS; Ledger628records/4current governedfiles PASS. Worktree HEAD moved externally to72ecf16e during work; no agent commit/reset/cleanup.
- No fullSelfCheck or Play in this step. Renderer-owned destruction not yet executed, so no VERIFIED claim. Include Renderer destroy/reuse and closure in stable lifecycle/platform package acceptance rather than rerun unrelated role matrices.

Next: platform parent can resume physics history/native own-frame motion source gaps; preserve this package as pending Renderer/closure validation. Final Q06 exit must include this owner fix, not merely the platform direct-Free fixture.
