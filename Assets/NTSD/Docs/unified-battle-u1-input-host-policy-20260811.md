# U1 Canonical Input 与 Host Policy 验收（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 阶段：U1
> 结论：单机、手动回放和未来网络模式已共享同一离散输入边界；U1 完成，允许进入 U2，但没有实现服务器业务或宣称 U9 性能目标完成。

## 1. Canonical owner 与模式边界

- 每个逻辑 tick 的唯一输入载体是 `FrameInputSet`；玩家身份、目标 tick、held、pressed 和 released 都在该边界内冻结。
- `LocalFrameInputProvider` 只把 Unity 输入意图转换成目标逻辑帧的数据，不直接推进世界，也不把渲染帧轮询当作战斗真值。
- `BattleLockstepSession` 与 `StrictDelayedInputBuffer` 负责显式帧输入的提交、完整性、幂等重复、冲突重复和锁定边界；相同玩家/帧的相同包幂等，内容冲突按协议错误拒绝。
- `LockstepReplayJournal` 只记录 canonical frame；导出不保留调用方可变数组引用。
- `SimulationTickDriver` 仍是三个模式共享的唯一逻辑 tick 入口，没有新增第二套战斗循环。

## 2. Host Policy 修正

`OfflineLocalTickPolicy` 原候选实现会在一次 Unity `Update` 内按 `maxCatchUpTicksPerFrame` 连续执行最多 4 个完整逻辑 tick。该行为会让单机场景也进入网络追帧式突发负载，与本计划的单机边界冲突。

本阶段固定为：

- `OfflineLocal` 普通自动驱动每个 Unity `Update` 最多执行一个逻辑 tick；
- 尚未消费的 accumulator 积压保留并受 `maxBacklogTicks` 上限约束，后续 Unity `Update` 再继续；
- 每个本地自动 tick 都发布表现，不再因“同一 Update 中还有后续 tick”而抑制表现；
- `ManualReplay` 和 `NetworkLockstep` 不读取墙钟，也不会由 Unity `Update` 自动消费输入；它们只能经显式 transaction 推进；
- `maxCatchUpTicksPerFrame` 保留给显式追帧/吞吐诊断边界，不再支配普通单机自动驱动。

这项修正不会让低于 30 Hz 的单 tick 自动变快；它只消除单机一帧内无条件连跑多个完整 tick 的错误宿主策略。U9 仍需把单 tick 和 Unity 可见帧 P95 分别压到正式门限内。

## 3. 可重放与输入协议证据

聚焦 EditMode job：`f3b950d710974934bf77f53999ba58de`，23/23 PASS，覆盖：

- local held/pressed/released 与既有交叉动作映射；
- canonical player identity、frame key、容量与完整玩家集合；
- future frame 乱序到达；
- 相同重复幂等、冲突重复拒绝；
- reset 后 late boundary 和 journal cursor；
- warmed 256 帧 strict buffer steady state 0 managed allocation；
- OfflineLocal 每个 Update 最多一个自动 tick；
- ManualReplay/NetworkLockstep 不消费墙钟；
- 同一三帧 journal 在新建 `SimulationWorld` 与 driver 上重放，逐 tick `LastFrameChecksumValue` 完全一致。

测试发现问题也已关闭：新增 host-policy 测试最初只进入 Bee 依赖图、没有进入 `Assembly-CSharp-Editor.rsp`，因此 Test Runner 返回 0 条。强制 Unity 资产刷新后确认测试源进入编译响应文件，再执行得到 2/2 PASS；没有把未编译测试计为通过。

## 4. 编译、自检与权威边界

- fresh Unity script compile：0 error；新增测试已实际进入 `Assembly-CSharp-Editor`。
- 完整 `BattleRuntimeSelfCheck`：`2026-08-11 10:48:39` fresh PASS，结果文件 `Temp/NTSD_BattleRuntimeSelfCheck.result`。
- Authority400 的 seed、roster、输入 journal、RNG、slot、rest、stats 与 event oracle 沿用 U0 固定夹具；U1 新增的本地 journal 重放测试证明 Unity 内部同输入逐 tick checksum 稳定。
- Production Authority400 仍在 tick 0 被用户已确认的 Unity DAT 适配 manifest 差异阻断；U0 的 authority-DAT full/full 诊断是 6/6 `equal-diagnostic`，不是 production parity certificate。本阶段没有修改 DAT、战斗 pass 或结果规则，也不把该诊断扩大为正式证书。
- T8 默认 `stage.dat` 部署与 Android 真机仍排除。

## 5. 阶段结论

U1 的输入 owner、模式边界、单机自动 tick 上限、回放一致性和未来网络帧输入合同均已由 fresh 编译、23 项聚焦测试和完整 self-check 覆盖。服务器业务、Socket、ACK、Jitter Buffer、房间、登录与重连均未实现；下一阶段只进入 U2 表现发布边界。
