# Task Contract — CLIENT-CPP-RESULTS-ACTIVATION-RESET-ALIGNMENT-001

> 状态：`FOCUSED_TEST_PASS / RESULTS_ACTIVATION_RESET_ALIGNMENT_READY / GOVERNANCE_CLOSED / USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_2_2 / RELATED_94_94 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

按Server同名Task执行：只修改`BattleResultsRuntimeState.ActivateSummary(...)`，在winner/navigation defaults之后依次调用既有result-table reset和live-guard reset，并增加focused test。禁止修改full-domain scan、mode gate、4v4 reserve、schema、host action、package、30Hz、Input Actions、transport/recovery或marker。

已完成：test-first `0/2`；Unity compile0；final focused `2/2`；related `94/94`；fresh SelfCheck `PASS`（23:14:19）；Server Debug/Release及四套回归通过。只增加既有两个reset调用，其他禁止范围未改。
