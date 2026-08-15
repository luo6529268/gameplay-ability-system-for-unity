# U5 Cpoint Writer 所有权迁移（2026-08-12）

## 1. 本批结论

本批把 `PreInteraction` 中真实 cpoint 状态写入收敛到每个 `SimulationWorld` 独立持有的 `BattleCpointWriter`：

- kind1 的 reciprocal link 校验、decrease/escape、动作选择、即时 action、持有位置/速度/伤害/统计同步与投掷分支；
- kind2 的无效 link 回退；
- held cpoint 的连续位置、动作、方向和 wait 同步；
- cpoint 投掷时当前实体及其 owned object 的 transform/速度传播。

`LF2Entity.RunCpointCheckStep10`、`RunCpointMismatchTailStep10` 与 `RunWeaponSyncHeldStep10` 现在只保留对象适配入口，生产写入由 world-owned writer 执行；旧 character 专用重复写入已移除。没有新增 partial class。

这仍是 U5 writer 所有权迁移，不是 U6 的最终 SoA 存储迁移。当前战斗字段仍位于 `LF2Entity` / `NTSDEntityRuntime`，Unity 对象仍是兼容 adapter。

## 2. 权威依据与边界

唯一战斗逻辑权威：

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs` 的 Step10 / PreInteraction 调用顺序；
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Interaction\CPointRuntime.cs` 的 kind1、kind2、held sync、action、throw、injury 与 invalid-link 行为；
- 上述调用链继续使用原 runtime slot 升序、同 tick reciprocal link、捕获快照、RNG 与 frame 可见边界。

本批没有使用 tick-end shadow 作为 link 真值，也没有引入额外 state/frame/team/距离过滤。为测试兼容保留的两个 protected helper 不再位于生产 pass 调用链中。

## 3. 新鲜验证证据

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore /m:1 /v:q`：0 error，76 个既有 warning；
- cpoint、held、hit 与生命周期联合 EditMode job `07b114009a07489d951da85b13df0efb`：201/201 PASS，0 failed、0 skipped；
- 完整 `BattleRuntimeSelfCheck`：`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-08-12 18:09:43` fresh 返回 `PASS`；
- Authority400 full/full：
  - Unity：`Temp/NTSDParity/u5-cpoint-writer-unity-authority-dat-diagnostic-20260812.jsonl`；
  - C#：`Temp/NTSDParity/u5-cpoint-writer-authority-20260812.jsonl`；
  - compare：`Temp/NTSDParity/u5-cpoint-writer-compare-authority-dat-diagnostic-20260812.json`；
  - 显式允许 authority-DAT 诊断夹具后为 6/6 `equal-diagnostic`、`firstDifference=null`、manifest 相同、`fixed-world-camera`。这是诊断证据，不是 production certificate；
- 1000 AI、30 warmup + 60 sample：`Temp/NTSD_ProductionEntityStress.u5-cpoint-writer-1000ai-60-20260812.json`：
  - average/P95/max：23.0081/28.6700/32.4747 ms/tick；
  - 60/60 正式逻辑 tick 为 0 B，Gen0/1/2 collection 均为 0；
  - final lockstep overall hash `7181ea5a2c0a993536eb0aca6ae9a756368647cc334bd64a158bccdb37e829a9`，与迁移前同 seed 报告一致；
  - 状态为 `StoppedCleanly`，临时 AI、命中、碰撞、表现和声音开关全部恢复，failure 为空。

短样本性能相较上一批约有正常运行波动，不能据此声明性能提升；行为 hash、正式 tick 零 GC 和 30 Hz 单 tick 预算没有出现材料性回归。外层最大 backlog 仍为 7 tick、丢弃 backlog 28 tick，因此 U9 尚未关闭。

## 4. 剩余 U5 工作

- 标准角色伤害以及武器/特殊攻击/type3 对象伤害的完整 canonical writer；kind16 与 alternate damage 已迁入 `BattleDamageWriter`，见 `Docs/unified-battle-u5-damage-writer-20260812.md`；
- opoint 分段结构命令 writer；
- spawn/destroy/free/unregister/generation 的统一 structural writer；
- `LF2Entity` 字段向 BattleKernel 唯一真值的最终迁移属于 U6，不在本批宣称完成。
