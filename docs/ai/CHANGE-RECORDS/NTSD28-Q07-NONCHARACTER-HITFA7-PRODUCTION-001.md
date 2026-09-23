<!-- CHANGE-RECORD
id: NTSD28-Q07-NONCHARACTER-HITFA7-PRODUCTION-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_NONCHARACTER_HITFA7_AUTHORITY_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable SimulationTickDriver28/NativeAi28::step_non_character_hit_fa behavior7
evidence: R15 same-input tick1 Unity-only slot50 OID206 clone birth and source-model B0/B2/main first difference
-->

# NTSD28-Q07-NONCHARACTER-HITFA7-PRODUCTION-001

Before: real type3/action kind scenario emits an extra Unity-only slot50 OID206/action40 at tick1. Unity `RunHitFa7FrameLogic` unconditionally invokes `SpawnHitFa7Clone` before checking the preassigned target. Formal non-character behavior7 has no such unconditional spawn and returns when target context is missing. The Unity target-present body advances precise Y once; its integer Y synchronization, missing-target behavior and clamp/order differ. The earlier Task wording that Y advanced twice was incorrect and is superseded.

Intended after: authority-backed non-character behavior7 preassigned-target gate and motion/side-effect order without the spurious clone, preserving character/type0 behavior and other hit_Fa families. Same-scenario source-model/Unity three-stream trace should be rerun to locate any remaining difference. No claim of formal EXE observable parity from this Change alone.

Risk: `LF2Entity` is a shared runtime base. Gate by current DAT type rather than CLR name; first check producer/consumer order and cover target-missing and target-present real-DAT representatives. Do not suppress the clone only in the diagnostic path or alter the source scenario. Scope, focused validation and rollback are in the matching Task. Actual code and tests will be recorded after implementation.

2026-09-23 implementation: original Editor imported the new test and ran the exact two EditMode cases in job `cade51ba91a0445abcfe3a2cc5e443b8`; both failed before the production edit. The missing-target case observed an extra slot, and the real OID875 target-present case observed World count 3 instead of 2. `LF2Entity.RunCurrentDatFrameLogicBeforeAdvance` now dispatches DAT non-characters to a separate behavior7 method. The method consumes only a live preassigned target, applies the formal double X contribution, Z dead zone, one Y acceleration/advance, action60 threshold, clamps and facing. Character behavior7 and the preexisting unrelated knockout diff in this shared file were left intact. The new test and Unity-generated meta are the only new test paths. Original Editor compile, focused GREEN, same-scenario three-stream comparison and regressions are pending; this is `CODE_WRITTEN`, not verified.

Independent review correction: exact native `double` 0.7/0.4 constants replaced initial float literals, and behavior7 no longer synchronizes integer Y before the physics integrator. Focused target-present assertions now require exact Vx/Vy/Vz/precise-Y and unchanged integer Y through the local step, plus the speed clamps/action60. Recompile, rerun and a target-present full-tick previous-Y witness remain to be evaluated before claiming that branch fully aligned.

Validation after correction: original Editor refreshed/recompiled and ran exact job `870e12d4f9784c70baafd151636a9565`, 2/2 PASS; related non-character reduced and native-AI alias focused job `0fa36428f8bb4a0ea99674a78cb62cd6`, 2/2 PASS. Earlier pre-correction 2/2 GREEN is superseded; the wrong-namespace regression filter selected 0 tests and is not credited. The unchanged frozen type3 scenario recapture compares source-model/Unity main raw 3 ticks/150 field occurrences/zero differences; B2 joint input/RNG is equal, B0 shared input/slots/lifecycle equal with known RNG topology difference. Full result and both capture pairs are in `artifacts/diagnostics/NTSD28-Q07-NONCHARACTER-HITFA7-PRODUCTION-001/REPORT.md`. Saved Scene hashes remain unchanged. Target-present full tick/previous-Y and formal EXE observable comparison are pending; this status is scoped focused pass, not full parity.

Final-code regression rerun: original Editor job `53986dbd474043debbe237ac1ac98937`, 2/2 PASS after the precision/integer-Y correction. Change Ledger validation PASS (708 records), `git diff --check` PASS, and Menu/Battle saved Scene hashes match the pre-task values.

Additional focused seam: local behavior followed directly by non-character mechanics preserved old `NativePreviousY104=-30`; original Editor job `218c8c1ed3264db3bb812b75dc0cc956` passed 2/2. This test bypasses frame motion. Formal OID875/action55 has `dvy:1` between native AI and physics, so its local `Y=-21.6` assertion is not a completed-tick expectation; a source-matched full-tick witness remains required.
