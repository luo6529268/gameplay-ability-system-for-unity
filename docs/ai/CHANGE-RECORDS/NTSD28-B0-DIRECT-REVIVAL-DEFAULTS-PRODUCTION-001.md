# NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleMatchConfigRuntimeAdapter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B0DirectRevivalDefaultsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan playable/scenario direct combatant revive_lives_30c/revive_next_lives_310/revive_next_hp_314 defaults 1/0/0; Unity direct participant adapter and runtime HP2Orig/HPOrig/RespawnCount mappings; EXE B1E13AE1, closure 39DDDA15.
evidence: RED 1/3: invalid branch passed while valid slots 0/19 retained stale HP2Orig77 instead of default1. Production adds only HP2Orig/HPOrig/RespawnCount=1/0/0 after the existing valid direct-registration gate. Focused 3/3, existing direct-owner regression 15/15, targeted NTSD_Battle Play and both 0-error builds pass; active entity snapshot is 1/0/0 while independent raw backing remains 0/0/0; Console 0 and Scene unchanged. Full SelfCheck remains blocked earlier by unrelated CPoint victim-Vz assertion.
-->

> 状态：`VERIFIED / RED_1_OF_3 / DIRECT_DEFAULTS_1_0_0 / FOCUSED_3_OF_3 / DIRECT_OWNER_REGRESSION_15_OF_15 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_RUNTIME_RESUMABLE`

RED1/3后只给direct成功准备路径补1/0/0。focused3/3、direct-owner15/15、targeted Play与builds通过；
active snapshot=1/0/0且raw backing=0/0/0，Console/Scene不变。SelfCheck独立CPoint阻塞；B4 gate恢复。
