<!-- CHANGE-RECORD
id: NTSD28-Q07-FUSION-FULL-DRIVER-FIRST-DIFF-001
status: FOCUSED_TEST_PASS
change-kind: Q07_D024_FUSION_COMPLETE_DRIVER_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/fusion_full_session_source_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07FusionFullDriverEditorTests.cs
authority: formal root NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 step SimulationTickDriver28 step BattleWorld28 advance_native_fusions; formal data/fusion.dat row0; user D-024
evidence: docs/ai/TASKS/NTSD28-Q07-FUSION-FULL-DRIVER-FIRST-DIFF-001.md
-->

# NTSD28-Q07-FUSION-FULL-DRIVER-FIRST-DIFF-001

Pre-change: `NTSD28-USER-SOURCE-FUSION-DISTANCE-001` has source-gap40/physical-gap50 focused evidence, but directly calls the physics helper and fusion method. It does not prove the complete formal or Unity Driver feeds and consumes the same source coordinates in its actual pass order. Current formal fusion row0 and indexed OID7/OID8/OID51 definitions are present. The shared Unity raw scenario validator has no named fusion fixture and must not be bypassed or disguised as another scenario.

Declared change: the three exact code paths in metadata and a fixed formal scenario/raw evidence directory. Formal and Unity diagnostic scripts only; no production or content change. The diagnostic is allowed to yield a PASS or a first difference. Its output cannot be promoted to root-EXE, natural Play, OID52 row1 or full lifecycle parity.

Expected side effects and invariant: no runtime behavior outside opt-in diagnostics; existing scenario schemas and input/RNG contracts unchanged; protected Menu/Battle Scene and GameConfig/ProjectBattleModeConfig bytes remain stable; test teardown follows ordered shutdown with zero objects/slots/borrowers. Rollback and validation are specified in the Task Contract. Actual files, commands, raw traces, first difference, failures and final status will be appended after execution.

Formal fixture correction: first complete-Session run with both action9 participants facing right merged both X320 and putative negative X331; the partner runs right during the full tick, so that negative control was invalid. A later X347 run rejected at the exact post-physics source gap50. These are retained raw diagnostic results and do not imply a production fusion defect. The Task's corrected candidate uses the partner facing left and X304/X315; its cross-domain physical/source gap remains to be measured.

Actual diagnostic code: the declared C++ probe writes full `GameSession28::step` tick0–3 rows; the existing Unity raw capture helper now admits only the exact `ntsd28-q07-fusion-full-driver/1.0` fixture and seeds the paired native RNG; the new Editor test runs the complete `SimulationTickDriver.StepOneTick` for positive/negative scenarios, writes raw rows and compares source and lifecycle fields. No production, DAT or Scene code was changed in this package.

Formal validation: `formal/compile-04.log` exit 0 using the paired playable build closure; `formal/run-03.log` exit 0; `formal/csv-validation.txt` PASS for both eight-row v3 traces. The positive fuses at tick1 to OID51/action290/HP200/source X303 with slot1 suspended; the negative remains OID7/OID8 at source X335/283 (gap52). Formal root EXE SHA and catalog identities are recorded under `formal/`. Earlier failed fixtures and the first C++ compile error are retained as diagnostic history.

Original Editor: `Assets/Refresh` executed through the already-running project's Unity MCP bridge. A missing test namespace caused the first C# compile error; adding `using NTSD.EditorTools;` repaired it, and the Editor assembly timestamp advanced. The two-case focused EditMode job was started; result and shutdown verification remain pending. This Record is `CODE_WRITTEN`, not a parity claim.

First Unity run (`get_test_job` id `80264dba3d5046b2a62d19c47a5471ad`) executed exactly the two target cases and wrote raw rows under `unity/`; both tests reported `tick0 slot0 gate328`, formal `0` versus Unity default `-1`. Offline comparison of all captured rows showed only three pre-tick fixture defaults differ: `gate328` formal0/Unity-1 and `primary330`/`partner334` formal-1/Unity0; the rest of the captured tick0–3 declared fields agree in both cases. The test now sets these three fields on both initial entities before tick0 capture to pair exact initial state. This is a fixture correction, not a production-rule edit. Original Editor refresh and focused rerun follow.

Final focused evidence: original Editor job `f86e8f6c602842439f1b955aa2d84d26` ran two parameterized complete-Driver cases, 2 passed/0 failed. Both successful Unity raw CSVs have eight rows and zero differences in the 30 declared comparison fields against formal v3. The helper's ordered shutdown postcondition reached zero objects, slots and borrowers in both cases after independent production repair `NTSD28-Q07-FUSION-UNIFIED-AI-SHUTDOWN-001`. File names, initial correction, proof limits and incident history are in `artifacts/diagnostics/NTSD28-Q07-FUSION-FULL-DRIVER-FIRST-DIFF-001/ACCEPTANCE.md`. This closes only the controlled-world row0 integration gate, not Q07/D-024 or natural Play/EXE equivalence.
