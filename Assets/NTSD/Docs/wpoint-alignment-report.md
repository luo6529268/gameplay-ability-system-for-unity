# WPoint / Weapon 模块：NTSD 2.4 反汇编 vs Unity 对齐报告

> 更新日期：2026-03-31
>
> **对齐原则**：能直接对齐反汇编逻辑的必须对齐；因 Unity 框架限制（继承/组件/异步等）无法完全对齐时，只要求最终运行结果与反汇编等价，差异需在本文档中注明。
>
> **权威来源**：`J:\QQFile\NTSD2.4\ntsd24_full_disasm.txt`（NTSD 2.4 完整反汇编，522 个函数）
>
> Unity 源码：`LF2Weapon.cs` / `LF2WeaponBase.cs` / `LF2WeaponPointFactory.cs` / `LF2WeaponPointModule.cs` / `NTSDGlobal.cs`
>
> **反汇编关键函数（已人工逐条核实）：**
> | 函数 | 地址范围 | 用途 |
> |------|----------|------|
> | `ParseCharData` | `0x0040CA30~0x0041104D` | DAT 解析（wpoint 字段、weapon_strength_list） |
> | `AI_Process2` | `0x0041AAC0~0x0041B2B8` | wpoint 运行时处理（kind=1/2/3 持有/投掷/丢弃） |
> | `SceneManager_Init` | `0x00417AD0~0x00419B32` | 渲染/HUD（含 wpoint 显示计数器，非物理） |
> | `GameMode_Process` | `0x0041BDA0~0x0041BEFB` | 角色间 ITR 碰撞主循环（含 attacking 过滤） |
> | `sub_419F80` | `0x00419F80~0x0041AAB2` | ITR 命中处理 |
> | `sub_4063B0` | `0x004063B0~0x004089FE` | 武器碰撞逻辑（HP 扣减、vrest 检查） |
> | `Entity_Collision` | `0x004138F0~0x00413E09` | 实体碰撞 |
> | `Entity_AI_Update` | `0x0042C8C0~0x0042FA63` | 武器拾取 AI + 持有攻击（weapon_strength_list 使用点） |
> | `Random_Int` | `0x00419D40~0x00419D8D` | 随机数生成（用于 kind=3 丢弃速度） |

---

## 一、已修复项（P0/P1/P2）

### 1.1 ✅ ProcessAttack() — 轻武器持有攻击

- **反汇编** `GameMode_Process (0x0041BDA0)`：武器 state==1001 且持有者 wpoint.attacking>0 时进入碰撞。
- **修复**：`LF2Weapon.ProcessAttack()` 实现：用 `_weaponStrengthList[attacking-1]` 参数构造 ITR，通过 `SceneQuery.QueryBodies` 找目标，调用 `character.Hit()`。
- **修复**：`LF2WeaponBase.Act()` 里 attacking 检查移至 `if (GetState()==WeaponOnHand && IsLight && wpoint.attacking>0)`，不再被 `fwpoint.kind==2` 限制。

### 1.2 ✅ weapon_strength_list 解析与注入

- **反汇编** `ParseCharData (0x0040DF24)`：解析 `<weapon_strength_list>` 块。
- **修复**：
  - `WeaponStrengthEntry` 加入 `LF2FrameData.cs`
  - `LF2CharacterData.weapon_strength_list` 字段加入 `LF2CharacterData.cs`
  - `CharacterAnimtorManager.ExtractWeaponParameters()` 从 `Lf2DatBlock` 解析并填充
  - `LF2ObjectPointFactory` 在武器创建后调用 `SetWeaponStrengthList()` 注入

### 1.3 ✅ WPoint kind=3 强制丢弃

- **反汇编** `AI_Process2 (0x0041B21D~0x0041B28D)`：双方 arest=0，随机帧，随机速度。
- **修复**：`LF2WeaponPointFactory.ProcessForceDropPoint()` 实现全部逻辑。

### 1.4 ✅ weapon_hp / weapon_drop_hurt / sound 从 DAT 读取

- **反汇编** `ParseCharData 0x0040D8F0~0x0040DA46`。
- **修复**：
  - 字段加入 `LF2CharacterData`（`weapon_hp`, `weapon_drop_hurt`, `weapon_hit_sound`, `weapon_drop_sound`, `weapon_broken_sound`）
  - `CharacterAnimtorManager.ApplyWeaponProperty()` 解析写入
  - `LF2WeaponBase.InitializeHealth()` 从 `GetCharacterData(_objectId)` 读取

### 2.1 ✅ 重武器 Interaction 状态过滤

- **反汇编** `sub_4063B0 (0x00407309~0x00407386)`：state 1004/2004（地面）才触发碰撞。
- **修复**：`LF2Weapon.Interaction()` 正确区分 1004/2004 地面碰撞，旧类 `LF2HeavyWeapon`（仅允许 2000）已删除。

### 2.2 ✅ kind=2 投掷 arest 重置条件

- **反汇编** `AI_Process2 (0x0041B155)`：仅当 dvx≠0 且为重武器时重置双方 arest。
- **修复**：`LF2WeaponBase.Act()` 中加入 `if (wpoint.dvx != 0 && IsHeavy)` 条件。

### 2.3 ✅ wpoint 显示计数器（不需移植）

- 调试功能，不移植。

### 2.4 ✅ hit-cooldown 默认值

- **反汇编** `0x0042266F`：正常命中 10 帧。
- **修复**：`NTSDGlobal.Default.Weapon.VRest` 从 9 改为 10。

### 2.5 ✅ wpoint.attacking 读取来源

- **反汇编** `GameMode_Process 0x0041BDF8`：读持有者角色当前帧 wpoint.attacking。
- **验证**：Unity `Act(wpoint, ...)` 中 `wpoint` 参数即来自持有者角色帧，读取方向已正确；额外修正了 attacking 检查不应被 `fwpoint.kind==2` 限制的问题（见 1.1）。

---

## 二、类结构变更

| 变更 | 说明 |
|------|------|
| `LF2LightWeapon.cs` + `LF2HeavyWeapon.cs` **已删除** | 合并为 `LF2Weapon.cs` |
| `LF2Weapon._weaponType`（int） | 对应反汇编 `[+6F8h]`，0=轻，1=重 |
| `LF2ObjectLogicPool` | 统一创建 `LF2Weapon`，按 type 调用 `SetWeaponType()` |
| `LF2States.HeavyWeaponInSky = 2000` | 新增常量 |
| `LF2States.HeavyWeaponOnGround = 2004` | 新增常量 |

---

## 三、反汇编未找到对应逻辑（FLF 独有，暂标待确认）

| 编号 | 条目 | 说明 |
|------|------|------|
| A.1 | `attacked()` / `killed()` 委托给持有者 | FLF weapon.js 特有，NTSD 反汇编未找到等效路径 |
| A.2 | `Act()` 返回值 hit 分支 → `inc_wait(hit_stop=3)` | NTSD 的 hit-pause 机制不同（0F0h 字节数组），待确认 |
| A.3 | Interaction 命中后 bounce-back vx 设置 | FLF `vx=-3` 反弹，NTSD 反汇编未找到对应 |
| A.4 | `visualeffect_create` 在 Hit accept 后调用 | NTSD 视觉效果机制待确认 |
| A.5 | `stat.picking++` 拾取计数 | NTSD 反汇编未见 |
| A.6 | 精灵边框修正（borderleft/right/top/bottom） | FLF 渲染特有，NTSD 无等效 |

---

## 四、遗留待办（依赖其他系统）

| 编号 | 问题 | 依赖 | 反汇编依据 |
|------|------|------|-----------|
| B.1 | state 3003 命中 itr.kind==2：建立投射物-角色持有关系 | 抓取系统（未完成） | `Game_FrameUpdate 0x422729` |
| B.2 | AoE 多目标同帧命中广播 vrest=40 | 全局碰撞主循环收集命中列表 | `Game_FrameUpdate 0x422857` |
| B.3 | ✅ kind=1 持有时角色被击倒(Falling/BeingCaught)触发武器脱落：arest=0 + Random_Int(16) 随机帧 + 速度继承 * 1/3 | `LF2WeaponBase.Act()` 开头加前置检查 | `AI_Process2 0x41AFFC~0x41B08D` |

---

## 五、常量汇总

| 常量 | 反汇编值 | Unity 当前值 | 状态 |
|------|----------|-------------|------|
| hit-cooldown 正常值 | 10（`0Ah`，0x0042266F） | 10 | ✅ |
| hit-cooldown 特殊值（AoE） | 40（`28h`，0x0042285F） | 未实现（见 B.2） | ⏳ |
| kind=3 武器帧 | rand(6) → [0,5] | rand(6) ✅ | ✅ |
| kind=3 vx | rand(7)-3 → [-3,3] | rand(7)-3 ✅ | ✅ |
| kind=3 vy | -rand(4) → [-3,0] | -rand(4) ✅ | ✅ |
| kind=3 vz 乘数 | **0.2**（dbl_4433E0） | ~~0.3f~~ → **0.2f** ✅ | ✅ |

---

## 六、深度验证发现的逻辑差异（新增）

### C.1 ⚠️ kind=2 投掷路径与 weapon.type 的实际对应关系

反汇编 `AI_Process2 0x41B0A9~0x41B166` 完整 dispatch：

| weapon.type | 行为 | 帧 | arest |
|-------------|------|----|-------|
| 1（重武器） | Heavy throw path | 固定 40 | 双方归零 |
| 4（特殊重武器） | Heavy throw path | 固定 40 | 双方归零 |
| 6（饮料类） | Heavy throw path | 固定 40 | 双方归零 |
| 2（轻特殊武器） | Light throw path | Random_Int(6) [0,5] | 双方归零 |
| 0（普通轻武器） | **无投掷路径，直接跳 kind=3** | — | — |

- **Unity 当前实现**：`if (fwpoint.kind == 2)` 分支内执行投掷逻辑，轻重武器都能投掷（依据 dvx 是否非零）。这与反汇编不符——type=0 轻武器在 dvx 非零时应进 kind=3，不应进投掷路径。
- **影响**：type=0 武器（LF2Weapon type=0）现在可以被"投掷"，但反汇编里它只会被"强制丢弃"（kind=3 路径）。
- **已修复**：`Act()` 中按 weapon.type 分流：type=1/4/6→heavy throw（帧40，双方arest归零），type=2→light throw（随机帧，双方arest归零），type=0→NeedsKind3Drop标志→ProcessForceDropPoint。`SetPos()` 偏移调用已移除（position sync在持有阶段已完成）。
