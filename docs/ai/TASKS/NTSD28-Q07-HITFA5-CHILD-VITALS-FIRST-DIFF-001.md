# NTSD28-Q07-HITFA5-CHILD-VITALS-FIRST-DIFF-001

Status: `VERIFIED / SCOPED_FIRST_DIFFERENCE`. Parent BATCH-04/Q07/D-024 remains open.

Authority: paired playable `NativeAi28::step_non_character_hit_fa` hit_Fa5 branch creates a transient with `request.hp = 0`, then `SimulationTickDriver28::step` reaches the higher child slot in its same-tick frame tail; `BattleWorld28::apply_native_type3_frame_hp_drain` consumes indexed `w/e.dat` frame0 `hit_a:3/hit_d:1`. Formal complete Session tick1 child action is 1. Unity full Driver reaches child action 0 after the epoch hard error was repaired. Static Unity `LF2SpecialAttack.InitializeHealth` and `BattleSpawnVitalsWriter.Apply` suggest default HP500, but actual complete-Driver newborn HP is unmeasured.

Scope: add one assertion in existing `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HitFa5FullDriverEditorTests.cs` before the child action check, only for positive group at completed tick1. Compare actual child HP with formal zero. No production, DAT, Scene, config, UI or nonbattle edit. Retain existing action and eight-tick row checks unchanged. This is a defect-characterization probe; a passing test for a confirmed difference is not parity.

Validation: original Editor refresh/compile, exact positive and negative focused cases; report actual HP and current next first difference. Check Scene/config hashes and ledger validator. If HP is 500, establish the birth-vitals first difference and design a separate production transaction with a generic spawn-semantic contract, not OID219 special logic. Rollback of new assertion requires explicit approval; preserve all existing dirty changes.

Observed: original Editor complete-Driver positive reaches tick1 child with HP497 while formal source initializes HP0. Indexed frame0 drains 3; therefore the apparent action0/1 difference follows the already divergent birth HP. Negative eight-tick control remains 1/1 PASS. Diagnostic Task closes as a confirmed first difference, while its formal-expected positive assertion remains RED until a separate production Change; see Record and `artifacts/diagnostics/NTSD28-Q07-HITFA5-CHILD-VITALS-FIRST-DIFF-001/RESULT.md`.
