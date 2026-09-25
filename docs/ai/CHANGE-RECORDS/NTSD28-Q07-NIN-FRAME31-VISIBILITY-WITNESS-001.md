<!-- CHANGE-RECORD
id: NTSD28-Q07-NIN-FRAME31-VISIBILITY-WITNESS-001
status: VERIFIED
change-kind: DIAGNOSTIC_TOOL_ONLY
code-path: Tools/NTSD28Q07Diagnostics/nin_frame31_lfr_probe.cpp
authority: formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable source
evidence: paired source 12-tick LFR and root EXE exit0/PASS, 36/36 compared fields, actor sprite on ticks1-7; Unity pixels pending
-->

# NTSD28-Q07-NIN-FRAME31-VISIBILITY-WITNESS-001

Before edit: the formal DAT/PNG census proves OID30 frame31's `pic:81` is within cumulative sheet capacity but its 79×79 source rectangle is below the image. The paired D3D11 point/CLAMP sampler suggests a repeated edge row, while Unity's original rect builder would drop the empty rectangle. A generic clamped-cell publisher has since been written and observed for OID32, but no OID30 controlled source/release trace or Unity-specific publication witness exists. No frame31 GUI pixels or natural input route are proven.

Declared write: add only the Task-named standalone source/LFR diagnostic tool, based on existing `pup_frame105_lfr_probe.cpp` APIs, with a fixed OID30/action31 controlled world and create-new outputs. Expected side effect is diagnostic files only. No battle rule, content, Unity runtime, Scene, GameConfig or nonbattle behavior changes. Source tool compile/run and root-EXE replay/field comparison are required; formal GPU and Unity pixels remain pending unless independently measured. Rollback is limited to this diagnostic addition under protected-worktree rules.

Actual edit: added only `Tools/NTSD28Q07Diagnostics/nin_frame31_lfr_probe.cpp`. It initializes OID30/action31, records 12 no-input steps through the paired `GameSession28`/`GameSessionLfr28` entry, writes action/base/effective pic and sprite counts, and refuses existing CSV/LFR outputs. Compile, source run, root-EXE replay, and comparison remain pending at this status.

Validation: paired closure g++ compiled exit0/empty log; source tool exit0 recorded 12 ticks; root formal EXE hidden waited playback exit0/PASS/failureCode0 with unchanged formal EXE SHA. Source/release action/effective pic/aggregate sprite count matched 36/36; the source snapshot has one OID30 sprite at action31/pic81 for ticks1–7. Formal/staged nin.png bytes match, and its clamped bottom row x80–158 is 79/79 opaque white. This is not root-EXE GPU capture, natural-input reachability or original Unity production publication/pixel proof. Exact inputs, hashes and caveats are in `artifacts/diagnostics/NTSD28-Q07-NIN-FRAME31-VISIBILITY-WITNESS-001/REPORT-20260925.md`. Scene disk hashes stayed stable; diff check exited0. Next is original-Editor OID30 catalog/entity pixel witness, then natural route. No DAT, PNG, Unity production, Scene or nonbattle modification.

The subsequent original-Editor `execute_code` read-only catalog attempt failed before code execution with a Mono compiler command-line length error; it supplied no Unity gameplay or publication result. The Editor was explicitly stopped, observed non-Play, and both Scene disk hashes stayed stable. Next observation should use an opt-in Editor probe rather than repeating this transport failure.
