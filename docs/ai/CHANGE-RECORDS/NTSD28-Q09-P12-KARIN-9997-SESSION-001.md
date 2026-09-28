<!-- CHANGE-RECORD
id: NTSD28-Q09-P12-KARIN-9997-SESSION-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/karin_state9997_session_trace.cpp
authority: formal root NTSD2.8-Logan EXE and paired playable GameSession/RenderSnapshot with indexed Karin OID77/OID314 content
evidence: docs/ai/TASKS/NTSD28-Q09-P12-KARIN-9997-SESSION-001.md
-->

# NTSD28-Q09-P12-KARIN-9997-SESSION-001

Pre-code record. Existing Unity CentralOnly/Legacy body rendering lacks state9997 viewport/owner-relative placement; the current project mode Asset lacks etc-mode, while an existing formal root mode0 trace reports etc-mode1. The declared diagnostic reads current formal content through a full paired playable Session and observes its actual render snapshot. It must not alter formal or Unity production.

Declared code path and symbol: new `Tools/NTSD28Q09Diagnostics/karin_state9997_session_trace.cpp`, `main` only. Expected side effects: new diagnostic EXE/log/TSV artifacts; no product or resource changes. Its output may narrow P-12 and Q08 G-04 next gates, but cannot establish Unity runtime first difference, root pixel parity, natural input reachability or whole-Q completion.

Acceptance, risk and rollback are in the Task Contract. Risks: controlled initial frame may be reset by full Session, mode gate may differ from the earlier root trace because background selection differs, and camera may move. Record all observed values and preserve failure outputs. Rollback scope is only the new diagnostic source and its artifacts, subject to the repository deletion-approval rule; do not revert unrelated dirty work.

Validation pending: paired-source compile, two no-overwrite runs, formula check, file hashes, Change Ledger validator, diff check and Scene hashes. Formal root EXE and original Unity Editor are not run in this task.

First actual validation: paired-source g++ exited 0. First run accidentally passed the VFS subdirectory as the complete root and both cases stopped at initialization with exit4; separate failure logs retained, no TSV created. Correct runtime-root retry for X20/X500 produced no-overwrite TSVs and both exited9: tick0–3 actor417, tick4–5 actor418, no OID314. This is not a product failure verdict. Before same-source extension, the Task now declares an initial action415→417 natural frame-entry control with a 12-tick bound and fresh no-overwrite outputs. Unity/formal production remains untouched.

Second actual validation before this amendment: initial415 near/normal controlled full Session generated OID314/state9997 at tick3 and sprite left/top X20=0/571, X500=461/571 with selected etc-mode1. Both right-facing cases can satisfy either owner-relative or generic clamp, so they are insufficient to identify the branch by output alone. The Task now declares a left-facing input control and a separate physical-child-facing field before the diagnostic code is extended. Prior outputs remain immutable.

Final scoped result: only the declared Tools diagnostic `main` was extended with an explicit source-facing input and physical-child-facing TSV field. Paired-source g++ C++17 compile of the 28-core/playable Session closure exited0. Authentic 415→417 left-facing near and normal cases both exited0; at tick3 the OID314 child has physical facing1 but render-snapshot facing0, selected etc-mode1, owner0 and left/top 0/571 or 461/571. At X500, owner-relative left461 differs from ordinary left-facing formula460. Initial417 no-child and invalid-root first runs remain retained. No-overwrite retry exited3 with identical file hash. Source/EXE/TSV hashes and precise limits are in `artifacts/diagnostics/NTSD28-Q09-P12-KARIN-9997-SESSION-001/REPORT.md`. This verifies the controlled paired-source Session witness only. Root-EXE visible pixels, natural action0 input and Unity same-state runtime are pending; P-12/Q09/BATCH-05 and overall goal stay open. Formal/Unity production, DAT, Scene, Asset and nonbattle paths were not edited. Change Ledger validator and diff check are recorded below after final run.

Final governance: `Tools/Validate-ChangeLedger.ps1` exited0 (`PASSED`, 957 records, 29 governed code files in the pre-existing composite diff); `git diff --check` exited0 with existing LF/CRLF warnings only. `git status --short` for Menu/Battle scenes is empty; their final SHA-256 values are `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` and `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. Original Editor was queried through the local MCP bridge (PID11944/port6403), confirmed idle and non-Play in Battle Scene, then MCP `refresh_unity` force/all/compile-request returned success. The bridge reconnected after domain reload; `get_editor_state` again reported idle, non-Play, not compiling or updating, and `read_console` returned no C# compiler-error entry (one MCP client-handler exit message only). No Unity Q09 focused test or Play was run for this Tools-only package, so the refresh does not upgrade its runtime claim.
