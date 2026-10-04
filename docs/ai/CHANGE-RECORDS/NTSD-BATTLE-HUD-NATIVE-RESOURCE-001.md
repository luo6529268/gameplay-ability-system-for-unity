<!-- CHANGE-RECORD
id: NTSD-BATTLE-HUD-NATIVE-RESOURCE-001
status: IN_PROGRESS
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
