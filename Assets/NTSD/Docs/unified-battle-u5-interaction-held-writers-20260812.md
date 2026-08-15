# U5 Interaction / Held Writer 所有权迁移（2026-08-12）

## 1. 本批结论

本批把两组真实交互写入从分散的对象 resolver / 静态辅助类收敛到每个 `SimulationWorld` 独立持有的普通实例：

- `BattleInteractionWriter`：负责命中候选 kind `1/3` 的抓取与 kind `2/7` 的拾取、link、holder、target、pickup 写入；
- `BattleHeldObjectWriter`：负责 `HeldObjectProcessAll` 中的持有帧/位置同步、受击掉落、投掷、随机掉落与 link 清理；
- `SimulationQueryAndLinkModule` 仍按 runtime slot 升序遍历，只把单对 holder/held 的状态写入委托给 world writer；
- `LF2CharacterInteractionResolver`、`LF2CharacterDatInteractionResolver`、`LF2WeaponInteractionResolver` 与 `LF2SpecialAttack` 只保留具体对象适配和 dispatch，不再各自复制 kind `1/3/2/7` 的状态写入；
- 原静态 `LF2HeldObjectRuntime` 已移除，没有新增 partial class。

这是 U5 的 writer 所有权迁移，不是 U6 的最终 SoA 存储迁移。当前字段仍存放在 `LF2Entity` / `NTSDEntityRuntime`，Unity 对象仍是兼容 adapter；只有到 U6 把 BattleKernel 数据存储变成唯一真值并移除对象式热循环后，才能宣称该域完成最终数据化。

## 2. 权威依据

唯一战斗逻辑权威仍为：

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Interaction\HitResolve.cs`
  - `ApplyGrabCandidate`；
  - `ApplyPickupCandidate`；
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Interaction\WeaponRuntime.cs`
  - `RunHeldObjectStep12`；
  - `RunHeldObjectStep12ForPair`；
  - `ReleaseHeldWeaponRuntime`；
  - `ReleaseHeldWeaponForConsume`；
  - `ClearReleasedHeldSlot`。

本批没有调整 C# 权威的 slot 顺序、frame 写入、RNG 调用顺序、release tick、link 保留/清理字段或同 tick 可见性。

## 3. 新鲜验证证据

- Unity 脚本定向导入完成；`dotnet build Assembly-CSharp-Editor.csproj --no-restore /m:1 /v:q`：`0 error`，76 个既有 warning；
- held/link/hit 相关联合 EditMode job `f96f9a461ece4cd48b2bed8f3d64abda`：209/209 PASS，0 failed、0 skipped，11.214 s；
- 完整 `BattleRuntimeSelfCheck`：`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-08-12 17:42:09` fresh 返回 `PASS`；
- Authority400 full/full 诊断：
  - Unity：`Temp/NTSDParity/u5-held-writer-unity-authority-dat-diagnostic-20260812.jsonl`；
  - C#：`Temp/NTSDParity/u5-held-writer-authority-20260812.jsonl`；
  - compare：`Temp/NTSDParity/u5-held-writer-compare-authority-dat-diagnostic-20260812.json`；
  - `equal-diagnostic`、6/6 tick、`firstDifference=null`、manifest 相同、`fixed-world-camera`。这是 authority-DAT 诊断证据，不是 production certificate；
- 1000 AI、30 warmup + 60 sample、正式表现和声音开启：`Temp/NTSD_ProductionEntityStress.u5-held-writer-1000ai-60-20260812.json`：
  - 平均 22.3180 ms/tick；
  - P95 29.1818 ms；
  - 最大 32.7218 ms；
  - 60/60 正式样本 tick 为 0 B，GC collection 0 次；
  - 最终 lockstep overall hash `7181ea5a...29a9` 与迁移前同 seed 报告完全相同；
  - teardown 全部恢复，active GameObject、world entity、claimed slot、pool active 均为 0，cleanup exception 为 0。

该短样本的单 tick average/P95/max 均在 30 Hz 的 33.333 ms 预算内，但外层运行仍记录最大 backlog 7 tick、丢弃 backlog 21 tick。因此它证明 writer 迁移没有破坏行为、零 GC 或单 tick 预算，不等于 U9 的长时间稳定 30 FPS 验收已经完成。

## 4. 后续状态

- cpoint kind1/kind2 的持续同步、动作、投掷、伤害和失效恢复已在后续批次迁到 world-owned `BattleCpointWriter`；完整证据见 `Docs/unified-battle-u5-cpoint-writer-20260812.md`；
- weapon 专用 `LF2WeaponHeldStateResolver` 仍是对象 adapter，U6 才迁移其剩余逻辑存储；
- character/object damage writer 虽已完成权威合同和只读计划核验，但 HP、统计、frame、速度、effect 等正式写入仍在对象 resolver；
- W05A～W05E 已覆盖 opoint 槽位、下一表现 tick、generation/ghost、单个/六个生成释放和 death cleanup 的零分配合同，但正式分段结构命令所有权尚未迁移；
- spawn/destroy/free/unregister/generation 的统一 structural writer 尚未完成；
- U5 仍未完成，不能进入 U6，也不能宣称 U0～U9 完成。
