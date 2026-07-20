# 全量差异盘点：生命周期 / 对象池 / 战斗表现（2026-07-18）

## 修复后核销（2026-07-18 16:46，覆盖本报告原始只读状态）

本报告原 `LP-01..04` 表是修复前快照；当前状态为：

| ID | 当前状态 | fresh 覆盖 |
|---|---|---|
| `LP-01` | 代码已修 / self-check verified | generic held release 写 `ReleaseTick` |
| `LP-02` | 代码已修 / self-check verified | same-Z runtime-slot tie-break |
| `LP-03` | **代码已修 / fresh self-check verified** | typed/generic formal throw 均保持 `Zz=0`，不再额外抬层 |
| `LP-04` | 代码已修 / self-check verified | entity/shadow negative `HitStop` gates |
| `LP-05` | **代码已修 / fresh self-check verified** | formal release 保留 target/holder indices，consume 写 `0/0`，force-clear 完整清理 |

fresh 证据为 source `16:44:31.210` < Unity DLL `16:45:52.868` < result `16:46:29.080` **PASS**；`dotnet build` **0 errors / 18 warnings**。此处只关闭脚本定义的生命周期和排序契约；4 组 Play Mode 仍由用户验证，不能据此宣称真实场景或完整逐帧 certificate 已完成。

## 1. 审计边界

- 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release_C#`。
- Unity 可保留 `GameObject`、`Transform`、`SpriteRenderer`、`SortingGroup`、对象池与渲染回调，但不得改变 runtime 真值、战斗时序和可观察结果。
- 本报告只读盘点当前源码，不修改生产代码。
- 包含：实体 reset/reuse/free、runtime transient/link、`ReleaseTick`、held pose、位置/速度/朝向投影、实体/武器/阴影可见性与排序。
- 不包含：raw DAT 表示差异、T8 默认 `stage.dat` 部署、普通 HUD、音频资源部署。
- 用户已明确要求 Unity 使用固定世界相机；因此不恢复 C# 的角色驱动 camera 表现链。该项记为批准的 Unity adapter，不计待修复差异。

## 2. 当前盘点结论

| 分类 | 数量 | 说明 |
|---|---:|---|
| 当前源码可确认差异 | 4 | `LP-01..04` |
| 已核对等价/已补齐 | 5 组 | `EQ-01..05` |
| 需要 Play Mode 才能关闭 | 4 组 | `PV-01..04` |
| 批准的 Unity adapter | 2 组 | fixed-world camera、Unity renderer/pool host |

这里的“4 个差异”只表示本报告边界内、已由双方当前源码证明的差异，不代表其他审计分区已经完成，也不代表完整战斗差异总数已经冻结。

## 3. 当前确认差异

### LP-01：generic held shell 的正式释放没有写 `ReleaseTick`

- 权威 C#：`BattleCore/Interaction/WeaponRuntime.cs:169-212,287-303`。无论 held 实体的 CLR 类型是什么，只要 wpoint `Dvx != 0` 进入正式投掷，或 wpoint `Kind == 3` 进入随机释放，都会调用 `ReleaseHeldWeaponRuntime`，写 `held.ReleaseTick = currentTick`。
- Unity：`Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:391-415`。generic `LF2Entity` 分支的 `ThrowHeldObject` 和 `DropRandomly` 只调用 `ClearLinks`；只有 `held is LF2WeaponBase` 时才经 `ReleaseHeldWeaponRuntimeInternal(..., stampReleaseTick: true)` 写 tick。
- 触发前置：holder 持有一个不是 `LF2WeaponBase` CLR 壳、但当前 DAT 类型按武器/held object 参加 step12 的对象；wpoint `dvx != 0` 或 `kind == 3`。
- 预期：链接清除与 `ReleaseTick=current tick` 同时发生。
- 实际：generic shell 链接清除，但 `ReleaseTick` 保持 `-1` 或旧值。
- 可观察影响：同 tick 释放保护、trace/checksum 和后续 link/attack 过滤可能分叉。Naruto 手持技能对象若走 shared-DAT/转换壳，属于该风险的真实 Play 验证对象。
- 状态：**确认差异；未修复；未 Play Mode 验证。**

### LP-02：同 Z 实体缺少 runtime-slot 稳定排序

- 权威 C#：`src/Host/SdlBattleRenderer.cs:476-497`。绘制顺序先按 `ZInt`，同 Z 时按对象 slot 升序稳定排序。
- Unity：`Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:4108-4113` 与 `LF2Sprite.cs:151-158`。普通对象只写 `sortingOrder = ZInt + Zz`（以及特定 overlay 偏移），没有把 runtime slot 编入同 Z tie-break。
- 触发前置：两个或以上实体处于同一 `ZInt` 且没有不同的 cover/overlay 偏移；分身、贴身武器、同线攻击都常见。
- 预期：slot 较小者先画、slot 较大者后画。
- 实际：相同 `sortingOrder` 下交给 Unity renderer/hierarchy/material 决定，不能保证权威 slot 顺序。
- 可观察影响：角色、分身、武器或特效的前后遮挡不稳定；直接对应用户报告的螺旋丸层级问题。
- 状态：**确认差异；未修复；未 Play Mode 验证。**

### LP-03：正式投掷额外写 Unity-only `Zz=1`

- 权威 C#：`BattleCore/Interaction/WeaponRuntime.cs:169-212`。step12 已通过 held `ZInt/YInt` 的 cover `+/-1` 完成层级位置，正式投掷只写 frame、速度、owner/link 与 `ReleaseTick`，绘制顺序仍按最终 `ZInt/slot`。
- Unity：`Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:77-98,391-402`。真实 weapon 与 generic held 正式投掷都会额外写 `PS.zz/Runtime.Zz = 1`；`LF2Entity.GetRenderSortingOrder` 又把 `Zz` 加入 sorting order。
- 触发前置：持有对象通过 wpoint `dvx != 0` 正式投掷。
- 预期：release 后排序由已有 `ZInt` 和 slot 决定。
- 实际：Unity 比权威再高一个 sorting order。
- 可观察影响：投掷起始帧/穿过角色时前后层级不同；与手持武器释放瞬间的视觉跳层有关。
- 状态：**确认差异；未修复；未 Play Mode 验证。**

### LP-04：实体和阴影缺少权威 `HitStop` 闪烁/隐藏 gate

- 权威 C#：`src/Host/SdlBattleRenderer.cs:519-548`。
  - 阴影在 `HitStop <= -70` 或 `abs(HitStop) % 4 >= 2` 时不绘制。
  - 实体在 `HitStop <= -25` 或 `abs(HitStop) % 4 >= 2` 时不绘制。
- Unity：`Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:416-448` 与 `LF2ObjectRenderer.cs:206-225`。阴影只检查 state、negative link、oid223/224；实体只检查 frame/pic/资源与 presentation suppression，均未检查 `HitStop`。
- 触发前置：实体进入负 `HitStop` 的闪烁/隐藏区间。
- 预期：实体和阴影按不同阈值及四拍相位隐藏。
- 实际：Unity 保持绘制。
- 可观察影响：受击、消失或特殊状态期间多显示角色/对象或阴影，表现与权威不一致。
- 状态：**确认差异；未修复；未 Play Mode 验证。**

## 4. 已核对等价或当前已补齐

### EQ-01：runtime reset 核心默认值

- 权威：`NtsdEntityRuntime.cs` 的 `FrameWaitCounter=0`、`ReleaseTick=-1`、`HolderCopy=99`、`KnockbackVx/Vy/Vz=0.1`、transient `Mp=0/Mp2..4=1000`。
- Unity：`Assets/NTSD/Scripts/Simulation/NtsdEntityRuntime.cs:34,77,115,120-122,157-160,320-480` 已存在相同字段与 reset 值。
- 结论：旧审计中“缺 `FrameWaitCounter` / `ReleaseTick` storage / holder-copy 与 knockback reset 不同”的结论对当前源码已经失效。

### EQ-02：whole-world reset 会清 registry/slot/pending/pool runtime 状态

- 权威：`BattleCore/Simulation/SimulationWorld.Passes.cs:13-97` 重置全实体、cooldown、runtime/flow。
- Unity：`SimulationWorld.Registry.partial.cs:138-209` 的 `ResetRuntimeState -> ResetRegisteredObjects` 会清 pending 队列、bucket、`_runtimeSlotUsed`、raw runtime/rest，并 reset 实体和显示。
- 结论：旧 `LC-03`“只 reset BattleRuntimeState，不清 registry”的结论对当前源码已经失效。

### EQ-03：held pose 的整数挂点公式

- 权威：`WeaponRuntime.cs:112-154` 使用 holder `XInt/YInt/ZInt`、holder frame center/wpoint、held frame center/wpoint，并按 cover 对 `ZInt/YInt` 做 `+1/-1`。
- Unity：`LF2WeaponHeldStateResolver.cs:317-367` 使用相同整数输入和相同左右/cover 公式，最后同步整数位置。
- 结论：当前静态公式未发现差异；这不代替真实 Naruto 螺旋丸 Play Mode 的跟手验收。

### EQ-04：真实 `LF2WeaponBase` 的 `ReleaseTick` writers

- Unity `LF2WeaponReleaseFlowResolver.cs:23-35` 已为正式 throw/kind3/consume 路径提供当前 tick 写入；damaged drop/force clear 不写，和权威路径区分一致。
- 剩余差异仅是 `LP-01` 的 generic shell 分支，不能再概括成“Unity 完全缺少 ReleaseTick”。

### EQ-05：阴影位置不再从角色 Transform 增量继承

- Unity `LF2Entity.UpdateShadow:416-448` 每次直接从 runtime `XInt/ZInt` 计算 world position；`LF2ObjectRenderer.UpdatePosition:255-266` 先刷新实体，再独立刷新阴影。
- 结论：用户此前“角色移动导致其他对象阴影一起移动”的直接父子/增量更新问题，在当前静态实现中未重现。仍需 `PV-04` 做真实场景回归。

## 5. 必须保留为 Play Mode 未验证

### PV-01：Naruto 防前跳螺旋丸完整链

- 输入：Naruto 出现后执行防前跳，观察 oid434 生成、手持、移动、攻击与释放。
- 必看：runtime slot/link/holder 双向字段、held frame、`XInt/YInt/ZInt`、角色与螺旋丸 sorting order、攻击键是否进入螺旋丸 frame/itr 而不是普通武器攻击。
- 当前结论：held pose 静态公式等价，但 `LP-01/02` 仍可能影响该场景，**未做本轮 Play Mode 验证**。

### PV-02：Naruto 奔跑防跳后续招与命中链

- 必看：第一段命中后的 next/frame/opoint/owner/link、下一招是否按同 tick/pass 边界发布，命中对象是否保持正确关系与层级。
- 当前结论：本报告没有用 Play Mode 复现，不能沿用旧 PASS 作为当前 freshness 证据。

### PV-03：投掷武器首次命中与 rest 窗口

- 必看：release tick、throw 起始 sorting、首次命中、ARest/VRest、后续每 tick candidate 与 HP。
- 当前结论：`LP-01/03` 对 generic shell 或投掷显示有直接影响；未做本轮 Play Mode 验证。

### PV-04：实体/武器/掉落物阴影独立性

- 场景中固定一个掉落物/武器，移动角色左右，逐 tick 记录实体 runtime `XInt/ZInt`、shadow world position、camera/render offset。
- 当前结论：静态链已独立，但 `LP-04` 的 hit-stop shadow gate 仍不等价；本轮未做真实场景验证。

## 6. Unity adapter 与不计差异项

1. `SimulationWorld.StageRender.partial.cs:120-137` 每个 preframe 把 `_cameraX/_cameraVel` 与 entity `RenderOffsetX` 清零，符合用户指定的 fixed-world camera。C# renderer 的 camera/perspective 链不恢复。
2. `LF2ObjectPool` 的 GameObject 池与 `LF2ReferencePool` 的 CLR 对象池属于 Unity host 适配。当前没有发现其 reset 后残留 runtime 真值的新差异。池预热产生层级对象本身不是战斗规则差异；若 `NTSD_Battle` 仍出现不必要的六个场景管理对象，应单独用 Play hierarchy 记录创建者和 `.Instance` 路径，不能从权威固定 slot 模型直接推断。

## 7. 后续修复顺序（清单冻结后执行）

1. `LP-01`：统一 generic/typed held release tick 契约。
2. `LP-02/03`：按 `ZInt + slot tie-break` 设计 Unity sorting adapter，并移除或证明 `Zz` 的权威等价来源。
3. `LP-04`：把实体与阴影的 `HitStop` gate 放入表现投影，不反写 runtime。
4. 运行 `PV-01..04`，逐项记录 scene、角色、输入、tick、slot/link/frame/position/sorting 与实际结果。
5. 只有编译、focused self-check、fresh full self-check 和相应 Play Mode 全部通过后，才能把该项标为“已对齐”。
