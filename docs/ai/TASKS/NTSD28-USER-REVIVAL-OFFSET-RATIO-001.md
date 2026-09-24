# NTSD28-USER-REVIVAL-OFFSET-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Parent D-024 all-entity ratio; Q07 paused.

Paired playable `BattleWorld28` normal state14 revival with remaining lives computes teammate integer-average X/Z, then adds synchronized random relative offsets (`0x90`/`0x91`). Unity `BattleRespawnModule.ApplyRespawnWithoutStoredCount` currently adds the same raw offsets. With the fixed full-background view, only the random displacement relative to the teammate average should receive World X/Z screen-fraction factors. Preserve average absolute position, source integer mirrors, RNG values and order, HP/PP, floor Y, lifecycle and DAT.

Declared scripts: `Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs` two precise-X/Z assignments and `Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalNormalFloorRngProductionEditorTests.cs` actual pass test. Show configured-view RED and default control using live World/peer group, then factor final random offsets once. Run original Editor focused, existing default source/tail tests and Ledger/diff. Full Play/EXE visible ratio and stage/floor contract remain open.

Rollback: inverse only this reviewed diff; preserve unrelated dirty work.

Evidence: original Editor actual-pass RED `ee8e9b4a6e98494885f4dcf0bbfd570d` (one default pass, configured X first-difference); whole focused class GREEN `693f7ece801947a29e21511f861a9ed1` 9/9. No full Battle Scene/EXE visible certificate.
