# Task Contract — NTSD28-B6-CPOINT-THROW-ATOMIC-PRODUCTION-001

> 状态：`SUPERSEDED / INVALID_FULL_RESOURCE_SCOPE / NO_VERIFICATION`
> 依赖：`NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001 / VERIFIED`

## 目标

使 Unity kind-1 CPoint throw tail 与 NTSD 2.8-Logan 一致：复用原生资源 pure core，正
`throwinjury` 写 environment injury/self source 而不污染 WeaponCount，并在无独占 depth 输入时
保留 caught Vz。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointThrowAtomicProductionEditorTests.cs`（新）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs`
- 本 Change 的 Task/Record/Ledger/STATE/handoff/总表。

## 禁止范围

- 不改 Config、Scene、Prefab、资源、ProjectSettings、Authority。
- 不扩展完整 CPoint schema，不处理 input action、held injury/cover sync、caughtact combo。
- 不改 HitPlan；CPoint advance 是 hit plan 后的独立 pass。
- 不改变 throw position/action/link/dircontrol 与 `throwinjury==-1` transform 现状。

## 验收

1. RED 覆盖：正 injury 的 environment/resource/display/self source、WeaponCount sentinel、
   no-depth/both-depth Vz preservation、exclusive depth overwrite、nonpositive injury exclusion。
2. focused tests通过且 warm path 0 allocation（若测试基础设施可稳定测量）。
3. B6/CPoint related、B5、NTSD28 broad regression、fresh SelfCheck通过。
4. 抓取 Play probe更新并通过；Unity compile 0、Console 0 error、Scene dirty/hash不变。
5. Change Ledger validator通过。

## 回滚

反向移除本 Change 在 `BattleCpointWriter` 的资源/environment/Vz写入并删除新 focused test；恢复
同 Change 修改的旧断言。不得用 Git reset/restore。

## Supersede

静态编译后的依赖复审确认：本包错误地计划在 authoritative `baseMaxMp`、selected-mode override 与
child suppression 尚未闭合时连接完整 MP transaction。该范围违反既有 B5 readiness 合同，故本包
不进入验收，也不得作为“resource已对齐”证据。后继包
`NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001` 只保留可独立精确实现的 display lead、
environment/self-source、WeaponCount exclusion 与 Vz；完整 MP transaction 后置。
