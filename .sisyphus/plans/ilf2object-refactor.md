# ILF2Object 接口重构 - 职责分离

## TL;DR

> **Quick Summary**: 将 `ILF2Object` 接口拆分为 `ILF2Poolable`（对象池专用）和 `ISimObject`（模拟系统专用）的组合，实现职责分离。
> 
> **Deliverables**:
> - 新增 `ILF2Poolable.cs` 接口文件
> - 修改 `ILF2Object.cs` 为组合接口
> - 可选：更新 `LF2ObjectLogicPool.cs` 使用 `ILF2Poolable`
> 
> **Estimated Effort**: Quick
> **Parallel Execution**: NO - sequential (接口依赖关系)
> **Critical Path**: Task 1 → Task 2 → Task 3 → Task 4

---

## Context

### Original Request
用户希望重构 `ILF2Object` 接口，实现职责分离（方案 A）。当前 `ILF2Object` 继承 `ISimObject`，同时承担对象池管理和模拟系统两个职责，导致职责混淆和强制耦合。

### Interview Summary
**Key Discussions**:
- 当前问题：`ILF2Object` 同时承担对象池管理和模拟系统两个职责
- 方案 A：新增 `ILF2Poolable` 接口，`ILF2Object` 改为组合接口
- 用户确认：可以修改现有代码，确认推进重构

**Research Findings**:
- `ILF2Object` 在 `LF2ObjectPointFactory`, `LF2ObjectLogicPool`, `LF2ObjectRenderer` 中使用
- `LF2ObjectLogicPool` 只使用 `ObjectTypeEnum`, `ObjectId`, `Reset()` 三个成员
- FLF 原版没有这些接口（JavaScript 动态类型）

### Metis Review
**Identified Gaps** (addressed):
- `Init()` 签名问题：保留在 `ILF2Object` 中，不移动到 `ILF2Poolable`（因为 `Init()` 是模拟相关的）
- `Destroy()` 位置：保留在 `ILF2Object` 中
- `LF2ObjectLogicPool` 修改范围：作为可选任务，用户可决定是否执行

---

## Work Objectives

### Core Objective
将 `ILF2Object` 接口拆分为职责清晰的组合接口，实现对象池管理和模拟系统的解耦。

### Concrete Deliverables
- `Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Poolable.cs` - 新接口文件
- `Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Object.cs` - 修改为组合接口
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectLogicPool.cs` - 可选更新

### Definition of Done
- [ ] Unity 编译通过，无新增错误
- [ ] 所有现有实现类（LF2SpecialAttack, LF2LightWeapon, LF2HeavyWeapon）正常工作
- [ ] 接口职责清晰分离

### Must Have
- `ILF2Poolable` 接口包含：`ObjectTypeEnum`, `ObjectId`, `Reset()`
- `ILF2Object` 继承 `ILF2Poolable` 和 `ISimObject`
- 向后兼容，现有代码改动最小

### Must NOT Have (Guardrails)
- 不修改 `ISimObject` 接口
- 不修改 `ILF2LivingObject` 接口（碰撞系统依赖）
- 不修改 `Init()` 方法签名
- 不添加新方法，只做接口重组织
- 不修改 `LF2ObjectPool`（GameObject 池）

---

## Verification Strategy (MANDATORY)

> **UNIVERSAL RULE: ZERO HUMAN INTERVENTION**
>
> ALL tasks in this plan MUST be verifiable WITHOUT any human action.

### Test Decision
- **Infrastructure exists**: YES (Unity Test Framework)
- **Automated tests**: NO (接口重构，编译验证为主)
- **Framework**: Unity 编译检查

### Agent-Executed QA Scenarios (MANDATORY)

**Verification Tool**: Bash (Unity batchmode compilation)

```
Scenario: Unity compilation succeeds after refactoring
  Tool: Bash
  Preconditions: Unity Editor installed at standard path
  Steps:
    1. Run Unity in batchmode: Unity.exe -batchmode -nographics -quit -projectPath "$PWD" -logFile "compile-check.log"
    2. Check exit code
    3. Grep compile-check.log for "error CS"
  Expected Result: Exit code 0, no "error CS" in log
  Evidence: compile-check.log
```

---

## Execution Strategy

### Sequential Execution (No Parallelization)

```
Task 1: Create ILF2Poolable interface
    ↓
Task 2: Modify ILF2Object to inherit ILF2Poolable
    ↓
Task 3: Verify compilation
    ↓
Task 4 (Optional): Update LF2ObjectLogicPool
```

### Dependency Matrix

| Task | Depends On | Blocks | Can Parallelize With |
|------|------------|--------|---------------------|
| 1 | None | 2 | None |
| 2 | 1 | 3 | None |
| 3 | 2 | 4 | None |
| 4 | 3 | None | None |

---

## TODOs

- [x] 1. 创建 ILF2Poolable 接口

  **What to do**:
  - 在 `Assets/NTSD/Scripts/Animation/LF2Objects/` 目录下创建 `ILF2Poolable.cs`
  - 定义接口包含：`ObjectTypeEnum`, `ObjectId { get; set; }`, `Reset()`
  - 添加 XML 文档注释

  **Must NOT do**:
  - 不包含 `Init()` 方法（这是模拟相关的）
  - 不包含 `Destroy()` 方法
  - 不继承其他接口

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: 单文件创建，简单明确
  - **Skills**: []
    - 无需特殊技能

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 2
  - **Blocked By**: None

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Object.cs` - 现有接口定义，参考风格
  - `Assets/NTSD/Scripts/Simulation/ISimObject.cs` - 接口风格参考（XML docs, namespace）
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectLogicPool.cs:108-131` - 池实际使用的成员

  **Acceptance Criteria**:
  - [ ] 文件创建: `Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Poolable.cs`
  - [ ] 接口包含: `ObjectTypeEnum`, `ObjectId { get; set; }`, `Reset()`
  - [ ] 命名空间: `NTSD.Animation.LF2Objects`

  **Agent-Executed QA Scenarios**:
  ```
  Scenario: ILF2Poolable.cs file created with correct content
    Tool: Bash (file check)
    Steps:
      1. Check file exists: Test-Path "Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Poolable.cs"
      2. Grep for "interface ILF2Poolable"
      3. Grep for "ObjectTypeEnum"
      4. Grep for "ObjectId"
      5. Grep for "Reset()"
    Expected Result: All checks pass
    Evidence: File content
  ```

  **Commit**: YES
  - Message: `refactor(LF2Objects): add ILF2Poolable interface for pool lifecycle`
  - Files: `Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Poolable.cs`

---

- [x] 2. 修改 ILF2Object 为组合接口

  **What to do**:
  - 修改 `ILF2Object.cs`，使其继承 `ILF2Poolable` 和 `ISimObject`
  - 移除 `ILF2Object` 中已在 `ILF2Poolable` 定义的成员（避免重复）
  - 保留 `ObjectType`, `Init()`, `Destroy()` 在 `ILF2Object` 中

  **Must NOT do**:
  - 不修改 `Init()` 签名
  - 不修改 `Destroy()` 签名
  - 不添加新方法

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: 单文件修改，明确的改动点
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 3
  - **Blocked By**: Task 1

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Object.cs` - 当前接口定义
  - `Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Poolable.cs` - 新创建的接口（Task 1）
  - `Assets/NTSD/Scripts/Simulation/ISimObject.cs` - 模拟系统接口

  **Acceptance Criteria**:
  - [ ] `ILF2Object` 继承 `ILF2Poolable, ISimObject`
  - [ ] `ILF2Object` 保留 `ObjectType`, `Init()`, `Destroy()`
  - [ ] 无重复成员定义

  **Agent-Executed QA Scenarios**:
  ```
  Scenario: ILF2Object correctly inherits ILF2Poolable and ISimObject
    Tool: Bash (grep)
    Steps:
      1. Grep ILF2Object.cs for "ILF2Object : ILF2Poolable, ISimObject"
      2. Grep for "ObjectType" (should exist)
      3. Grep for "Init(" (should exist)
      4. Grep for "Destroy()" (should exist)
    Expected Result: All patterns found
    Evidence: Grep output
  ```

  **Commit**: YES
  - Message: `refactor(LF2Objects): ILF2Object now composes ILF2Poolable + ISimObject`
  - Files: `Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Object.cs`

---

- [x] 3. 验证 Unity 编译

  **What to do**:
  - 运行 Unity batchmode 编译检查
  - 确认无编译错误
  - 确认所有实现类正常

  **Must NOT do**:
  - 不修改任何代码
  - 不跳过编译检查

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: 单一验证任务
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 4
  - **Blocked By**: Task 2

  **References**:
  - `AGENTS.md` - Unity 编译命令参考

  **Acceptance Criteria**:
  - [ ] Unity 编译退出码为 0
  - [ ] 编译日志无 "error CS" 错误

  **Agent-Executed QA Scenarios**:
  ```
  Scenario: Unity compilation succeeds
    Tool: Bash
    Preconditions: UNITY_EXE environment variable set
    Steps:
      1. Run: & $env:UNITY_EXE -batchmode -nographics -quit -projectPath "$PWD" -logFile "compile-check.log"
      2. Check exit code equals 0
      3. Grep compile-check.log for "error CS" - should return empty
    Expected Result: Exit code 0, no compilation errors
    Evidence: compile-check.log saved to .sisyphus/evidence/task-3-compile.log
  ```

  **Commit**: NO (验证任务，无代码改动)

---

- [ ] 4. (可选) 更新 LF2ObjectLogicPool 使用 ILF2Poolable

  **What to do**:
  - 将 `LF2ObjectLogicPool` 中的 `ILF2Object` 引用改为 `ILF2Poolable`
  - 更新池的泛型类型声明
  - 更新 `Get()` 和 `Release()` 方法的参数/返回类型

  **Must NOT do**:
  - 不修改池的核心逻辑
  - 不修改预热逻辑
  - 不影响 `LF2ObjectPointFactory` 的调用

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: 单文件类型替换
  - **Skills**: []

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: None
  - **Blocked By**: Task 3

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectLogicPool.cs` - 当前实现
  - `Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs:389-410` - 工厂调用池的代码

  **Acceptance Criteria**:
  - [ ] `_availablePools` 类型改为 `Dictionary<LF2ObjectType, LinkedList<ILF2Poolable>>`
  - [ ] `_activeObjects` 类型改为 `HashSet<ILF2Poolable>`
  - [ ] `Get()` 返回 `ILF2Poolable`
  - [ ] `Release()` 参数为 `ILF2Poolable`
  - [ ] Unity 编译通过

  **Agent-Executed QA Scenarios**:
  ```
  Scenario: LF2ObjectLogicPool uses ILF2Poolable
    Tool: Bash (grep)
    Steps:
      1. Grep LF2ObjectLogicPool.cs for "ILF2Poolable"
      2. Grep for "LinkedList<ILF2Poolable>"
      3. Grep for "HashSet<ILF2Poolable>"
    Expected Result: All patterns found
    Evidence: Grep output

  Scenario: Unity compilation still succeeds
    Tool: Bash
    Steps:
      1. Run Unity batchmode compilation
      2. Check exit code
    Expected Result: Exit code 0
    Evidence: compile-check.log
  ```

  **Commit**: YES
  - Message: `refactor(LF2Objects): LF2ObjectLogicPool now uses ILF2Poolable`
  - Files: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectLogicPool.cs`

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 1 | `refactor(LF2Objects): add ILF2Poolable interface for pool lifecycle` | ILF2Poolable.cs | File exists |
| 2 | `refactor(LF2Objects): ILF2Object now composes ILF2Poolable + ISimObject` | ILF2Object.cs | Grep check |
| 3 | (no commit) | - | Unity compile |
| 4 | `refactor(LF2Objects): LF2ObjectLogicPool now uses ILF2Poolable` | LF2ObjectLogicPool.cs | Unity compile |

---

## Success Criteria

### Verification Commands
```powershell
# 1. Check ILF2Poolable.cs exists
Test-Path "Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Poolable.cs"

# 2. Check ILF2Object inherits correctly
Select-String -Path "Assets/NTSD/Scripts/Animation/LF2Objects/ILF2Object.cs" -Pattern "ILF2Poolable"

# 3. Unity compilation check
& $env:UNITY_EXE -batchmode -nographics -quit -projectPath "$PWD" -logFile "compile-check.log"
# Expected: Exit code 0
```

### Final Checklist
- [ ] `ILF2Poolable.cs` 文件存在且内容正确
- [ ] `ILF2Object` 继承 `ILF2Poolable, ISimObject`
- [ ] Unity 编译通过
- [ ] 所有现有功能正常工作
