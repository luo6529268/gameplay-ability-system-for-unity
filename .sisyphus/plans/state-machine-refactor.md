# 状态机重构 - 对象内部化

## TL;DR

> **Quick Summary**: 将状态机从外部单例/静态类移入各对象内部，对齐 FLF 设计（每个对象类型管理自己的 states）
> 
> **Deliverables**:
> - 重构 `LF2LivingObject` 基类，添加状态机基础设施
> - 重构 `LF2Character`，内部化 Character 状态处理
> - 重构 `LF2WeaponBase`，内部化 Weapon 状态处理
> - 重构 `LF2SpecialAttack`，内部化 SpecialAttack 状态处理
> - 删除/废弃 `CharacterStates` 单例、`LF2WeaponStates`、`LF2SpecialAttackStates` 静态类
> 
> **Estimated Effort**: Large
> **Parallel Execution**: NO - sequential (基类必须先完成)
> **Critical Path**: Task 1 → Task 2 → Task 3 → Task 4 → Task 5 → Task 6

---

## Context

### Original Request
用户希望重构状态机设计，将状态处理逻辑从外部类移入对象内部，对齐 FLF 原版设计。

### 当前问题
1. **职责混杂**: `CharacterStates` 单例管理所有对象类型的状态
2. **命名误导**: `CharacterStates` 却包含 Weapon/SpecialAttack 的状态
3. **类型强转**: Handler 参数是 `ILF2LivingObject`，实际需要强转到具体类型
4. **外部暴露**: 状态处理逻辑在外部静态类中，违反封装原则

### FLF 原版设计
```javascript
// FLF: 每个对象类型有自己的 states 对象
character.prototype.states = { 0: fn, 1: fn, ... }
weapon.prototype.states = { 1000: fn, 1001: fn, ... }
specialattack.prototype.states = { 3000: fn, 3001: fn, ... }

// 调用方式：对象自己调用自己的状态机
this.states[this.frame.D.state].call(this, event)
```

### 目标设计
```csharp
// 状态机是对象内部的，外部只能通过 StateUpdate() 触发
public class LF2Character : LF2LivingObject
{
    protected override void InitializeStates()
    {
        _states[LF2States.Standing] = State_Standing;
        _states[LF2States.Walking] = State_Walking;
        // ...
    }
}
```

---

## Work Objectives

### Core Objective
将状态机从外部单例/静态类移入各对象内部，实现：
1. 每个对象类型管理自己的状态
2. 外部只能通过 `StateUpdate()` 触发，不能直接访问状态处理器
3. 使用 `LF2States` 常量类管理状态值

### Concrete Deliverables
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs` - 添加状态机基础设施
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` - 内部化状态处理
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs` - 内部化状态处理
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs` - 内部化状态处理

### Definition of Done
- [ ] Unity 编译通过，无新增错误
- [ ] 所有状态处理逻辑移入对象内部
- [ ] 外部无法直接访问状态处理器
- [ ] 使用 `LF2States` 常量注册状态

### Must Have
- 基类 `LF2LivingObject` 持有 `_states` 字典
- 子类通过 `InitializeStates()` 注册状态处理器
- 子类通过 `OnGenericStateEvent()` 实现 generic 逻辑
- 统一入口 `StateUpdate()` 调度 generic + specific
- 使用 `LF2States.XXX` 常量作为状态键

### Must NOT Have (Guardrails)
- 不修改 `LF2States` 常量类
- 不修改 `ILF2LivingObject` 接口
- 不修改状态值的含义
- 不改变游戏行为，只是重构代码结构

---

## Verification Strategy

### Test Decision
- **Infrastructure exists**: YES (Unity Test Framework)
- **Automated tests**: NO (重构，编译验证 + 手动测试为主)
- **Framework**: Unity 编译检查

### Agent-Executed QA Scenarios

```
Scenario: Unity compilation succeeds after refactoring
  Tool: Bash
  Steps:
    1. Run Unity batchmode compilation
    2. Check exit code
    3. Grep log for "error CS"
  Expected Result: Exit code 0, no compilation errors
```

---

## Execution Strategy

### Sequential Execution

```
Task 1: 重构 LF2LivingObject 基类
    ↓
Task 2: 重构 LF2SpecialAttack（最简单，先验证模式）
    ↓
Task 3: 重构 LF2WeaponBase
    ↓
Task 4: 重构 LF2Character（最复杂，最后处理）
    ↓
Task 5: 清理废弃代码
    ↓
Task 6: 验证编译
```

### Dependency Matrix

| Task | Depends On | Blocks |
|------|------------|--------|
| 1 | None | 2, 3, 4 |
| 2 | 1 | 5 |
| 3 | 1 | 5 |
| 4 | 1 | 5 |
| 5 | 2, 3, 4 | 6 |
| 6 | 5 | None |

---

## TODOs

- [ ] 1. 重构 LF2LivingObject 基类 - 添加状态机基础设施

  **What to do**:
  - 添加 `StateHandler` 委托定义（protected）
  - 添加 `_states` 字典（protected）
  - 添加 `InitializeStates()` 抽象方法
  - 修改 `OnGenericStateEvent()` 为 protected virtual
  - 修改 `StateUpdate()` 实现调度逻辑（generic + specific）
  - 移除现有的 `_StateHandlers` 字段（如果有）

  **Must NOT do**:
  - 不修改其他公开 API
  - 不修改 `ILF2Object` 接口实现

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs:281-297` - 现有 StateUpdate
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs:187-193` - 现有 _StateHandlers
  - `Assets/NTSD/Scripts/Animation/Tools/LF2States.cs` - 状态常量

  **Acceptance Criteria**:
  - [ ] `StateHandler` 委托定义存在
  - [ ] `_states` 字典在基类中声明
  - [ ] `InitializeStates()` 是 abstract 方法
  - [ ] `StateUpdate()` 调度 generic + specific

  **Commit**: YES
  - Message: `refactor(LF2Objects): add state machine infrastructure to LF2LivingObject`

---

- [ ] 2. 重构 LF2SpecialAttack - 内部化状态处理

  **What to do**:
  - 实现 `InitializeStates()`，使用 `LF2States` 常量注册状态
  - 重写 `OnGenericStateEvent()`，移入 `LF2SpecialAttackStates.Generic_XXX` 逻辑
  - 添加私有状态处理方法：`State_ProjectileFlying()`, `State_ObjectFlying()` 等
  - 移入 `LF2SpecialAttackStates` 中的所有状态逻辑

  **Must NOT do**:
  - 不改变状态处理的实际逻辑
  - 不修改公开 API

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs` - 当前实现
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttackStates.cs` - 要移入的逻辑
  - `Assets/NTSD/Scripts/Animation/Tools/LF2States.cs:259-312` - 投射物状态常量

  **状态映射**:
  ```csharp
  _states[LF2States.ProjectileFlying] = State_ProjectileFlying;   // 3000
  _states[LF2States.ProjectileHiting] = State_ProjectileHiting;   // 3001
  _states[LF2States.ProjectileHit] = State_ProjectileHit;         // 3002
  _states[LF2States.ProjectileTeleport] = State_ProjectileTeleport; // 3003
  _states[LF2States.ObjectFlying] = State_ObjectFlying;           // 3005
  _states[LF2States.ObjectExpanding] = State_ObjectExpanding;     // 3006
  ```

  **Acceptance Criteria**:
  - [ ] `InitializeStates()` 使用 `LF2States.XXX` 注册状态
  - [ ] `OnGenericStateEvent()` 包含 generic 逻辑
  - [ ] 所有状态处理方法是私有的
  - [ ] `LF2SpecialAttackStates` 的逻辑已移入

  **Commit**: YES
  - Message: `refactor(LF2Objects): internalize LF2SpecialAttack state machine`

---

- [ ] 3. 重构 LF2WeaponBase - 内部化状态处理

  **What to do**:
  - 实现 `InitializeStates()`，使用 `LF2States` 常量注册状态
  - 重写 `OnGenericStateEvent()`，移入 `LF2WeaponStates.Generic_XXX` 逻辑
  - 添加私有状态处理方法
  - 移入 `LF2WeaponStates` 中的所有状态逻辑
  - 注意：`LF2LightWeapon` 和 `LF2HeavyWeapon` 可能需要额外处理

  **Must NOT do**:
  - 不改变状态处理的实际逻辑
  - 不修改公开 API

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs` - 当前实现
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponStates.cs` - 要移入的逻辑
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LightWeapon.cs` - 子类
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2HeavyWeapon.cs` - 子类
  - `Assets/NTSD/Scripts/Animation/Tools/LF2States.cs:213-255` - 武器状态常量

  **状态映射**:
  ```csharp
  _states[LF2States.WeaponInSky] = State_WeaponInSky;           // 1000
  _states[LF2States.WeaponOnHand] = State_WeaponOnHand;         // 1001
  _states[LF2States.WeaponThrowing] = State_WeaponThrowing;     // 1002
  _states[LF2States.WeaponJustOnGround] = State_WeaponJustOnGround; // 1003
  _states[LF2States.WeaponOnGround] = State_WeaponOnGround;     // 1004
  ```

  **Acceptance Criteria**:
  - [ ] `InitializeStates()` 使用 `LF2States.XXX` 注册状态
  - [ ] `OnGenericStateEvent()` 包含 generic 逻辑
  - [ ] 所有状态处理方法是私有的
  - [ ] `LF2WeaponStates` 的逻辑已移入
  - [ ] `LF2LightWeapon` 和 `LF2HeavyWeapon` 正常工作

  **Commit**: YES
  - Message: `refactor(LF2Objects): internalize LF2WeaponBase state machine`

---

- [ ] 4. 重构 LF2Character - 内部化状态处理

  **What to do**:
  - 实现 `InitializeStates()`，使用 `LF2States` 常量注册所有角色状态
  - 重写 `OnGenericStateEvent()`，移入 generic 逻辑
  - 添加私有状态处理方法：`State_Standing()`, `State_Walking()` 等
  - 移入 `CharacterStates` 单例中的所有状态逻辑
  - 这是最复杂的任务，状态最多（0-19 + 扩展状态）

  **Must NOT do**:
  - 不改变状态处理的实际逻辑
  - 不修改公开 API

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` - 当前实现
  - `Assets/NTSD/Scripts/Animation/Character/CharacterStates.cs` - 要移入的逻辑
  - `Assets/NTSD/Scripts/Animation/Tools/LF2States.cs:17-165` - 角色状态常量

  **状态映射**:
  ```csharp
  _states[LF2States.Standing] = State_Standing;        // 0
  _states[LF2States.Walking] = State_Walking;          // 1
  _states[LF2States.Running] = State_Running;          // 2
  _states[LF2States.Attack] = State_Attack;            // 3
  _states[LF2States.Jump] = State_Jump;                // 4
  _states[LF2States.Dash] = State_Dash;                // 5
  _states[LF2States.Rowing] = State_Rowing;            // 6
  _states[LF2States.Defending] = State_Defending;      // 7
  _states[LF2States.BrokenDefend] = State_BrokenDefend;// 8
  _states[LF2States.Catching] = State_Catching;        // 9
  _states[LF2States.BeingCaught] = State_BeingCaught;  // 10
  _states[LF2States.Injured] = State_Injured;          // 11
  _states[LF2States.Falling] = State_Falling;          // 12
  _states[LF2States.Frozen] = State_Frozen;            // 13
  _states[LF2States.Lying] = State_Lying;              // 14
  _states[LF2States.StopRunning] = State_StopRunning;  // 15
  _states[LF2States.Injured2] = State_Injured2;        // 16
  _states[LF2States.Charging] = State_Charging;        // 17
  _states[LF2States.Burning] = State_Burning;          // 18
  // ... 更多扩展状态
  ```

  **Acceptance Criteria**:
  - [ ] `InitializeStates()` 使用 `LF2States.XXX` 注册所有状态
  - [ ] `OnGenericStateEvent()` 包含 generic 逻辑
  - [ ] 所有状态处理方法是私有的
  - [ ] `CharacterStates` 的逻辑已移入

  **Commit**: YES
  - Message: `refactor(LF2Objects): internalize LF2Character state machine`

---

- [ ] 5. 清理废弃代码

  **What to do**:
  - 删除或标记废弃 `CharacterStates` 单例
  - 删除 `LF2WeaponStates` 静态类
  - 删除 `LF2SpecialAttackStates` 静态类
  - 清理所有对这些类的外部引用

  **Must NOT do**:
  - 不删除仍在使用的代码
  - 确保所有逻辑已移入对象内部

  **References**:
  - `Assets/NTSD/Scripts/Animation/Character/CharacterStates.cs`
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponStates.cs`
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttackStates.cs`

  **Acceptance Criteria**:
  - [ ] `CharacterStates` 已删除或标记 `[Obsolete]`
  - [ ] `LF2WeaponStates` 已删除
  - [ ] `LF2SpecialAttackStates` 已删除
  - [ ] 无编译错误

  **Commit**: YES
  - Message: `refactor(LF2Objects): remove obsolete external state classes`

---

- [ ] 6. 验证编译和功能

  **What to do**:
  - 运行 Unity 编译检查
  - 确认无编译错误
  - 手动测试基本功能（如果可能）

  **Acceptance Criteria**:
  - [ ] Unity 编译通过
  - [ ] 无 "error CS" 错误
  - [ ] 游戏基本功能正常

  **Commit**: NO (验证任务)

---

## Commit Strategy

| After Task | Message | Files |
|------------|---------|-------|
| 1 | `refactor(LF2Objects): add state machine infrastructure to LF2LivingObject` | LF2LivingObject.cs |
| 2 | `refactor(LF2Objects): internalize LF2SpecialAttack state machine` | LF2SpecialAttack.cs |
| 3 | `refactor(LF2Objects): internalize LF2WeaponBase state machine` | LF2WeaponBase.cs, LF2LightWeapon.cs, LF2HeavyWeapon.cs |
| 4 | `refactor(LF2Objects): internalize LF2Character state machine` | LF2Character.cs |
| 5 | `refactor(LF2Objects): remove obsolete external state classes` | CharacterStates.cs, LF2WeaponStates.cs, LF2SpecialAttackStates.cs |

---

## Success Criteria

### Final Checklist
- [ ] 所有状态处理逻辑在对象内部
- [ ] 外部只能通过 `StateUpdate()` 触发
- [ ] 使用 `LF2States` 常量管理状态值
- [ ] 废弃的外部类已清理
- [ ] Unity 编译通过
- [ ] 游戏行为不变
