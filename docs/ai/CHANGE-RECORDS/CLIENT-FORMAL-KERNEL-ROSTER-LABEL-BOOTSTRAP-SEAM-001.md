# CLIENT-FORMAL-KERNEL-ROSTER-LABEL-BOOTSTRAP-SEAM-001 — Roster / Label Bootstrap Seam

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-ROSTER-LABEL-BOOTSTRAP-SEAM-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRosterLabelState.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleMatchConfigRuntimeAdapter.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRosterLabelBootstrapSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldScalarStateSeamEditorTests.cs
authority: User standing Client authorization; Server Cut F boundary Task/Change; C++ release live bootstrap/slot/label evidence.
evidence: USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / ROSTER_LABEL_BOOTSTRAP_SEAM_READY / GOVERNANCE_CLOSED / SOURCE_SEAM_ONLY / UNITY_COMPILE_0 / FOCUSED_5_5 / RELATED_87_87 / SELFCHECK_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> 状态：`FOCUSED_TEST_PASS / ROSTER_LABEL_BOOTSTRAP_SEAM_READY / GOVERNANCE_CLOSED / USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / SOURCE_SEAM_ONLY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## 1. 改前事实与计划

两个value owner的storage/reset只依赖BCL，但配置方法直接依赖`NTSD.App`。按Server Task先加focused red，再拆state/adapter；Results/root/package/marker不动。

## 2. 实际结果

两个state定义已进入BCL-only Client source，配置/normalizer进入Client adapter；instance call syntax不变，AppManager只替换两处static owner。test-first4/1 expected red、compile0、focused5/5、pair10/10、related87/87和fresh SelfCheck PASS。Results/root/package/checksum/snapshot/restore/marker未改；S0/S5未验证。
