# Task Contract — CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SHARED-OWNER-001

> 状态：`FOCUSED_TEST_PASS / SHARED_WORLD_SCALAR_OWNER_READY / GOVERNANCE_CLOSED / USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / PACKAGE_0_5_0_DIRECT_AND_LOCKED_ARTIFACT_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## 1. 目标与范围

仅将已闭合seam的`BattleWorldScalarState.cs`和原GUID移动到Server-owned `com.ntsd.battle-kernel/Runtime/Core`，保持namespace/API/字段/默认值/方法/调用者不变；package tuple升为`0.5.0`，新增18-line corpus的.NET与Unity consumers，并只把既有seam结构测试的Client-path断言升级为package single-owner断言。

## 2. 验收

先取得.NET owner absent红灯；再证明single source/GUID、assembly owner、18-line SHA、五类型字段顺序/default/reset/stage transitions、Unity compile和相关回归、SelfCheck、direct+locked package consumers、Server及双Ledger。

## 3. 禁止

禁止修改snapshot/checksum/restore/roster/results/root/stage campaign/entity/content；禁止改battle rules、30Hz、tick/pass/input、Scene/资源/Input Actions、TargetTick/InputDelay、formal AI、wire/recovery/transport/数据库/公网或marker。

## 4. 完成证据

Single source/GUID位于Server-owned Core，原Client路径不存在；direct Debug/Release与exact0.5.0 locked artifacts通过；Unity compile0、package+seam10/10、related83/83、fresh SelfCheck PASS；Server双配置build与四suite通过。只完成shared value owner，formal marker与S0/S5未晋升。
