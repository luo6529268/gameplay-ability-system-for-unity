# Task Contract — NTSD28-B3-C25-WRITER-INVENTORY-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / C25A-P-WRITERS-MAPPED / NEXT-C25-SKELETON`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25`
> 依赖：`NTSD28-B3-C23-C24-WORLD-CLOCK-001 / VERIFIED`

## 目标

在修改 C25 生产代码前，闭合修复后 Authority C25a～C25p 与 Unity 现有 writer、遍历、出生可见性、销毁时点和表现发布边界的逐项映射，确定可以独立实施和回滚的下一包。

## 只读范围

- Authority `simulation_tick_driver.cpp:931-1075`、`battle_world.cpp` 对应 slot API 与 `game_session.cpp::step()`。
- Unity `NTSDBattleTickSystem`、`SimulationWorld`、`BattleLateEntityLifecycleModule`、`LF2Entity`、`BattleStructuralWriter`、`BattleLogicObjectPointRuntime`、Registry/slot 查询、post-frame 与既有 W05/structural 测试。
- 只修改本 Task/Record、Ledger、STATE、handoff、总表及 C25 writer manifest；不修改任何 C#、Scene、Prefab、DAT、资源、ProjectSettings 或 Authority 文件。

## 不变量

- C25 是单个动态升序 live-slot 事务；不得继续用多个全局 scan 冒充。
- 新生高 slot 必须在同 tick 执行完整 C25，落入已扫描低 slot 必须等下一 tick。
- C25o 消费当前 slot 后必须跳过 C25p；OPoint、state18 粒子和 weapon fragments 必须发生在 lifecycle 前。
- 用户保留的 C15 随机掉武器路径不得删除或改写。
- Render/presentation 必须观察完整 core/session tick，不得继续在 C25 之前冻结旧状态。

## 验收

- C25a～p 每项均有 Authority writer、Unity 当前 writer、已确认差异和实施路由。
- 明确记录 legacy serial、global post-tail、early lifecycle 与 presentation-before-tail 四个结构问题。
- 明确下一包先建立单一生产入口、正确 placement 与动态 slot skeleton；具体行为按 a-b、c-e、f-j、k-p 后续包闭合。

## 回滚

本包只增加治理文档；删除新增 Task/Record/manifest 引用即可，不触碰任何运行行为。

## 完成证据

- 修复后 Authority 身份：EXE `B1E13AE1...9033`，82-file playable closure `39DDDA15...6109`。
- 逐行核对 C25 `simulation_tick_driver.cpp:931-1075` 及 16 个 `BattleWorld28` slot writer。
- 逐行核对 Unity 三个分散 owner、deferred mutation/slot query、structural immediate spawn 与既有高/低 slot 测试。
- 结果固化于 `docs/ai/MANIFESTS/NTSD28-B3-C25-WRITER-INVENTORY.md`；Authority 目录零写入。

