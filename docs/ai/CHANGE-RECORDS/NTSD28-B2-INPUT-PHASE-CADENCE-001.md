# NTSD28-B2-INPUT-PHASE-CADENCE-001 — 1tu/2tu human input sampling

<!-- CHANGE-RECORD
id: NTSD28-B2-INPUT-PHASE-CADENCE-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/App/MatchConfig.cs
code-path: Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28InputPhaseCadenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28 input phase and InputRouter28 sample_pending order in current-EXE build closure.
evidence: TEST-FIRST-CS1739-CS1061-CS0117-14 / UNITY-COMPILE-0 / NEW-5-5 / FINAL-RELATED-86-86-JOB-73C995C5553842F881AB8B172736737F / FULL-SELFCHECK-PASS / CONSOLE-0 / TWO-TU-TICK2-EDGE5-TICK3-4-TICK6-1 / ONE-TU-FORCES-ZERO / SNAPSHOT-CHECKSUM-PASS / AI-PROXY-EXCLUDED
-->

> 状态：`FOCUSED_TEST_PASS / PHASE_CADENCE_READY / AI_PROXY_PENDING`

默认2tu的pending/current采样与显式oneTu已落地。focused5/5、related86/86、full SelfCheck、
compile/Console0；AI/proxy/RNG consumer不在本包。
