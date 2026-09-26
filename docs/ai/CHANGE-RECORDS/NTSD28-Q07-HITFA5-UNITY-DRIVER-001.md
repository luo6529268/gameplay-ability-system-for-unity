<!-- CHANGE-RECORD
id: NTSD28-Q07-HITFA5-UNITY-DRIVER-001
status: VERIFIED
change-kind: Q07_D024_HITFA5_UNITY_COMPLETE_DRIVER_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HitFa5FullDriverEditorTests.cs
authority: formal root NTSD2.8-Logan.exe and paired playable SimulationTickDriver28 step NativeAi28 hit_Fa5, indexed OID219 w/e.dat frame51
evidence: NTSD28-Q07-HITFA5-FULL-SESSION-SOURCE-001 source positive-negative eight-tick CSV; Unity existing direct-frame focused tests only
-->

# NTSD28-Q07-HITFA5-UNITY-DRIVER-001

Pre-change: Existing raw capture helper has a strict two-combatant formal scenario gate but no OID219 hit_Fa5 source-injection scenario. The existing Unity hit_Fa5 tests call frame logic directly and cannot show full Driver placement, motion, lifecycle or source-rule position across complete ticks. The Task Contract above declared a narrowly named scenario branch and a focused Editor test with source positive/negative controls before code edit. Expected first outcome could be PASS or an observed first difference; neither predeclared parity.

Implementation: `NTSD28UnityRawCaptureEditor.ValidateScenario` accepts only the named eight-tick formal OID2/OID2 neutral fixture and selects the paired-source RNG seed path. New `NTSD28Q07HitFa5FullDriverEditorTests` reuses its complete Driver harness, loads staged formal OID219, preplaces a slot20/action51 controller and compares the positive/negative CSV rows after each completed tick. Scenario JSON and test `.meta` were added only in the declared paths.

Validation: original Editor PID11944 imported/compiled the new files with no C# error. Run01's new strict seed predicate rejected both fixtures before Driver; test-only correction followed. Run02 group3 negative completed eight ticks and passed; group1 positive raised a unified snapshot occupancy-epoch hard breach after child birth. Run03 isolated group1 tick1, slot50 born, published epoch stale, postcommit breach count1. Exact jobs and causal boundary are recorded in `artifacts/diagnostics/NTSD28-Q07-HITFA5-UNITY-DRIVER-001/FIRST-DIFF.md`. This Record is verified **as a diagnostic change exposing a parity failure**, not as a behavior fix. Existing hard-breach contract must remain. Production repair, native root-EXE same-world and Play/GPU verification remain separate.

Governance verification: first `Validate-ChangeLedger.ps1` exit1 because this Record incorrectly put two code paths on one metadata line; corrected to two `code-path:` entries, second validator exit0 (`change-ledger-validation-v2.log`, 885 records/20 governed code files). `git -c core.safecrlf=false diff --check` exit0. No unrelated code or protected Scene/config file was modified by this package.
