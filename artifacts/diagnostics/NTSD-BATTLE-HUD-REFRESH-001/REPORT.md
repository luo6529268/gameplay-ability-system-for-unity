# NTSD-BATTLE-HUD-REFRESH-001 / COMPILE_PASS
Current field-only HUD implemented; current scene/name binding and 795x172 image preserved.
- BattleHudView: cached snapshot refresh, character name/head/vital rendering, null/hidden/disable clear, name-inclusive binding validation. Does not deactivate itself while empty.
- SimulationTickDriver: first bound active human participant; ObjectId-based catalog name and existing loaded portrait; HP/HP3, HPBound/HP3, MP/MPMax. Completed worker publication copied before acknowledgement, synchronous copy at presentation. Cache cleared in existing shutdown phase 5.
- BattleHudViewEditorTests: five focused cases, guarded opt-in Editor runner.
Compilation: runtime project 0 errors; Editor project 0 errors; explicit inclusion of new tests 0 errors. These builds do not prove current Unity Editor compilation or behavior.
Tests: NOT RUN. Existing Editor was running another test suite and had not imported the final new tests. MCP local endpoint unavailable. Request closed to prevent unattended delayed execution. SelfCheck, real Play, screenshot, dedicated-worker/re-entry verification pending.
No Scene/Prefab/DAT/image/font/menu writes, no commit/push. Prechange and postchange manifests preserve current user script baseline. See compile-with-new-tests.txt, editor-tests.txt, ledger-final.txt and docs/ai/CHANGE-RECORDS/NTSD-BATTLE-HUD-REFRESH-001.md.

Desktop screenshot correction: editor-busy-recheck.png copied screen coordinates covered by another application; it is NOT Unity evidence. Use editor-status-recheck.json for UI Automation window state; PrintWindow capture editor-busy-window.png separately inspected.
