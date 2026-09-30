<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-FORMAL-VIEW-CONSTANT-CONSOLIDATION-001
status: COMPILE_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/GameConfig.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
authority: user D-024 2026-09-29 unified proportional battle spatial conversion and formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033
evidence: docs/ai/TASKS/NTSD28-Q07-D024-FORMAL-VIEW-CONSTANT-CONSOLIDATION-001.md; docs/ai/CHANGE-RECORDS/NTSD28-Q07-D024-UNIFIED-SPATIAL-PROJECTION-001.md
-->

# NTSD28-Q07-D024-FORMAL-VIEW-CONSTANT-CONSOLIDATION-001

Status: `COMPILE_PASS / STATIC_VERIFIED`.

Pre-change: `GameConfig` battle reference defaults and two `SimulationTickDriver` absent-Asset fallback callsites independently spell 1333 and 730; all other production formal-view literals belong to the new `BattleSpatialProjection`. These are battle-only parameters. The serialized Asset remains 2048×1152; source/default 1333×730 semantics remain.

Planned modification: change the battle default fields and both Host fallback pairs to `BattleSpatialProjection.FormalViewWidthPx/HeightPx`; do not change any other GameConfig field or Host logic. This provides one formal-dimension definition for future ratio changes. Expected runtime side effect: none for current constants. Validation and rollback per Task. Actual edits/results to append after script modification.

Actual edit (2026-09-29): `GameConfig.BattleFixedViewRunReferenceWidthPx/HeightPx` defaults now read the two `BattleSpatialProjection` constants; both `SimulationTickDriver` `ConfigureFixedViewRunDistance` absent-Asset fallback pairs read the same constants. Serialized Asset values remain 2048×1152 and are untouched. No other script or consumer changed in this package. Original Editor compile, focused static scan, validator and protected hash check pending.

Validation (2026-09-29): original Unity Editor requested `refresh_unity(force/all/compile=request)` while idle; Editor.log reports `*** Tundra build success (6.56 seconds), 6 items updated`, updated `Assembly-CSharp.dll` at 15:41:29, no `error CS` in the fresh compile tail. Static production scan of the three scoped scripts finds numeric 1333/730 only in `BattleSpatialProjection` constants and a descriptive GameConfig tooltip; the GameConfig default and both Host fallbacks now read those constants. `Tools/Validate-ChangeLedger.ps1` passed (1014 records, 43 governed code files in the shared dirty diff); scoped `git diff --check` passed. Menu Scene and both configuration Asset SHA-256 remained unchanged. During this refresh `NTSD_Battle.unity` acquired an unrelated deletion of a `Text (TMP)` UI object at 15:41:08; this package did not edit the Scene, and the change was preserved for its owner. No new Play or test run was needed for these compile-time equivalent constants; the preceding projection package's original-Editor focused EditMode run was 5/5. Q07 birth/collision wiring remains open.
