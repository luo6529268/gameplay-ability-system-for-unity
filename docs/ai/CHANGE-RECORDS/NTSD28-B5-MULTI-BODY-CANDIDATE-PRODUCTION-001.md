# NTSD28-B5-MULTI-BODY-CANDIDATE-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-MULTI-BODY-CANDIDATE-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_MULTIPLICITY
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5MultiBodyCandidateProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
authority: NTSD 2.8-Logan per-overlapping-BDY candidate loop and downstream selection; EXE B1E13AE1, closure 39DDDA15.
evidence: invalid fixture compile run d2e7ce8eb01d40f4a7636ac671f83789 excluded; valid red c13d2e3c86454c48bf2be0063e4d622b failed 7/8; focused 04c83ea644284613801c9639bff3d228 8/8; initial RoleAware 53f8def4ba3e44a98e182e7cacd85833 66/67 exposed only stale overlap-count 2->3, corrected RoleAware 3062f003f8f54b3ebb150b0596521085 67/67; HitPlan 201046c73c634f9fa623189e8e1d7f7e 184/184; B5 d911f1c054134f16a41aa2dffc19a5af 469/469; broad ddcc0fc6157d44f78bacd590e4f1a295 934/934; 05:51:00Z SelfCheck PASS; Console expected7; Scene D4266C6D unchanged; Ledger 313/272 PASS.
-->

> 状态：`VERIFIED / MULTI_BODY_CANDIDATE_MULTIPLICITY_ALIGNED`

回滚为移除production多BDY枚举与本包测试，恢复首重叠单candidate；不触碰direct query。

brute、loose与role-aware exact/fallback现均按BDY源顺序逐项进入既有selection/store owner；20容量、nearest tie RNG与direct query保护均有focused证据。
