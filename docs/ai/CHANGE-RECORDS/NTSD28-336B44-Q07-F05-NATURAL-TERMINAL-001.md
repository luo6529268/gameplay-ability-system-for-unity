<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F05-NATURAL-TERMINAL-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/terminal_counter_natural_probe.cpp
authority: selected336B44 formal playable GameSession step and BattleWorld terminal frame gate
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F05-NATURAL-TERMINAL-001.md
-->

# NTSD28-336B44-Q07-F05-NATURAL-TERMINAL-001

Created before the only declared diagnostic code path is written. Current Unity F05 production fix passed original Editor focused21/21, but no formal root/natural Scene evidence. This child will build a bounded source GameSession/LFR natural low-base-HP candidate, record actual reachability and only conditionally run formal root same-LFR parity. No change to battle implementation, DAT values or protected assets. Success requires a real terminal gate and source/root same-tick comparison; otherwise retain a negative finding without closing F05. Acceptance and review-only rollback in Task. Status PLANNED; validation not run.

Code written in the sole declared `terminal_counter_natural_probe.cpp`: starts formal Tayuya36/action243 vs Naruto2/baseHP18 with unchanged neutral inputs for128 complete `GameSession28::step` ticks, records the two natural entity states/counter/revive fields, frame-event messages, RNG and exact LFR packets. It counts true terminal-held frame events, rather than inferring a gate from HP0. Only diagnostic output is expected; no production/DAT/Scene/config files changed. Native compile and source run pending. Status `CODE_WRITTEN`; no F05 parity or closure claimed.

2026-10-01 scoped native child VERIFIED: g++ selected playable closure compile exit0, source128tick first HP0 tick24; prior counter2 at tick65 and first terminal-held state14/action230/counter0 at tick66, 63 actual held messages total. Selected formal root SHA rechecked and same-LFR playback exit0/passed:true/failureCode0. Offline 4480/4480 comparable entity/RNG fields zero differences; 63 root terminal tick observations equal source message ticks. This child does not close Unity Scene/F05 parent. Report: `artifacts/diagnostics/NTSD28-336B44-Q07-F05-NATURAL-TERMINAL-001/ROOT-REPORT.md`. No production/DAT/Scene/config edit; review-only rollback of the declared native diagnostic file.
