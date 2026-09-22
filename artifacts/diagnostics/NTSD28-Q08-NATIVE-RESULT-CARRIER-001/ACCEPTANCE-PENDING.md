# Q08 native result carrier — focused validation, Play pending

Status: `FOCUSED_TEST_PASS / ISOLATED_SELFCHECK_PASS / REAL_BATTLE_PENDING / Q08_PARENT_OPEN` (2026-09-22).

The source-matched, separate carrier is now produced before combat in `NTSDBattleTickSystem.RunTick` and stored in `BattleResultsRuntimeState`. It records a 1..39/exclude-5 living-group mask, first terminal classification, monotonically advancing timer, 80/101/350 phase, source-style stored timer reset at 350 and mode transition code. The existing two-side result page and mode-4 reserve path still run separately after combat and are **not** source matched by this carrier package. >=144 Attack/Jump continue input is not wired; effective-combatant input ownership requires a separate source-matched audit. Current menu direct mode1 remains ordinary battle; formal paired story selection is not represented by Unity MatchConfig.

The new deterministic fields are reset on full Results reset/rematch, captured/restored in roster-results schema 2 and aggregate schema 26, included in checksum schema 29, and exported under `nativeResultFlow` separately from old UI `results`. Aggregate `IsValid` and component capture/restore reject a mask outside valid bits before mutation. The existing `BattleStateSnapshotRestore` preflight checks `snapshot.IsValid`, so that code path did not need an edit. The Q05 current-schema identity expectation was updated only from 25/28 to 26/29; protocol schema is unchanged.

Validation in independent Unity 2022.3.62f3 Library (`ntsd-q07-isolated-validation-20260922`):

| Check | Result | Evidence |
|---|---|---|
| Q08 focused full-tick classifier/timer and three existing controls | 8/8 PASS | [XML](../NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001/UNITY-NATIVE-CARRIER-FIRST.xml) |
| Native state snapshot restore, checksum difference/recovery, invalid-mask no-mutation | 1/1 PASS | [XML](../NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001/UNITY-NATIVE-CARRIER-RESTORE.xml) |
| Adjacent outcome writer seam | 2/2 PASS | [XML](../NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001/UNITY-NATIVE-CARRIER-OUTCOME-SEAM.xml) |
| After-world Results scene input seam | 3/3 PASS | [XML](../NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001/UNITY-NATIVE-CARRIER-SCENE-SEAM.xml) |
| Q05 current content/schema identity assertion | target test PASS | [class XML](../NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001/UNITY-NATIVE-CARRIER-Q05-SCHEMA.xml) |
| Full BattleRuntimeSelfCheck in isolated clone | PASS marker in log | [log](../NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001/UNITY-NATIVE-CARRIER-SELFCHECK.log) lines 27253/27264 |

The Q05 identity **class** has 2PASS/4FAIL: the target `TraceIdentityUsesFrozenSemanticBytesAndCurrentJointSchemas` passes; one other test cannot find the clone's `Tools/NTSD28AuthorityTrace/Scenarios/neutral-common-two-entity.json`, and three old raw-capture tests still expect 47 where current raw capture is 50. These four are not evidence of a native result carrier failure, but the class must not be called green. No broad repeat was run. The SelfCheck's transient `Temp` result was removed at Editor exit, so the fresh PASS claim relies on its explicit run log markers, not a surviving result file.

Original Battle Scene SHA-256 remains `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. Formal EXE/source and DAT/images were not modified; no computer-use. The shared repository HEAD changed during validation to `81cacc3e` and now contains these script edits; this was observed, not initiated by this package's commands.

Remaining for this child: same-state natural combat lethal first-difference trace, representative real battle Play and close/re-enter, final Ledger validation and actual production result-page consumption. The last item is the parent Q08 owner: current old UI can still activate at phase11, while formal result record is at timer101. Do not claim Q08/G-05/G-06 completion from the independent carrier's green tests.
