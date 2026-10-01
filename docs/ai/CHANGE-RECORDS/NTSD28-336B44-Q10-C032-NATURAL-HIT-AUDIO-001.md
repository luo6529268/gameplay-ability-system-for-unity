<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-C032-NATURAL-HIT-AUDIO-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/environment_tail_natural_marker_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07F03NaturalMarkerBattlePlayProbeEditor.cs
authority: selected336B44 formal playable GameSession28 step audio events and BattleWorld28 state12 strict penetration channel6
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-C032-NATURAL-HIT-AUDIO-001.md
-->

# NTSD28-336B44-Q10-C032-NATURAL-HIT-AUDIO-001

2026-10-01 `VERIFIED` scoped source/Unity audio: g++ compiled declared native diagnostic with the prior 336B44 playable source argv, exit0; `source-natural-audio.exe` ran formal runtime root with Tayuya36/action243 X500, Naruto2 X550, mode0/BG1/seed682973786, neutral128 ticks, exit0. It emitted six ordered audio events, builtin channel6 at ticks60/66 only, X373/360. The new source entities/RNG/relations/frames/LFR SHA exactly match the prior F03 source packet that the formal root replay passed. Original Editor Assets/Refresh completed, the edited Editor assembly is newer than script, zero compile errors observed; unique Scene `-03` completed128 production ticks `PASS/DONE`, six pending sounds, normal Play exit/clean. Offline `source-unity-audio-comparison-v1.json` shows 6/6 events and30/30 tick/order/cue/X/global-tick fields equal, F03 v2 entity/RNG samples identical. Editor idle/nonPlay; four protected SHA match. Validator and diff check to record below. No production/DAT/WAV/Scene/config change. Formal root LFR JSON omits audio events, so root sound output/device/pan/volume remain unverified; C032/Q10 parent open. [验收](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C032-NATURAL-HIT-AUDIO-001/ACCEPTANCE.md).

Created before either diagnostic script edit. Before: F03 natural Tayuya/Naruto source/root and original Scene128-tick entity/RNG results match, including C031 strict penetration at ticks60/66, but diagnostic outputs omit per-tick audio. After: source diagnostic records `last_tick()->audio_events` and guarded Editor probe records production `PendingSounds` after each complete tick under a unique `-03` run ID. Expected side effects are a new native output directory and one new original-Scene JSON; saved Scene, DAT, WAV, config and production runtime remain unchanged. Acceptance and honest root/device limits are in the Task. Rollback is review of only these diagnostic hunks, not blanket Git commands. Status `PLANNED`; no audio parity claimed yet.

Actual code written in the two declared diagnostic scripts: native emits `source-audio.csv` with tick, event order, enum source, native channel, world X and resource path; original Editor probe accepts only the unique `-03` run and captures ordered per-tick `PendingSoundEvent` cue/X/tick after successful production `StepOneTick`. Existing F03 outputs are not touched. Compilation, native run, Unity Play, comparison and protected hashes pending; status `CODE_WRITTEN`, no parity claim.

Final governance check after evidence/doc update: `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` exit0/PASSED, 1070 Records and8 governed code files; both edited diagnostic files are covered by this ID plus their original F03 IDs. `git diff --check` exit0, only pre-existing CRLF advisory warnings. Full unrelated test suite and audio hardware playback were not run. Review-only rollback of the two diagnostic hunks remains available; preserve accepted F03 outputs and all user content.
