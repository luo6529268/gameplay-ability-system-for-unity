# Q08 ordinary result-flow versus story selector — 2026-09-22

Status: `SOURCE_GATE_CONFIRMED / UNITY_SELECTOR_NOT_EQUIVALENT / PRODUCTION_UNCHANGED`.

The matching playable `GameSession28::step()` (`source/ntsd28_playable/src/game_session.cpp`, around lines 2722–2745) calls `BattleFlow28::step()` before the world combat update only when the session has **neither** `story_mission_id` nor `story_child_stage_id`. The same host validates the pair at initialization (around lines 1506–1520) and populates both when a story child is selected (around lines 2094–2095). A selected story child instead leaves `battle_flow_step_` empty; its stage progression is owned elsewhere. `battle_mode == 1` alone is not the exclusion predicate.

Unity's current battle bootstrap (`SimulationTickDriver.cs`, around lines 1623–1654) stores `Match.BattleGameModeId` (default 1), loads an optional `MatchConfig.stageCampaignFilePath`, and calls `ConfigureStageCampaigns(stageSeriesId, -1)`. `SimulationStageWaveModule.ConfigureStageCampaignValues` sets `StageProgressionValid` when the selected campaign has phases. `BattleRuntimeState` has `StageCampaigns`, `StageProgression` and `StageProgressionValid`, but no observed runtime fields corresponding to the playable host's paired story mission/child selection. The existing result producer does not gate on either story selection or stage progression. `BattleGameModeId == 1` is not a sound gate: it is also the default for a direct ordinary battle. `StageProgressionValid` is a candidate Unity stage-session signal, but it is derived from a loaded campaign and series, not from the formal host's two selected IDs; equivalence is unproven.

Consequences for the native carrier implementation:

1. Do not classify a configured story child through the ordinary group-five-excluding result timer. Do not suppress ordinary direct battles solely because they use mode 1.
2. Before production wiring, add paired story/direct fixture evidence through the existing bootstrap and full tick. Resolve which Unity field or exact new session flag expresses the selected story child, including reset and snapshot/checksum ownership if it affects deterministic behavior.
3. Keep the precombat versus postcombat first difference and the separate native result carrier contract in `CARRIER-CONTRACT.md`; this audit does not invalidate the already measured group/timer RED cases or authorize code/schema changes.

No production script, Scene, resource or formal source was modified by this audit. No Unity test was run for this selector in this audit, so the Unity-to-host selection mapping remains a required prechange gate.
