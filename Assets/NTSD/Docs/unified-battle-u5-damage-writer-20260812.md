# U5 Damage Writer 完整所有权迁移（2026-08-12）

## 1. 本批结论

本任务新增由每个 `SimulationWorld` 独立持有的 `BattleDamageWriter`，并把所有可达 damage 事务从对象 resolver 收敛到该 writer：

- kind16：injury 修正、HP/HPBound、kill/combo/damage 统计、声音、frame、AttackingCounter、vRest，以及有效 held target 的 link 释放、双向 rest、RNG frame、Vy 和快照同步；
- alternate damage：主声音、injury 修正与减伤、HP/HPBound、kill/combo/damage 统计、fall/attacking/hitstate/hitcount、frame delay、defend、地面/空中击退、aRest/vRest、active holder delay，以及 state1002/2000/3000 尾链。
- 标准角色伤害：标准 vital/stat、fall/倒地帧、击退、OID100 尾链、受击声音、aRest/vRest、caught hurt frame、active holder、state1002/3000 与 hit record 归属；
- 武器伤害：轻/重/投掷/饮料分支、耐久、relation、fall、飞行/重武器击退、随机帧、held-pair vRest、攻击者响应与 kind0 尾链；
- 特殊攻击/type3 对象伤害：kind9、kind0 object-hurt、relation/holder-copy、motion、state3005/3006、身份替换、PP/声音与 hit record 尾链。

`LF2CharacterHitResolver`、`LF2CharacterDatHitResolver`、`LF2Weapon` 与 `LF2SpecialAttack` 现在只在各自权威候选位置做对象适配并委托 world-owned writer；旧的重复 kind16 release helper 已移除，`LF2AlternateDamageResolver` 只保留决定是否进入 alternate 分支的纯判定。没有新增 partial class。

这是 U5 damage writer 所有权迁移完成，不是 U6 的最终 SoA 存储迁移。当前战斗字段仍位于 `LF2Entity` / `NTSDEntityRuntime`，对象 consumer 仍作为兼容 adapter 存在；这些真值存储与对象热循环将在 U6 处理。

## 2. 权威依据与原子边界

唯一战斗逻辑权威：

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs` 的 character/object hit pass 顺序；
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Interaction\HitResolve.cs` 的 kind16、alternate damage、held release、统计、rest、RNG 与状态尾链。

迁移以完整事务为边界，没有把扣血、统计、动作、速度、rest、声音或 RNG 拆成不同真值来源。writer 仍在原 consumer 的同一候选位置同步执行，不改变候选升序、pair preprocess、dispatch、OID300 abort 或 opoint/lifecycle 可见边界。

## 3. 新鲜验证证据

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore /m:1 /v:q`：退出码 0；`git diff --check` 无空白错误；
- 新增零分配定向测试 `WarmedAlternateDamageWriter_AllocatesNoManagedMemory`：Unity EditMode job `0e16b0e8401340d8aa53bc0c31e3e8ef`，1/1 PASS；预热后 512 次 writer 调用为 `0 B`；
- U5 六组联合回归：Unity EditMode job `c773f7ab85304d21971c4801a40fc078`，202/202 PASS，0 failed、0 skipped；
- 完整 `BattleRuntimeSelfCheck`：`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-08-12 18:40:57` fresh 返回 `PASS`；
- Authority400 full/full：
  - Unity：`Temp/NTSDParity/u5-damage-writer-unity-authority-dat-diagnostic-20260812.jsonl`；
  - C#：`Temp/NTSDParity/u5-damage-writer-authority-20260812.jsonl`；
  - compare：`Temp/NTSDParity/u5-damage-writer-compare-authority-dat-diagnostic-20260812.json`；
  - 显式允许 authority-DAT 诊断夹具后为 6/6 `equal-diagnostic`、`firstDifference=null`、manifest 相同、`fixed-world-camera`。这是诊断证据，不是 production certificate；
- 1000 AI、30 warmup + 60 sample：`Temp/NTSD_ProductionEntityStress.u5-damage-writer-1000ai-60-20260812.json`：
  - average/P95/max：23.3549/30.0303/35.8729 ms/tick；
  - 60/60 正式逻辑 tick 为 `0 B`，Gen0/1/2 collection 均为 0；
  - final lockstep overall hash `7181ea5a2c0a993536eb0aca6ae9a756368647cc334bd64a158bccdb37e829a9`，与迁移前同 seed 报告一致；
  - 状态为 `StoppedCleanly`，failure 为空，所有临时开关恢复，teardown 后 active/world/claimed/root 均为 0。

短样本 average/P95/max 相比 cpoint 批次有正常运行波动，且 max 曾超过 33.333 ms；外层最大 backlog 为 7、丢弃 backlog 为 29。因此本批只证明行为、零 GC 和迁移边界没有材料性回归，不宣称性能改善，也不关闭 U9。

## 4. 最终收口证据与边界

- 标准角色、kind16、alternate、武器及特殊攻击/type3 damage 均已由同一 world-owned writer 持有；
- U5 最终联合 EditMode job `b55c2edd04964be7b784f7bec65ab0f5`：220/220 PASS；
- 完整 `BattleRuntimeSelfCheck` 于 `2026-08-12 20:34:10` fresh PASS；
- 最终 Authority400 full/full 仍为 6/6 `equal-diagnostic`、`firstDifference=null`；该证据仍是诊断夹具结果，不是 production certificate；
- 最终 1000 AI 短样本见 `Temp/NTSD_ProductionEntityStress.u5-battle-results-writer-1000ai-60-20260812.json`：逻辑 tick allocation 为 0 B、Gen0/1/2 collection 为 0、cleanup restored；
- `LF2Entity` 字段向 BattleKernel/SoA 唯一真值的最终迁移属于 U6，不在本记录中扩大宣称。
