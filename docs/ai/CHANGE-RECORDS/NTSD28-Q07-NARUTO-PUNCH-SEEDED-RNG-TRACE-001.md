<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-PUNCH-SEEDED-RNG-TRACE-001
status: VERIFIED
change-kind: Q07_NARUTO_PUNCH_PAIRED_SOURCE_SEEDED_RNG_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/naruto_punch_formal_hit_probe.cpp
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable NativeRandom28
evidence: docs/ai/TASKS/NTSD28-Q07-NARUTO-PUNCH-SEEDED-RNG-TRACE-001.md; artifacts/diagnostics/NTSD28-Q07-NARUTO-PUNCH-UNITY-RAW-FIRST-DIFF-001/ACCEPTANCE.md
-->

# NTSD28-Q07-NARUTO-PUNCH-SEEDED-RNG-TRACE-001

Pre-change: original diagnostic source SHA-256 `AFC9B57A7AF3D1036C7DB8CA6BC460C83758FE16044211A114C6960C5C3E1F90`. It outputs 36 ordinary input/target cases with action, HP, phase and KO provenance, but no source RNG state. The original Editor's exact seed682973786 scenario captured per-tick Unity CRT and synchronized streams; root EXE LFR replay uses a different CRT seed because that format restores only synchronized random state. Treat this as an evidence gap, not a proven production mismatch.

Declared code path: extend only `run_case`'s CSV observation fields and its header in the existing Tools diagnostic to include `world->random().state()` and `synchronized_table_hash()` after each completed tick. All game inputs, world transitions, output guard and validation stay intact. Add new run directory and report; do not overwrite run1 or Unity run2. Expected side effect is a wider CSV only. Risk: decimal/hex parsing, integer truncation, per-tick alignment and false authority promotion. Compare exact tick/seed and preserve any first difference. Acceptance and rollback are in the Task.

First build attempt: the changed probe compiled, but the standalone link failed because its manually enumerated command omitted paired playable `src/lfr_recorder.cpp`, which the existing LFR diagnostic already requires. The full linker failure is preserved in `artifacts/diagnostics/NTSD28-Q07-NARUTO-PUNCH-SEEDED-RNG-TRACE-001/build.log`. This is a build invocation dependency, not an observed RNG/production defect. Rebuild with that source in the same declared paired closure and a separate log.

Second build succeeded with the missing existing LFR source, exit0 and empty `build2.log`. First run stopped before any tick because the second `GameSession28` constructor argument was incorrectly set to `<runtime>/vfs`; the paired constructor expects the complete root containing both `decoded_dat` and `vfs`. `source-run1.log` and its partial files are retained; rerun with the exact complete root under a unique `source-run2` output path. No gameplay or diagnostic source correction is implied by this path error.

Post-change: only the existing probe's `run_case` CSV row and matching header gained seven native RNG state fields; source SHA-256 `C219E5C2FE07348DCB579DE42656701580DD393152E93DD4CE92D78D4E7416C6`. Corrected run2 exited 0; all 36 cases still had authored attributed KO, all previous 1080×15 field values were unchanged, and summary/LFR bytes were identical to the prior formal root-EXE witness. Against preserved original-Editor Unity exact-seed run2, first X525/tick2x1 case matched 30×7=210 after-tick CRT/synchronized fields with no first difference; tick8 CRT state2524509468/calls3002 on both sides. This closes only same-seed paired-source/Unity RNG **state** comparison for that fixture; root EXE LFR playback CRT default-seed discrepancy and other scenarios remain unverified. Exact artifacts, limits and earlier invocation failures are in REPORT. No authority source, Unity script, resource, Scene, ProjectSettings or nonbattle change.

Final governance: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repository>` exit0, 853 Records, seven current governed code files covered; `git diff --check` exit0. Menu/Battle Scene disk SHA remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; all three live progress documents were NUL-free. No Unity Editor compile, SelfCheck or Play was run for this standalone C++ output-only diagnostic.
