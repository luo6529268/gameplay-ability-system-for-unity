# U5 分段 Structural Writer 收口（2026-08-12）

## 1. 结论

opoint、register/unregister、free/destroy 与 runtime slot generation claim/release 已统一经过每个 `SimulationWorld` 持有的 `BattleStructuralWriter`。Unity 对象池与 `LF2ObjectPointFactory` 继续负责资源物化，但不再零散定义结构写入的权威时序。

该 writer 显式记录 `CurrentEntityImmediate`、`CurrentPassSegment`、`NextPass`、`TickEnd` 与 `DeferredUnregisterFree`。它不会把所有生成和销毁合并成通用 tick-end command buffer，因此保留了 C# 权威中当前实体、当前 pass 以及后续 pass 能否观察新实体/已释放 slot 的差异。

## 2. 权威依据

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs`：character/object hit、late per-entity、随机武器与 postframe 的 pass 顺序；
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Frame\FrameTick.cs`：opoint 触发与当前帧推进边界；
- C# runtime 的 `RegisterEntity`、`FreeEntity`、slot/generation 生命周期调用链。

Unity 适配仍使用 GameObject pool，但 slot、generation、active/dormant、下一表现 tick 和 ghost-handle 判定由逻辑世界维护，Transform 或对象池可见性不参与逻辑真值。

## 3. 实现边界

- `LF2ObjectPointFactory` 通过 `ProcessLateOpointSegment`、`Spawn` 与 `SpawnMultiple` 委托结构 writer，再调用内部 materializer；
- `SimulationWorld.Register/Unregister` 统一委托 writer，再进入 core registry；
- `LF2Entity.FreeEntityLikeExe/DestroyEntityLikeExe` 统一委托 writer；
- runtime slot claim/release 在实际 registry 成功点记录，不提前伪造 generation；
- writer 命令 ordinal 每逻辑 tick 重新从 1 递增，仅用于诊断结构顺序，不成为新增 gameplay gate；
- 没有新增 partial class，也没有改变 Unity 对象池资源职责。

## 4. 验证

- W05A～W05E 覆盖最低空闲槽、下一表现 tick、generation/ghost、单个和六个 opoint、death cleanup 与预热后零分配；
- U5 最终联合 EditMode job `b55c2edd04964be7b784f7bec65ab0f5`：220/220 PASS；
- Unity fresh compile：0 C# error；
- 完整 `BattleRuntimeSelfCheck`：`2026-08-12 20:34:10` fresh PASS；
- Authority400 full/full：6/6 `equal-diagnostic`、`firstDifference=null`；这是诊断证据，不是 production certificate；
- 1000 AI 最终短样本报告 `Temp/NTSD_ProductionEntityStress.u5-battle-results-writer-1000ai-60-20260812.json`：正式 tick 0 B、Gen0/1/2 collection 为 0、cleanup restored。

本记录关闭 U5 的结构写入所有权，不表示 U6 已把全部结构数据迁成 SoA，也不关闭 U9 的稳态性能门禁。
