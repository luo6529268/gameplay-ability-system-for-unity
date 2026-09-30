<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-HAN-INMAP-PAIRED-CANDIDATE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/han_natural_earthquake_lfr.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs
authority: user D-024 unified proportional battle space, formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable candidate path
evidence: docs/ai/TASKS/NTSD28-Q07-D024-HAN-INMAP-PAIRED-CANDIDATE-001.md
-->

# NTSD28-Q07-D024-HAN-INMAP-PAIRED-CANDIDATE-001

Status: `RUNTIME_PENDING / TRANSLATED_RELATIVE_COLLISION_MATCH / SAME_SOURCE_STAGE_RED`.

Original state: formal and Unity paired Z650 candidate counts matched 1/0, but Unity mapped physical Z1025.75 was beyond the project stage upper bound760. The existing native paired LFR diagnostic and Unity original Battle Scene probe both hardcode source Z650. This cannot establish a valid project-map same-source candidate acceptance. The separate stage-boundary source/physical consumer audit remains open.

Planned change: opt in exactly source Z400 near/far two-case diagnostics, retaining the old Z650 interface and outputs. New names refuse overwrite. Unity receives source starts only and calls the already-owned World `BattleSpatialProjection` for physical starts; require runtime walkable polygon membership before claiming in-map. The native tool changes only diagnostic CLI/fixture values, never formal source, DAT or release behavior. Expected effect is evidence for a valid-map pair; production behavior is unchanged. Risk is native background rules or Unity input phase producing a new first difference; preserve its output and classify it without adjusting seeds or rules to make it green. Rollback and exit are in the Task.

Actual edits, commands, evidence and unverified items will be appended immediately after script changes.

Actual edits (2026-09-29): the existing native paired LFR diagnostic accepts only an additional `z400-jump3` selected mode, runs X520 and X580 at source Z400, appends `-z400` to only the new output stems and records `sourceZ` in its summary; the original default and `x520-jump3` modes keep source Z650 and their old filenames. The existing Unity Editor probe accepts two distinct in-map request paths with required source Z400 and matched X520/580; all four mapped paths use the existing World projection. It records live polygon membership and physical stage bounds and rejects the new in-map requests unless both starts are inside the map polygon and numeric bounds. Old request fields default to source Z650 and old paths remain. No production, formal source, DAT, Scene, map, camera or nonbattle code changed. Build and runtime validation pending.

Native result and corrected premise: g++ compiled the existing source closure and modified diagnostic with 0 errors. Two new source-Z400 paired Session cases exited0; their first action146 candidate counts at tick10 are near1/far0, but native stage clamps both combatants from Z400 to Z542 at tick1. The formal root EXE SHA was freshly confirmed and both new LFRs replayed with `passed=true`, `failureCode=0`, 50 declared ticks; 8 selected Han/Lee fields match paired CSV 400/400 per case, including root tick1 Z542. Formal background ID23 is `b/Hos/b.dat`, live bounds542..712. Current Unity map physical237..760 maps via zero-anchor rZ1152/730 to source150.18..481.60; the intervals are disjoint. Thus the original same-source valid-map premise is RED, and no production anchor/map change is justified by this diagnostic. Continue with Unity Z400 solely as a translated-stage *relative collision* control against the prior formal Z650 in-stage pair; this is weaker than same-state parity. Original Editor Play and protected hashes pending.

Original Editor result: generated Editor C# build passed with 0 errors (214 warnings); original Editor refreshed and loaded the new probe. The two original Battle Scene Play reports at Z400 are `OBSERVED_CACHE_LIMIT` with first action146 completed candidate near1/far0 and source X pairs 547/508 and 535/580 respectively, matching the formal selected row after startup phase offset. Both initial entities were checked inside the live walkable polygon and physical stage Z237..760; mapped Z631.2328767123. Both reports show ordered stop and zero borrowers, original Editor returned idle/non-Play. Battle/Menu Scene, SunagakureMap, GameConfig and ProjectBattleModeConfig SHA-256 remained at the protected baselines. Only a translated-stage relative candidate branch was accepted; collector-internal first-rejection detail, same absolute stage domain, all-world trace and Q07 remain pending. [Detailed evidence](../../../artifacts/diagnostics/NTSD28-Q07-D024-HAN-INMAP-PAIRED-CANDIDATE-001/ACCEPTANCE.md). No production code or content edited; rollback remains limited to the optional branches in the two listed scripts.
