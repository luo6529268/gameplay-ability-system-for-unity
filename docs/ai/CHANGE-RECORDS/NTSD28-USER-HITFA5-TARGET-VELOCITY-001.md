<!-- CHANGE-RECORD
id: NTSD28-USER-HITFA5-TARGET-VELOCITY-001
status: IN_PROGRESS
change-kind: FORMAL_HITFA5_INTEGER_TARGET_VELOCITY_AND_D024_RATIO_CHECK
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28FixedViewRunRatioEditorTests.cs
authority: formal indexed OID219 w/e.dat frame51 and paired playable NativeAi28 behavior5
evidence: docs/ai/TASKS/NTSD28-USER-HITFA5-TARGET-VELOCITY-001.md
-->

# NTSD28-USER-HITFA5-TARGET-VELOCITY-001

Status `IN_PROGRESS / EQUAL_START_FOCUSED_PASS / TARGET_HISTORY_PENDING`. Task: `docs/ai/TASKS/NTSD28-USER-HITFA5-TARGET-VELOCITY-001.md`.

Before this turn's test-script edit: add one configured-view static initial-gap 224 control to the existing declared OID219 focused test and generalize its final expected screen fraction from hardcoded 3 to the source integer quotient. Compare it with the already-measured moved-target case that reaches the same current integer gap 224 from an initial gap 151 plus one scaled 48px move. This is test-only; it must make the stateless-coordinate ambiguity concrete without changing production or DAT. Validate only the focused class in the original Editor, then update the Record with the actual result.

Original Editor first control run `3c052bcba30a43eab1de16429bbc197d` was 11/11 PASS, including static initial gap224 and the moved-target cases. The strengthened rerun `d248af5583144fa4822db084fa43cee3` was again 11/11 PASS and explicitly asserted the moved target's current integer gap equals224 after one production movement step (298 after two). Static initial gap224 and moved-history gap224 therefore have identical Unity producer inputs, while the formal source-rule velocities are respectively4 and3. Only the declared test script changed this turn; no production fix has been attempted. The pass is evidence of an unresolved defect and a proof that a memoryless current-gap scaling formula cannot satisfy both histories.

Cross-module implementation gate: `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/COORDINATE-HISTORY-CONTRACT-DESIGN.md` records the required battle/reference position domains, writer ownership inventory, snapshot/reset/checksum, and focused/full-scene acceptance. No new runtime coordinate field or behavior5 compensation was written under this Record.

2026-09-24 scope correction before further script edits: the equal-start 8/8 exit remains valid, but it does not close D-024 behavior5 after a target's prior scaled movement. The static source-formula witness in `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/TARGET-HISTORY-VELOCITY-FIRST-DIFFERENCE.md` predicts formal/Unity next Vx 3/4 after one formal-48px target move, and 4/5 after two. Extend only the already-declared focused Editor test script with a diagnostic case that moves the registered target through the production character mechanics outlet before birthing the indexed OID219 child. Do not modify production based only on the formula; first capture the actual current result and both control histories. No DAT, Scene, camera or nonbattle edits. Rollback is the reviewed inverse of this package's exact test/production lines, preserving other work.

Original Editor focused job `92b7728e66144755aafa43f9b4e8deb0` completed 10/10 PASS after the diagnostic extension. Two new tests measured current Unity child Vx4 versus formal integer-rule Vx3 after one target move, and current Vx5 versus formal Vx4 after two moves. They constructed the target history with production `CharacterMechanics.StepBattleLogic`, then loaded formal OID219 frame51, birthed an actual registered child and checked its next non-character step. These are characterization passes for a **confirmed parity defect**, not alignment passes. Existing four equal-start cases stayed GREEN. Production remains unchanged by this follow-up; no coordinate compensation has been implemented. Status stays `IN_PROGRESS` until a general coordinate-history contract and its implementation pass both histories plus full Driver/Play and formal comparison.

Original Editor actual indexed-behavior child-birth RED job `f7a183006eaa465fbda93743ce395277`: four same-start-position cases, default/configured × target delta ±151, all fail at birth Vx because formal integer quotient is ±3 while Unity writes ±3.02. The later movement-fraction assertions were not reached; GREEN must verify those and reject any extra inverse view factor.

Actual change: only `LF2Entity.RunHitFa5FrameLogic` child `directVx` now divides the integer target-child X difference by integer 50 before conversion to double. No DAT or camera change and no producer-side view factor. The focused test loads the staged formal OID219 `w/e.dat` frame51 (`hit_Fa: 5`), creates an actual registered World child, then runs one production non-character physics step. Formal and staged DAT SHA-256 both `178D2653E2BC4AFFF7B933DA2F28A87ECD6217CBD4F99291EA1B19F300704274`.

Original Editor after refresh and compile: focused ratio class job `467b1d07c3044bd6a596bca3f8cb7a8d` 8/8 PASS (four new birth/motion cases plus four existing ratio cases). Birth Vx is ±3 in default/configured Worlds; next X delta is `±3*Sx`, giving screen fraction `3/1333` for configured 2048-width view. This proves no extra inverse factor is needed for equal starting positions; it does not prove all dynamically diverged target positions or complete Driver/Play behavior. The earlier code-only GREEN job `f0bcf83ac4b64f92aeca2d60d41bf1cf` was also 8/8 but is superseded by the source-DAT-loaded rerun.

Validation pending: representative original Battle Scene Play and formal EXE visible trace; classify unindexed hit_Fa6 separately. Q07 remains paused while D-024 non-perceptual audit has open coordinate classes.

Pre-change: Unity behavior5 computes child directVx with floating division by 50.0; formal source divides integer target-child X positions by 50 before double conversion. With gap151, Unity writes3.02 while formal writes3. Existing D-024 shared non-character final position integration already multiplies motion by World view factor. A second inverse factor at this producer would undertravel an equal-start-position case; this package tests that proposition before considering any change.

Intended after: exact integer arithmetic at the behavior5 velocity producer, preserving target/child position operands and all spawn side effects. No DAT, Scene, camera or nonbattle change. Actual birth test and subsequent motion step must distinguish default and configured World without modifying shared integrator.

Rollback: inverse only this Change ID's reviewed diff, preserving all prior dirty work.
