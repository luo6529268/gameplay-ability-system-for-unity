# NTSD28-B2-ENVIRONMENT-STATE-CARRIER-001 — environment state carrier

<!-- CHANGE-RECORD
id: NTSD28-B2-ENVIRONMENT-STATE-CARRIER-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 environment_state_320 consumed by input_routing rowing redirect and physics/battle_world live paths; B0 correction proves Unity Unk328 is not equivalent.
evidence: TASK-CONTRACT-CREATED / SOURCE-CONSUMERS-CLOSED / B0-MISSING-BINDING-CONFIRMED / TEST-FIRST-RED-CS1061-CS0117 / COMPILE-0 / NEW-5-OF-5 / NATIVE-ACTION-CARRIER-10-OF-10 / SNAPSHOT-CHECKSUM-RING-31-OF-31 / NTSD28-GROUP-149-OF-149 / SELFCHECK-PASS / EXPECTED-ERRORS-7-THEN-CONSOLE-0 / LEDGER-136-92-PASS / PRODUCERS-B4-B5-B6-DEFERRED / RAW-PROJECTION-REMAINS-MISSING / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / CARRIER_READY / PRODUCERS_UNCONNECTED / AIR_CONSUMER_PENDING`

## 改前事实

- Unity runtime没有`environment_state_320`等价字段；raw exporter正确输出null。
- air rowing core不能用`Unk328`，也不能在无carrier时只实现默认0路径。
- entity snapshot3、aggregate snapshot5、checksum8已包含前一native action carrier集合。

## 预期改后职责

- runtime持有独立signed scalar并完成full lifecycle copy/reset。
- entity snapshot4、aggregate6、checksum9显式声明新增确定性状态。
- producer、raw trace和air consumer保持未连接，防止扩大语义结论。

## 验证记录

- Task Contract在任何脚本修改前建立。
- 2026-09-04 test-first：新增5个focused tests并请求Unity编译；编译仅因
  `EnvironmentState320`尚不存在而产生CS1061/CS0117，证明测试能够捕获缺失carrier；生产代码尚未修改。
- 实际实现：
  - `NTSDEntityRuntime.EnvironmentState320`是独立signed scalar；构造默认0，`ResetInputState()`保留，
    `Reset()`显式归0，`TryCopyCanonicalStateTo()`原值复制。
  - entity runtime snapshot schema 3→4，aggregate snapshot schema 5→6；复制继续复用预分配runtime。
  - checksum schema 8→9，并在每个runtime的native carrier段追加signed int32；default slot写0。
  - 既有native action carrier测试仅同步共享schema断言3/5/8→4/6/9。
- Unity脚本编译：生产实现后的最新编译段0 error，完成assembly reload。
- EditMode focused：
  - job `2b5f5c58a29d4f93a020511b41e55c9c`：新载体5/5；含4096次warm copy/checksum 0 B。
  - job `b4b25dc497b04271ac89da350be6416f`：既有native action carrier 10/10。
  - job `8f4ba742ec2449ad8a91c478661e854e`：snapshot/restore/checksum/history/snapshot ring 31/31。
  - job `ebb10ee1da4747dba95f78a89242f009`：`NTSD.Test.NTSD28.*`分组149/149。
- `BattleRuntimeSelfCheck`：2026-09-04约16:01输出“战斗运行时自检通过”；7条预期负向注册/绑定
  error读取后清除，随后Console error=0。
- `git diff --check`通过；`Tools/Validate-ChangeLedger.ps1`通过（136 records、92 governed code files）。
- 未验证/未实现：真实Play环境producer、raw trace和air rowing consumer；本包不声称环境行为或air行为已对齐。

## 回滚说明

按Task Contract恢复schema 3/5/8并移除独立scalar、copy/reset/checksum和新测试即可；没有写入Config、
DAT、Scene、Prefab或authority，也没有接入生产调用路径。
