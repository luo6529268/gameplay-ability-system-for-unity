# CLIENT-CPP-RESULTS-ACTIVATION-RESET-ALIGNMENT-001

<!-- CHANGE-RECORD
id: CLIENT-CPP-RESULTS-ACTIVATION-RESET-ALIGNMENT-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleResultsActivationResetAlignmentEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Server same-ID Task/Change; C++ release-live phase-11 activation/reset order; user standing governed Client authorization.
evidence: FOCUSED_TEST_PASS / RESULTS_ACTIVATION_RESET_ALIGNMENT_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / USER_STANDING_AUTHORIZED / UNITY_COMPILE_0 / FOCUSED_2_2 / RELATED_94_94 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> 事前状态：`IN_PROGRESS / PRE_CODE`。先写focused red；本包只增加既有table/live-guard reset调用，不改scan、reserve、schema或host action。

> Test-first red：Unity job `a23c8c6aecab4b59b246da11bda88704`为`0/2`；table case观察到`ResultSubcursor=5`而非`2`，guard case观察到`HadBoth=true`而非`false`。失败只命中声明的activation reset边界，无无关编译/测试失败。

> 实现与验证：`ActivateSummary(...)`只新增`ResetResultTableState();`后接`ResetLiveGuard();`；Unity compile0；focused job `2ab9cf74ef3a47df9e73f12316485d10`为`2/2`；related job `134adcc8aead4f38834b3d321e02871b`为`94/94`；fresh SelfCheck于23:14:19写入`PASS`；Server Debug/Release和四套回归全通过。Scan/reserve/schema/host action/package/marker未改，S0/S5仍`NOT_VERIFIED`。
