# Agent Guide (Unity / NTSD)

## Current Unity NTSD Authority Override

For current Unity NTSD reconstruction work, use the formal C++ release project as the gameplay authority:

- Primary source: `J:\QQFile\NTSD2.4\ntsd_release`
- Do not use disassembly documents as the active reference for Unity gameplay behavior unless the user explicitly asks for a historical comparison.
- Do not import or preserve debug macro, debug trace, debug shortcut, or debug-only behavior as formal gameplay logic.
- The C++ project is both the NTSD2.4 EXE reconstruction target and the intermediate baseline for the Unity project.
- Keep rendering, pooling, and MonoBehaviour integration Unity-native, but converge combat objects toward the C++ release model of unified entity data plus central battle tick/collision/hit systems.
- Older notes below that name disassembly or FLF as the authority are historical records only; for current Unity gameplay reconstruction, consult the C++ release implementation first.

This repository is a Unity project replicating **NTSD (Naruto The Setting Dawn)** game logic, built on top of EX Gameplay Ability System (EX-GAS).

Unity version: `2022.3.4f1c1`

---

## Authority Sources（权威来源）

1. `J:\QQFile\NTSD2.4\ntsd24_full_disasm.txt` — x86 反汇编，522 函数，**唯一权威**
2. `J:\QQFile\NTSD2.4\ntsd24_pseudoc.txt` — Hex-Rays 伪C，284 函数，快速参考（细节以汇编为准）
3. `J:\QQFile\NTSD 2.4.1 工具人亲测能玩/` — NTSD 游戏原始目录与数据文本

**任何逻辑实现必须能在反汇编中找到对应代码段，否则标注"待确认"，不得实现。**

---

## 反汇编提取工具（disasm_extract.py）

**脚本路径**：`I:\C++Test\NTSD\disasm_extract.py`

**用途**：从反汇编文本中机械性提取结构化索引，避免每次对齐验证都重新解析反汇编（节省 token）。

**运行方式**：
```powershell
python "I:\C++Test\NTSD\disasm_extract.py"
# 或指定路径：
python "I:\C++Test\NTSD\disasm_extract.py" <disasm_path> <output_dir>
```

**输出文件**（均在 `I:\C++Test\NTSD\`）：

| 文件 | 内容 | 用途 |
|------|------|------|
| `functions_index.json` | 所有 522 个函数的名称、起止地址、大小 | 快速定位函数 |
| `field_accesses.json` | 每个函数内所有 `[reg+offset]` 的读写地址列表 | 查某字段在某函数中的所有访问点 |
| `field_cmp.json` | 每个函数内所有 `cmp [reg+offset], N` | 查字段的比较值（推断枚举含义） |
| `calls.json` | 每个函数内所有 `call sub_XXXXXX` | 查函数调用关系 |
| `field_offset_summary.json` | 全局偏移汇总：每个偏移的读/写/cmp 次数 + 出现函数列表 | 快速确认某偏移的使用范围 |

**正确性说明**：
- 脚本只做文本匹配，不做推断，提取结果 100% 准确
- **不能做**：字段语义命名、控制流分支含义（这部分仍需 AI 一次性标注后存档）
- `cmp` 正则只匹配 `cmp [reg+offset], N` 直接形式；若代码先 `mov reg, [reg+offset]` 再 `cmp reg, N`，则不会被 `field_cmp.json` 捕获，但会出现在 `field_accesses.json` 的 reads 中

**典型查询示例**：
```python
import json

# 查 +364h 在哪些函数中被访问
with open('I:/C++Test/NTSD/field_offset_summary.json', encoding='utf-8') as f:
    summary = json.load(f)
print(summary['364H'])  # reads, writes, cmps, funcs 列表

# 查 Entity_AI_Update 中 +364h 的所有访问地址
with open('I:/C++Test/NTSD/field_accesses.json', encoding='utf-8') as f:
    accesses = json.load(f)
print(accesses['Entity_AI_Update']['364H'])  # addrs 列表

# 查 Entity_AI_Update 调用了哪些子函数
with open('I:/C++Test/NTSD/calls.json', encoding='utf-8') as f:
    calls = json.load(f)
print(calls['Entity_AI_Update'])
```

**何时需要重新运行**：反汇编文件本身不会变化，输出文件已存在时无需重跑。若输出文件丢失，重新运行脚本即可（约 15 秒）。

---

## 反汇编核心战斗模块索引

以下为从 `ntsd24_full_disasm.txt` 中确认的所有战斗相关函数，按模块分组。
**非战斗函数**（渲染、UI、网络、音频、CRT库）已排除。

### 主循环

| 函数 | 地址 | 大小 | 职责 |
|------|------|------|------|
| `Game_FrameUpdate` | 0x0041DB60 | 27344 | **顶层帧驱动**，每帧依次调用所有子模块 |
| `Game_Tick` | 0x00439A90 | 868 | 游戏主 tick，调用 Game_FrameUpdate |
| `PreFrame` | 0x0041C850 | 1971 | 帧前处理（输入快照、状态准备） |
| `Frame_PostProcess` | 0x0041BF00 | 167 | 帧后处理（清理临时状态） |
| `Entity_PostFrame` | 0x00424970 | 38 | 实体帧后处理 |

### 实体生命周期

| 函数 | 地址 | 大小 | 职责 |
|------|------|------|------|
| `Entity_Spawn` | 0x00406040 | 874 | 实体生成入口 |
| `sub_402340` | 0x00402340 | 2227 | 实体初始化（字段清零、team/owner 继承） |
| `sub_424630` | 0x00424630 | 831 | 实体 Reset（清零战斗字段，对象池回收前调用） |

### 帧推进与物理

| 函数 | 地址 | 大小 | 职责 |
|------|------|------|------|
| `Entity_FrameAdvance` | 0x00416240 | 2917 | 帧计时推进、next 跳转、武器飞行物理、落地弹跳 |
| `Entity_Collision` | 0x004138F0 | 1305 | 武器与地面/边界碰撞、state/type 特殊分支（N-1~N-5） |

### 帧逻辑与 opoint

| 函数 | 地址 | 大小 | 职责 |
|------|------|------|------|
| `Entity_FrameLogic` | 0x004030C0 | 12155 | opoint 生成、hit_Fa 特殊分支（N-11~N-14）、frame state 特殊处理（N-26~N-31） |

### 碰撞与命中

| 函数 | 地址 | 大小 | 职责 |
|------|------|------|------|
| `Entity_AI_Update` | 0x0042C8C0 | 12707 | itr 碰撞检测与命中处理（kind=0~16 全部在此） |
| `Collision_Check1` | 0x0041B740 | 1632 | cpoint 抓取逻辑（N-17~N-20） |
| `Collision_Check2` | 0x0041B2C0 | 1150 | 碰撞检测第二阶段（bdy/itr 矩形相交） |
| `sub_419F80` | 0x00419F80 | 2866 | 武器 itr 命中处理（kind=3 stick、kind=8 attach 等） |

### 输入处理

| 函数 | 地址 | 大小 | 职责 |
|------|------|------|------|
| `Entity_InputProcess` | 0x00414DC0 | 5242 | 实体输入状态机（按键 → 技能触发） |
| `Entity_ProcessInput` | 0x00438C20 | 214 | 输入预处理（从全局输入映射到实体） |
| `sub_4063B0` | 0x004063B0 | 9806 | 按键 buffer 滚动更新（+0C6h~+0D3h），调用 AI_Update/AI_Process |
| `GetKeyInput` | 0x00424E70 | 683 | 读取原始键盘输入 |

### AI

| 函数 | 地址 | 大小 | 职责 |
|------|------|------|------|
| `AI_Update` | 0x00409820 | 12757 | AI 行为决策主函数 |
| `AI_Process` | 0x00408A00 | 2191 | AI 处理辅助（状态评估） |
| `AI_Process2` | 0x0041AAC0 | 2040 | AI 处理第二阶段（饮料消耗等） |

### 游戏模式

| 函数 | 地址 | 大小 | 职责 |
|------|------|------|------|
| `GameMode_Process` | 0x0041BDA0 | 347 | 游戏模式逻辑（胜负判定、回合管理） |

### 非战斗（已排除，仅供参考）

| 函数 | 职责 |
|------|------|
| `RenderDispatch` / `PostRender` / `Background_Blit` | 渲染 |
| `Sound_Play` / `Sound_Load` / `sub_419B40` | 音频 |
| `NetInput_Process` / `NetSync_Update` | 网络 |
| `ParseCharData` / `ParseDataTxt` | 数据解析 |
| `DataEditor` / `sub_413030` | 编辑器/dat 写出 |
| `CharSelect_Dispatch` / `CS_*` / `Menu_*` | UI/菜单 |
| `sub_43A050` / `Sprite_Init` / `Sprite_Blit` | Sprite 渲染 |

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

## 主循环对齐状态（Game_FrameUpdate）

**对齐完成时间**：2026-05-03
**结论**：`RunOneSimTick` 执行顺序与反汇编 `Game_FrameUpdate`（0x0041DB60）完整对齐。

### RunOneSimTick 最终执行顺序

| 顺序 | C# Pass | 反汇编对应 | 地址 |
|------|---------|-----------|------|
| 1 | `VrestTickAll` | vrest/arest 递减（GameMode_Process 循环1） | 0x0041BDA0 |
| 2 | `PreInteractionTickAll` | Entity_InputProcess + Collision_Check1/2 | 0x00420E66 / 0x004218B5 |
| 3 | `SerialTickAll` | Entity_FrameAdvance (sub_416240) | 0x00421044 |
| 4 | `RandomWeaponDropTickAll` | 随机掉落武器（0x004215FA 区域） | 0x004215FA |
| 5 | `PostInteractionTickAll` | Entity_AI_Update Loop1(type==0) + Loop2(type>0) | 0x004215E9 / 0x004218A2 |
| 6 | `FramePostProcessAll` | Frame_PostProcess (sub_41BF00) | 0x004219CB |
| 7 | `EntityCollisionTickAll` | Entity_Collision (sub_4138F0) | 0x00421FBB |
| 8 | `LateTick` | 死亡清理 / Entity_Reset | 0x0041F61A 等 |

### 已排除的非战斗调用（不需要 C# 实现）

| 函数 | 地址 | 性质 | 排除原因 |
|------|------|------|---------|
| `Entity_PostFrame` (sub_424970, 38B) | 0x0041F581 / 0x00423BB0 | HUD 字段重置 | 写 `+2E4h/+2E8h/+2ECh/+2F0h/+0EBh`，全部只在 `CS_*` HUD 函数中使用 |
| `AI_Process2` (sub_41AAC0, 2040B) | 0x0042156C / 0x0042191A | HUD x坐标同步 + 饮料消耗 | `+68h` 是 HUD 字段；饮料消耗已在 `ProcessDrinkConsumption` 实现 |
| `GameMode_Process` (sub_41BDA0, 347B) | 0x0042158F | HUD AttackExempt显示 + itr碰撞 | `+0ECh` 是 HUD 字段；itr碰撞已在 `VrestTickAll+PostInteractionTickAll` 覆盖 |
| `PreFrame` 第1次 (sub_41C850) | 0x00420845 | 渲染路径 | 在游戏模式条件分支内，属于渲染帧路径，非逻辑帧 |
| `Background_Blit` / `Sprite_Blit` / `TextOut` | 多处 | 纯渲染 | Unity 渲染系统替代 |
| `Sound_Attenuation` (sub_419B40) | 0x0042298E | 音频 | Unity 音频系统替代 |

### HUD 字段备注（实体偏移，非战斗）

以下偏移仅在 `CS_1v1_HUD`、`CS_4v4_HUD`、`CS_Phase*` 等 UI 函数中使用，**与战斗逻辑无关**，C# 中不需要对应字段：

| 偏移 | 用途推测 | 出现函数 |
|------|---------|---------|
| `+2E4h` | HUD 显示计数器（每帧清零） | CS_HUD 系列 |
| `+2E8h` | HUD 显示计数器（每帧重置为 1000） | CS_HUD 系列 |
| `+2ECh` | HUD 显示计数器（每帧重置为 1000） | CS_HUD 系列 |
| `+2F0h` | HUD 显示计数器（每帧重置为 1000） | CS_HUD 系列 |
| `+0EBh` | HUD 标志字节（每帧清零） | CS_HUD 系列 |
| `+68h`  | HUD x 坐标浮点（角色头像/血条位置） | CS_HUD 系列 |
| `+0ECh` | HUD AttackExempt 显示标志 | CS_HUD 系列 |

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

### 饮料/食物武器（type=6）说明

`ProcessDrinkConsumption()` 中 `type_sub==0x7A`（饮料）和 `type_sub==0x7B`（食物）的逻辑已按反汇编实现，但**当前无法测试**，原因如下：

1. **不在 `_f1Weapons` 测试数组中**：饮料武器为 oid=122（`#healing`，weapon3.dat）、oid=123（`#beer`，weapon8.dat），均未包含在 `WeaponSpawner._f1Weapons` 中。`_f1Weapons` 即原版游戏全部可使用武器，饮料/食物不属于可直接使用的战斗武器，而是通过 opoint 生成。
2. **`type_sub` 字段未解析**：`CharacterAnimtorManager.BuildCharacterDataFromDat()` 中未解析 `type_sub` 字段，所有武器的 `type_sub` 恒为 0，导致 0x7A/0x7B 分支永远不会触发。此问题同时影响武器重力系数（`WeaponGravityTypeSub7C/78/65`）分支，但这些类型的武器在 `_f1Weapons` 中也不存在，优先级低。
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

### 遗留项（反汇编对照后更新）

以下为对照 `ntsd24_full_disasm.txt` 后确认的欠缺与差异，按优先级排列。

#### P0 — 影响战斗结果，立即修复

| # | 模块 | 反汇编地址 | 问题描述 |
|---|------|-----------|---------|
| ~~P0-1~~ | ~~饮料消耗 +4 目标错误~~ | `0x0041AC45` (`sub_41AAC0`) | **已关闭/不适用**：type=6 饮料（oid=122 `#healing`、oid=123 `#beer`）不在原版可用武器池中，原版游戏实际不存在可使用的饮料武器。`_f1Weapons` 中 oid=100 为 `#heal_scroll`（治疗卷轴，type=4，`weapon6.dat`），通过 opoint 生成 heart 实体回血，不走 `sub_41AAC0` 饮料路径。|
| ~~P0-2~~ | ~~type=4/6 大弹触发条件反转~~ | — | **已关闭**：第二轮深挖确认 C# 代码 `vy>8.5 AND |vx|<10` 与反汇编一致，原描述有误 |
| ~~P0-3~~ | ~~武器无法被 itr 命中~~ | `0x0041A0C9` (`sub_419F80`) | **已修复**：在 `LF2WeaponBase.cs` 和 `LF2SpecialAttack.cs` 的 `CanInteractTarget()` 中为 kind=8/14 添加 bypass，跳过 `LF2LivingObject` 过滤 |
| ~~P0-4~~ | ~~kind=8 heal_timer 写入目标错误~~ | `0x0042EC85` (`Entity_AI_Update`) | **已修复**：`LF2Character.Hit.partial.cs` kind=8 分支改为 `attacker.HealTimer = itr.throwvz + 1000` |
| ~~P0-5~~ | ~~kind=3 throw scatter 用固定值替代随机值~~ | `sub_419F80` | **已修复**：`HandleWeaponKind3Stick()` 中添加随机散落速度 vx=Random(7)-3, vy=-Random(4), vz=(Random(5)-2)*0.2 |
| ~~P0-6~~ | ~~kind=8 命中时多余的 HP 扣减~~ | `0x0042EC85` (`Entity_AI_Update`) | **已修复**：删除 `HandleWeaponKind8Attach()` 中 state=3002 分支（含 `victim.Health.HP -= itr.injury`） |
| ~~P0-7~~ | ~~kind=8 命中时多余的 FrameDelay 设置~~ | `0x0042EC85` (`Entity_AI_Update`) | **已修复**：删除 `LF2Character.Hit.partial.cs` 中 `FrameDelay = -3` 和 `attacker.FrameDelay = 3`；同时随 state=3002 分支删除 |
| ~~P0-8~~ | ~~kind=8 state=3002 分支无中生有~~ | `0x0042EC85` (`Entity_AI_Update`) | **已修复**：`HandleWeaponKind8Attach()` 中整个 `if (curState == 3002)` 分支（12行）已删除 |

#### P1 — 影响物理/数值表现

| # | 模块 | 反汇编地址 | 问题描述 |
|---|------|-----------|---------|
| ~~P1-1~~ | ~~type=4/6 大弹 vy 钳制值错误~~ | `0x00416D11` (`sub_416240`) | **已修复**：`LF2Weapon.OnLanded()` type=4/6 大弹路径中将 `vy < -2.5f` 改为 `vy < -10.0f`（注：前次修复写入了不存在的 `HandleWeaponType46Landing`，本次重新确认并修正至实际代码位置）|
| ~~P1-2~~ | ~~饮料消耗 MaxPP 钳制上限来源错误~~ | `0x0041AC55` (`sub_41AAC0`) | **已修复**：`ProcessDrinkConsumption()` 中将上限改为 `holder.Health.PPBound` |
| ~~P1-3~~ | ~~食物消耗 PP 钳制 150 未实现~~ | `0x0041AD96` (`sub_41AAC0`) | **已修复**：`ProcessDrinkConsumption()` 食物路径末尾添加 `if (OwnerEntityIndex > -1 && newPP > 150) newPP = 150` |
| ~~P1-4~~ | ~~type=0 空中帧切换全局计数器错误~~ | `0x004165FB` (`sub_416240`) | **已修复**：将 `HitStun < 6` 替换为 `(SimulationTickDriver.Instance?.CurrentTickIndex ?? 0) % 12 < 6`，对应 `dword_449038` |
| ~~P1-5~~ | ~~type=4/6 落地 vz 清零时机错误~~ | case 4/6 (`sub_416240`) | **已修复**：`HandleWeaponType46Landing()` 中将 `vz=0` 移到大弹/小弹判断之前 |
| ~~P1-6~~ | ~~`OwnerEntityIndex`（`[+756]`）字段缺失~~ | `sub_402340` / `sub_41AAC0` | **已修复**：在 `LF2Entity.cs` 中添加 `public int OwnerEntityIndex { get; set; } = -1` |
| ~~P1-7~~ | ~~kind=3 命中时双方 vx/vy 未清零~~ | `Entity_AI_Update` v19==3 分支 | **已修复**：`HandleWeaponKind3Stick()` 中添加 `PS.vx=0; PS.vy=0; targetCh.PS.vx=0; targetCh.PS.vy=0` |
| ~~P1-8~~ | ~~kind=3 命中时双方朝向未对齐~~ | `Entity_AI_Update` v19==3 分支 | **已修复**：`HandleWeaponKind3Stick()` 中添加 facing 对齐（`chFacingRight = victim.x > attacker.x`） |
| ~~P1-9~~ | ~~kind=3 命中时位置未对齐~~ | `Entity_AI_Update` v19==3 分支 | **已修复（近似）**：`HandleWeaponKind3Stick()` 中基于 `centerx`/`centery` 和精灵半宽计算 victim 位置；原始精灵宽度每帧不同无法精确，用当前帧精灵宽度近似（`Sprite.GetWidthPx()/2`） |
| ~~P1-10~~ | ~~kind=10/11 命中轻武器时缺少物理效果~~ | `0x0042D384` (`Entity_AI_Update`) | **已修复**：`LF2Weapon.Hit()` 中添加 frame=0、vx/vz*=0.9345、y=-2、vy-=3 |
| ~~P1-11~~ | ~~kind=10/11 命中重武器时缺少物理效果~~ | `0x0042D450` (`Entity_AI_Update`) | **已修复**：`LF2Weapon.Hit()` 中添加 frame=0、vx/vz*=0.9345、y=-2、vy-=2.3 |

#### P2 — 功能缺失，不影响核心战斗

| # | 模块 | 反汇编地址 | 问题描述 |
|---|------|-----------|---------|
| ~~P2-1~~ | ~~`state=9998` 即死逻辑~~ | `0x00421060` | **已修复**：`LF2WeaponBase.SimTransit()` 中在 `Trans.Trans()` 后检查 `Frame.D.state == 9998 → StateUpdate("die")` |
| ~~P2-2~~ | ~~wpoint.kind=1 攻击时武器帧跳转~~ | `0x0041B740` (`sub_41B740`) | **已关闭**：经核查 `sub_41B740` 是角色间 itr 碰撞，武器投掷帧跳转已在 `LF2WeaponBase.Act()` 的 `fwpoint.kind==2 && dvx!=0` 分支实现（type=1/4/6→frame=40，type=2→Random(6)）✅ |
| ~~P2-3~~ | ~~wpoint.kind=2 持有者不匹配脱落~~ | `0x0041B740` (`sub_41B740`) | **已修复**：`LF2WeaponBase.SimTU()` 中添加：当前帧 wpoint.kind=2 但持有者 StableId 不匹配或持有者 wpoint.kind≠1 时，`frame=212, vx=0, vy=-3.0, y clamp -2.0` |
| ~~P2-4~~ | ~~type=0 Burning 落地碰撞伤害~~ | `0x00416BE0` (`sub_416240`) | **已修复**：`LF2Weapon.OnLanded()` type=0 分支头部添加：`frame.state==Burning && (vy>17 || |vx|>9)` 时扣耐久、速度钳制、frame=185、对周围角色发起碰撞伤害 |
| ~~P2-5~~ | ~~武器 opoint 生成~~ | `0x00422185` (`sub_42C8C0`) | **已修复**：`LF2WeaponBase.OnFrameTransit()` 中：更新 `Frame.D`，检查 `opoint.kind != 0 && opoint.oid > 0`，守卫 `HitStun==0` 且 `(FrameDelay==0 \|\| WeaponType==0)`，满足则入队 `OPointCreateTask` |
| ~~P2-6~~ | ~~broken_weapon 生成~~ | `Generic_Die()` | **已关闭**：`Generic_Die()` 已调用 `PlaySound(WeaponBrokenSound)` + `CreateBrokenEffect()`；broken_weapon 实体生成依赖 opoint 框架（P2-5 已完成），无需额外代码 |
| ~~P2-7~~ | ~~weapon type=3 粘附后每帧跟随逻辑~~ | `0x0042DB0E` (`Entity_AI_Update`) | **已修复**：`LF2WeaponBase.SimTU()` 中添加：type=3 且持有中时，state=3005/3006 → 散落（vx=Random(7)-3, vy=-8, holder清除）；其他 → 清零速度、同步 team、位置跟随持有者、frame=30。特殊忍偶系（entity_type=0xD1）分支暂跳过 |
| ~~P2-8~~ | ~~武器帧切换时 sound 播放~~ | `sub_416240` | **已修复**：`LF2WeaponBase.OnFrameTransit()` 中帧切换时若 `frame.sound` 非空则 `PlaySound()` |
| ~~P2-9~~ | ~~next 帧跳转后 sound 播放~~ | `sub_416240` | **已修复**：与 P2-8 同一路径，`OnFrameTransit()` 统一处理帧切换（含 next 跳转），播放新帧 sound |
| ~~P2-10~~ | ~~落地时 sound 播放~~ | `sub_416240` | **已修复**：`LF2Weapon.OnLanded()` type=1 大弹已有；补全 type=2 大弹、type=4/6 大弹处调用 `PlaySound(WeaponDropSound)` |

#### 已确认无问题

| 项 | 结论 |
|---|------|
| `holder.attacking` / `weapon.FrameDelay` | `[+0B4h]` = `weapon.FrameDelay`，C# 已正确实现 ✅ |
| `wpoint.attacking` 字段 | C# `WeaponPoint.attacking` 已正确实现 ✅ |
| type=1 小弹 vz 清零 | 反汇编确认无 vz 清零，C# 多余清零影响极小（摩擦每帧 -1.0 自然衰减），可接受 ✅ |
| kind=8 传送坐标 | `victim.x = attacker.x`, `victim.z = attacker.z + 1.0`, `victim.frame = itr.dvx` — C# 对齐 ✅ |
| kind=3 catchingact/caughtact 帧跳转 | C# 已正确实现 ✅ |
| kind=3 不命中武器 | C# 已正确过滤 ✅ |
| itr kind=0/1/2/4/5/6/7/9/10/11/14 | 全部有对应 C# 处理路径 ✅ |
| type=4/6 大弹触发条件 | `vy>8.5 AND |vx|<10` — C# 已正确实现 ✅ |
| type=4/6 大弹 vy *= -0.7 | C# 已正确实现 ✅ |
| type=4/6 小弹 vx *= 0.7 | C# 已正确实现 ✅ |
| type=1 大弹 vy 阈值 9.9、vy=-8.0 | C# 已正确实现 ✅ |
| type=2 大弹 vy 阈值 9.0、vy=-5.0 | C# 已正确实现 ✅ |
| type=0 type_sub=999 落地处理 | frame=101, vx/vy/vz=0 — C# 已正确实现 ✅ |
| `_flightCounter` 初始值 = weapon_hp | C# 已正确实现 ✅ |
| PickerStableId 初始值 -1 | 对应 `[+3F8h]=-1` — C# 已正确实现 ✅ |
| wpoint.kind=2 持有者匹配时位置同步 | `CoincideXYWithWPoint` — C# 已正确实现 ✅ |
| 饮料消耗脱落路径 | frame=0, vx=Random(7)-3, vy=-8.0 — C# 已正确实现 ✅ |
| FrameDelay/held_by/state=2 守卫 | C# 已正确实现 ✅ |
| 回旋镖捕获 | dx<30, z 单向, dy<10, frame=60, HealTimer=100 — C# 对齐 ✅ |
| force drop 速度继承 | vx=holder.vx*1/3, vy=holder.vy — C# 对齐 ✅ |

#### 新发现待确认项（反汇编武器相关，尚未进入 P0/P1/P2）

以下为后续追加扫描 `ntsd24_full_disasm.txt` / `ntsd24_pseudoc.txt` 后发现的武器相关逻辑，尚未逐项归档优先级，也未确认当前 C# 是否已有等价实现。

| # | 模块 | 反汇编地址 | 记录内容 |
|---|------|-----------|---------|
| N-1 | type=3 持续耗耐久 | `0x004138F0` (`Entity_Collision`) | `entity_type==3` 时，若当前帧字段存在正值，会每 tick 扣 `weapon_hp`；HP<=0 时跳当前帧 `next`。区别于 P2-7 的粘附跟随逻辑。 |
| N-2 | state=0 空中转 frame 212 | `0x004138F0` (`Entity_Collision`) | `entity_type >= 0 && frame.state == 0 && y < 0` 时自动切 `frame=212`。区别于 P2-3 的 wpoint 持有者不匹配脱落。 |
| N-3 | type=2 静止落地转 frame 20 | `0x004138F0` (`Entity_Collision`) | `entity_type==2 && frame.state==2000 && y==0 && -0.1 < vx < 0.1` 时切 `frame=20`。 |
| N-4 | state=14 HP<=0 延迟处理 | `0x004138F0` (`Entity_Collision`) | `frame.state==14 && hp<=0` 时，在持有者/team/模式条件满足且 timer<=0 的情况下设置约 30 tick timer，并重置 frame timer。字段语义需进一步命名确认。 |
| N-5 | next=999 武器分流 | `0x004138F0` (`Entity_Collision`) | 当前帧 `next==999` 时，若 `y==0` 或 `entity_type!=0` 则 `frame=0`；否则切 `frame=212`。 |
| N-6 | 随机场景掉落武器池过滤 | 主循环 `0x00424970` 附近 | 随机掉落候选为 `oid>=100 && oid<200`，但 `oid=122/123` 有额外随机与模式过滤；生成 `oid=122` 后 HP 强制设为 200。 |
| N-7 | broken_weapon oid→碎片帧映射 | 主循环 `0x00424970` 后段 | broken_weapon 生成后会按原对象 `oid=100/101/120/121/122/123/124/150/151/213/217/218` 等选择不同随机 frame 范围，并设置随机速度；不是完全通用 opoint。 |
| N-8 | 特殊 oid 命中销毁/耗尽 | `0x0042C8C0` (`Entity_AI_Update`) | `oid=201` 命中非武器目标后自身失活；`oid=214` 命中非武器目标后自身 HP=0。 |
| N-9 | type=3 特殊数据替换 | `0x0042C8C0` (`Entity_AI_Update`) | `oid=209/213` 命中特定 `oid=200/203/205/206/207/215/216` 时，将目标 data pointer 替换成 `oid=209`，并同步 frame/team/owner。对应 P2-7 里暂跳过的特殊忍偶系分支，需要单独深挖。 |
| N-10 | itr kind=9 对 type=3 特判 | `0x0042C8C0` (`Entity_AI_Update`) | itr kind=9 命中 type=3 时播放 broken sound、设 `FrameDelay=-3`；目标 state=3005 则 frame=40，否则 frame=30 并清速度/同步 owner；命中非武器目标时攻击者 HP=0。 |
| N-11 | frame 字段 hit_Fa=5 友方批量生成 | `0x004030C0` (`Entity_FrameLogic`) | **字段已重新确认**：+2004 = frame.hit_Fa（ParseCharData 写入 +0x7D4）。hit_Fa=5 时，遍历存活友方非武器/非特殊目标，为每个目标生成 `oid=219`，继承 team/位置/方向，速度指向目标，并记录目标索引；原实体随后失活。 |
| N-12 | frame 字段 hit_Fa=6/9 敌方批量生成 | `0x004030C0` (`Entity_FrameLogic`) | hit_Fa=6 对敌方非武器目标生成 `oid=220`；hit_Fa=9 随机生成 `oid=221/222`，均带目标索引和速度/随机散射参数。 |
| N-13 | frame 字段 hit_Fa=7 同 oid 分裂 | `0x004030C0` (`Entity_FrameLogic`) | hit_Fa=7 查找当前实体相同 oid 的 data，生成同 oid 实体，新实体强制 `frame=40`、速度清零。 |
| N-14 | frame 字段 hit_Fa=4 近距离目标效果 | `0x004030C0` (`Entity_FrameLogic`) | hit_Fa=4 且目标索引有效、目标在近距离盒内时，自身速度清零并切 `frame=60`，同时向目标字段 `+228` 写入 `100`；字段语义待命名。 |
| N-15 | type=4/6 飞行中切 frame 40 | `0x00416240` (`Entity_FrameAdvance`) | `entity_type==4/6` 且当前帧 `state==1000`、`|vx|>9.0` 时，不等落地即强制切 `frame=40`。 |
| N-16 | type=3 帧字段 z 轴偏移 | `0x00416240` (`Entity_FrameAdvance`) | `entity_type==3` 且当前帧字段 `+2000 > 0` 时，每帧执行 `z_float += value - 50`；不同于 N-1 的 HP 消耗和 P2-7 的粘附跟随。 |
| N-17 | wpoint 字段 12 持有耐久/挣脱 | `0x0041B740` (`Collision_Check1`) | **字段已重新确认**：v33[12]=cpoint.decrease（非wpoint字段）。decrease>0每tick扣caught entity[+148]；decrease<0累加，<0时逃脱（双方frame=0，scatter速度，catcher.frame=181）。 |
| N-18 | wpoint 字段 15=-1 data 替换 | `0x0041B740` (`Collision_Check1`) | **字段已重新确认**：v33[15]=cpoint.throwinjury（非wpoint字段）。throwinjury==-1且throwvx!=0：caught.data替换为catcher.data，frame=0，释放抓取。 |
| N-19 | wpoint 字段 15>0 写被持对象字段 800 | `0x0041B740` (`Collision_Check1`) | **字段已重新确认**：v33[15]=cpoint.throwinjury。throwinjury>0且throwvx!=0：catcher[+800]=throwinjury（对应HealTimer）。 |
| N-20 | wpoint 字段 13 方向修正 | `0x0041B740` (`Collision_Check1`) | **字段已重新确认**：v33[13]=cpoint.dircontrol（非wpoint字段）。dircontrol==±1且entity[+136]==2(Trans.Wait==2)：根据左右输入修正朝向。 |
| N-21 | oid=122/123 特殊边界处理 | `0x0041DB60` (`Game_FrameUpdate`) | `oid==122/123` 且归属/队伍字段 `+836 > 0` 时，边界处理为 x 钳制到 `10` 或 `bgWidth-10`，不会像普通非角色对象在地面越界后立即失活。 |
| N-22 | itr kind=14 位移阻挡 flag | `0x0042C8C0` (`Entity_AI_Update`) | itr kind=14 不造成伤害，而是根据攻击者与目标 x/z 相对位置及目标速度设置 `+1000/+1004/+1008/+1012` 位移阻挡标记。 |
| N-23 | itr kind=15/16 拉扯/追踪分支 | `0x0042C8C0` (`Entity_AI_Update`) | itr kind=15/16 对非武器可扣血并切 `frame=200`；对 `entity_type==1/2/4/6` 武器会重置部分 frame、调整 vx/vz/y/vy 形成拉扯/追踪效果，并排除 `oid=201/202`。 |
| N-24 | kind=10/11 排除 oid=201/202 | `0x0042C8C0` (`Entity_AI_Update`) | itr kind=10/11 命中 `entity_type==1/4/6` 时，只有目标 oid 不是 `201/202` 才执行武器物理反应；此前仅记录了 oid=201/214 的命中销毁/耗尽。 |
| N-25 | 特殊 oid 7/8/51 合体拆分候选 | `0x00402340` (`sub_402340`) | `oid=7/8` 在 state=2、同队互补 oid、距离/冷却满足时合并为 `oid=51`，记录 `+808/+812/+816/+820/+824`，之后可拆回原 oid 并重置伙伴实体。更像角色/特殊实体变换，暂列低优先级候选，是否属于武器/特效 runtime 待 data 映射确认。 |
| N-26 | frame state 数据变身 | `0x0041DB60` (`Game_FrameUpdate`) | 非特殊 data 的 `state==9995` 会把当前实体 data pointer 替换成 `oid=50` 并 `frame=0`；`state=4000~4999` 替换为 `oid=state-4000` 并清 `+792`；`state=8000~8999` 替换为 `oid=state-8000`、`frame=0`、`+792=140`。属于通用 runtime data-transform，武器/特效关联需 data 映射确认。 |
| N-27 | state=9996 生成 217/218 碎片 | `0x0041DB60` (`Game_FrameUpdate`) | 非特殊 data 且当前帧 `state==9996`、方向字节满足时，生成 5 个特效/碎片实体：前 4 个 `oid=217`，第 5 个 `oid=218`，随机位置/速度/frame/facing，并写 `+236=6`。区别于 N-7 的 broken_weapon 死亡碎片映射。 |
| N-28 | 1100/1200 编码 frame 联动 owner/child | `0x0041DB60` (`Game_FrameUpdate`) | 当前 `frame/100==11/12` 时，将自身及 `OwnerEntityIndex==当前实体` 的子实体字段 `+8` 写为 `1100-currentFrame`，随后自身 `frame=0`；字段 `+8` 语义待命名，属于 owner-linked runtime 特效/状态联动候选。 |
| N-29 | 通用 opoint 多实体生成路径 | `0x0041DB60` (`Game_FrameUpdate`) | 帧字段 `+2044/+2172/+2264` 附近的 opoint runtime：按 opoint oid 生成 1 个或多个实体，支持 count/facing 编码、owner/team/方向继承、vx/vy/vz、z 多发散布、互相 hit-rest、`kind==2` 链接字段 `+152/+156/+160`，以及 spawned `oid=5/52` 的 HP/MP 初始化。区别于 N-11~N-14 的 `+2004` 特殊分支。 |
| N-30 | 输入组合触发 oid=998 团队效果 | `0x0041DB60` (`Game_FrameUpdate`) | 低索引存活角色的输入历史 `9,0,9,0` / `9,9,9` / `5,9,5` 会生成 `oid=998`，frame 分别为 `0/2/4`，并对同队非特殊实体写 `+1020/+1024` 随机坐标或 `+1028=1/0`。更像角色/特效 runtime，暂列低优先级候选。 |
| N-31 | state 13/18/19 转场生成 oid=999 特效 | `0x0041DB60` (`Game_FrameUpdate`) | 进入 `state==13` 或 `frame=200` 时播放 sound 15 并生成 15 个 `oid=999` 特效，frame 分布 `120/125/130/135`；`state==18/19` 转场或持续时生成 `oid=999` frame 140 特效，数量为进入时 7 个、持续时概率 1 个。属于状态转场视觉/特效 runtime，非 N-7 broken_weapon。 |

#### 覆盖置信度评估

当前武器/特效相关逻辑已追加扫描到 `N-1 ~ N-31`。本轮已覆盖已知武器类型/oid、`wpoint`、`itr kind`、`frame +2004`、`Game_FrameUpdate` 特殊 state/data transform/opoint/spawn 主要路径；旧的 85% 覆盖率结论已失效，后续应以逐项实现/排除为准。

#### 实现状态（经反汇编唯一权威 ntsd24_full_disasm.txt 验证）

| # | 状态 | 实现位置 | 备注 |
|---|------|---------|------|
| N-1 | ✅ 已实现 | `LF2WeaponBase.EntityCollision()` | 反汇编 0x41395F；entity_type==3 && frame.mp>0 → HP-=mp；HP<=0→frame=next |
| N-2 | ✅ 已实现 | `LF2WeaponBase.EntityCollision()` | 反汇编 0x413A2D；state==0 && y<0 → frame=212 |
| N-3 | ✅ 已实现 | `LF2WeaponBase.EntityCollision()` | 反汇编 0x413A55；type==2 && state==2000 && y==0 && |vx|<0.1 → frame=20 |
| N-4 | ✅ 已实现 | `LF2WeaponBase.EntityCollision()` | 反汇编 0x413AB7；state==14 && HP<=0 → FrameDelay=30, HitStun=0 |
| N-5 | ✅ 已实现 | `LF2WeaponBase.SimTransit()` | 反汇编 0x413B84；next==999 pre-Trans 拦截；y<0&&type==0→212，否则0 |
| N-6 | ✅ 已实现 | `SimulationWorld.RandomWeaponDropTickAll()` | 反汇编 ~0x421655；场上武器/特效实体<4 且 rand(200)==0 时，从 CharacterAnimtorManager 收集 oid∈[0x64,0xC8) 候选，oid=0x7A/0x7B 以 rand(2)==0 过滤，随机选一个通过 OPointCreateTask 生成。**待补**：反汇编 0x42167F `dword_449094` 游戏模式字段（1/2/3/4）对 oid=0x7A/0x7B 的额外过滤，等游戏模式系统实现后补齐 |
| N-7 | ✅ 已实现 | `LF2Character.ApplyBrokenWeaponFragments()` | 反汇编 0x4228B8；_brokenWeaponFlag<0 时按源 oid 生成 oid=999 碎片（0x96=13个/frame 0-5，0x97=15个，0x65/0xDA=7个，0x64/0xD5/0xD9=5个，0xC9=3个，0x78/0x7C=3个，0x79=4个，0x7A/0x7B=9个），随机速度/位置 |
| N-8 | ✅ 已实现 | `LF2SpecialAttack.ApplyPostHitSelfDestruct()` | 反汇编 0x0042DAAC；oid=201→deactivate，oid=214→HP=0，被非武器命中时 |
| N-9 | ✅ 已实现 | `LF2SpecialAttack.Hit_State3000()` | pseudoC pos~810786；attacker.oid==209 且 self.oid∈{200,203,205,206,207,215,216}：self.data=karasu数据/Team同步/HealTimer同步/frame=40/PN=40 |
| N-10 | ✅ 已实现 | `LF2SpecialAttack.Hit()` | kind=9 命中 type=3：broken sound、attacker.FrameDelay=-3；3005→frame=40+NoBounce=true；else→frame=30+NoBounce=true+清速度+同步owner。修复 ApplyPostHitSelfDestruct ObjectId→_objectId bug |
| N-11 | ✅ 已实现 | `LF2SpecialAttack.ApplyHitFa11Spawn()` + `ApplyHitFa11FindTarget()` + `ApplyHitFa11Tracking()` | case 11：一次性生成14实体（oid=211/221/212）→ 寻找最近敌方（Wrapper比较同data，abs(z)<=2，非武器，HP>0）→ 写OwnerEntityIndex；per-frame：vx±=2.0（基于vx符号），vx钳制±17，vz钳制±1.4，facing |
| N-12 | ✅ 已实现 | `LF2SpecialAttack.ApplyHitFa2_14Tracking(12)` | per-frame 追踪：vx±0.7，vz±0.4（±5死区），vz*=0.7142857，vz dead zone 40→±1.0，vx钳制±14，vz钳制±1.4，vz二次钳制±2.2 |
| N-13 | ✅ 已实现 | `LF2SpecialAttack.ApplyHitFa13Spawn()` | 遍历敌方存活非武器目标，随机选目标 StableId，生成 oid=228（直接速度继承，y+=rand(7)-3），deactivate self；OwnerEntityIndex 通过 StableId 传递（异步队列限制） |
| N-14 | ✅ 已实现 | `LF2SpecialAttack.ApplyHitFa2_14Tracking(14)` | 同 N-12 追踪逻辑（vz*=0.7142857，vz dead zone 40→±1.0，vx钳制±14，vz钳制±1.4）+ vy 钳制±1.5 |
| N-15 | ✅ 已实现 | `LF2Weapon.WeaponFlightPhysics()` 步骤4 | 反汇编 0x416466；type==4/6 && state==1000 && |vx|>9 → frame=40 |
| N-16 | ✅ 已实现 | `LF2Weapon.WeaponFlightPhysics()` 步骤3 | 反汇编 0x41637D；type==3 && hit_j>0 → vz+=hit_j-50 |
| N-17 | ✅ 已实现 | `LF2Character.States.partial.cs` `ApplyCollisionCheck1CaughtLogic()` | 反汇编 sub_41B740；v33[12]=cpoint.decrease：>0每tick扣 _caughtDecayAccum，<0累加后若<0则逃脱（caught/catcher frame=0，catcher.vy=±2.25，caught.vy=-2.125，catcher.frame=181） |
| N-18 | ✅ 已实现 | 同上 `ApplyCollisionCheck1CaughtLogic()` | throwinjury==-1 且 throwvx!=0：caught.FrameCache 替换为 catcher.data（GetCharacterConfig），frame=0，释放抓取 |
| N-19 | ✅ 已实现 | 同上 `ApplyCollisionCheck1CaughtLogic()` | throwinjury>0 且 throwvx!=0：catcher.HealTimer = throwinjury（对应反汇编 catcher[+800]） |
| N-20 | ✅ 已实现 | 同上 `ApplyCollisionCheck1CaughtLogic()` | dircontrol==1/-1 且 Trans.Wait==2（对应entity[+136]==2）：根据 Controller.IsRight/IsLeft 修正 PS.dir |
| N-21 | ✅ 已实现 | `LF2WeaponBase.SimTU()` | oid==122/123 且 Team>0(对应+836>0)时 x 钳制到 [10, bgWidth-10]，替代普通越界失活 |
| N-22 | ✅ 已实现（阈值已验证） | `LF2Character.Hit.partial.cs` | 反汇编 0x0042F08C `lea eax,[esi+5]` / 0x0042F103 `lea eax,[esi+2]`：x阈值=5，z阈值=2，与C#实现完全一致 |
| N-23 | ✅ 已实现 | `LF2Character.Hit.partial.cs` + `LF2WeaponBase.WhirlwindForce(itr, attacker)` | 武器侧累加式 vx±1/vz±0.5（由 attacker-target 位置差决定方向），y≥-2时y=-2/vy=0，vy>-6时vy-=3 |
| N-24 | ✅ 已实现 | `LF2Weapon.cs` | kind=10/11 武器侧 oid=201/202 排除检查修复：改为 _objectId（ObjectId 总为0的bug已修复） |
| N-25 | ✅ 已实现 | `LF2Character.ApplyMergeLogic()` | 反汇编 sub_402340；oid=7/8、HP>0、state=2、HP<177、_mergeTimer==0 时找同队互补 oid 伙伴（距离<50/8），合并为 oid=51，HP/PP求和钳制，frame=122，timer=4500；oid=51 且 _mergeFlag==1、frame<9或>260、timer<=0 时拆回原 oid，伙伴 frame=112、HP减半 |
| N-26 | ✅ 已实现 | `LF2WeaponBase.ApplyDataTransformByState()` + `LF2Character.SimTransit()` | 4000~4999/8000~8999：无entity_type守卫，所有实体（WeaponBase）；state=9995：仅角色（LF2Character，反汇编0x4219F1守卫） |
| N-27 | ✅ 已实现 | `LF2Character.SimTransit()` → `SpawnFragments9996Character()` | state==9996 仅角色（反汇编0x421B05守卫），facing==right 时生成5个碎片（前4=oid217，第5=oid218），HitStun=6 |
| N-28 | ✅ 已实现 | `LF2WeaponBase.ApplyFrameSyncToChildren()` | Frame.N/100==11/12：遍历OwnerEntityIndex==StableId子实体写FluteWeight=1100-Frame.N，然后self同写+Frame.N=0 |
| N-29 | ✅ 已实现 | `LF2WeaponBase.OnFrameTransit()` | 升级迭代 frameData.opoints 列表（全部 opoint），dvz 使用 Dirh()*op.dvz，兼容旧单 opoint 路径 |
| N-30 | ✅ 已实现 | `LF2Character.ApplyDeathRespawn()` + `ApplyInputSequenceRespawn()` | 反汇编 0x421085；死亡触发：state=0x0E、HP<=0、ShakeTimer∈(0,5)、_respawnTriggerCount>0 → HP链式复制、team=1、frame=219、spawn oid=998、同队传播；输入触发：_inputSeq 匹配 9,0,9,0/9,9,9,9/9,5,9,5 → spawn oid=998 |
| N-31 | ✅ 已实现 | `LF2Character.ApplyFrozenBurningParticles()` | state=13/frame=200进入时：PlaySound(15)+15个oid=999(frame 120/125/130/135)；state=18/19进入时7个，持续25%概率1个，frame=140 |

仍存在不确定性的区域：
- N-11~N-14：字段已重新确认为 frame.hit_Fa（+0x7D4），dat 文件中有实际使用（weapon9.dat hit_Fa=12，criminal.dat hit_Fa=14），Entity_FrameLogic 全部 case（1~14）已实现，含 hit_Fa=3（per-frame nearest-enemy search + case 1 tracking）。

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

## 记忆系统使用规则（强制）

**工具**：`mem0___search_memory` / `mem0___add_memory`（通过 `.factory/mcp.json` 中的 mem0 MCP server 提供）

### 会话开始时（强制）
每次开始新任务前，**必须**先调用 `mem0___search_memory` 检索相关历史记忆，关键词包括：
- 当前要处理的模块名、函数名
- 相关反汇编地址或字段偏移
- 相关 bug 编号（如 P0-x、N-x）

检索结果用于：了解该模块的历史决策、已知 bug、对齐状态，避免重复分析或走回头路。

### 任务完成后（强制）
每次完成编码、分析或 bug 修复后，**必须**调用 `mem0___add_memory` 记录：
- 实现了什么、修复了什么
- 关键反汇编对齐结论
- 遗留问题或待确认项

### 补充记忆（按需）
用户说"记住这个"时，立即调用 `mem0___add_memory`。

---

---

## NTSD C++ 工程对齐规则（强制）

### 项目背景
- C++ 工程路径：`J:\QQFile\NTSD2.4\ntsd_cpp`
- 反汇编权威来源：`J:\QQFile\NTSD2.4\ntsd24_full_disasm.txt`
- 伪C参考：`J:\QQFile\NTSD2.4\ntsd24_pseudoc.txt`
- 进度文档：`J:\QQFile\NTSD2.4\ntsd_cpp\ALIGNMENT_REPORT.md`
- 目标：loading → characterSelect → battle 全流程，逻辑行为和游戏表现与原版 NTSD **100% 一致**
- oh-my-task 项目：`NTSD-CPP`，版本：`NTSD_CPP_Battle`

---

### 每次会话开始（强制三步）
1. `oh-my-task___get_current_task` → 确认当前在哪个子任务
2. `mem0___search_memory` → 检索当前函数/模块历史记录
3. 从断点继续，不重新解释背景

---

### 处理顺序
1. 按**系统模块**分组处理，不单独处理孤立函数
2. 优先处理 ALIGNMENT_REPORT.md 中已标记 ✅ 的函数（重新审计）
3. 同一模块内所有函数处理完毕后，才进入下一模块
4. **串行处理**，不并行，避免函数依赖冲突

---

### oh-my-task 任务结构

```
系统模块（主任务）
    ├── 子任务1：生成函数验证清单（从反汇编提取所有分支）
    ├── 子任务2：归类所有 call 指令（三选一处理）
    ├── 子任务3：逐条实现清单条目（修改 C++ 代码）
    ├── 子任务4：生成测试清单
    └── 子任务5：用户执行测试 → 用户确认 → complete_task
```

**强制规则：**
- 只处理当前激活的子任务，不跳跃到下一个
- 子任务完成后，AI 只能说"请确认是否通过"，**不能自己调用 `complete_task`**
- 用户说"完成"后，AI 才调用 `oh-my-task___complete_task`
- 主任务完成同样需要用户确认

---

### 函数拆解规则

**验证清单格式（每个函数必须生成）：**

| 分支编号 | 反汇编地址 | 触发条件 | EXE 行为 | C++ 对应行号 | 状态 |
|---------|-----------|---------|---------|------------|------|

- 条目来源：从反汇编所有跳转指令（jz/jnz/jl/jg/je 等）**机械提取**，不能主观概括
- 每条必须带反汇编地址，不能省略

**call 指令处理规则（三选一，不能省略）：**
1. 已在模块内其他函数覆盖 → 标注"见函数 XXX，已覆盖"
2. 需要单独拆解 → 列为新子任务，当前位置标注"依赖子任务 XXX"
3. 简单工具函数 → 内联展开，直接写出行为

**模块完成条件：** 模块内所有函数的所有 `call` 都已归类，无悬空调用。

---

### ✅ 标记条件（全部满足才能标记）
1. 验证清单所有条目的"C++ 对应行号"列已填写
2. 所有 `call` 已归类，无悬空
3. 测试清单已生成并由**用户**执行
4. **用户**确认测试通过

**禁止行为：**
- AI 不能自己标记 ✅
- AI 不能自己调用 `oh-my-task___complete_task`
- AI 不能说"对齐完成"然后继续下一个函数
- 当前函数未经用户确认前，不得开始下一个函数

---

### 测试清单格式

| 测试编号 | 触发方式 | 原版 NTSD 预期表现 | ntsd_new.exe 实际表现 | 结果 |
|---------|---------|-----------------|-------------------|------|

用户对照原版游戏逐条测试，填写"实际表现"和"结果"列，AI 不参与判断。

---

### 任务完成后（强制）
调用 `mem0___add_memory` 记录：
- 实现了什么、修复了什么
- 关键反汇编对齐结论
- 遗留问题或待确认项

---

---

## 执行者与监督者协作规则（强制）

### 角色分工
- **执行者**（当前会话）：负责按步骤处理任务，不自行判断完成
- **监督者**（独立会话）：负责审查执行结果，给出通过或修正意见

### 执行者强制规则

**每完成一个子任务后，必须立即：**

1. 调用 `mem0___add_memory` 记录以下内容：
   - 子任务编号和名称
   - 处理了哪个函数/模块
   - 具体做了什么（清单条目数、修改了哪些代码行）
   - 发现的问题或差异
   - 是否有悬空 call 未处理
   - 格式前缀：`[执行结果][模块X][函数名]`

2. 调用 `oh-my-task___get_current_task` 确认任务状态

3. 告知用户："子任务已完成，请去监督者会话审查"

**执行者禁止行为：**
- 不能自己调用 `oh-my-task___complete_task`
- 不能自己标记 ✅
- 不能在用户确认前继续下一个子任务
- 上下文压缩后必须重新执行会话开始三步（get_current_task → search_memory → 从断点继续）

### 监督者审查流程

用户说"审查 XXX 模块/函数"时：

1. 调用 `mem0___search_memory`，关键词 `[执行结果][模块X][函数名]`
2. 对照反汇编验证执行者的结论
3. 输出审查结果：**通过** 或 **有问题：XXX**
4. 有问题时，给出具体修正指令，用户带回执行者

### 上下文压缩后的恢复（强制）

无论何时感觉丢失了上下文，立即执行：
1. `oh-my-task___get_current_task` → 知道当前子任务
2. `mem0___search_memory` 关键词"当前模块 执行结果" → 恢复进度
3. 继续未完成的子任务，不重新开始

---

## NTSD C++ 100% 对齐方案（强制）

### 前置条件（必须先完成，否则后续对齐不可信）

| 编号 | 问题 | 解决方式 |
|------|------|---------|
| P0-1 | 浮点精度差异 | `_controlfp(_PC_80, _MCW_PC)` — x87 80位精度 |
| P0-2 | 帧率精度差异 | `timeBeginPeriod(1)` + `timeGetTime()` 忙等待 33ms |
| P0-3 | 渲染像素差异 | SDL2 改用 Surface blitting + `SDL_SetColorKey` |
| P0-4 | 未覆盖函数 | `calls.json` 扫描每个模块完整调用链，确保无遗漏 |

---

### 机械匹配清单格式（强制）

旧格式（禁止使用）：
- "触发条件 + EXE行为 + C++对应" — AI 主观判断，不可信

新格式（强制）：

| 分支编号 | 反汇编原文（指令+操作数） | 常量十六进制值 | C++ 对应代码原文 | 完全一致 |
|---------|------------------------|--------------|----------------|---------|

**规则：**
- 浮点常量从反汇编文本直接提取十六进制定义（如 `dq 3FF0000000000000h`），不靠 AI 推断
- `jl/jle/jg/jge` 直接看指令原文，对应 `</<=/>/>= `，不允许 AI 翻译
- `movsx` vs `movzx` 直接看指令，决定有无符号
- 不允许"语义等价"，只允许"完全一致"
- 有疑问的条目标"待确认"，不能标 ✅
- 每条差异必须带反汇编地址

---

### 误差控制机制（双重验证）

每条有浮点常量、符号扩展、边界条件的分支，必须同时：
1. AI 给出解读结论
2. `disasm_extract.py` 机械提取十六进制值验证
3. 两者一致才标 ✅，不一致标"待确认"

---

### 处理顺序

| 优先级 | 模块 | 原因 |
|--------|------|------|
| 0 | P0-1/P0-2/P0-3/P0-4 | 前置条件 |
| 1 | 模块D（Entity_FrameAdvance + Entity_Collision） | 物理基础 |
| 2 | 模块C（Entity_InputProcess） | 输入基础 |
| 3 | 模块E（Entity_FrameLogic） | opoint/hit_Fa |
| 4 | 模块F（碰撞子函数） | 命中处理 |
| 5 | 模块A（主循环） | 调度层 |
| 6 | 模块B/G（生命周期/冷却） | 辅助逻辑 |

---

### 模块完成条件（全部满足）
1. P0-1/P0-2/P0-3/P0-4 已完成
2. 所有分支的"完全一致"列已填写，无"待确认"
3. 所有 call 已归类，无遗漏
4. 行为测试通过（用户确认明显错误已消除）

---

## Common Pitfalls
- Do not commit `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`
- Do not rename serialized fields — breaks existing scenes/prefabs
