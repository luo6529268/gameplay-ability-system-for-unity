# Fix ItrArestTest Check Position

## TL;DR

> **Quick Summary**: 移除三处入口级 `ItrArestTest()` 调用，对齐 FLF 原版将 arest 检查移到 per-target 命中判定内部。
> 
> **Deliverables**:
> - LF2Character.Generic_PreInteraction() 入口移除 arest 检查
> - LF2SpecialAttack.Interaction() 入口移除 arest 检查
> - LF2WeaponBase.Interaction() 入口移除 arest 检查
> - 在各自的 TryApplyHit / dispatch 内部添加 arest 检查（仅攻击类 kind）
> 
> **Estimated Effort**: Quick
> **Parallel Execution**: NO - sequential
> **Critical Path**: Task 1 → Task 2 → Task 3 → Task 4

---

## Context

### Original Request
用户发现 `Generic_PreInteraction` 函数入口处调用 `ItrArestTest()` 与 FLF 原版不符。

### Research Findings
- FLF 原版 `character.pre_interaction()` (character.js:2203-2285)：
  - **没有**在函数入口检查 arest
  - 仅在 kind 1/3（抓取）的 per-target 循环内部检查 `if (!$.itr.arest)`
  - kind 2/7（拾取）完全不检查 arest
- FLF 原版 `specialattack.interaction()` (specialattack.js:342-389)：
  - 在 per-target 循环内部检查 `if (!$.itr.arest)`（第 364 行）
- FLF 原版 `weapon.interaction()` (weapon.js:215-274)：
  - 在 per-target 循环内部检查 `if (!$.itr.arest)`（第 244 行）

### Problem
当前 Unity 实现在三处函数入口都调用了 `ItrArestTest()`，这会：
1. 错误阻止 kind 2/7（拾取武器）的执行
2. 与 FLF 原版语义不一致

---

## Work Objectives

### Core Objective
对齐 FLF 原版 arest 检查位置，从入口级移到 per-target 命中判定内部。

### Concrete Deliverables
- 修改 `LF2Character.cs` 的 `Generic_PreInteraction()` 和 `DispatchPreInteractionByKind()`
- 修改 `LF2SpecialAttack.cs` 的 `Interaction()` 和 `TryApplyHit()`
- 修改 `LF2WeaponBase.cs` 的 `Interaction()` 和 `TryApplyHit()`

### Definition of Done
- [ ] 三处入口级 `ItrArestTest()` 调用已移除
- [ ] 攻击类 kind 的 dispatch 内部添加了 arest 检查
- [ ] LSP 诊断无新增 error

### Must Have
- 保留 `LF2LivingObject.ItrArestTest()` 方法定义
- 保留 `ItrArestUpdate()` 调用位置不变

### Must NOT Have (Guardrails)
- 不删除 `ItrArestTest()` 方法本身
- 不改变 vrest 相关逻辑
- 不改变其他文件

---

## Verification Strategy

### Test Decision
- **Infrastructure exists**: YES (Unity Test Framework)
- **Automated tests**: Tests-after
- **Framework**: Unity Test Framework

### Agent-Executed QA Scenarios (MANDATORY)

```
Scenario: Character pre_interaction kind 2 (pickup) works without arest block
  Tool: Manual code review + LSP diagnostics
  Steps:
    1. Verify Generic_PreInteraction() no longer has ItrArestTest() at entry
    2. Verify kind 2/7 dispatch path has no arest check
    3. Run LSP diagnostics on LF2Character.cs
  Expected Result: No entry-level arest check, kind 2/7 unblocked

Scenario: Attack kind still respects arest
  Tool: Manual code review
  Steps:
    1. Verify TryApplyHit() in SpecialAttack/WeaponBase has arest check
    2. Verify arest check is inside per-target loop
  Expected Result: Attack kinds still gated by arest at correct position
```

---

## TODOs

- [ ] 1. Remove entry-level ItrArestTest from LF2Character.Generic_PreInteraction

  **What to do**:
  - 移除第 597 行的 `|| !ItrArestTest()` 条件
  - 在 `DispatchPreInteractionByKind()` 的 kind 1/3 分支内部添加 arest 检查

  **Must NOT do**:
  - 不要移除 kind 2/7 的 dispatch 逻辑
  - 不要在 kind 2/7 添加 arest 检查

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs:591-697`
  - FLF: `character.js:2203-2285`

  **Acceptance Criteria**:
  - [ ] 第 597 行不再包含 `ItrArestTest()`
  - [ ] kind 1/3 的 handler 内部有 `if (!ItrArestTest()) return false;`
  - [ ] kind 2/7 的 handler 无 arest 检查
  - [ ] LSP diagnostics: no new errors

  **Commit**: YES
  - Message: `fix(character): move arest check from entry to per-kind dispatch (FLF alignment)`
  - Files: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`

---

- [ ] 2. Remove entry-level ItrArestTest from LF2SpecialAttack.Interaction

  **What to do**:
  - 移除第 376 行的 `|| !ItrArestTest()` 条件
  - 在 `TryApplyHit()` 内部添加 arest 检查

  **Must NOT do**:
  - 不要在 pre_interaction kind handlers 添加 arest 检查

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs:368-407`
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs:445-459` (TryApplyHit)
  - FLF: `specialattack.js:342-389`

  **Acceptance Criteria**:
  - [ ] 第 376 行不再包含 `ItrArestTest()`
  - [ ] `TryApplyHit()` 入口有 `if (!ItrArestTest()) return false;`
  - [ ] LSP diagnostics: no new errors

  **Commit**: YES
  - Message: `fix(specialattack): move arest check to TryApplyHit (FLF alignment)`
  - Files: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`

---

- [ ] 3. Remove entry-level ItrArestTest from LF2WeaponBase.Interaction

  **What to do**:
  - 移除第 256 行的 `|| !ItrArestTest()` 条件
  - 在 `TryApplyHit()` 内部添加 arest 检查

  **Must NOT do**:
  - 不要在 pre_interaction kind handlers 添加 arest 检查

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs:248-287`
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs:325-339` (TryApplyHit)
  - FLF: `weapon.js:215-274`

  **Acceptance Criteria**:
  - [ ] 第 256 行不再包含 `ItrArestTest()`
  - [ ] `TryApplyHit()` 入口有 `if (!ItrArestTest()) return false;`
  - [ ] LSP diagnostics: no new errors

  **Commit**: YES
  - Message: `fix(weapon): move arest check to TryApplyHit (FLF alignment)`
  - Files: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`

---

- [ ] 4. Run final diagnostics and verify

  **What to do**:
  - 对三个修改文件运行 LSP diagnostics
  - 确认无新增 error

  **Acceptance Criteria**:
  - [ ] LF2Character.cs: no new errors
  - [ ] LF2SpecialAttack.cs: no new errors
  - [ ] LF2WeaponBase.cs: no new errors

  **Commit**: NO (verification only)

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 1 | `fix(character): move arest check from entry to per-kind dispatch (FLF alignment)` | LF2Character.cs | LSP |
| 2 | `fix(specialattack): move arest check to TryApplyHit (FLF alignment)` | LF2SpecialAttack.cs | LSP |
| 3 | `fix(weapon): move arest check to TryApplyHit (FLF alignment)` | LF2WeaponBase.cs | LSP |

---

## Success Criteria

### Verification Commands
```
LSP diagnostics on:
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
```

### Final Checklist
- [ ] 三处入口级 ItrArestTest 已移除
- [ ] 攻击类 kind 在 TryApplyHit 内部有 arest 检查
- [ ] kind 2/7 (pickup) 无 arest 检查
- [ ] 无新增编译错误
