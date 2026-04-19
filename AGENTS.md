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

### P0 待处理
| 缺口 | 说明 |
|------|------|
| 重武器 Interaction 状态过滤错误 | 应为 state 2004，非 2000 |
| ProcessAttack() 空 stub | weapon_strength_list 未解析 |
| State 14 (Lying) state_entry/exit 未实现 | 倒地后无落地动画和无敌时间 |

### 已确认需修复的 Bug

**BUG-1：`HitInvincible` 字段是错误实现，必须完整移除**

反汇编依据：`+0xB0h` 偏移是 fall 分档阈值（20/40/80），不是无敌帧计数器。受击保护只靠 `vrest`/`arest`。

需修改的文件：
1. `Assets/NTSD/Scripts/Animation/Character/LF2HitCountersModule.cs` — 删除 `HitInvincible` 属性及 `SetHitInvincible()`
2. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs` — 删除 `TUUpdate()` 中的 HitInvincible 递减
3. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.Hit.partial.cs` — 删除 `Hit()` 中的 HitInvincible 检查和赋值

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
