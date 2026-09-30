<!-- CHANGE-RECORD
id: NTSD28-Q09-P08-NATURAL-EQUAL-HP-ROOT-LFR-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/ita_equal_hp_root_lfr_probe.cpp
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 GameSessionLfr28
evidence: docs/ai/TASKS/NTSD28-Q09-P08-NATURAL-EQUAL-HP-ROOT-LFR-001.md; artifacts/diagnostics/NTSD28-Q09-P08-ROOT-LFR-ENTRY-AUDIT-20260929/REPORT.md
-->

# NTSD28-Q09-P08-NATURAL-EQUAL-HP-ROOT-LFR-001

Pre-edit state: prior P-08 paired natural-hit/WARP case starts Ita at current HP180/base HP500. Formal LFR records only base HP and reinitializes current HP to that value, so the existing case cannot prove root same-initial-state playback. The original Unity Battle natural mark and three-pixel paired WARP cases are already scoped and will not be rerun.

Planned actual file/symbol: add only `Tools/NTSD28Q09Diagnostics/ita_equal_hp_root_lfr_probe.cpp::main` for a bounded paired-source equal-current/base-HP natural input, tick trace and LFR. Possible side effects are only new diagnostic file and uniquely named outputs. It must not write authority or project runtime/asset files; no DAT or nonbattle change. Record false or failed probes rather than altering expected outputs. Acceptance and rollback are in the Task. Compilation, root playback, field comparison and protection checks are pending.

Actual change: added only the declared Tools diagnostic `main` and source tick CSV/LFR outputs. Built with the paired playable core and LFR sources; first link failed because the reused WARP argv lacked `-lz` and retained `-municode`, and a second PowerShell invocation did not start the compiler because it treated `-std=c++17` as the executable. Both attempts are preserved in the artifact account. Corrected compile exited0 with empty `compile-04.log`. Paired Session HP30/base30 naturally reached surviving HP10 on tick8 and exactly one standing bleed mark on tick22; independent second run yielded the same LFR SHA. Formal root unchanged EXE replay exited0 with `passed=true/failureCode=0`, declared22 ticks; selected actor/target action, X, HP/baseHP fields matched132/132, 0 differences. CRT state differed at tick1, and root trace does not export bleed command pixels; no full-state or GPU parity claim. Details and hashes: [ACCEPTANCE](../../../artifacts/diagnostics/NTSD28-Q09-P08-NATURAL-EQUAL-HP-ROOT-LFR-001/ACCEPTANCE.md).

Validation: no Unity script/Asset/Scene change; Unity compile, SelfCheck and Play were not run for this Tools-only subgate. Root/paired trace and focused machine comparison passed within the stated fields. Formal EXE and four protected Unity file SHA values remained stable. Ledger validator exit0/PASSED/COVERED, scoped `git diff --check` exit0 and new-file whitespace check PASS; logs and limits are in the acceptance. Remaining P-08: formal root marker/GPU visibility and Unity same-state/same-viewport comparison; Q09/BATCH-05 and the total goal remain open. Rollback remains user-approved removal of only this new script/artifact, with no existing dirty work touched.
