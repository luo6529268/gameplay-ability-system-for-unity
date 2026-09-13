<!-- CHANGE-RECORD
id: NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001
status: FOCUSED_TEST_PASS
change-kind: RESTORE_ALL_CAPTURED_RAW_RUNTIME_PAYLOADS
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05UnclaimedRawRestoreEditorTests.cs
authority: Q03 snapshot contract captures every materialized raw runtime independently of claimed entities; current +2F8 test observes raw 37 captured but remains370 after successful restore in both profiles.
evidence: RED_OBSERVED / FOCUSED_46_OF_46_PASS / SELFCHECK_PASS / VERIFIED_RAW_PAYLOAD_RESTORE_ONLY / INTERMEDIATE_UNPUBLISHED
-->

# Q05 已捕获未占用raw payload恢复修复

准确3脚本，+2F8 carrier联验发现的必要独立修复。原状BattleWorldEntityRuntimeSnapshot保存所有materialized raw（TryCapture和TryCopyRaw已证37）；BattleStateSnapshotRestore payload循环先if(!view.Claimed)continue，导致未占用raw完全跳过，既有字段也有同一风险。此为已有snapshot职责修复，不是新增恢复策略或网络接口。

改为每个slot独立恢复snapshot raw presence，再对claimed恢复entity；没有raw source时重置已存在目的raw为初始化态，不强建不存在页面。已有snapshot raw来源有而目的尚无时，按已捕获来源materialize必要页（正常warm路径不分配）。恢复前验证snapshot全部present runtime canonical storage和已有目标raw storage，失败保持World/queues/输入等不变；此校验不读取创建singleton或Flush。

新增source buffer只读HasCanonicalPayloadStorage验证其present entity/raw，不改变payload/schema。全payload复用TryCopyCanonicalStateTo，不能只特判新增2F8字段或复制raw对象引用。默认/reset/copy语义沿用NTSDEntityRuntime，不改实战tick/pass/lifecycle顺序。

RED先额外用旧X/HP/Spawner/InputHistory证明一般raw payload问题，再测both profiles完整checksum回滚一致；损坏captured raw storage必须pre-mutation拒绝；warm restore不分配由既有回归覆盖。+2F8 capture/restore用例闭合后才可其限定出口。当前同Q05未发布中间窗口，版本仍待统一13/21/24/2/2及旧版本拒绝/双OPoint guard，禁止跨版本交换。

用户禁止computer-use；只桥接/日志/结果/进程。没有资源/Scene/UI/GAS/外部package修改。Foot任务外变化和Scene旧精度差异保持。回滚须批准，只准确差量。无新manager/queue/worker，停止阶段仍已有World/snapshot owner。

## 实施已写

RED4/4失败：两profile旧raw.X仍777而非12.5、损坏raw snapshot仍返回true。准确三脚本已写：source present payload canonical检查、目标现有raw storage非创建式preflight、all-slot raw独立copy/reset，再claimed entity copy。新页面只在source已捕获raw且目的不存在时必要创建；warm路径无新增分配。没有创建manager/Flush/修改版本。下一Unity compile/定向与相关snapshot/ECS/SelfCheck验证。

联合focused job62766860024a4edebce34a419618a0a6 46/46 PASS；本2F8 11、raw修复4、既有runtime snapshot/restore/checksum/runtime-slot/ECS shadow共31，包含warm capture/restore no allocation。完整SelfCheck已请求待结果；Ledger490/110 PASS。

## 限定出口

FOCUSED_TEST_PASS / VERIFIED_RAW_PAYLOAD_RESTORE_ONLY；完整证据、真实命令与限制见artifacts/diagnostics/NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001/REPORT.md。SelfCheck08:57:07Z新PASS，CS0/Scene旧SHA保持，46联合测试全过含warm零分配。没有新producer或正式schema发布；下一NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001，Q05/总目标未完成。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，490 Records / 110 governed code files；主2F8 artifact ledger-final.txt保存结果。
