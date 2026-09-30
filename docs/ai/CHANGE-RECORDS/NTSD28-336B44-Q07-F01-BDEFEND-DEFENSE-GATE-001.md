<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F01-BDEFEND-DEFENSE-GATE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleOrdinaryDefenseResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleOrdinaryCharacterDamageRouteResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryDefensePureResolverEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryDefenseReducedHitProductionIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorAtomicProductionIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: selected formal 336B44 playable DefenseResolver28::match_ordinary through BattleWorld28::classify_confirmed_defense; user reset F01
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F01-BDEFEND-DEFENSE-GATE-001.md
-->

# NTSD28-336B44-Q07-F01-BDEFEND-DEFENSE-GATE-001

Created before script edits. Current Unity common defense resolver gates on ITR effect and two production callers pass effect; formal playable gates on raw ITR bdefend. This package will correct only the common argument/consumer and focused tests, preserving remaining kind/state/direction/spark/dbdefend/dvx/OID order. Potential downstream change is which ordinary hit follows reduced defense versus armor/unarmored route when effect and bdefend disagree. No nonbattle or asset modification is permitted. Validation and limited rollback are in the Task. No formal root natural Play proof is claimed from source review or NUnit tests.

Scope amendment before touching the additional self-check hunk: original Editor two RED cases became GREEN 2/2, adjacent defense tests 18/18 PASS. Fresh full `BattleRuntimeSelfCheck` then failed at its own old `effect61 must be outside ordinary defense` assertion. The selected formal rule gates on `bdefend`, so this same behavior package now owns only the precise `CheckAlternateHurtTriggerMatrix` assertion and opposite-field control, not unrelated self-check contents. The earlier FAIL result is preserved in the report directory.

Actual code: common `BattleOrdinaryDefenseResolver.Resolve` parameter/61 gate now receives bdefend, both real production callers pass their ITR bdefend, and existing pure/production tests name and distinguish the fields. One self-check hunk changed the obsolete effect61 expectation and added bdefend61 negative control. No change to source EXE, DAT, Scene, asset, camera, other effect uses, hit order or formulas. Original Editor recompilation succeeded; exact tests 2/2, adjacent defense tests 18/18, full self-check PASS. Formal root same-state hit, natural CAG DAT interaction and Battle Scene Play are unverified. [Detailed evidence](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F01-BDEFEND-DEFENSE-GATE-001/REPORT.md). Rollback only the owned hunks after worktree review; preserve shared dirty files.

Final checks: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` returned PASS (1049 records, 9 governed code files in diff); `git -c core.safecrlf=false diff --check` passed. Formal root EXE and Battle/Menu Scene, GameConfig and ProjectBattleModeConfig SHA-256 remain unchanged against this package's protected baseline. Original Editor was ready/out of Play. No exhaustive EditMode sweep was run; the one full self-check was justified by the changed shared hit resolver.

2026-09-30 evidence addendum: separate Change `NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001` closed the formal-root natural contact witness, not this Unity runtime/Play exit. Unchanged formal content generated OID619 on tick11, source recorded kind0 effect1/bdefend61 contact on tick12, frozen root trace recorded applied damage50, and 20 declared ticks × 6 selected fields matched source. Root trace does not expose the raw bdefend field; `nativeParityClaim=false`. Original Unity Battle Scene natural Play remains pending. [Evidence](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001/REPORT.md).

2026-09-30 scoped closure: `NTSD28-336B44-Q07-F01-GUREN-CAG-SCENE-PLAY-001` completed the missing original Battle Scene Play. Relative tick11 CAG birth, tick12 Lee HP500→450/action186 and all six selected fields over 20 ticks matched the formal source 120/120, with clean saved Scene and Editor out of Play. Combined with original RED/GREEN opposite-field tests, adjacent 18/18 and full self-check, the owned bdefend gate is `VERIFIED` within F01. This does not close all ordinary defense variants, player-input parity, presentation, Q07 or total alignment. [Play report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F01-GUREN-CAG-SCENE-PLAY-001/REPORT.md).
