<!-- CHANGE-RECORD
id: NTSD28-R06-NATURAL-REVIVAL-ENTRY-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/natural_revival_lfr_probe.cpp
authority: root NTSD2.8-Logan.exe SHA-256 B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 step, GameSessionLfr28, BattleWorld28 advance_native_revivals; alignment R06
evidence: docs/ai/TASKS/NTSD28-R06-NATURAL-REVIVAL-ENTRY-001.md
-->

# NTSD28-R06-NATURAL-REVIVAL-ENTRY-001

Before edit: controlled Lee revival complete-Driver parity is 29-field zero-diff but injects an already dead state. The formally proven Sasuke→OID87 selected-armor HP3 fixture naturally kills at tick16; its target has only one life and therefore cannot witness the ordinary lives2 revival gate. Source `advance_native_revivals` requires HP<=0, state14, render phase1..4, lives>=2 and a known floor; this task measures whether the existing natural sequence reaches those preconditions without altering production.

Declared change: add only the new C++ diagnostic file named in the header. Preserve the existing HP3 probe, all prior raw data, Unity scripts, formal authority source, DAT, Scenes, mode Asset and nonbattle behavior. Emit a new non-overwriting LFR plus target lifecycle CSV and revival events, then compile/replay with exact identities. No Unity code change is authorized by this Record. Bounded run, failure handling and rollback are in the Task.

Validation pending: paired-source compile, root-EXE LFR replay, exact first revival or negative reachability result, `Tools/Validate-ChangeLedger.ps1`, `git diff --check` and protected file checks. Do not claim R06 or Q07 completion from this formal-only first gate.

Actual code: the one new `Tools/NTSD28Q07Diagnostics/natural_revival_lfr_probe.cpp` builds a separate LFR/CSV from the proven Sasuke→OID87 HP3 input, with target lives2 and a bounded optional 70/365-tick window. It captures KO, HP/action/state/lives/render phase and revival event for slot1. It refuses to overwrite raw outputs. The first compile arguments accidentally included the previous `revival_peer_full_session_source_probe.cpp`, so link failed on duplicate `wmain`; preserved as `compile-01.log`. Corrected and v2 compilation exited0; source runs at 365 and 70 ticks exited0 with natural death tick16, lying tick27 and revival tick54.

Root replay could not meet the declared same-state positive gate. The root EXE has no LFR life-count override and starts slot1 lives1; root tick54 trace remains HP-73/action231/state14/lives1, whereas paired source has HP3/action212/lives1 after consuming life2. The 365-tick replay exits46 at HP checksum source row150; the 70-tick replay exits46 at final header offset276. These results are retained rather than attributed to production. No Unity test was added or run. Exact raw files, scope and next route are in `artifacts/diagnostics/NTSD28-R06-NATURAL-REVIVAL-ENTRY-001/ACCEPTANCE.md`. `FOCUSED_TEST_PASS` refers only to the paired-source natural-reachability subgate; root same-state/R06/Q07 remain open.

Validation: paired source compilation corrected and v2 exited0; both source diagnostic runs exited0; root EXE hash after remained the authority SHA. `git diff --check` exited0 with only existing line-ending warnings. Windows PowerShell 5.1 validator initially exited1 because Git emitted an autocrlf warning to native stderr; the same `Tools/Validate-ChangeLedger.ps1` under PowerShell 7 exited0, reporting 988 Records, 19 governed code files and explicit coverage of this new C++ file (prior unrelated stale-declaration warnings remain). Its full output is `ledger-validation-pwsh.txt`. Battle/Menu Scene, GameConfig and ProjectBattleModeConfig disk hashes matched the protected baseline and were Git-clean. No Unity compile, NUnit, SelfCheck or Play was run because this task added no Unity code and the root same-state precondition failed.
