<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-AI-STAGE-EDGE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28D024AiChildScenePlayProbeEditor.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 playable native_ai.cpp and user D-024 proportional project-map exception
evidence: docs/ai/TASKS/NTSD28-Q07-D024-AI-STAGE-EDGE-PLAY-001.md
-->

# NTSD28-Q07-D024-AI-STAGE-EDGE-PLAY-001

Status: `VERIFIED / SCOPED_AI_STAGE_NEAR_BOUNDARY`.

Before: the World AI stage-bound supplier passed a focused original-Editor RED→GREEN and 30/30 adjacent tests, but no real Battle Scene AI near/far decision Play. The current child probe runs production Menu→Battle AI but does not position a target at the fractional project-map near threshold or report resulting depth keys. Existing World `BattleSpatialProjection` is the only approved ratio/anchor entry.

Planned exact edit: add an opt-in stage-edge request and report path to `NTSD28D024AiChildScenePlayProbeEditor.cs`; keep its normal child route unchanged. Use source Z160/161, set physical locations with `world.SpatialProjection`, run complete Driver ticks, record target, branch preconditions, depth keys, shutdown and pool state. No runtime production or Scene/DAT/config change.

Expected side effects: temporary in-memory participant positions/hit stop and an optional clean Scene switch during the diagnostic; no saved asset changes. Acceptance, risks and inverse-hunk rollback are in the [Task](../TASKS/NTSD28-Q07-D024-AI-STAGE-EDGE-PLAY-001.md). Actual diff, commands, outcomes and any unresolved interpretation will be appended after edit.

Actual edit: added `stageEdgeTargetZ` opt-in request/report fields, clean Battle→Menu in-memory switch, `RunStageEdge` and `SetStageEdgePosition` to the exact probe file. The existing child-spawn route remains unchanged. Both test positions use `world.SpatialProjection`; no conversion constant was added at a consumer.

Validation: generated Editor C# build exit0 / 0 errors with 22 preexisting MSBuild assembly-reference warnings. Original Editor recompiled/reloaded the modified probe and ran independent real Play sessions: source target Z160 selected depth down1/up0, source target Z161 selected depth down0/up1, each on complete Driver tick76 with target slot0. Both reports PASS, orderly shutdown/Menu return and pool borrowers0; all five protected hashes unchanged. [Acceptance and raw report paths](../../../artifacts/diagnostics/NTSD28-Q07-D024-AI-STAGE-EDGE-PLAY-001/ACCEPTANCE-20260929.md). `git diff --check` passed before this record update; final validator result is appended below.

Limit: this is a controlled abnormal-target near-boundary Play witness, not a natural full-match AI or far-boundary/root-EXE visual A/B. The generic `aiTargetSlot` output remains -1 because it belongs to the older child-spawn path; the new before/after target fields both equal0. No DAT, map, Scene, config, camera, nonbattle or production AI script was edited. Rollback remains inverse hunk of this probe only.

Final governance check: `Tools/Validate-ChangeLedger.ps1` exit0, 1026 records and 66 governed dirty code files covered; focused `git diff --check` exit0. The warning list concerns historical declared paths absent from the current diff and is not a validation failure. No commit or push was performed.
