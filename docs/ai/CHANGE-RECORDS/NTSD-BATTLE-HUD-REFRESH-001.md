<!-- CHANGE-RECORD
id: NTSD-BATTLE-HUD-REFRESH-001
status: COMPILE_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Battle/BattleHudView.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHudViewEditorTests.cs
authority: User explicitly authorizes current HUD implementation with character names; presentation only, no battle rule change
evidence: docs/ai/TASKS/NTSD-BATTLE-HUD-REFRESH-001.md
-->
# NTSD-BATTLE-HUD-REFRESH-001
Current state: field-only HUD; TMP PlayerName already bound, scene/image 795x172; no producer or consumer of BattleHudState. Preserve existing user changes.
Plan: driver copies first bound active human roster character's ObjectId, HP/HPBound/HP3 and MP/MPMax into a reusable UI state at main-thread completed publication boundaries. Name from loaded character catalog, head from existing UI resource manager. View consumes only cached state, remains enabled when empty, clears on disable. No simulation writes, new services, queues, async work, scene or font edits.
Shutdown: stop publishing at Stopping; clear cached state in existing phase 5 after worker Join; view clears when host is not Running or on disable. No new disposal dependency; idempotent clearing. Do not reorder shutdown.
Validation: compile, focused clamp/name/null/clear/reappear/binding tests; targeted Play if existing Editor is available and safe. Worker cache must be captured before acknowledgement, never read live world from HUD.
Rollback: exact prechange backups in artifacts/diagnostics/NTSD-BATTLE-HUD-REFRESH-001/prechange.json after checking for later edits; separately record restoration. No irreversible changes intended.

Implementation: View LateUpdate consumes driver cached state; Apply/clear/binding validation completed. Driver publishes first bound human roster slot from catalog and existing vitals before worker acknowledgement or synchronous presentation; clears cache at phase 5. Four focused Editor tests and opt-in request runner added. Runtime generated project build passed 0 errors, 55 warnings; Unity/Play pending.

Final evidence: dotnet build Assembly-CSharp.csproj --no-restore -v:q -clp:ErrorsOnly exited 0, 55 warnings. Generated Editor project exited 0, 297 warnings (compile-editor.txt). New Editor tests were NOT yet in the generated csproj, so a separate build passed with /p:CustomAfterMicrosoftCommonTargets=<absolute artifacts/.../include-hud-tests.targets>, explicitly including BattleHudViewEditorTests.cs: exit 0, 350 warnings, 0 errors (compile-with-new-tests.txt). These are generated-project compiler checks, NOT Unity Editor compilation acceptance.
Five tests written: name/independent ratios, null/hidden/return, clamp/NaN/null name, missing name binding, host cached copy and Stopping rejection. They have not executed. Existing Editor assemblies remained timestamped 2026-10-03T20:32:32Z / 20:32:34Z, before final new tests. The Editor was observed running unrelated NUnit tests; local MCP endpoint refused connection. No second Unity instance, scene save/switch, or Play entry was attempted. Request closed via NOT_RUN receipt to prevent delayed execution. BattleRuntimeSelfCheck, real HUD scene/screenshot, worker-mode runtime, shutdown/re-entry acceptance remain pending.
Audit: Tools/Validate-ChangeLedger.ps1 PASSED before final metadata update, with repository historical warnings; final receipt ledger-final.txt. Script git diff --check passed. No scene, prefab, DAT, font, image or menu writes by this task. Runtime name is from the current world's RuntimeDataCatalog.GetCharacterData(ObjectId).name, avoiding auto-created managers. Portrait uses CharacterUIResourceManager.TryGetInstance().GetHeadSprite. One existing HUD selects the first bound active human roster participant; multi-HUD behavior was not added.
