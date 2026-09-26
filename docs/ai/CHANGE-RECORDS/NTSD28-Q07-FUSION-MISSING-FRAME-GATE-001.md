<!-- CHANGE-RECORD
id: NTSD28-Q07-FUSION-MISSING-FRAME-GATE-001
status: FOCUSED_TEST_PASS
change-kind: Q07_FUSION_DECLARED_FRAME_GATE
code-path: Tools/NTSD28Q07Diagnostics/fusion_missing_frame_source_probe.cpp
code-path: Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07FusionFullDriverEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable BattleWorld28 advance_native_fusions; formal fusion row1 and indexed OID52 DAT
evidence: artifacts/diagnostics/NTSD28-Q07-FUSION-MISSING-FRAME-GATE-001/ACCEPTANCE.md
-->

# NTSD28-Q07-FUSION-MISSING-FRAME-GATE-001

Pre-change hypothesis: formal fusion row1 names OID52/action310, absent from indexed OID52 DAT; it initially appeared that formal live path would reject publication while Unity accepted any action below999. The paired formal source probe disproved this: `DatDocument::frame(310)` returns a native zero-initialized frame for an undeclared ID below999. Run1 exit3 preserves the wrong diagnostic expectation; corrected run2 exit0 observed OID10+11→52/action310 at tick1 with `fused=1/unresolved=0`, retained through tick3. No Unity production edit is justified by the original hypothesis.

Revised diagnostic behavior: preserve the formal zero-frame projection and compare row1's actual complete-Driver merge in the original Editor. If it agrees, close only this row1 tick0–3 gate without production changes or unrelated reruns. If it differs, preserve first-difference evidence and declare any additional production path before editing. No per-OID branch, DAT modification or nonbattle behavior change.

First original Editor test job `087c61b3e2894e888b9102cde136742e` failed before the Driver at the raw helper's strict row0-only fixture guard, `Q07 fusion participants differ from the formal row0 fixture.` This is a diagnostic schema gate, not observed production behavior. The next edit only admits the exact new OID10/11 row1 scenario alongside the existing row0 positive/negative scenarios; it must not relax unrelated identity fields.

Actual edits: the new paired playable C++ source probe records row1's complete Session and CSV. The existing original-Editor fusion test file adds the exact row1 complete-Driver test. The existing raw-capture helper admits only the exact OID10/11 row1 fixture in addition to row0; its other identity checks remain. The declared production module was inspected but not edited. DAT values and protected assets were not edited.

Validation: corrected formal runs 2/3 exit0; original Editor row1 1/1 and neighboring row0 positive/negative 2/2 pass. Eight keyed rows, 177 populated comparisons across 30 declared fields have zero formal/Unity difference (`ACCEPTANCE.md`, `COMPARISON.json`). `pwsh` Change Ledger validator and `git diff --check` pass. Four protected asset hashes are unchanged. This record is `FOCUSED_TEST_PASS` for controlled tick0–3 row1 parity, not Q07 closure. Natural activation, row1 split after its timer, and root EXE same-world evidence remain unverified. Rollback is restricted to this record's declared diagnostic and test hunks; existing dirty work is preserved.
