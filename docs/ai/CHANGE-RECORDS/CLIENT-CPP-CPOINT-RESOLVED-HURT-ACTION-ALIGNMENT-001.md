# CLIENT-CPP-CPOINT-RESOLVED-HURT-ACTION-ALIGNMENT-001

<!-- CHANGE-RECORD
id: CLIENT-CPP-CPOINT-RESOLVED-HURT-ACTION-ALIGNMENT-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/CPointResolvedHurtActionAlignmentEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: User standing authorization GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002; Queue0dc/0dd/0de; C++ release parse_cpoint/hit.cpp live path.
evidence: VERIFIED / TEST_FIRST_MISSING_RESOLVER_RED / UNITY_COMPILE_PASS / UNITY_FOCUSED_5_5_PASS / UNITY_RELATED_238_238_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / CPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS / GOVERNANCE_VALIDATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> Status: `VERIFIED / TEST_FIRST_MISSING_RESOLVER_RED / UNITY_COMPILE_PASS / UNITY_FOCUSED_5_5_PASS / UNITY_RELATED_238_238_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / CPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS / GOVERNANCE_VALIDATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

The Server Task is the complete contract. No 0df source has changed.

Test-first compile produced 10 expected new-test-only missing-helper
diagnostics and no unrelated compile error.

Focused job `414ca8942aa640109008f7371b13d868` passed 5/5. Related job
`aa5b377062ac4ccc916e54986e94acfd` ran 237 tests and failed only the two
existing caught-victim fixture cases because their hand-built CPoint omitted
parser-resolved `injury`/`cover`. The Server Task was amended before the
fixture-only correction.

Fresh SelfCheck at 2026-08-31 14:12:21 failed only C-15/C-33. The already
declared SelfCheck path contains the same parser-incomplete hand-built CPoint:
aliases `230/232` without resolved `injury`/`cover`. This first-difference is
recorded before adding only those resolved fixture values.

Final evidence: fresh compile0；focused job
`414ca8942aa640109008f7371b13d868` 5/5；expanded related job
`3c4a0e8084ea447983ad13ec783753bc` 238/238；fresh SelfCheck
2026-08-31 14:17:26 PASS；3700-byte/16-LF/0-CR/final-LF corpus SHA
`7FDEA9EB056452FD204BA1302E46F6D042F7818CF3EECB4C6D112AD514C75E88`；
Server Debug/Release and all four executable suites PASS；Server/Client
Ledgers and workflow PASS. Queue0df VERIFIED/CLOSED；marker false and S0
NOT_VERIFIED.
