# Task Contract — NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001

> 状态：`VERIFIED / PRODUCTION-CONNECTED / SESSION-EXACTLY-ONCE / LEGACY-PHYSICAL-ISOLATED / REAL-PLAY-PHYSICAL-PASS / EFFECTS-DEFERRED-DOWNSTREAM`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / FUNCTION KEYS`  
> 建立日期：2026-09-04

## 目标

把F1～F12 pure router与F3/F6～F9 Session carrier接入`SimulationTickDriver`正式LocalFreeRun/显式tick边界：
F1/F2/F5复用B1 Host owner，F3/F6～F9只queue后每逻辑tick固定dispatch，F4/maintenance/F11/F12输出typed Host handoff。
旧F7/F8/F9 effect不得由正式physical入口继续触发；其旧diagnostic API暂时隔离保留，供后续B8迁移前历史probe使用。

## Authority 与原状

- Authority：`main.cpp:1924-1984,2401-2417`与`game_session.cpp:2444-2471,2632-2664`；crosswalk manifest。
- 前置pure route和Session carrier均已focused ready并进入reset/snapshot/restore/checksum。
- Unity当前B1 Host physical latch直接产F1/F2/F5；旧`BattleFunctionKeyInputLatch` physical读取Config allow-listed F7/F8/F9，
  tick前直接写旧`InitStatsRequest/Mode2Request`。
- `StepOneTickInternal(int)`与`StepOneTickInternal(FrameInputSet)`当前可能连续调用旧apply seam；新Session dispatch必须保证
  每个逻辑tick恰好一次，否则accepted event byte会被第二次空dispatch错误清零。

## 允许修改

- 新建`Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeFunctionKeyPhysicalLatch.cs`。
- 修改`Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeFunctionKeySessionState.cs`增加masked byte queue seam。
- 修改`Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`接入capture/route/dispatch/typed handoff与exactly-once seam。
- 最小修改`Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs`：正式battle runtime接管F6/F7时禁止旧force-walk/run调试副作用。
- 新建`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeyProductionIntegrationEditorTests.cs`。
- 最小更新`Assets/NTSD/Scripts/Simulation/Input/BattleFunctionKeyInputLatch.cs`注释，明确旧路径仅为legacy diagnostic；
  不改变其旧diagnostic数据结构与历史probe行为。
- 上述Unity生成`.meta`以及本Task/Record、Ledger、STATE、handoff、总表。

不得修改B1 Host policy语义、GameConfig/asset、Input Actions、Scene/Prefab、DAT、Authority、旧F7/F8/F9 effect代码、
F6 hit consumer、F8/F9 object consumer、F11/F12 audio/overlay、pass顺序或内容资源。

## 实现合同

- LocalFreeRun每Update先capture B1 Host edge与F3/F4/F6～F12；即使paused也可积压Session命令，直到F2/恢复后tick消费。
- B1 edge输出必须经过pure router后才映射回既有`SimulationHostControlCommand`，不复制Host transition。
- 新physical latch使用held-mask false→true边沿处理F3/F4/F6～F10；F11/F12连续状态每Update刷新且F12同时held胜出。
- route physical context与formal main一致：battle真实、main/delay当前传true、F3 lock真实；Session tick context用Unity现有
  mode/results bridge，global delay仍true并明确留B8 producer。
- 每tick将masked event byte转入world carrier并恰好dispatch一次；dedicated worker成功、失败fallback、直接FrameInput
  三路径都不得重复dispatch或漏dispatch。
- F4、maintenance、continuous volume只保存在typed Host handoff/diagnostic seam，不关闭Scene、不写audio。
- 正式physical入口不再调用旧Config allow-list latch；旧`QueueBattleFunctionKeyCommandsForDiagnostics`仍是明确legacy-only。
- `BattleTestBootstrap`只能隔离F6/F7键冲突，不得改角色生成、输入映射或其他debug行为。

## 验收

- test-first missing physical/integration seam red。
- focused覆盖physical rising/rearm、Ctrl maintenance、F11/F12、context reject、B1 command mapping、driver F7与F8/F9
  exactly-once dispatch、F3 same-window lock、main-state bridge、typed Host handoff、legacy isolation。
- compile0；新focused与B1/route/carrier/old diagnostic/snapshot/checksum/worker相关回归全pass；full SelfCheck PASS；
  Console0；validator通过。
- 若可安全自动运行，再做paused driver真实tick定向验证；物理键人工验收、B8/B10 effects保持`RUNTIME_PENDING`。

## 回滚

删除新physical latch/test；恢复driver旧capture/apply调用和旧latch physical owner；移除carrier byte queue helper与typed
handoff字段。B1 Host policy、Scene/Config/asset未改，可局部回退。

## 完成证据

- test-first fresh compile：23条预期missing physical/driver seam `CS0246/CS0103/CS1061`。
- 新physical latch覆盖F3/F4/F6～F12 rising/continuous/maintenance；F11+F12同held为F12，4096 warm capture 0 allocation。
- driver正式Update在paused return前capture；B1 F1/F2/F5 edge经router再回到既有Host owner；Session byte在同步、worker与
  worker-fallback形状中每tick最多dispatch一次。dedicated worker定向test实际通过。
- 正式physical不再调用旧Config-gated latch；旧API只保留legacy diagnostic。`BattleTestBootstrap`在Running world下不再
  让F6/F7同时触发force-walk/run。
- 首轮integration `6/9`的3个失败均由test fixture仍处Preparing使route正确拒绝；fixture进入Running后`9/9`。
  加入worker、zero-allocation与bootstrap guard后最终`11/11`（job `899d5d81b8574e819261ecba9f25df40`）。
- 最终router/carrier/integration、B1 Host、旧diagnostic、snapshot/restore/checksum/ring、worker联合`96/96`
  （job `5235384ab4244169889be51446c4b6e5`）。
- Unity compile 0；full SelfCheck 2026-09-04 20:41:07 `PASS`；7条既有negative日志审阅后清空，Console 0；
  validator `PASSED / Records 156 / governed code files 114`。
- 后续`NTSD28-B2-FUNCTION-KEY-PHYSICAL-PLAY-PROBE-001`已在真实Play以Keyboard device1完成F3/F4/F6～F12
  九组PASS、tick0→5，Scene SHA不变；本integration包据此提升为VERIFIED。
- F4/F6～F12 downstream effects仍按B3/B5/B8/B10/B11路由，不属于本包完成面。
