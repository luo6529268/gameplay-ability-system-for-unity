# Partial → Resolver Cleanup Plan

## 进度更新（本 session）

**Resolver 文件：10/11 已建**（Catch/Action/Hit/CharInteraction/WeaponInteraction/WeaponHeldState/WeaponReleaseFlow 本 session 新建 + State/DamageState/WeaponFrameLogic 原有）。最后 1 个 `LF2CharacterWeaponLinkResolver` agent 运行中。

## 第二层破坏：3 个 LF2WeaponBase 方法丢失（partial 拆分误删）

`LF2WeaponBase.Interaction.partial.cs` / `LF2WeaponInteractionResolver.cs` 调用了这些但当前树里 0 定义。**权威 = C# 工程 git 历史**，全部在 commit `8101df55:LF2WeaponBase.cs` 完整定义过（`protected virtual`），恢复即可（非造逻辑）：

| 方法 | 8101df55 行 | 当前 defs | 处置 |
|---|---|---|---|
| `HandleWeaponKind3Stick` | :813 | 0 | 从 8101df55 恢复到 LF2WeaponBase（改 internal 供 resolver 调） |
| `HandlePreInteractionKind1` | :887 | 0 | 恢复 |
| `HandlePreInteractionKind3` | :977 | 0 | 恢复（Kind3Stick + Kind1 都依赖它） |
| `HandlePreInteractionKind2` | — | 2(dup) | 去重（保 canonical） |
| `HandlePreInteractionKind7` | — | 2(dup) | 去重 |
| `TryApplyHit` | :831 | 1 | OK |

恢复方法体已核实（带原始 C++ release 地址注释，如 0x42E97B）。`HandleWeaponKind3Stick` 依赖 `HandlePreInteractionKind3`；`Kind3` 依赖 `TryApplyHit`（已存在）。

**注意**：resolver 用 `_weapon.HandlePreInteractionKind1(...)` 调用，所以恢复后这些方法在 LF2WeaponBase 上须至少 `internal`（原 `protected virtual`，改 `internal` 或加 internal 包装器）。

## 第三层"疑似缺失"已排除（本 session 验证）

WeaponLink agent 报告 4 个方法"无 body 可恢复"，实际核实后**全部无需重建逻辑**：
- `RunWeaponSyncHeldStep10`：主文件 override(:2766) 先 `base.RunWeaponSyncHeldStep10()`（LF2Entity:2816 有真实 cpoint 抓取同步逻辑）再调 resolver。base 已做实事，resolver 空 body 合法（thin override）。
- `ClearConsumedHeldWeaponReference` / `ClearReleasedHeldWeaponReference` / `ClearHolderLinkRuntimeOnly`：死代码链——每个仅 1 个调用者即自身 delegation stub，无真实入口。resolver 空 body 安全编译。

**结论：无需第三层逻辑重建。修复 = 纯机械（删重复 partial）+ 恢复 3 个 weapon 方法（8101df55）+ 零散改名。**

## 诊断（已验证事实）

HEAD 当前 **572 个编译错误，从未编译成功过**。BMD-023 两个 commit（`ecebff21`/`01da3430`）叠在这个坏树上。

错误分布：
| Error | 数量 | 含义 |
|---|---|---|
| CS0111 | 499 | 重复成员 |
| CS0246 | 41 | 类型未找到（缺失的 resolver 类） |
| CS0102 | 16 | 重复类型成员 |
| CS0115 | 7 | override 不匹配 |
| CS0260 | 6 | 缺 partial 关键字 |
| CS0535 | 3 | 未实现 abstract |

受影响主类：`LF2Character`（344）、`LF2WeaponBase`（155）。

### 根因：partial → resolver 组合迁移半途而废

**主文件已完成迁移**（`LF2Character.cs` / `LF2WeaponBase.cs`）：
- 已声明所有 resolver 字段（`LF2Character.cs:53-59`、`LF2WeaponBase.cs:35-38`）
- 已实例化（构造器里 `new LF2CharacterXResolver(this)`）
- 已把 state/事件入口委托给 resolver（如 `LF2Character.cs:2462` `_stateResolver.StateStanding(...)`）

**但两件事没做完**：
1. **8 个 resolver 类文件不存在** → CS0246
   - Character 缺 5：`LF2CharacterHitResolver`、`LF2CharacterActionResolver`、`LF2CharacterCatchResolver`、`LF2CharacterInteractionResolver`、`LF2CharacterWeaponLinkResolver`
   - Weapon 缺 3：`LF2WeaponInteractionResolver`、`LF2WeaponHeldStateResolver`、`LF2WeaponReleaseFlowResolver`
   - 已存在 3：`LF2CharacterStateResolver`、`LF2CharacterDamageStateResolver`、`LF2WeaponFrameLogicResolver`
2. **旧 `.partial.cs` 文件还在**，持有迁移前的方法体 → 与主文件的委托 stub 重复 → CS0111/CS0102

`SimulationWorld` 的 partial 是健康的（主体已标 `partial`），**不在本次范围**。

### 已确立的组合模式（参考 `LF2CharacterStateResolver.cs`）
```csharp
internal sealed class LF2CharacterStateResolver
{
    private readonly LF2Character _character;
    public LF2CharacterStateResolver(LF2Character character) { _character = character; }
    public bool StateStanding(string eventType, object eventData) { ... _character.IsHeavyWeapon() ... }
}
```
方法体里的 `this.member` / 裸 `member` → 改写为 `_character.member`，私有成员需要在主类加 `internal` 访问器（现有 resolver 已建立此惯例，如 `SetAnimCounterInternal`）。

---

## 目标

零编译错误，行为不变（纯机械提取，逻辑不动）。self-check 恢复可跑。

## Must NOT（护栏）
- 不改变任何方法的实际逻辑（只搬家 + 改成员访问路径）
- 不动 `SimulationWorld.*` partial（健康）
- 不动 BMD-023 已改的逻辑（`LF2Character.cs:2205`、`SimulationWorld.Passes.partial.cs:140/168/186`）
- 不引入反汇编中不存在的逻辑
- 不改公开 API 签名

---

## 验证约束（重要）

**当前 Claude Code 会话无法调用 UnityMCP 执行工具**（只读 resources）。验证方式：
1. 改完文件后，Unity Editor 获得焦点会自动重编译
2. 读 `C:/Users/Logan/AppData/Local/Unity/Editor/Editor.log`
3. `grep -c "error CS"` 确认错误数下降
每个 resolver 完成后走一次这个循环，错误数必须单调下降。

---

## 执行策略（增量，一次一个 resolver）

按"错误数削减 / 依赖简单"排序。每步：创建 resolver 类 → 从对应 partial 搬方法体 → 删除已清空的 partial → 重编译验证 → commit。

### Phase 0：基线快照
- 记录当前 572 错误的完整清单到 `.omc/plans/partial-cleanup-baseline-errors.txt`
- 确认 BMD-023 两 commit 不受影响

### Phase 1：Character resolvers（5 个）
| 步 | 创建 resolver | 主要来源 partial | 主文件委托点 |
|---|---|---|---|
| 1a | `LF2CharacterWeaponLinkResolver` | `LF2Character.HeldObject.partial.cs` (384) | `_weaponLinkResolver.*`（14 处）|
| 1b | `LF2CharacterCatchResolver` | `LF2Character.Catch.partial.cs` (271) | `_catchResolver.*`（10 处）|
| 1c | `LF2CharacterHitResolver` | `LF2Character.Hit.partial.cs` (826) | `_hitResolver.ResolveHit` |
| 1d | `LF2CharacterActionResolver` | `LF2Character.Input.partial.cs` (523) | `_actionResolver.ProcessReleaseInput` |
| 1e | `LF2CharacterInteractionResolver` | 分散（TryCaughtA / TryConsumeUnifiedStep）| `_interactionResolver.*` |

### Phase 2：Weapon resolvers（3 个）
| 步 | 创建 resolver | 主要来源 partial | 主文件委托点 |
|---|---|---|---|
| 2a | `LF2WeaponHeldStateResolver` | `LF2WeaponBase.Held.partial.cs` (311) | `_heldStateResolver.*` |
| 2b | `LF2WeaponReleaseFlowResolver` | `LF2WeaponBase.ReleaseFlow.partial.cs` (243) | `_releaseFlowResolver.*` |
| 2c | `LF2WeaponInteractionResolver` | `LF2WeaponBase.Interaction.partial.cs` (212) | `_interactionResolver.RunInteraction` |

### Phase 3：纯重复 partial 清理
剩余 partial 文件的方法**已在主文件里有 canonical 实现**（如 `Generic.partial.cs` 的 RunTUPhase/RunFramePhase、`Damage.partial.cs` 的 State_Rowing stub）。逐一确认主文件是 canonical 后，删除 partial 文件（连同 `.meta`）：
- `LF2Character.Generic/Damage/Combat/EventShell/FrameControl/Late/Lifecycle/Locomotion/PreFrame/ReleaseFlow/Spawn.partial.cs`
- `LF2WeaponBase.Helpers/Lifecycle.partial.cs`
- `LF2OtherObject.*.partial.cs`（若同类问题）

### Phase 4：残余错误
- CS0246 `NTSDBattleVolume`、`SimulationInputButtons`（9+3）：与 resolver 无关，单独溯源（可能是缺失的类型 / using）
- CS0115/CS0535：override/abstract 链，resolver 迁移后应自动消失，剩余单独处理

### Phase 5：运行时验证
- 编译 0 错误后，跑 `BattleRuntimeSelfCheck.RunAllChecksStatic()`（含 BMD-023 两个 self-check）
- 确认 `[BattleRuntimeSelfCheck] 战斗运行时自检通过。`

---

## Commit 策略
每个 resolver 一个 commit（`refactor(LF2Objects): extract LF2CharacterXResolver from partial`），错误数在 message 里记录（如 `572 → 531`）。Phase 3 批量删除一个 commit。

## 风险
- **无 golden master**：baseline 从未编译，无法做前后行为对比。只能靠"纯机械提取"保证等价 + self-check 覆盖。
- **私有成员访问**：大量 `this.privateMember` 需要加 `internal` 访问器，可能引入大 diff。
- **多 partial 共享方法**：某方法可能被多个 partial 引用，搬家时要确认唯一归属。
- **规模**：5375 行跨 26 文件，多 commit / 可能多 session。

## 工作量
Large。sequential（resolver 之间基本独立，但都依赖主文件已有的委托 wiring，不能并行改同一主文件）。
