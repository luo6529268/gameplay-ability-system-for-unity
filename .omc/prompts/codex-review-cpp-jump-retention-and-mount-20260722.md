# Architect verification: C++ jump input retention and overflow mount binding

Review the current dirty-worktree diff only for these changes; do not edit files:

1. `SimulationWorld.SerialTickAll` no longer clears current action/directional keys before `SimTransit`, matching the user-authorized C++ behavior where current keys remain visible through frame advance and later frame_tick. Verify human and AI input phases still roll/clear keys correctly, no repeated edge/combo/cooldown risk is introduced, and battle-entry `NeedClearInput` remains correct.
2. `BattleRuntimeSelfCheck` updates old clear-key assertions and adds 211->212 jump tests for directional DAT velocity and inherited Vx/Vz. Verify the tests exercise real behavior and are not false positives.
3. `BattleCentralPresentationMountRegistry.BindOwnerRuntime` directly updates the renderer's own EntityModel mount in addition to ActiveMounts, to fix dynamic pool overflow mounts remaining Invalid. Verify lifecycle correctness, shadow behavior, inactive mount semantics, duplicate mount behavior, and generation safety.

Evidence already obtained: dotnet build 0 errors / 42 existing warnings; Unity source timestamp < Assembly-CSharp.dll < fresh full self-check result; full `BattleRuntimeSelfCheck` PASS.

Return severity-rated findings (P0-P3). Explicitly state whether any P0-P2 blocks completion. Cite exact files/lines.
