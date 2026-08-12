# U2 表现发布边界验收（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 阶段：U2
> 结论：逻辑 tick 已只发布纯数据，中央表现命令、资源解析、排序、Mesh 与音频在 Unity host 边界物化；U2 完成，允许进入 U3，但 U9 的 1000 AI / 30 FPS 门禁尚未完成。

## 1. 逻辑与表现 owner

- `NTSDBattleTickSystem` 的 `RenderDispatch` 只捕获并发布预分配的逻辑快照，不再在 Play Mode 的每个逻辑 tick 内完成中央 Mesh 提交。
- `BattlePresentationFrame` 显式记录 `CommandsMaterialized` 与 `PresentationOrderMaterialized`。发布给逻辑侧的原始帧保持未物化，冻结的 host 帧才解析 sprite/resource、排序并构建表现命令。
- `SimulationTickDriver.LateUpdate` 每个 Unity 可见帧最多调用一次 `PresentLatestFrame`。同一 Unity 帧内即使显式推进了多个逻辑 tick，也只物化最新已发布帧；中间 tick 的战斗事件仍保留在逻辑 journal/快照边界中。
- Legacy-only 与 shadow 模式继续保留既有 oracle 顺序；Central-only 模式在 host 侧生成相同的表现 rank map，失败回退仍能取得冻结排序结果。
- `PendingSoundEvent` 在逻辑 tick 后复制为有序值事件，`LateUpdate` 每个可见帧统一批量派发；音频 sink 不参与 battle checksum。

表现层仍然是 Unity-native。上述改动没有把 `Transform`、Sprite、Material、Mesh 或 AudioSource 提升为战斗逻辑真值，也没有增加服务器、Socket 或网络库实现。

## 2. 确定性与不可变性证据

聚焦测试验证了以下合同：

- 原始发布帧的命令和排序标志保持未物化；冻结 host 帧才变为已物化。
- host 对冻结帧的资源解析、排序和 command build 不反写原始逻辑帧。
- 同一 host 周期存在多个逻辑帧时只呈现最新一帧。
- 有表现与无表现运行相同输入 journal 时，逐 tick lockstep checksum 一致。
- 声音事件保持 tick 内顺序，批量派发后不会残留到下一帧。
- `BattlePresentationPhaseDiagnostics` 使用 active/completed 双缓冲；下一逻辑 tick 不会擦除上一完整 host 样本。

fresh EditMode job：`f9a9e42b0c714791b7ea08a6a3da9b8d`，262/262 PASS，覆盖中央最新帧物化、生产压力工具、声音派发和 lockstep session。

## 3. 1000 AI 测量

报告：`Temp/NTSD_ProductionEntityStress.combat1000.u2-instrumented-b-20260811.json`

- 状态：`StoppedCleanly`；180 个正式逻辑 tick、210 个 Unity 可见帧。
- 逻辑 tick Avg/P95/Max：25.710 / 36.749 / 52.435 ms。
- Unity 可见帧 Avg/P95/Max：50.532 / 72.075 / 1046.607 ms。
- 最终 lockstep hash：`a13929d82b19c54e871522a1921f658ddfa88a7e7bc8655149d76006e1c504e1`，与 U0 Combat1000 基线相同。
- 逻辑 tick、driver update、表现发布三个受控 managed-memory 作用域均为 0 B；正式窗口内 Gen0/Gen1/Gen2 collection 均为 0。
- Unity Profiler 的整帧 `GC Allocated In Frame` 仍包含 Editor/Profiler/外围 PlayerLoop 分配；当前运行时不能把它作为受支持的 player-loop 硬门，因此不得把受控作用域 0 B 扩大为“整个 Unity Editor 帧绝对 0 B”。完整整帧门禁留给 U9。

表现阶段归因：

- `RenderDispatch/PresentationPublishTotal` Avg/P95：8.857 / 12.864 ms。
- `BeginFrame/BuildCommands` Avg/P95：2.655 / 3.848 ms。
- `BeginFrame/SortEntities` Avg/P95：2.263 / 3.217 ms。
- 逻辑快照 `BeginFrameTotal` Avg/P95：1.139 / 1.738 ms。
- 逻辑 pass 内 `RenderDispatch` Avg/P95：1.147 / 1.747 ms。

这些数据证明 U2 已把重型表现工作从逻辑 pass 中分离并可独立归因，但没有证明总帧性能已经提高到 30 FPS。当前 Unity 可见帧 P95 仍超过 33.333 ms；该门禁只能在 U3～U8 完成后由 U9 判定。

## 4. 编译与完整自检

- fresh Unity scripts refresh/compile：成功，Editor ready，未发现脚本编译错误。
- 完整 `BattleRuntimeSelfCheck`：`2026-08-11 12:24:42` fresh PASS。
- `git diff --check`：目标脚本和计划文档无 whitespace error；仅报告工作树的既有 LF/CRLF 提示。
- T8 默认 `stage.dat` 部署和 Android 真机仍排除。

## 5. 阶段结论

U2 的逻辑发布、host 物化、最新帧策略、音频派发、checksum 不变性和受控作用域零分配均已有新鲜证据，可以进入 U3。U3 只建立只读 ECS shadow：旧 runtime 仍是唯一 canonical writer，shadow 不得反写或改变战斗结果。
