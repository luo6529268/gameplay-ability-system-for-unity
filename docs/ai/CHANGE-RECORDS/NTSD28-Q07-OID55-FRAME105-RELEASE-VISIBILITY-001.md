<!-- CHANGE-RECORD
id: NTSD28-Q07-OID55-FRAME105-RELEASE-VISIBILITY-001
status: VERIFIED
change-kind: DIAGNOSTIC_TOOL_ONLY
code-path: Tools/NTSD28Q07Diagnostics/pup_frame105_lfr_probe.cpp
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable source
evidence: formal source bridge and root EXE 12-tick playback 36/36 compared fields; pixel pending
-->

# NTSD28-Q07-OID55-FRAME105-RELEASE-VISIBILITY-001

Before code edit: the only observed fact is formal-equivalent DAT/source-image geometry plus frame104 `next:105`; dynamic reachability and formal release sprite visibility remain unknown. Add the Task-declared standalone C++ diagnostic using existing GameSession28/GameSessionLfr28 APIs, not production source modifications. It must refuse overwrite, read authority files only, and keep all outputs in the project's diagnostic artifact directory. Validate compile, bounded source run, root EXE replay, hashes and diff/ledger. No DAT, image, Unity script, Scene, nonbattle route or player configuration change. The direct action103 initial state is a controlled fixture, not natural selection. Rollback and exit are in the Task Contract.

Implementation/verification 2026-09-25: added only the declared C++ tool. Paired source build compiled 0 errors with empty log. Initial wrong root invocation failed before creating outputs; corrected double-runtime-root invocation created 12-tick CSV/LFR. Root formal EXE replay PASS and before/after EXE SHA stayed fixed. Source and release trace action/effective pic/aggregate sprite count matched 36/36; source snapshot alone showed one actor command at action105 ticks5–7. No GPU pixels or natural input reachability claimed. Formal point/clamp sampler plus sampled fully transparent `pup.png` edge predicts invisible body in this case; the three other edge cases remain candidates. Exact inputs, hashes and limitations: `artifacts/diagnostics/NTSD28-Q07-OID55-FRAME105-RELEASE-VISIBILITY-001/REPORT-20260925.md`. No Unity production, DAT, image, Scene, Prefab or nonbattle edit. Rollback limited to this diagnostic source under protected-worktree rules.

Follow-up process evidence: the direct GUI-subsystem invocation wrote a valid report but returned before process completion, so no exit code was claimed from it. A separate create-new `Start-Process -WindowStyle Hidden -Wait -PassThru` replay observed exit0/PASS and a trace byte-identical to the first; report records both output hashes. This confirms the Task's process-exit gate without changing the LFR or formal root.
