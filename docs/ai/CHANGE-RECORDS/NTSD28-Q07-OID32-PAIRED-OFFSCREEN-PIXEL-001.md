<!-- CHANGE-RECORD
id: NTSD28-Q07-OID32-PAIRED-OFFSCREEN-PIXEL-001
status: VERIFIED
change-kind: DIAGNOSTIC_TOOL_ONLY
code-path: Tools/NTSD28Q07Diagnostics/hunter_frame95_offscreen_probe.cpp
authority: formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable D3D11 source
evidence: paired-source WARP PNGs and pixel-analysis-20260925.json, scoped GPU witness
-->

# NTSD28-Q07-OID32-PAIRED-OFFSCREEN-PIXEL-001

Before code edit: formal root EXE source/release trace and original Unity published-catalog omission are observed, while actual paired-source GPU pixels for OID32/action95 are unknown. Add only the Task-declared standalone C++ diagnostic, link the existing paired renderer source, read formal runtime root without writing, and output two create-new WARP PNGs plus geometry. This cannot alter battle rules or formal EXE identity. Use no computer-use/new Unity project. Validate compile, renderer exit, PNG dimensions and pixel ROI, SHA, Ledger and diff. Do not claim native root-EXE GPU capture, Unity entity pixel parity, or natural action95 reachability. Rollback and risk are in the Task.

After code edit: added only `Tools/NTSD28Q07Diagnostics/hunter_frame95_offscreen_probe.cpp`, a standalone diagnostic with no runtime or asset write path. It creates independent same-state action95/action0 sessions, captures tick0 sprite geometry, invokes the paired playable D3D11 WARP offscreen renderer and writes create-new PNGs. GCC 15.1 compile exit0, empty compile log, diagnostic exit0, 1333×730 PNG readback and exact ROI pixel analysis PASS. Action95 pic64's 79×79 projected region is 6,241 pure white pixels; RGB comparison changes exactly those 6,241 pixels and none outside. Evidence: `artifacts/diagnostics/NTSD28-Q07-OID32-PAIRED-OFFSCREEN-PIXEL-001/REPORT-20260925.md`, PNGs, geometry CSV and JSON. `VERIFIED` closes only this diagnostic GPU witness; direct root-EXE pixels, Unity entity pixels and natural reachability are unverified. Formal source, root EXE, DAT/PNG, Unity production, Scene and Prefab unchanged.

Final checks: root release EXE SHA-256 remains `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; diagnostic source SHA-256 `E3B202E499FA21B92D01A9C590C62ADADE329A97864257166303A75170657DDA`. `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repository>` exited 0 (`831` Records, `27` governed code files in current diff); `git diff --check` exited 0. The broad validator also reported warnings for historical Record paths outside the current diff; no validation errors. No Unity compile/Play was run for this diagnostic-only C++ package.
