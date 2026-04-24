# Agent Guide (Unity / NTSD)

This repository is a Unity project replicating **NTSD (Naruto The Setting Dawn)** game logic, built on top of EX Gameplay Ability System (EX-GAS).

Unity version: `2022.3.4f1c1`

---

## Authority Sources（权威来源）

1. `J:\QQFile\NTSD2.4\ntsd24_full_disasm.txt` — x86 反汇编，522 函数，**唯一权威**
2. `J:\QQFile\NTSD2.4\ntsd24_pseudoc.txt` — Hex-Rays 伪C，284 函数，快速参考（细节以汇编为准）
3. `J:\QQFile\NTSD 2.4.1 工具人亲测能玩/` — NTSD 游戏原始目录与数据文本

**任何逻辑实现必须能在反汇编中找到对应代码段，否则标注"待确认"，不得实现。**

---

## Key Code Locations

- EX-GAS runtime: `Assets/GAS/Runtime/`
- EX-GAS editor: `Assets/GAS/Editor/`
- **NTSD replica（主要开发区域）**: `Assets/NTSD/Scripts/`

Assemblies:
- Runtime: `Assets/GAS/Runtime/com.exhard.exgas.runtime.asmdef`
- Editor: `Assets/GAS/Editor/com.exhard.exgas.editor.asmdef`

Third-party: Odin Inspector (paid), UniTask (`com.cysharp.unitask`), Unity Test Framework.

---

## Build / Test

Builds and tests are driven by Unity. Set `UNITY_EXE` to your Unity editor path.

```powershell
$env:UNITY_EXE = "C:\Program Files\Unity\Hub\Editor\2022.3.4f1c1\Editor\Unity.exe"
```

Run EditMode tests:
```powershell
& $env:UNITY_EXE -batchmode -nographics -quit `
  -projectPath "$PWD" `
  -runTests -testPlatform EditMode `
  -testResults "$PWD\TestResults-EditMode.xml" `
  -logFile "$PWD\UnityTest-EditMode.log"
```

---

## Coding Conventions (C# / Unity)

- Indentation: 4 spaces, Allman braces
- Types/Methods/Properties: `PascalCase`
- Local variables: `camelCase`
- Private fields: `camelCase` (or `_camelCase` — follow nearby conventions)
- `using` order: `System.*` → .NET → `UnityEngine`/`UnityEditor` → project namespaces
- Prefer `[SerializeField] private` for inspector fields
- Prefer UniTask for async flows
- Avoid per-frame allocations; use pooling

---

## NTSD Module Structure

| Path | Purpose |
|------|---------|
| `Animation/LF2Objects/` | Object runtime (LF2Character, LF2WeaponBase, LF2SpecialAttack). **主要开发区域**，使用 C# partial classes |
| `Animation/Character/` | Per-character logic: IdUpdate, HitCounters, ItrRestTracker |
| `Animation/LF2Tasks/` | Async task base for object operations |
| `Animation/Manager/` | CharacterAnimatorManager |
| `Animation/` (root) | Data models, parsers, loaders, CharacterAnimator |
| `DatParser/` | Parses NTSD `.dat` files |
| `Input/` | Input system: ComboConfig, KeyEventPool, InputBase |
| `Simulation/` | Deterministic sim tick: SimContext, ISimTickable, SimInputBuffer |
| `Define/` | Shared enums/constants |
| `NTSD_Extensions/` | NTSD-specific GAS extensions |
| `Gen/` | **Auto-generated** — do not edit manually |
| `App/` | App lifecycle: AppManager, BattleBootstrap, MatchConfig |
| `Load/` | Resource loading: NTSDResourceLoader, GlobalTickDriver |
| `UI/` | UI controllers |
| `Tools/` | Utility: ReferencePool, Log, SingletonBehaviour |
| `TimeWheel/` | Timer scheduling |
| `LevelEditor/` | Editor-only boundary wall tooling |

### Partial class pattern (LF2Character)

- `LF2Character.cs` — core class definition
- `LF2Character.Generic.partial.cs` — generic/shared behaviours
- `LF2Character.States.partial.cs` — state machine logic
- `LF2Character.Hit.partial.cs` — hit/combat logic
- `LF2CharacterStateModule.cs` — state module helper

### Do not modify
- `Assets/NTSD/Scripts/Gen/` — auto-generated
- `Assets/Plugins/` — third-party packages

---

## 核心对齐原则（每次对话必读）

**权威来源**：`J:\QQFile\NTSD2.4\ntsd24_full_disasm.txt`（x86 反汇编，522 函数，唯一权威）

**对齐要求**：
- **能对齐的直接对齐**：逻辑、常量、字段读取顺序等，尽量与反汇编一致
- **框架限制无法对齐时，只要求最终结果一致**：实现方式可适配，但运行时行为必须与反汇编等价
- **不得引用任何第三方项目作为依据**：逻辑来源必须能在反汇编中找到对应代码段

---

## NTSD 战斗核心：当前状态与差距

### 已完成
- `LF2Character.Hit()` 主方法完整
- `State_Injured` / `State_Falling` frame 事件已实现
- `Generic_PreInteraction()` / `Generic_PostInteraction()` 已实现
- `HitStateCount`（0xB8h）：被打后设 45，每帧衰减 — **已正确实现**
- `AttackExempt`（0xECh）：命中后设 6，每帧衰减 — **已正确实现**
- Spark slot 系统（10 slots，timer 驱动）— **已实现**
- opoint 纯音效帧（pic=999）— **已实现**
- LF2SpecialAttack FrameCache 加载 — **已修复**
- 武器系统全模块（Python → C#）迁移 — **已完成**（详见下方武器系统章节）



---

## 武器系统迁移（Python → C#）

**参考源文件**：
- `J:\QQFile\NTSD_beta1.8\NTSD_EXE_FLOW\battle_entity\weapon_system.py`
- `J:\QQFile\NTSD_beta1.8\NTSD_EXE_FLOW\battle_entity\effect_entity.py`

### 模块迁移状态（11/11 完成）

| # | Python 函数 | C# 对应位置 | 状态 |
|---|---|---|---|
| 1 | `update_weapon_pickup` | `HandlePreInteractionKind1/2/7`, `ApplyPickupGrabbedBy`, `ApplyPickupFrameJump` | ✅ |
| 2 | `sync_held_weapon` | `LF2WeaponBase.Act()` | ✅ |
| 3 | `release_held_weapon` | `LF2WeaponBase.Act()` force-drop 路径 | ✅ |
| 4 | `draw_wpoint_weapon` | 纯渲染，不迁移 | ⏭️ |
| 5 | `check_held_weapon_collision` | `LF2Weapon.ProcessAttack()` via weapon_strength_list | ✅ |
| 6 | `apply_weapon_collision` | `LF2Weapon.ApplyAttackerResponse()` | ✅ |
| 7 | `process_held_weapon_durability` | `LF2WeaponBase.ProcessDrinkConsumption()` | ✅ |
| 8 | `scatter_held_weapon_7A` | `ProcessDrinkConsumption()` HP≤0 路径（内联） | ✅ |
| 9 | `scatter_held_weapon_7B` | `ProcessDrinkConsumption()` HP≤0 路径（内联） | ✅ |
| 10 | `update_boomerang_catch` | `LF2WeaponBase.CheckBoomerangCatch()` | ✅ |
| 11 | `update_frame_advance_effect` | `FrameTransistor.Trans()` + `LF2LivingObject.OnTransitDestroy()` | ✅ |
| 12 | `update_physics_effect` | `LF2Weapon.WeaponFlightPhysics()` + `CharacterMechanics.WeaponDynamics()` + `LF2Weapon.OnLanded()` | ✅ |

### 历次 Bug 修复汇总

| Bug | 修复内容 |
|-----|---------|
| BUG1&2: OnLanded 条件变量错误 | `PS.y > 0.0001f` |
| BUG3: 投掷 vy 零值守卫 | 无条件赋值 |
| BUG4: force drop vz=0 错误 | 移除 vz 赋值 |
| BUG5: 回旋镖 dz 对称判断 | 改为单向检测（对齐反汇编 0x405187） |
| kind=7 错误走 TryApplyHit | 改为近身拾取逻辑 |
| GrabbedBy 分流缺失 | 添加 `ApplyPickupGrabbedBy()` |
| kind=2 帧跳转缺失 | 添加 `ApplyPickupFrameJump()` |
| Force drop vy 乘1/3 错误 | 改为直接复制 |
| CharType==1 vx 来源错误 | 改为 `holder.vz * 1/3` |
| 多余的 force drop vz 赋值 | 删除 |
| y_float clamp 缺失 | 添加 `if PS.y > -2.0f → PS.y = -2.0f` |

### 遗留项
- **`state=9998` 即死逻辑**：对应反汇编 EXE 0x00421060，武器不受影响，需在 `LF2SpecialAttack` 帧推进路径中实现

---

## 架构目标

- **帧同步**：所有影响战斗结果的逻辑必须在帧同步安全路径上执行，不得依赖浮点不确定性或 Unity Physics
- **性能**：避免 O(n²) 碰撞遍历，避免每帧 GC 分配
- **AI**：AI 只输出输入序列注入 InputBuffer，不直接操作状态机

### 特效系统
- **纯视觉特效**（击中闪光）：`SparkRenderer` 渲染，挂在 `AppManager`，由 `SimulationTickDriver` 驱动
- **战斗特效**（带 itr 碰撞）：注册进 `SimulationWorld`，实现 `ISimObject`，走 `SerialTickAll`

---

## 问题定位原则（强制）

**dat 文件没有任何问题，不需要修改。**

任何时候，若分析 bug 得出"dat 文件数据有误"或"需要修改 dat"的结论，**必然说明定位方向错了**，应立即回归代码层面重新分析。

---

## Common Pitfalls
- Do not commit `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`
- Do not rename serialized fields — breaks existing scenes/prefabs
