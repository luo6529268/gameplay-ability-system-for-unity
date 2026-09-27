<!-- CHANGE-RECORD
id: NTSD28-Q07-HELD-WEAPON-DUAL-DOMAIN-FULL-TICK-001
status: FOCUSED_TEST_PASS
change-kind: Q07_D024_HELD_TYPE1_WEAPON_COMPLETE_TICK_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/naruto_held_weapon_motion_source_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HeldWeaponDualDomainEditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; paired playable GameSession28/SimulationTickDriver28/BattleWorld28 held WPoint; user D-024
evidence: artifacts/diagnostics/NTSD28-Q07-HELD-WEAPON-DUAL-DOMAIN-FULL-TICK-001/ACCEPTANCE.md
-->

# NTSD28-Q07-HELD-WEAPON-DUAL-DOMAIN-FULL-TICK-001

Pre-change state: character, OID440 projectile and OID204 non-sound effect have configured-view complete-Driver movement representatives. Type1 held weapon has only a direct WPoint two-view component witness. The existing native Naruto pickup and original Unity Battle Play are reachable but have distinct initial stage/slots/phase, so no same-world moving weapon trace exists.

Planned after-state: add a source diagnostic with fixed natural pickup, rightward X movement on ticks8–14 and downward Z movement on ticks15–21, a strict replay schema and one original-Editor complete-Driver test. Verify rule-space identity and ratio-scaled physical movement without an OID-specific production branch. Capture all rows and the first difference. This package does not authorize a production fix, asset deletion or a broader behavior change.

Affected symbols: new C++ `run_case`/entry; `NTSD28UnityRawCaptureEditor.ValidateScenario`, `ConfigureWorldAndRoster` native RNG selection and existing `WithLoganScenarioForReplayTests`; new Editor test method. Script-side changes must stay in these paths. No DAT value, Scene, camera, menu, UI, mode, object-pool or production battle code is in scope. Side effects are diagnostic files and test-only setup/teardown; protect the existing Scene/config hashes and old assets.

Validation: source probe compile and 24-tick output, original-Editor focused one-test compile/run, adjacent scenario validation, source/view comparison, protected hashes, diff check and Ledger validator. A passing test is scoped to this OID2/OID120 representative; natural Player/GPU and Q07 aggregate remain pending. If a first difference appears, leave it recorded and create a separate exact production Change Record before changing a writer. Rollback only the new diagnostic package and narrow test harness additions after inspection; never restore or clean unrelated work.

Code written: the new C++ diagnostic runs the original `GameSession28` path and records 24 native rows. The strict new scenario schema was added to the existing Editor replay validator and native RNG selector; the new Editor test creates formal OID120 in slot50, runs complete Driver ticks and writes rows before assertions. A matching scenario JSON and test `.meta` were added. No production script, DAT data, Scene or nonbattle path was edited. The first C++ build passed for X-only movement; expanding the fixed schedule to X and Z exposed the native enum spelling `down`/`depth_down` at compile time, corrected within the same diagnostic. Final C++ build includes all 28 core translation units and two playable translation units, exit 0; `native-24-v2.csv` has natural pickup tick2, 9 moving held-X ticks and 7 moving held-Z ticks. Original Unity Editor refresh/compile and focused test are pending.

Final scoped result: original Editor refreshed and compiled both new test and strict scenario. The initial test's unlinked tick1 `0`/`-1` sentinel comparison failed and was corrected only in the active-relation assertion; the failure and one-row raw capture remain. Exact rerun job `9a5f3b154ea64cd88246e3a1697242b7` passed 1/1 with 24 source/Unity rows, actor/view X/Z residual maxima `7.105427357601e-14`/`7.105427357601e-15`, and 23 held rows preserving raw local WPoint X/Z offsets. One previously passing Lee scenario guarded the modified strict harness, job `c5e067c6db0a4d62bdc8f75a67e55faf`, 1/1. Protected four hashes and unique `.meta` GUID are in `ACCEPTANCE.md`. This is a source/Editor complete-Driver representative, not root EXE same-world or all-entity Q07 closure.

Final verification: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <this repository>` exit 0 (`Change ledger validation PASSED`, 901 records and 11 governed code files in the current diff); `git diff --check` exit 0. The original Editor DLL is newer than both edited Editor scripts; recent Editor log check found zero C# compiler errors. The two completed exact test summaries are archived as `unity-test-jobs-summary.json`. No broad test suite or new Player/Play run was requested for this diagnostic-only package.
