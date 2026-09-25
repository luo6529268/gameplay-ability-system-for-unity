<!-- CHANGE-RECORD
id: NTSD28-Q07-SASUKE-NEEDLE-RELEASE-PLAYBACK-001
status: VERIFIED
change-kind: DIAGNOSTIC_NATIVE_HARNESS_ONLY
code-path: Tools/NTSD28Q07Diagnostics/sasuke_needle_lfr_probe.cpp
authority: formal NTSD2.8-Logan release EXE and corresponding playable GameSession28/GameSessionLfr28 build closure
evidence: existing Sasuke physical Play action261/frame264/four OID440 source-formula witness but no controlled formal EXE playback
-->

# NTSD28-Q07-SASUKE-NEEDLE-RELEASE-PLAYBACK-001

2026-09-25 scoped result: the declared C++ harness compiled with the formal playable/core source closure (`g++`, 28 core sources plus four playable files, exit 0, empty compile log). It generated one 26-packet LFR and 26 source rows without overwriting prior output. The unchanged formal root EXE (SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` before and after) replayed that LFR with `--lfr-slot0-action 110` and both formal VFS root options: exit 0, `passed:true`, 27 completed ticks including terminal row and 28 trace rows including tick zero. For ticks 1–26, 26 × 5 source/release fields (`action`, `inputLastAction144`, HP, MP, OID440 count) had zero differences. Both reached action261/MP400 at tick6 and first produced four OID440 children at tick15/action264. Prior original-Editor physical L/D/J Play independently reached those milestones but used a different initial world; this is a common-milestone comparison only. The report explicitly says `nativeParityClaim:false`; independent native recording, same-world Unity parity, hit/lifetime, pixel and Q07 exit remain open. An initial unquoted spaced-path invocation failed exit10 without a report; a second invocation lacking `--complete-vfs-root` failed exit45 (`background DAT was not found for id 23`); preserved. Evidence/hashes: `artifacts/diagnostics/NTSD28-Q07-SASUKE-NEEDLE-RELEASE-PLAYBACK-001/REPORT.md`. `VERIFIED` applies only to this diagnostic package.

Final governance checks: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exited 0 (`Change ledger validation PASSED`, 803 Records, 8 current governed code files covered); `git diff --check` exited 0 with only existing LF/CRLF advisory warnings. Original Unity Editor compilation/Play was not run for this C++-only package.

Task: `docs/ai/TASKS/NTSD28-Q07-SASUKE-NEEDLE-RELEASE-PLAYBACK-001.md`. This Record is created before any C++ script edit. Existing Unity original-Editor Sasuke physical L/D/J Play and exact formal DAT/source preflight are scoped evidence, but the corresponding formal EXE input and OID440 sequence has not been observed in a controlled playback.

Declared write scope is the new governed `Tools/NTSD28Q07Diagnostics/sasuke_needle_lfr_probe.cpp` and new artifacts under `artifacts/diagnostics/NTSD28-Q07-SASUKE-NEEDLE-RELEASE-PLAYBACK-001/`; the formal source and executable are read-only. The diagnostic may generate an LFR and source CSV from a two-entity ordinary battle, then replay only through root formal EXE. It must refuse prior output overwrite. It must not write Unity production, Scene, DAT, PNG, menu or nonbattle code. Expected side effects are only new diagnostic files.

Acceptance: source harness compiles using the official playable/core closure; root EXE hash checked before/after; replay report/trace are produced; source/release first difference for explicitly named action, HP/MP and OID440 fields is recorded. Initial action, roster, seed, position, input masks/phase and content identity must be stated before any same-state claim. `nativeParityClaim:false` remains a release-report limitation if source-generated LFR is used. `Tools/Validate-ChangeLedger.ps1` and `git diff --check` must pass before handoff. Rollback is limited to this new file/output with the repository's destructive-change approval rule.
