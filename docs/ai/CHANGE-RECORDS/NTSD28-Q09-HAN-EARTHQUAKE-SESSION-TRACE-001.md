<!-- CHANGE-RECORD
id: NTSD28-Q09-HAN-EARTHQUAKE-SESSION-TRACE-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/han_earthquake_session_trace.cpp
authority: formal root NTSD2.8-Logan EXE and paired playable GameSession/NativeEarthquake with current formal Han DAT
evidence: docs/ai/TASKS/NTSD28-Q09-HAN-EARTHQUAKE-SESSION-TRACE-001.md
-->

# NTSD28-Q09-HAN-EARTHQUAKE-SESSION-TRACE-001

PLANNED before diagnostic source creation. The Task declares the exact one-file C++ write scope, controlled current-content Session input, output/no-overwrite behavior, build/test acceptance, noninterference and rollback. This diagnostic will not alter formal or Unity production. It can close only the current-content paired Session reachability subgate; root EXE pixels, natural action0 input, Unity background consumer and P-13/Q09 remain open.

2026-09-27 CODE_WRITTEN: added only `Tools/NTSD28Q09Diagnostics/han_earthquake_session_trace.cpp`. It creates a current-formal-content playable GameSession with Han OID726/action149 and opposing Lee, then records 24 complete ticks of action/state, actor X, camera X, earthquake owner and background offsets. It refuses existing output and treats a missing 150(+2,0)→151(0,0) sequence as failure while retaining rows. Compile, run, hashes, validator and Scene check are pending. No production behavior changed.

First g++ compile failed with one diagnostic-only type error: `DatDocument` has no `object_id`. The exact one-line field read was corrected to `EntityState28::object_id` before retry; no formal or Unity source was changed. The first compile log is retained as `compile.log`.

Second compile succeeded (g++ exit 0). First controlled run of action149 exited 8: tick1 actor action0, all 24 rows without frame150 or background offset; original `controlled-han-v1.tsv` and `run-v1.log` retained. This is RED evidence that action149 alone lacks a required live precondition, not evidence that formal earthquake is absent. The Task now allows only same-source tick0 capture and alternative current-DAT action264 entry for a second bounded diagnostic; production and formal content remain untouched.

Third compile succeeded and the two new controlled cases both exited 8 with rows retained. Tick0 verified requested actions 149/state9 and 264/state15. Action149 becomes action0 at tick1; action264 advances 268→269, then remains 269 through tick24 without taking `hit_g:265`. Their missing quake is a precondition finding, not a release-rule contradiction. The Task now permits only a direct initial-frame150 control in the same diagnostic to test current formal DAT->Session->snapshot consumption. This direct control cannot close natural reachability or formal root pixel gates.

Final scoped VERIFIED: fourth g++ compile exited 0; the direct frame150 controlled Session run exited 0, emitted background offset `(2,0)` at completed tick1 and `(0,0)` at tick2 as actor advanced to frame151, owner slot0 both ticks. Original 149/264 failures remain archived. Only the declared diagnostic source was written; final source SHA `2E40E8E71263D358BAA1F506F8EF432C22D56C0856F6D64C71BDEC206EEC3FA3`, v4 EXE SHA `9D788AD57EEB2CB42B70AA13DBD619D2E12070C3BFFBB71BADC1BFFB76388AB9`, successful TSV SHA `E1EEAD83FC6B50F6674271286DCDED03154FDC5EF1801288BE23854C2F1C3EBB`. Formal root EXE identity rehashed but no root GUI/LFR pixel run; original Editor compile/Play not run; Scene SHA unchanged. Natural entry and Unity background consumer/P-13/Q09 remain pending. Full method and limits in package ACCEPTANCE.md. Change Ledger and diff checks are next.

Final governance check: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exited 0, `Change ledger validation PASSED` with 935 records and 41 governed code files in the current diff; full output is `ledger-validation.log`. `git diff --check` exited 0 with only existing LF/CRLF notices. Original Menu/Battle Scene SHA values were rechecked unchanged. No Unity compilation or Play was claimed for this source-only package.

2026-09-27 controlled-landing continuation declared before same-source edit: `physics_integrator.cpp` requires strict previous-Y floor crossing for type0 `hit_g`; the failed 264 fixture began at Y0 and therefore did not test that branch. The Task permits one `264-air` variant at Y=-20, plus Y/motion rows, without changing original source outputs or production. The previous scoped direct-frame VERIFIED result remains valid; whether this stronger branch passes is pending.

Final continuation: g++ v5 exited 0. The `264-air` controlled Session exited 8 after tick4 landed and published action266/state55050/Y0/background `(0,0)`; no sampled 265/55052 or `(2,0)` occurred. The exact internal selection of 265 before post-step 266 is inferred from the formal landing and wait/next code, not directly observed. Failure TSV SHA `86FE5D33550006BF510DF6256B75A7BCD41E4205B7CAE4C8A84341F383A97AC8` retained. Final source SHA `FACF01659180F757E7A086B8BE6E0B6BC744FF5C33C7A71AE67E402259E15390`, v5 EXE SHA `4DEFD4FF93E93411E280E4DDB7306EE99989644E3515E99F9E2210F2F2201A1A`. Status returns to VERIFIED **only for the direct-frame current-content consumer**; the natural 149/CPoint route, root EXE pixels and Unity presentation remain open. Final validator/diff/Scene recheck pending.

Final recheck after v5 and handoff integration: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exited 0 (`Change ledger validation PASSED`, 935 records, 1 governed code file in current diff, this diagnostic source COVERED). `git diff --check` exited 0; Git emitted only LF/CRLF conversion notices. Menu/Battle Scene SHA-256 remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. Source, v5 binary and 264-air TSV were rehashed to the values above. No original Unity Editor compile, SelfCheck or Play was run in this Tools-only continuation.
