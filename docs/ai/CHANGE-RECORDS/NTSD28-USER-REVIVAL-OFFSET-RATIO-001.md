<!-- CHANGE-RECORD
id: NTSD28-USER-REVIVAL-OFFSET-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_REVIVAL_RELATIVE_XZ_RATIO
code-path: Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalNormalFloorRngProductionEditorTests.cs
authority: D-024 and paired playable BattleWorld28 normal state14 revival
evidence: docs/ai/TASKS/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001.md
-->

# NTSD28-USER-REVIVAL-OFFSET-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Task: `docs/ai/TASKS/NTSD28-USER-REVIVAL-OFFSET-RATIO-001.md`.

Original Editor actual-pass RED job `ee8e9b4a6e98494885f4dcf0bbfd570d`: default World control passed; configured World expected precise X160.727681920 vs raw150.0. First X assertion precedes Z and integer/RNG assertions, which are covered after the production correction.

Pre-change: normal revival copies integer teammate-average X/Z as a base and adds raw synchronized random relative X/Z. The configurable World already scales motion at other outlets but not this revival placement. Formal raw source remains the rule authority; D-024 is a user-approved screen-fraction exception for the final relative displacement only.

Intended after: multiply two random centered offsets at their final precise position writes by corresponding World factors. Keep peer selection/average, integer mirrors, RNG sequence, Y/floor, HP/PP, frame and DAT unchanged. Original Editor RED/GREEN with actual pass, focused existing default test. No Scene/camera/nonbattle edit.

Rollback: inverse this package's reviewed lines only, preserving all other current work.

Actual change: the two centered synchronized random X/Z offsets are multiplied by the World view factors only after teammate integer averages are computed. Absolute average, integer mirrors, RNG order and count, Y/floor, HP/PP and lifecycle remain unchanged. Original Editor refreshed and compiled; actual-pass RED `ee8e9b4a6e98494885f4dcf0bbfd570d` (configured X150 vs expected160.727681920) became focused whole-class GREEN `693f7ece801947a29e21511f861a9ed1` 9/9, including configured/default World, integer mirrors, no-peer and negative integer-average controls. Full Play, EXE visible revival ratio, stage/floor semantics remain open.
