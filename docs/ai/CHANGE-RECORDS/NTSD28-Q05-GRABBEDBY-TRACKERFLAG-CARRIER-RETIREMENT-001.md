<!-- CHANGE-RECORD
id: NTSD28-Q05-GRABBEDBY-TRACKERFLAG-CARRIER-RETIREMENT-001
status: FOCUSED_TEST_PASS
change-kind: REMOVE_RETIRED_GRABBEDBY_TRACKERFLAG_STORAGE
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.Lifecycle.partial.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6Kind2PickupAtomicProductionIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyGrabbedByRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyTrackerRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05GrabTrackerCarrierEditorTests.cs
authority: Q03 JOINT-FIELD-MATRIX / Goal20 verified GrabbedBy and TrackerFlag producer-reader retirement; formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 authority and live relationship/lifecycle contract.
evidence: RED_6_FAIL_1_PASS / FOCUSED_407_PASS / SELFCHECK_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING
-->

# Q05 两个退休关系标记载体

父FIVE-RESERVED-CARRIER步骤2内先完成本包，剩余ReleaseTick/WeaponState/HolderCopy仍待。准确11脚本：6生产、4旧测试/含probe、1新测试。当前完整字词搜索证明生产仅runtime/Entity包装、copy/reset/init、自赋值同步、ECS数组/比对/hash；两字段本来没有独立checksum/parity字段，因此不虚构相应删除。

删除NTSDEntityRuntime.GrabbedBy/TrackerFlag和复制/reset，LF2Entity两个包装及其自赋值同步，Character/Weapon/Other初始化零赋值，BattleEcsLinkStore数组及构造/Capture/Clear/Matches/RuntimeFingerprint对应项。保留真正LinkState/Holder/Target/Caught/Catcher、TrackerParent及快照handle、Spawner/Owner/独立+2F8；不能按tracker相似名称删除真实引用。

旧GrabbedBy/Tracker tests、Kind2 setup和SelfCheck中的退休sentinel删除/改为成员不存在断言；原有关系建立、释放、inactive reverse字段、pickup replacement和缓存不能代替runtime查询的断言完整保留。旧Goal20 request Play probe改为报告两字段absent，剩余三reserved仍按当前默认检查，不提前删它们或覆写历史报告。

先新absence六用例和真实TrackerParent/owner snapshot回归RED，再生产；跑相关Goal20关系/工厂/pickup、snapshot/restore/ECS/2F8与完整SelfCheck，必要实际Play probe。没有新增manager/queue/worker或关闭阶段；不改规则顺序/33ms/3ms/十一阶段。实体snapshot继续既有canonical copy，不新造serializer。

当前Q05未发布中间payload，版本仍12/20/23/1/1，后续步骤3identity/双OPoint guard、步骤4统一13/21/24/2/2与旧版本拒绝、步骤5回放/Play；本包不发布baseline、不推进Q07。保留Unity/GAS、非战斗、Scene/InputActions/Gen/Plugins/外部包、stage.dat USER_HOLD和例外。用户禁止computer-use，仅桥接/日志/结果/进程；Foot18任务外删除及目录/Scene旧SHA保护。回滚须批准，只准确差量。

## 已写

RED7为6FAIL/1PASS（真实TrackerParent/Owner/Spawner/2F8 snapshot往返原已通过）；两个flag各在runtime/Entity/ECS均存在。现准确11脚本已写，移除存储/init/copy/reset/ECS及包装/自赋值；旧测试去掉退休sentinel，实际关系和inactive reverse其余字段断言保持。Goal20 probe报告absent，不改原历史artifact；compile/focused/SelfCheck/Play待。

补充精确前置回链：NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001、NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001均已有VERIFIED行为退休证据，本包只完成其明确保留给Q05的storage清理。联合407/407 PASS，完整SelfCheck09:43:33Z >09:42:47Z PASS；当前运行既有Goal20_R12 request Play probe。

## 限定出口

407/SelfCheck/Goal20实际关系Play通过，两个flag存储删除、真实TrackerParent/Owner/Spawner/2F8及其他关系保持。Scene旧SHA和Foot既有缺失不变，CS0。准确证据/命令/嵌入报告默认字段限制见artifact REPORT。下一NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001，其余三reserved和联合版本后继，未发布中间baseline。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，492 Records / 130 governed code files，ledger-final.txt保存。
