<!-- CHANGE-RECORD
id: NTSD28-Q07-CPOINT-RESOURCE-FIRST-DIFF-001
status: VERIFIED
change-kind: Q07_CPOINT_RESOURCE_DIAGNOSTIC
code-path: Tools/NTSD28AuthorityTrace/cpoint_resource_first_diff_witness.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryAccountingCoverProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointThrowAtomicProductionEditorTests.cs
authority: formal root NTSD2.8-Logan.exe paired playable battle_world.cpp CPoint resource transfer and real hid.dat frame239
evidence: source call-chain candidate only; same-state first difference is not yet measured
-->

# NTSD28-Q07-CPOINT-RESOURCE-FIRST-DIFF-001

Pre-change: `BattleCpointWriter.ApplyHeldInjury` and `ApplyThrowInjuryDisplayLead` resolve the resource attacker and write display steps but do not call the existing native PP transaction. Formal paired `battle_world.cpp` calls `apply_native_hit_resource_transfer` in both branches before damage/environment updates. This static gap is a candidate, not yet a measured formal EXE/Unity first difference.

Declared paths and symbols: new `cpoint_resource_first_diff_witness.cpp::wmain` for a source-world controlled observation; `NTSD28B6HeldInjuryAccountingCoverProductionEditorTests` and `NTSD28B6CpointThrowAtomicProductionEditorTests` for narrow PP positive/control assertions. No other script path or production edit is authorized by this Record. The native witness reads formal DAT bytes and uses the paired source as compiled input; it never writes to the formal source or runtime.

Expected effects: diagnostic output and test cases only, no gameplay or resource change. Risks: direct source-world state can differ from formal host configuration, and synthetic Unity fixtures can differ from the natural scene. The Task Contract defines evidence and rollback. Report those bounds explicitly; no full Q07 closure from this package.

Actual paths: one new `cpoint_resource_first_diff_witness.cpp` and focused PP positive/local-gate/counter-control tests in the two declared C# files. No production or formal-resource file changed. The source build runner was extended from held to held plus real Reaper throw; the first build/read-only held result and final build-v2 results are preserved separately.

Validation: `Build-AuthoritySourceCapture.ps1` final build-v2 exit0, formal EXE and 28-core/host source hashes validated; five real-DAT source World cases exit0. Original Editor compile requested; Console error count 0. First current test job `ad3500c1a2c8496ebb6728c0f86f4759` 3/5 pass and 2 positive expected failures (Unity PP100 versus expected130/122). After matching throw operands to real source injury30/attacking0, job `82a1b42c6b824a7db6dce675b58095d3` again 3/5 pass, held/throw positive expected122 but Unity PP100. This failure is the finding; tests intentionally remain RED until the separately governed production fix. Full initial frame/world state and formal EXE natural catch were not matched. See `FIRST-DIFF-REPORT.md` and `SOURCE-WITNESS-RUN1.jsonl`.

Result: `VERIFIED / DIAGNOSTIC_ONLY`; the separate production ID `NTSD28-Q07-CPOINT-RESOURCE-TRANSACTION-001` resolved the scoped PP gap. Rollback is limited to the declared witness and test additions; existing dirty files remain protected. `Tools/Validate-ChangeLedger.ps1` exited0 (871 records, four governed diff files covered), `git diff --check` exited0, protected Scene/config hashes remained their baselines and three recovered v3 progress documents retained zero NUL bytes. Natural formal EXE/Play remains outside this diagnostic completion.
