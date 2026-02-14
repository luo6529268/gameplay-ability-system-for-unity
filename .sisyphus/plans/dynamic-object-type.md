# 动态设置 LF2ObjectType

## TL;DR

> **Quick Summary**: 将 LF2LivingObject 的 ObjectTypeEnum 从子类硬编码改为动态设置，支持从 data.txt 读取 type 值
> 
> **Deliverables**:
> - 修改 LF2LivingObject 基类，增加 `_objectType` 字段和 `SetObjectType` 方法
> - 修改各子类，保持 override 但使用基类字段
> - 更新 LF2ObjectPointFactory 支持新的枚举名
> 
> **Estimated Effort**: Quick
> **Parallel Execution**: NO - sequential
> **Critical Path**: Task 1 → Task 2 → Task 3

---

## Context

### Original Request
将 LF2LivingObject 的 type 改成从 data.txt 动态获取，而不是子类硬编码。

### 当前架构
- `LF2LivingObject` 有 `abstract LF2ObjectType ObjectTypeEnum { get; }`
- 各子类硬编码返回值：
  - `LF2Character` → `LF2ObjectType.Character`
  - `LF2LightWeapon` → `LF2ObjectType.LightWeapon`
  - `LF2HeavyWeapon` → `LF2ObjectType.HeavyWeapon`
  - `LF2SpecialAttack` → `LF2ObjectType.SpecialAttack`

### 目标架构
- 基类存储 `_objectType` 字段
- 提供 `SetObjectType(int)` 方法用于初始化时设置
- 子类可选择 override 或使用基类默认实现

---

## Work Objectives

### Core Objective
支持从 data.txt 动态设置对象类型

### Concrete Deliverables
- `LF2LivingObject.cs` - 增加字段和方法
- `LF2Character.cs` - 修改 override
- `LF2LightWeapon.cs` - 修改 override
- `LF2HeavyWeapon.cs` - 修改 override
- `LF2SpecialAttack.cs` - 修改 override
- `LF2ObjectPointFactory.cs` - 更新枚举名

### Definition of Done
- [ ] 编译通过
- [ ] 子类仍然返回正确的 type

### Must NOT Have (Guardrails)
- 不要破坏现有的子类行为
- 不要改变接口签名

---

## TODOs

- [ ] 1. 修改 LF2LivingObject 基类

  **What to do**:
  - 在 `#region 声明字段 - 身份标识` 区域增加字段：
    ```csharp
    /// <summary>
    /// 对象类型（从 data.txt 读取，对应 LF2 type: 0-6）
    /// </summary>
    protected LF2ObjectType _objectType = LF2ObjectType.Character;
    ```
  - 将 `abstract LF2ObjectType ObjectTypeEnum { get; }` 改为：
    ```csharp
    public virtual LF2ObjectType ObjectTypeEnum => _objectType;
    ```
  - 增加设置方法：
    ```csharp
    /// <summary>
    /// 设置对象类型（从 data.txt 的 type 字段）
    /// </summary>
    public void SetObjectType(int typeFromData)
    {
        _objectType = (LF2ObjectType)typeFromData;
    }
    
    public void SetObjectType(LF2ObjectType type)
    {
        _objectType = type;
    }
    ```

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs:948` - 当前 abstract 定义
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs:139-149` - 身份标识区域

  **Acceptance Criteria**:
  - [ ] `ObjectTypeEnum` 从 abstract 改为 virtual
  - [ ] 增加 `_objectType` 字段
  - [ ] 增加 `SetObjectType` 方法

  **Commit**: YES
  - Message: `refactor(LF2LivingObject): change ObjectTypeEnum from abstract to virtual with backing field`
  - Files: `LF2LivingObject.cs`

---

- [ ] 2. 修改各子类的 ObjectTypeEnum

  **What to do**:
  - `LF2Character.cs:33` - 改为设置基类字段：
    ```csharp
    public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;
    ```
    保持不变（子类 override 优先级高于基类默认值）
  
  - `LF2LightWeapon.cs:10` - 保持不变
  - `LF2HeavyWeapon.cs:10` - 保持不变
  - `LF2SpecialAttack.cs:35` - 保持不变

  **实际上不需要修改子类**，因为：
  - 子类的 `override` 会覆盖基类的 `virtual`
  - 子类硬编码的值仍然有效
  - 只有当子类不 override 时，才会使用基类的 `_objectType`

  **Acceptance Criteria**:
  - [ ] 各子类编译通过
  - [ ] 各子类仍返回正确的 type

  **Commit**: NO (与 Task 1 合并)

---

- [ ] 3. 更新 LF2ObjectPointFactory 支持新枚举名

  **What to do**:
  - `LF2ObjectPointFactory.cs:393-407` - 更新 switch 支持 type 4/5/6：
    ```csharp
    switch (objectType)
    {
        case 0:
            objTypeEnum = LF2ObjectType.Character;
            break;
        case 1:
            objTypeEnum = LF2ObjectType.LightWeapon;
            break;
        case 2:
            objTypeEnum = LF2ObjectType.HeavyWeapon;
            break;
        case 3:
            objTypeEnum = LF2ObjectType.SpecialAttack;
            break;
        case 4:
            objTypeEnum = LF2ObjectType.ThrowWeapon;
            break;
        case 5:
            objTypeEnum = LF2ObjectType.Other;
            break;
        case 6:
            objTypeEnum = LF2ObjectType.Drink;
            break;
        default:
            Log.Error($"[Factory] Unsupported object type: {objectType}");
            return null;
    }
    ```

  **References**:
  - `Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs:389-411`

  **Acceptance Criteria**:
  - [ ] switch 覆盖 type 0-6
  - [ ] 使用新的枚举名 ThrowWeapon, Other

  **Commit**: YES
  - Message: `feat(LF2ObjectPointFactory): support all LF2 object types 0-6`
  - Files: `LF2ObjectPointFactory.cs`

---

## Success Criteria

### Verification Commands
```
# Unity 编译检查
Unity Editor 打开项目无报错
```

### Final Checklist
- [ ] LF2LivingObject.ObjectTypeEnum 是 virtual 而非 abstract
- [ ] 各子类仍然正确返回各自的 type
- [ ] LF2ObjectPointFactory 支持 type 0-6
