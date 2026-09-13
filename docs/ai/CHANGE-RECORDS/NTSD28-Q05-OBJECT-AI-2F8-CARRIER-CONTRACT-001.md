<!-- CHANGE-RECORD
id: NTSD28-Q05-OBJECT-AI-2F8-CARRIER-CONTRACT-001
status: FOCUSED_TEST_PASS
change-kind: INDEPENDENT_NATIVE_2F8_CARRIER_COPY_RESET_DERIVED_HASH
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q052F8CarrierEditorTests.cs
authority: Formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; battle_world.h EntityState28.object_ai_excluded_group_source_slot_2f8 int=-1; playable battle_world.cpp:8032 writer/native_ai.cpp:290 reader, Q03 frozen JOINT-FIELD-MATRIX.
evidence: RED_OBSERVED / FOCUSED_46_OF_46_PASS / SELFCHECK_PASS / VERIFIED_CARRIER_AND_RAW_RESTORE_ONLY / INTERMEDIATE_UNPUBLISHED
-->

# Q05 独立+2F8载体

父NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001步骤2内先执行本独立载体子包，随后mass/Oscillate/five-reserved删除；无第二版本发布窗口。准确5脚本，snapshot buffer已有canonical copy链无需为新字段重复造serializer。

原状runtime仅Spawner/Owner而无+2F8。正式原版字段是int默认-1的原始slot，held release type1/4/6且parent WPoint dvx非零写parent slot，common AI读取当前该slot实体battle_group；不是缓存group/stable id，不能覆盖Spawner或Owner。

新增NTSDEntityRuntime.ObjectAiExcludedGroupSourceSlot2F8（int/default -1）；canonical copy与Reset准确覆盖。BattleEcsIdentityStore同名独立int数组，默认/clear -1，CaptureIdentity/MatchesIdentity/完整RuntimeFingerprint纳入；checksum在spawnerSlot后纳入新int，parity键固定objectAiExcludedGroupSourceSlot2F8，在同字段组输出。snapshot claimed实体与materialized raw slot复用TryCopyCanonicalStateTo；不新建snapshot域或业务owner。

保留SpawnerSlotIndex、OwnerSlotIndex/OwnerStableId/RelationOwnerSlotIndex及TrackerParent独立合同，五reserved/mass/Oscillate后继。当前没有新producer或AI consumer接线（Q06），不能把字段默认-1写成目标筛选已经对齐。不为该字段改变原版slot上限或把int合法域限制为当前capacity；原位复制保留int32，consumer后续按原版bounds判断。

本包为INTERMEDIATE_UNPUBLISHED_Q05_WINDOW：字段集合已变化，但按批准的同一窗口步骤4统一entity12→13/aggregate20→21/checksum23→24/两shell1→2，当前不发布新baseline或Q07资源。旧版本拒绝及完整trace身份迁移仍后继，不能把中间schema标成可跨版本交换。

验收：RED先证明缺字段；int32极值/default/copy/reset独立性、claimed和raw两份snapshot捕获/原位恢复、ECS捕获/差异识别/清理、每个profile checksum及parity变化，既有完整runtime-copy/snapshot/restore/checksum回归、编译/SelfCheck、无新增分配与schema后继声明。字段无新queue/worker/manager，停止阶段沿用World runtime与derived清理，不改变十一阶段。

Unity/GAS、非战斗、Scene/InputActions/Gen/Plugins/外部Server、33ms/3ms、stage.dat USER_HOLD与例外保持。禁止computer-use；保留Foot任务外18删除及新blue/red/yellow目录、Scene旧精度差异。回滚须批准，仅本包差量，不能覆盖既有工作。

## 实施进展

RED11/11失败，缺独立runtime/ECS字段；XML与job已保存。准确五脚本已写：独立int32/-1、canonical copy/reset、ECS数组/匹配/fingerprint、checksum/parity投影；snapshot复用既有canonical链。没有接held release/AI producer，没有改schema或发布baseline，仍INTERMEDIATE_UNPUBLISHED。测试首次编译的ECS namespace及EntityRuntime属性拼写已按当前代码纠正，RED基于成功编译后的实际测试。下一compile/focused/SelfCheck。

联验34中三失败：两profile未占用raw已captured37但restore后仍370，独立NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001先修既有全raw恢复漏项（非特判2F8）；另一Authority400 full parity诊断仅实体投影/未输出raw值，初测试误期望29，已按现有ProjectDefault/Extended分层改为两profile都验raw checksum，只有extended验证raw JSON值，保持既有authority trace表现。所有原失败XML留证。

联合focused job62766860024a4edebce34a419618a0a6 46/46 PASS；本2F8 11、raw修复4、既有runtime snapshot/restore/checksum/runtime-slot/ECS shadow共31，包含warm capture/restore no allocation。完整SelfCheck已请求待结果；Ledger490/110 PASS。

## 限定出口

FOCUSED_TEST_PASS / VERIFIED_CARRIER_AND_RAW_RESTORE_ONLY；完整证据、真实命令与限制见artifacts/diagnostics/NTSD28-Q05-OBJECT-AI-2F8-CARRIER-CONTRACT-001/REPORT.md。SelfCheck08:57:07Z新PASS，CS0/Scene旧SHA保持，46联合测试全过含warm零分配。没有新producer或正式schema发布；下一NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001，Q05/总目标未完成。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，490 Records / 110 governed code files；主2F8 artifact ledger-final.txt保存结果。
