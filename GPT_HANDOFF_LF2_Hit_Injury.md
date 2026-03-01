# LF2Character 受击系统交接文档（给 GPT 直接开工）

## 1. 任务目标

在 Unity 项目中补完 `LF2Character` 的受击系统，使 `Hit(...)` / `Injury(...)` 更接近 FLF/NTSD 语义，并按既定 8 项全部完成。

要求：
- 直接开工，不反复确认（仅在“阻塞性不确定”时提一个澄清问题）
- 最小改动、局部实现、可编译
- 复用现有常量、模块和函数，不引入新依赖

---

## 2. 关键词速查（关键语义）

- `itr.kind`：受击类型主分支
  - `0/4/9`：普通攻击类
  - `10/11/15/16`：特殊攻击类
  - `5000-5999`：NTSD 特殊伤害段
  - `6000-6999`：NTSD 特殊跳帧段
- `caught_cpointhurtable()`：被抓状态下是否可受伤（必须复用）
- `bdefend`：防御累计值（用于破防）
- `fall`：倒地累计值（用于受伤/倒地/KO 分流）
- `vaction`：受击后动作帧（需做有效帧检查）
- `dvx/dvy/dvz`：受击速度（击退/击飞）
- `arest/vrest`：命中冷却/受击冷却控制
- `PostEffect`：受击后视觉/状态后处理（含重武器掉落时机）

---

## 3. 项目结构与关键文件

### 主修改文件
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
  - `Hit(InteractionArea itr, LF2LivingObject attacker, Vector3 attackerPos, PhysicsState.FlfVolume vol)`
  - `Injury(int damage)`
  - `caught_cpointhurtable()`

### 读取参考（不一定修改）
- `Assets/NTSD/Scripts/Animation/Character/LF2HitCountersModule.cs`
  - 受击计数器：`Fall` / `Bdefend` 及 `Add/Reset`
- `Assets/NTSD/Scripts/Simulation/NTSDGlobal.cs`
  - `NTSDGlobal.Gameplay.DefendBreakLimit`
  - `NTSDGlobal.Gameplay.FallKO`
  - `NTSDGlobal.Default.Fall.Value`
- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
  - `InteractionArea` 字段：`dvx/dvy/dvz`, `injury`, `fall`, `vaction`, `effect`, `bdefend`, `arest`, `vrest`

---

## 4. 当前状态（接手前事实）

- 当前阶段仅完成分析，未完成代码落地。
- `Hit(...)` 仍有多个 TODO 或简化分支。
- `Injury(...)` 目前仅简单减 HP，缺少完整保护与计数更新。
- 既定 8 项任务均待完成。

---

## 5. 既定执行计划（8 项，按顺序）

1. **Hit 前置状态检查**
   - state10（被抓）接入 `caught_cpointhurtable()`
   - state14（躺地）受击规则
   - state19 + attacker state3000 火焰免疫路径确认

2. **`kind 5000-5999` / `6000-6999` 完整化**
   - 5000 段：明确伤害处理路径（建议统一走 `Injury`）
   - 6000 段：跳帧边界检查 + 无效帧兜底

3. **`kind 0/4/9` 普攻主流程**
   - 防御判定（含方向/条件）
   - `dvx/dvy/dvz` 应用
   - `bdefend` 累加与 `DefendBreakLimit` 破防

4. **Fall 系统**
   - `fall` 累加、重置策略
   - 受伤动作 vs 倒地/KO（`FallKO`）分流

5. **特殊 kind 分支补全**
   - `10/11/15/16` 补齐最小完整状态迁移，不留空分支

6. **PostEffect 实装**
   - 写回 effect 相关参数（Time/Dvx/Dvy 等）
   - 处理重武器掉落时机（按受击类型/倒地时机）

7. **`Injury(int damage)` 强化**
   - HP 扣减下限保护（不得小于 0）
   - 更新已有计数/统计（只用项目中已存在字段/模块）

8. **诊断与修错**
   - 对改动文件做 LSP/编译诊断
   - 修复新增错误，不扩散重构

---

## 6. 实施约束（必须遵守）

- 最小改动，优先局部补全，不做大重构
- 复用现有常量与模块，不新增第三方依赖
- 保持现有代码风格（C# Allman、现有命名）
- 不修改无关系统，不触碰第三方目录（除非绝对必要）
- 不使用类型/错误屏蔽式“糊补丁”

---

## 7. 验收标准（完成定义）

- `Hit(...)` 对上述 kind 与状态分支均有可执行逻辑（非 TODO、非空分支）
- `Injury(...)` 具备 HP 下限保护，且有可用计数更新
- 修改后无新增诊断错误（至少文件级）
- 输出变更说明：
  - 改动函数列表
  - 每项功能实现摘要
  - 诊断结果

---

## 8. 可直接复制给 GPT 的执行指令

```text
请根据本文件《LF2Character 受击系统交接文档（给 GPT 直接开工）》执行实现。

要求：
1) 直接开始编码，不要反复确认；仅在“阻塞性不确定”时提一个澄清问题。
2) 严格按文档第 5 节的 8 项顺序实施。
3) 只在必要范围改动，以 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` 为主。
4) 完成后给出：
   - 修改函数列表
   - 8 项各自实现摘要
   - 诊断结果（有无新增错误）
```
