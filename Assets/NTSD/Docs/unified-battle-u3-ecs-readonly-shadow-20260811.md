# U3 ECS World 与只读 Shadow 验收（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 阶段：U3
> 结论：NTSD 专用混合 ECS 的固定容量存储、身份和查询基础已经建立；旧 `SimulationWorld` 仍是唯一 canonical writer，ECS shadow 默认关闭且不具备反写路径。U3 完成，允许进入 U4；U9 的 1000 AI / 30 FPS 门禁尚未完成。

## 1. 本阶段边界

U3 只建立迁移地基，不迁移任何战斗规则的写入所有权：

- `BattleEcsCapacityProfile` 在 world 建立时封印 slot 和 sparse 容量；正式 tick 内不会自动扩容。
- `BattleSlotBitSet` 提供按 runtime slot 升序的权威查询；`BattleSparseSet<T>` 只用于非普遍存在的数据，其 swap-remove dense 顺序明确不是权威顺序。
- `BattleEcsWorld` 以按 slot 直接索引的 SoA store 保存 Identity、Motion、Frame、Vital、Input 和 Links，并记录 claimed、active、pending、dormant、对象种类、body、itr、AI 与 holder membership。
- `EntityHandle` 的等价合同由 slot 与 generation 共同约束；测试覆盖 slot 复用后旧 generation 不再匹配。
- `BattleRuntimeFingerprint` 覆盖当前 `NTSDEntityRuntime` 的公开战斗标量、输入历史、方向、生命周期抑制字段和 pending destroy 状态，用于发现尚未显式进入 SoA store 的字段分叉。
- `BattleEcsShadowModule` 的默认模式是 `Disabled`。`Compare` 模式只从 canonical runtime 复制并逐槽比较；没有 shadow-to-runtime 写回 API。
- tick 后 shadow 诊断异常被记录在自身计数器中，不得掩盖 canonical battle 的异常或改变 tick 结果。

本阶段没有实现服务器、Socket、ACK、Jitter Buffer、房间、登录、重连或网络库，也没有新增 `partial` 类型/文件或全局可变 session static。

## 2. 正确性与确定性证据

fresh ECS 聚焦测试 job：`9c40ed36a4054b8484fe5868026704a4`，8/8 PASS。覆盖：

- bitset 的权威 slot 升序；
- sparse set 的固定容量和 swap-remove 查找正确性；
- core store、membership 与完整 runtime fingerprint；
- dormant 与 pending-destroy 槽不会进入 active-pass bitset；
- canonical 字段被修改后 shadow 能定位 mismatch，重新 capture 后恢复一致；
- generation reuse 拒绝陈旧 handle；
- tick hook 默认关闭，只有显式 Compare 才 capture；
- 预热后单实体 capture/validate 0 B；
- `Extended1000` 的 1000 个 active slot 顺序、内容和 membership 全部一致，预热后的 capture/validate managed allocation 为 0 B。

交叉回归 job：`a20d07ad2c24449b825bba5adac40b32`，14/14 PASS，覆盖：

- U3 ECS shadow；
- lockstep checksum；
- parity structural witness；
- dynamic runtime slot / late-opoint 容量与最低合法槽选择。

完整 `BattleRuntimeSelfCheck`：`2026-08-11 13:24:00` fresh PASS。

Authority400 对照：

- Unity trace：`Temp/NTSDParity/u3-authority400-unity-authority-dat-diagnostic.jsonl`；
- compare：`Temp/NTSDParity/u3-authority400-compare-authority-dat-diagnostic.json`；
- 6/6 tick 为 `equal-diagnostic`，`firstDifference=null`，400 个槽和 full domains 一致。

该对照仍是 authority-DAT diagnostic，而不是 production manifest certification：Unity 使用用户已确认的 DAT 适配方式，tick 0 的生产 manifest 与权威工程不同。U3 没有把该已知资源前置差异改写成战斗逻辑差异，也没有据此声称生产 DAT 完全相同。

## 3. 编译、分配与性能边界

- Unity fresh refresh/compile：脚本编译 0 error。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 的 `dotnet build --no-restore`：均为 0 error。
- `git diff --check`：U3 目标路径无 whitespace error；只有现有 LF/CRLF 转换提示。
- `Extended1000` 直接 shadow 测试在预热后 capture + validate 为 0 B，并验证 1000 个槽的完整顺序和内容。
- 默认 `Disabled` 模式不会执行逐槽 capture/compare；因此生产默认路径没有新增 U3 扫描。

这些证据只关闭 U3 的“数据世界与只读 shadow”门。它们不等于真实 1000 AI 已达到 30 FPS，也不替代 U9 的 Idle/Move/Dispersed/Combat/Concentrated、60 秒窗口、P95、整帧 GC、cleanup 和 Windows Player 验收。

## 4. U4 入口合同

U4 从低风险、纯数值、高频 pass 开始。每个切片必须遵守：

1. 先从权威 C# 调用链确认字段、顺序、可见边界和副作用；
2. 同一字段在同一阶段只有一个 canonical writer；
3. 旧实现作为只读 oracle，逐 tick 比较全部受影响字段；
4. writer 切换只能发生在 world reset/合法启动边界，不能在运行中的 tick 热切换；
5. checksum、RNG、slot、事件和表现发布结果保持一致；
6. 任何 mismatch、capacity fault、分配或性能负收益都会阻止默认晋升。

第一批候选顺序保持为 cooldown、基础 frame/motion/bounds，再进入 CharacterInput facts 与 AI decision。复杂 interaction、hit、rest 和生命周期仍留在 U5。

## 5. 阶段结论

U3 已建立可验证、可关闭、不可反写的 ECS shadow 数据地基。canonical battle 仍由旧 runtime 写入，结果敏感逻辑尚未迁移，因此本结论是“U3 架构阶段完成”，不是“ECS 迁移完成”或“1000 AI 性能完成”。下一阶段为 U4 的逐切片 canonical writer 迁移。
