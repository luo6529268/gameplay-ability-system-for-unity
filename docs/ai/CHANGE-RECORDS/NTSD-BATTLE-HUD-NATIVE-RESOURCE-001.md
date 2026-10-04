<!-- CHANGE-RECORD
id: NTSD-BATTLE-HUD-NATIVE-RESOURCE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleHudChangeTracker.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHudEventEditorTests.cs
authority: User skill-resource HUD regression; actual native Unity writer PP contract
evidence: docs/ai/FILE-OPERATIONS/NTSD-BATTLE-HUD-NATIVE-RESOURCE-001/RECORD.md
-->
# NTSD-BATTLE-HUD-NATIVE-RESOURCE-001
User reports blue resource bar fails during skill use; HP is suspected, not reproduced. Source root cause: native ApplyNativeInputActionCore spends Health.PP->Runtime.PP; HUD Bind incorrectly reads MP and SetVitalValue omits Pp; legacy MP setter incorrectly dirties blue HUD. Fix event source to canonical PP while preserving MPMax (actual native max_mp initialization uses Health.MaxMP). HP/HPBound/HP3 mapping retained. No values, DAT, timing, skill costs, world rules or view polling changes.
Declared paths: NTSDEntityRuntime.cs (MP no HUD emission; Pp maps to HUD Mp); BattleHudChangeTracker.cs (bind PP); existing BattleHudEventEditorTests.cs (use canonical field and add real action/recovery/spawn regression). No deleted BattleHudViewEditorTests recreation. Driver/view remains unchanged unless independently proven necessary.
Acceptance: reproduce through actual skill writer+formal staged DAT, then same case passes; real damage/recovery/spawn writers, max changes, lifecycle binding, cached event/filter and Image fill as environment allows. Compile and ledger; exact-source isolated managed tests or dedicated Unity environment, never claim OS/Scene proof from setters. Original Editor ownership unconfirmed; no takeover.
Shutdown unchanged: same existing tracker ownership, stop/reset/detach and main-thread dispatch transaction, no new manager/queue. Same runtime values/storage serialization, vital store observer retained. Rollback exact pre-task backups with later-change checks and audited restore. No irreversible operation.

CODE_WRITTEN: MP setter no HUD side effect; canonical Pp maps HUD Mp in existing vital callback; initial tracker Bind reads PP; existing event tests migrated to PP plus native skill/recovery/spawn test. Red real Anko353 writer PP500->425 legacyMP500 dirtyFalse captured red2-result.txt. No view polling introduced.

2026-10-04 final evidence / RUNTIME_PENDING:
Root cause reproduced with old compiled production runtime and actual staged formal Anko DAT frames: ApplyNativeInputAction(character,353), PP500->425, MP500 unchanged, HUD dirtyFalse (red2-result.txt). HUD tracked legacy MP rather than native PP.
Fix: PP vital change maps to HUD Mp; legacy MP no longer emits HUD; initial Bind reads PP, MPMax retained (native max_mp carrier). No driver/view polling or gameplay/DAT values changed. Three declared source files, actual patch diff-0/1/2.patch. Existing tests only; deleted BattleHudViewEditorTests not restored.
Compilation: full runtime independent dotnet/MSBuild34warnings0errors, full Editor final275warnings0errors. Not original Editor compilation.
Independent Unity2022.3.62f3 EditMode executeMethod loaded freshly compiled production Assembly-CSharp.dll; 50 assertions PASS plus all9 zero-argument methods in actual BattleHudEventEditorTests invoked with NUnit assertions PASS. This is reflection-driven Unity engine validation, not a Unity TestRunner XML run, full SelfCheck run, physical input test or original-scene Play.
Final run unity-preview6.log and unity-result.txt; harness HudNativeValidation.cs.txt. Earlier failed fixture/engine-free attempts retained as historical evidence; latest final run succeeded. Fixture uses formal Anko frame table with minimal sealed object catalog, not whole DAT metadata/world collision loop.
Executed actual ApplyNativeInputAction twice(500->425->350), ApplyMpRecovery(351), ApplyNativeHitResourceTransaction drain(326), private standard HP damage transaction(500->470/HPBound490), ApplyHpRecovery(471), BattleSpawnVitalsWriter.Apply(HP150/PP180/maxima150,180). Actual world.HudChanges -> SimulationTickDriver.DispatchBattleHudChanges -> MMEventManager -> BattleHudView -> actual uGUI Image.fillAmount asserted each phase. Separate max changes, disable unsubscribe, late enable full cached refresh, stale version, handle generation rebind, detached producer all pass. Lifecycle methods invoked explicitly in isolated EditMode. No HP notification defect observed.
Original Editor ownership/dirty state unresolved; no control performed, no scene screenshot or full battle Play claimed. Battle scene disk SHA changed concurrently from 402139f7924af2dd93f2bb0a3a27fe29e1f9298b7293a2a0a757cc78ec8694cb; lastwriteUTC06:10:45. Current SHA in postchange.json. This task did not write scene. Scene work paused pending ownership clarification. Menu scene, BattleHudView and NTSDButtonRipple remain SHA-identical to pre-task. Preserve parallel changes.
Status RUNTIME_PENDING (original scene). No commit/push.

Final governance validation: Tools/Validate-ChangeLedger.ps1 exit0 PASS; historical warnings retained in ledger-validation.txt.
