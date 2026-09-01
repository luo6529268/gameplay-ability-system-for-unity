# CLIENT-CPP-STAGE-CAMPAIGN-PARSER-DEFAULTS-ALIGNMENT-001

<!-- CHANGE-RECORD
id: CLIENT-CPP-STAGE-CAMPAIGN-PARSER-DEFAULTS-ALIGNMENT-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/BattleStageCampaignLoader.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStageCampaignLoaderDefaultsEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Server same-ID Task/Change; C++ release parser retention semantics; user standing bounded Client authorization.
evidence: FOCUSED_TEST_PASS / STAGE_CAMPAIGN_PARSER_DEFAULTS_ALIGNED / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / USER_STANDING_AUTHORIZED / TEST_FIRST_2_FAIL_2_PASS / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_30_30 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> Status: `FOCUSED_TEST_PASS / STAGE_CAMPAIGN_PARSER_DEFAULTS_ALIGNED / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / USER_STANDING_AUTHORIZED / TEST_FIRST_2_FAIL_2_PASS / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_30_30 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

Only assignment-on-success behavior and focused evidence are in scope.

## Result

Missing/invalid stage `id` and spawn `times` now retain `-1/1`; required spawn
`id` failure still omits the row. Valid/order/duplicate behavior and all excluded
systems remain unchanged. Focused/related/SelfCheck/Server evidence passed.
