# R8-WP01E-R01 — production stress initial-service / clean-restart discrimination

> 日期：2026-08-23  
> 状态：`VERIFIED`  
> Change ID：`R8-PERFBOOT-001`

## Goal

修正 production stress request processor 把“BattleTestBootstrap 初始异步服务加载中的半成品状态”误判为
“已健康 managed runtime 被破坏”的问题，使压力请求先等待既有 Bootstrap ready 合同，仅在先前已健康或
Bootstrap 已声明完整却失效时执行一次 clean Play restart。

## First failure

WP01E E-02 首次 Combat1000 短样本没有创建实体或进入性能采样。request processor 连续两次在 Bootstrap
完成前观察到：

`driverComponent=True, driverSingleton=True, world=True, poolComponent=False, poolSingleton=False, poolRuntime=False`

首次检查立即退出 Play，第二次达到一次重启上限并终止：

`Production managed runtime was invalidated again after the single clean Play Mode restart`

报告未生成，因此这是 harness/bootstrap lifecycle first failure，不是 1000 AI 帧率、GC 或 gameplay 失败。

## Source diagnosis

1. `PollRequest()` 先调用 `AreProductionServicesReady()`；
2. 当 `SuppressEntityCreationForProductionStress=true` 且 `ProductionStressServicesReady=false` 时，该方法
   有意返回 false，不访问 lazy `LF2ObjectPool.Instance`；
3. `CaptureManagedRuntimeState()` 使用非创建型 `LF2ObjectPool.TryGetInstance()`，此时 pool 合法地尚不存在；
4. 当前代码把 `managedRuntime.HasServiceFootprint` 作为 `managedRuntimeExpected`，driver/world 的正常先行创建
   便足以触发 restart；
5. `BattleTestBootstrap` 在 DAT/sprite/表现服务准备完成后才将 `ProductionStressServicesReady=true`，但当前
   processor 没有给它到达该边界的机会；
6. `MMSingleton<T>.Instance` 仍保留“缺失时自动创建”合同，不需要把 pool 预固化到场景。

## Allowed implementation

- 仅调整 `ProductionEntityStressRequestProcessor.PollRequest()` 传给纯函数
  `EvaluatePlayRestartDecision()` 的“runtime expected”事实来源；
- 初始 Bootstrap 未 ready 时必须返回 `WaitForInitialServices`；
- 先前已记录 healthy runtime 后失效，或 Bootstrap 已明确 ready 但 runtime 仍无效时，仍必须执行原有一次
  clean restart / 第二次 fail-closed；
- 在 `ProductionEntityStressEditorTests` 新增纯策略矩阵，覆盖初始partial footprint、bootstrap-ready invalid、
  previously-healthy invalid、restart pending 和 retry-limit；
- 不改变 120 秒 service deadline、请求持久化、reload recovery、runner、spawn、teardown 或报告格式。

## Prohibited

- 不修改 `LF2ObjectPool`、`MMSingleton`、生产 gameplay、AI、collision、render、capacity、worker、scene或DAT；
- 不在场景预创建六个 manager/service；
- 不把 `TryGetInstance()` 改为有副作用的 getter；
- 不取消 fail-closed restart limit；
- 不运行、构建、修改或写入 C++ authority。

## Verification

1. source diff仅限两个批准文件；
2. fresh scripts compile 0 error；
3. 新策略 focused tests、完整 `ProductionEntityStressEditorTests` 与 capacity focused tests PASS；
4. full `BattleRuntimeSelfCheck` PASS；
5. 重新运行同一个 Combat1000 capacity-pressure smoke：不得发生 premature restart，必须进入1000 active采样；
6. E-02报告仍按原合同裁决，不能用“成功启动”替代其0 B/capacity/central/teardown门；
7. Ledger validator PASS。

## Stop conditions

- 修正需要更改 Bootstrap、pool或production runtime，而不是Editor request processor；
- 初始等待修正后仍缺服务，first difference移动到独立生产初始化模块；
- 出现domain reload/restart语义扩大或需要长期架构调整；
- 需要改变现有对象池按需创建合同。

## Rollback

只回退本 Change ID 在 request decision caller 与对应测试中的改动，保留首次失败报告与 Record；不得触碰
其他 dirty worktree。

## Result

- fresh compile0；focused 263/263；14:35:20 self-check PASS；
- 同一Combat1000请求不再premature restart，1000 active和180 sampled ticks完整执行；
- terminal/report、0 B/0 collection、central draw/pixel与teardown均PASS；
- pool/Bootstrap/scene/gameplay/C++ 0改动；
- 本Task关闭，只裁决initial-service/restart discriminator，正式30FPS仍归WP01E。
