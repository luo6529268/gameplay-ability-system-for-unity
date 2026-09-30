<!-- CHANGE-RECORD
id: NTSD28-Q09-P08-ROOT-LFR-PAIRED-WARP-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/ita_equal_hp_lfr_playback_warp_probe.cpp
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSessionLfrPlayback28 D3D11Renderer28
evidence: docs/ai/TASKS/NTSD28-Q09-P08-ROOT-LFR-PAIRED-WARP-001.md; artifacts/diagnostics/NTSD28-Q09-P08-NATURAL-EQUAL-HP-ROOT-LFR-001/ACCEPTANCE.md
-->

# NTSD28-Q09-P08-ROOT-LFR-PAIRED-WARP-001

Pre-edit state: the equal-current/base-HP natural mark reaches tick22 in paired complete Session and root EXE LFR playback matches 132 selected state fields, but the root headless trace has no mark or GPU pixels. Earlier paired WARP used a different HP180/base500 initial condition and cannot be merged with this LFR case.

Planned file/symbol: only add `Tools/NTSD28Q09Diagnostics/ita_equal_hp_lfr_playback_warp_probe.cpp::wmain`. It reads the prior LFR, applies only trace-proven initial action/facing/MP overrides, advances complete playback, selects the natural mark and renders paired mark-on/off PNGs. Side effects are new diagnostic files only; there is no DAT, Unity, scene, project mode or nonbattle change. A missing mark is a retained failed measurement, not permission to force HP/frame or broaden scope.

Acceptance, protection and rollback: see the Task. After code, record compile/run/pixel/validator evidence and limitations. A paired WARP pass will not be called root-EXE GPU parity or Unity same-viewport parity.

Actual change: added only the declared Tools `wmain` diagnostic. Initial compile failed on C++ vexing parse; corrected compile passed, then first run failed because an iterator read did not set `eofbit`; corrected reader and compile passed. Next run reached tick22 but incorrectly expected LFR final headers after 22 calls; the manager declares row22 as the terminal extra call. Final code captures the tick22 snapshot, performs all 23 calls, verifies the final headers, then renders the retained snapshot. Earlier failures remain in separate logs. Paired playback selected source fields match 88/88 across 22 declared ticks; WARP on/off RGB comparison finds only the exact three red pixels at `(461,595..597)`. The initial RGBA `ImageChops.getbbox` calculation incorrectly reported zero because unchanged alpha masked RGB; corrected independent RGB calculation and the original false-negative artifact are both retained. See [ACCEPTANCE](../../../artifacts/diagnostics/NTSD28-Q09-P08-ROOT-LFR-PAIRED-WARP-001/ACCEPTANCE.md).

Validation: final compile/run exits0, final LFR headers PASS, independent pixel attribution PASS, protected root EXE and four Unity file SHA values unchanged, `Tools/Validate-ChangeLedger.ps1` exit0/PASSED/COVERED, `git -c core.safecrlf=false diff --check` exit0 and new-file trailing whitespace scan clean. No Unity script or Asset change; no Unity compile, SelfCheck or Play was run for this Tools-only subgate. Formal root EXE headless replay has no GPU pixel field; paired WARP does not close root-executable or Unity same-viewport visual parity, P-08/Q09/BATCH-05 or the total goal.
