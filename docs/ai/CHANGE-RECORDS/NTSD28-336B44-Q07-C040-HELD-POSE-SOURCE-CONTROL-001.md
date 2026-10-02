<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C040-HELD-POSE-SOURCE-CONTROL-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c040_held_pose_source_control.cpp
authority: 336B44 playable BattleWorld28 catch settlement and formal Hinata/Bee DAT frames
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C040-HELD-POSE-SOURCE-CONTROL-001.md
-->

# NTSD28-336B44-Q07-C040-HELD-POSE-SOURCE-CONTROL-001

PLANNED before script edit. The sole code path is an additive Tools C++ diagnostic. It will exercise the current formal playable catch-settlement pass with controlled reciprocal Hinata/Bee relation and two hold timers. The aim is to discriminate victim current-frame center from vaction-frame CPOINT using actual formal DAT, without substituting source inspection or a synthetic formula for a runtime pass result. Existing natural negative probes remain untouched. Expected side effects are new output files only. No formal source, DAT, Unity production/test, Scene, asset, menu or non-battle change. Risk: initialization or relation invariants may make the controlled case invalid; preserve failures and report the bounded result. Rollback is forward correction; no deletion is authorized.

Actual code: added only `Tools/NTSD28Q07Diagnostics/c040_held_pose_source_control.cpp`. It initializes the formal playable `GameSession28`, sets reciprocal relation/hold conditions and invokes the real `BattleWorld28::settle_catch_relations`; it derives expected coordinates from formal DAT and checks returned action, position and pass counters. No runtime or DAT code was edited. First compile failed because the reused argv still contained the previous probe `main`; corrected argv without source or formal changes, then compiled with exit 0. Two process runs both exited 0 and emitted identical CSV SHA-256 `4314A9F693949B9CADFF23B8BA5D1726974A1C83EBF9DABB32452BFF0713C056`. Hold 5 retained action132, hold 0 selected action130, both actual XYZ 529/1/399 matched expected, pass active/synchronized 1/1. See [report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-HELD-POSE-SOURCE-CONTROL-001/REPORT.md). Formal EXE LFR, natural full Driver, Unity focused test and Scene remain unverified. No deletion or rollback occurred.
