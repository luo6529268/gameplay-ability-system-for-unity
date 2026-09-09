# Task Contract — NTSD28-B2-FUNCTION-KEY-ROUTE-CROSSWALK-001

> 状态：`VERIFIED / FULL-F1-F12-CROSSWALK / IMPLEMENTATION-SPLIT-DEFINED / NEXT-ROUTE-CONTRACT`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / FUNCTION KEYS`  
> 建立日期：2026-09-04

## 目标

闭合 NTSD 2.8-Logan playable live closure 中 F1～F12 的路由优先级、auto-repeat/context 拒绝、Host/Session/
continuous/maintenance 分类、Session 状态事务、事件 bit 消费顺序与下游效果归属；对照 Unity 当前分裂的
F1/F2/F5 Host latch 与旧 F7/F8/F9 ModeRule 路径，产出可按依赖实施的最小拆包。

## Authority 与当前缺口

- Authority：`source/ntsd28_playable/include/ntsd28_playable/native_function_keys.h`、正式调用者
  `source/ntsd28_playable/src/main.cpp`、`source/ntsd28_playable/src/game_session.cpp`及其正式 tests。
- Unity 已有 B1 F1/F2/F5 cadence/host control；不得重复实现或建立第二个 Host 真相。
- Unity `BattleFunctionKeyInputLatch` 只认识经 Config allow-list 的 F7/F8/F9，且 F7 当前映射为全属性初始化，
  与 Authority “tick 尾部只把全部 active entity current MP 写为 500”不等价。
- Unity 缺 F3、F4、F6、F10、F11、F12 的统一 route/result 契约，也缺 F3 lock、F6 hit-resource、事件计数、
  F7 pending-full-MP、F8/F9 shared last-wins pending object command 的正式 carrier。

## 允许操作

- 只读 Authority header/main/GameSession/tests 与 playable build closure；只读 Unity Host、FunctionKey、Flow、
  snapshot/checksum、results/random-weapon/audio 候选。
- 新建/更新 `docs/ai/MANIFESTS/NTSD28-B2-FUNCTION-KEY-ROUTING.md` 及本 Task、Record、Ledger、STATE、handoff、总表。
- `code-path: NONE`；不得修改 C#/C++/tool source、Config/DAT、Scene/Prefab、ProjectSettings、Packages 或 Authority。
- 实施必须另立 Task/Change；B2 只建立 route/runtime carrier/生产接线，F4/F7/F8/F9/F11/F12 的下游效果按
  B3/B8/B10/B11 owner 路由，不在本审计包提前完成。

## 验收

- F1～F12 逐键记录 disposition、command、route gate、Session gate、状态副作用、消费时点与 Unity owner。
- 明确 repeat、Ctrl+F9/F10 maintenance、F11/F12 continuous bypass context、F3 one-way lock、F8/F9 global-delay
  与 shared last-wins 的严格优先级。
- 明确 Authority queue bit `0x04/0x10/0x20/0x40/0x80` 的固定消费顺序，不把物理到达顺序错误投影成执行顺序。
- 区分已由 B1 完成的 F1/F2/F5、B2 应实现的纯 route/carrier/integration、以及 B3/B8/B10/B11 下游效果。
- 给出唯一下一实施包的文件所有权、测试先行标准与不得改动项；validator 通过。

## 回滚

仅删除/回退本次 manifest 与治理路由；production 与 Authority 零修改。

## 完成证据

- `docs/ai/MANIFESTS/NTSD28-B2-FUNCTION-KEY-ROUTING.md` 已冻结formal live closure、九级route priority、
  F1～F12逐键矩阵、maintenance、event mask、固定dispatch顺序、reset与Unity结构差异。
- 已区分runtime state primitive的last-accepted-wins与正式queued-byte同窗口固定F8→F9导致F9胜出，未把物理顺序
  写成执行顺序。
- 已确认当前formal caller的global-delay实参恒clear；保留defined seam但不冒充动态拒绝已被formal观察。
- 实施拆为route contract→Session carrier→production integration；B1/B3/B5/B8/B10/B11 owner边界已锁定。
- validator：`PASSED / Records 153 / governed code files 104`；本包`code-path: NONE`。
