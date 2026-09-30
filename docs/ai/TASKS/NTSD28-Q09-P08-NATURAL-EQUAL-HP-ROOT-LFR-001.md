# NTSD28-Q09-P08-NATURAL-EQUAL-HP-ROOT-LFR-001

Status: `VERIFIED_SCOPED_ROOT_TRACE_ENTRY / P-08_OPEN`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / P-08`. [限定验收](../../../artifacts/diagnostics/NTSD28-Q09-P08-NATURAL-EQUAL-HP-ROOT-LFR-001/ACCEPTANCE.md)。

Authority: root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, its declared playable `GameSession28`, `GameSessionLfr28`, `RenderSnapshotBuilder28` and current formal runtime DAT. The [entry audit](../../../artifacts/diagnostics/NTSD28-Q09-P08-ROOT-LFR-ENTRY-AUDIT-20260929/REPORT.md) proves that the prior current-HP180/base-HP500 case cannot be replayed with matching initial HP through LFR.

Scope: add one Tools-only diagnostic that starts Ita with equal current/base HP and Naruto at normal action0, supplies bounded ordinary Attack input, and records complete paired Session ticks. First find a naturally reachable surviving low-HP standing bpoint/bleed command without writing HP or frame after initialization. If found, produce an LFR from that same Session and replay it with the unchanged root EXE. Compare only fields actually present in the root trace and report, and retain any failed input attempts. Do not claim root GPU pixels from an LFR report; the existing paired WARP HP180 case stays independent.

Declared code path: `Tools/NTSD28Q09Diagnostics/ita_equal_hp_root_lfr_probe.cpp` only. Outputs must use a new artifact directory and refuse overwriting existing case files. Do not edit authority source/EXE/resources, Unity production, DAT values, images, Scenes, project Assets or nonbattle code. Source and root fields must be compared only at matched tick/initial state. If HP selection cannot naturally reach an authored mark, report a bounded negative and leave P-08 open instead of forcing an action or editing DAT.

Acceptance: new diagnostic compiles against the paired playable closure; selected paired complete tick trace demonstrates actual damage, survivor, standing frame and mark, with equal initial HP/baseHP; LFR recorder exits successfully; root headless playback exits0 and reports `passed:true/failureCode:0`; selected trace fields match without false full-state or pixel claims. Check root identity, protected Unity file hashes, ChangeLedger validator and `git diff --check`. No Unity test matrix is needed for this Tools-only formal subgate.

Rollback: preserve all existing dirty work and prior evidence. Any removal of this new script or generated diagnostics needs the repository's explicit deletion approval.
