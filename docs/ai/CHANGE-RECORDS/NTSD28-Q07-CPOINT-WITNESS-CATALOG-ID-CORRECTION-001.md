<!-- CHANGE-RECORD
id: NTSD28-Q07-CPOINT-WITNESS-CATALOG-ID-CORRECTION-001
status: VERIFIED
change-kind: Q07_CPOINT_DIAGNOSTIC_CATALOG_ID_CORRECTION
code-path: Tools/NTSD28AuthorityTrace/cpoint_resource_first_diff_witness.cpp
authority: formal root NTSD2.8-Logan.exe paired catalog.csv rows 218-219 and decoded_dat/data/data.txt lines 269-270
evidence: prior source-world witness used temporary object IDs 36 and 333 and called rea.dat Reaper although formal index declares Hidan OID56/type0
-->

# NTSD28-Q07-CPOINT-WITNESS-CATALOG-ID-CORRECTION-001

Pre-change: the prior real-DAT source World witness correctly loaded `hid.dat` and `rea.dat` and forced type0, but `SpawnRequest.object_id` used 36 for `hid.dat` and 333 for `rea.dat`. The formal catalog declares Hidan OID24/type0 and Hidan conditional OID56/type0. 'Reaper' was an inaccurate descriptive label. The prior output remains historical diagnostic evidence, not a formal same-index state.

Declared script edit: only `cpoint_resource_first_diff_witness.cpp::run_case` and `run_throw_case` object IDs, 36→24 and 333→56. No production/test C# or resource file changes. Expected side effect is only corrected source-world object identity; the five observed resource outputs must be freshly measured. Task Contract records validation, rollback and evidence limit.

Actual script edit: only the declared diagnostic `SpawnRequest.object_id` sites changed: both held entities 36→24, throw catcher 333→56 and caught 36→24. DAT bytes, object type0, frame IDs, resource rules and output keys remain unchanged. Status: `CODE_WRITTEN`; compile/replay and correction addenda pending. Keep Q07 and total goal active.

Correction validation: final paired-source builder exit0, formal root EXE hash `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and unchanged paired source manifest hash; corrected five-case run exit0, `sameOutput=True` by SHA comparison with preserved run1 JSONL. The selected PP/HP/display/environment results are identical. `CORRECTION-REPORT.md` records exact hashes and boundaries. Only diagnostic code/document labels changed; production CPoint code and Unity tests were not rerun. Final Ledger/diff/protected asset checks follow below.

Result: `VERIFIED / DIAGNOSTIC_IDENTITY_ONLY`. The previous label is superseded, not silently deleted; natural formal EXE and Unity Play remain open. Rollback limited to the diagnostic IDs and addenda, preserving run1 and other user work.

Final checks after addenda: `Tools/Validate-ChangeLedger.ps1` PASS, 872 records/five current governed code-diff files covered; `git diff --check` exit0. Menu/Battle Scene and two protected config SHA-256 values remain `785F828C…81E13`, `2EE465D8…B77A`, `0527D737…B8EA7`, `88E10D43…6F55C`. Validator log: `Temp/NTSD28Q07CpointResourceFirstDiff/catalog-correction-ledger-validation.log`.
