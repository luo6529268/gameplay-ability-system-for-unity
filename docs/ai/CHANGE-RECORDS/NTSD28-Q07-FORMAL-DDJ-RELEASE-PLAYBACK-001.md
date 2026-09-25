<!-- CHANGE-RECORD
id: NTSD28-Q07-FORMAL-DDJ-RELEASE-PLAYBACK-001
status: VERIFIED
change-kind: DIAGNOSTIC_NATIVE_HARNESS_ONLY
code-path: Tools/NTSD28Q07Diagnostics/ddj_lfr_probe.cpp
authority: formal NTSD2.8-Logan release EXE and corresponding playable build-closure GameSession28/GameSessionLfr28/headless playback
evidence: Unity original Editor physical L/S/K formal DDJ witness formal-ddj-play-3.json PASS but release same-packet playback pending
-->

# NTSD28-Q07-FORMAL-DDJ-RELEASE-PLAYBACK-001

Pre-change state: Unity diagnostic result `formal-ddj-play-3.json` shows request271, action272, PP500→150, completed frame495 and OID518 at tick16. Formal playable `GameSession28` accepts per-step physical buttons, `GameSessionLfr28` can encode an ordinary battle, and root release EXE has `--headless-playback-lfr` and trace/report output. No matching formal EXE input run is recorded for this specific DDJ case. A source-generated LFR is not an independent native recording and does not prove native same-phase parity by itself.

Exact scope: create one governed battle-diagnostic source at `Tools/NTSD28Q07Diagnostics/ddj_lfr_probe.cpp`, preserving the identical original compile-input copy under `artifacts/diagnostics/NTSD28-Q07-FORMAL-DDJ-RELEASE-PLAYBACK-001/ddj_lfr_probe.cpp`, plus generated binary/LFR/report/trace in that artifact directory. Both copies must have the same SHA-256; no production or formal-authority files may be edited. Build uses current formal C++ files as read-only inputs, outputs solely to the diagnostic directory. Preserve all worktree changes and prior DDJ results.

The first validator run rejected a non-governed artifact `code-path`; its `NONE` fallback also requires `GOVERNANCE_ONLY`, which would misclassify a C++ diagnostic. The exact same source is therefore additionally registered under governed `Tools/` with no overwrite of the artifact copy. The compiled artifact copy and governed copy are byte-identical; this correction makes the diagnostic auditable without describing it as production battle code.

Expected behavior: produce a controlled Naruto OID2/action110 HP/MP500 ordinary battle with explicit defend/down/jump and release packets, report source step fields and serialize the LFR; formal EXE playback should accept the manager and emit its own result/trace. Compare the observed action/frame/MP/spawn with Unity, recording any first difference. Same seed, roster, stage, positions and input-phase equivalence must be demonstrated separately before claiming same-state parity.

Validation: compile diagnostic harness against the official playable/core closure, run with formal decoded DAT/VFS, verify root EXE SHA before/after, run headless playback with unique output paths, inspect reported pass/fail and tick rows, run `Tools/Validate-ChangeLedger.ps1` and `git diff --check`. Rollback by removing only this new diagnostic harness/output under an explicit repository-approved cleanup; never touch formal content or existing user work.

2026-09-25 result: compiled diagnostic harness with current formal core/playable C++ inputs (`g++ -std=c++17 -O0 -municode`, 0 compile diagnostics), writing only this diagnostic folder. Two constructor-root preflights failed as documented in `REPORT.md`; corrected formal `resources/runtime` root produced 26 input ticks/LFR. Root formal EXE SHA was unchanged. The first release playback PASS exposed LFR's missing initial action (tick0 action0 versus source110); separate `--lfr-slot0-action 110` playback PASS (`failureCode=0`, `nativeParityClaim=false`) restored the declared initial action. `comparison.json` proves 26/26 source-versus-release tick rows equal for five named fields and 22/22 source-versus-Unity overlapping rows equal for four named fields, including tick6 resolved272/frame495/MP150 and tick16 OID518. It does not prove independent native capture or full same-world/visual equivalence; Q07 remains open. All artifact hashes and limits are in `REPORT.md`.

Final governance validation: exact original compile-input source and `Tools/NTSD28Q07Diagnostics/ddj_lfr_probe.cpp` share SHA-256 `D11577882C4690804945053F44804D0D6C510F603189F314BD4F26F7BE285991`; no existing file was overwritten. After setting the governed metadata path, `Tools/Validate-ChangeLedger.ps1` exited 0, reporting 801 Records and 7 current governed code files covered. The two earlier failed validator outputs were retained under `Temp/NTSD28-Q07-FORMAL-DDJ-RELEASE-PLAYBACK-001-ledger*.txt`; they were metadata-path failures, not battle-runtime or compilation failures.
