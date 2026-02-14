# Fix Sprite Width Source for Collision Detection

## TL;DR

> **Quick Summary**: 新增 `GetDatSpriteWidth()` 方法从 Dat 配置获取精灵宽度，修改 `GetSpriteWidthPxForCollision()` 优先使用 Dat 配置宽度。
> 
> **Deliverables**:
> - 新增 `LF2LivingObject.GetDatSpriteWidth()` 方法
> - 修改 `GetSpriteWidthPxForCollision()` 优先使用 Dat 宽度，fallback 到运行时宽度
> 
> **Estimated Effort**: Quick
> **Parallel Execution**: NO
> **Critical Path**: Task 1

---

## Context

### Original Request
用户指出 `GetSpriteWidthPxForCollision` 使用的是运行时精灵图宽度，而 FLF 原版 `mech.volume()` 使用的是 Dat 文件配置的宽度 `data.bmp.file[0].w`。

### Research Findings

**FLF 原版** (livingobject.js:40):
```javascript
$.sp.width = data.bmp.file[0].w  // Dat 配置宽度
```

**FLF mech.volume()** (mechanics.js:206):
```javascript
if (ps.dir === 'left') {
    vx = sp.w - O.x - O.w  // 使用 Dat 配置宽度做镜像翻转
}
```

**当前 Unity 实现**:
```csharp
public virtual float GetSpriteWidthPxForCollision()
{
    return Sprite?.GetCurrentSpriteWidthPx() ?? 0f;  // 运行时纹理宽度
}
```

### Data Source Trace

```
Dat 文件 "file(x-y): path w: h: row: col:"
    ↓ Lf2DatParserV2.Parse (line 147-148)
Lf2SpriteFileDef.Width
    ↓ CharacterAnimtorManager.BuildCharacterDataFromDat (line 539)
SpriteFileInfo.width
    ↓ LF2CharacterData.files
LF2LivingObject.Data.characterData.files[0].width
```

---

## Work Objectives

### Core Objective
对齐 FLF 原版，碰撞检测使用 Dat 配置宽度而非运行时纹理宽度。

### Concrete Deliverables
- 新增 `GetDatSpriteWidth()` 方法
- 修改 `GetSpriteWidthPxForCollision()` 逻辑

### Definition of Done
- [ ] `GetDatSpriteWidth()` 返回 `Data.characterData.files[0].width`
- [ ] `GetSpriteWidthPxForCollision()` 优先使用 Dat 宽度
- [ ] LSP 诊断无新增 error

### Must Have
- Fallback 逻辑：如果 Dat 数据不可用，使用运行时宽度

### Must NOT Have (Guardrails)
- 不修改 `LF2Sprite.GetWidthPx()` 方法
- 不删除运行时宽度获取能力

---

## TODOs

- [ ] 1. Add GetDatSpriteWidth and modify GetSpriteWidthPxForCollision

  **What to do**:
  在 `LF2LivingObject.cs` 中：
  
  1. 在 `GetSpriteWidthPxForCollision()` 方法前新增：
  ```csharp
  /// <summary>
  /// 获取 Dat 配置的精灵宽度（对应 FLF data.bmp.file[0].w）
  /// 用于 mech.volume() 中的朝向翻转计算
  /// </summary>
  public virtual float GetDatSpriteWidth()
  {
      var files = Data?.characterData?.files;
      if (files != null && files.Count > 0)
      {
          return files[0].width;
      }
      return 0f;
  }
  ```
  
  2. 修改 `GetSpriteWidthPxForCollision()` 为：
  ```csharp
  /// <summary>获取精灵宽度用于碰撞检测（子类可重写）</summary>
  public virtual float GetSpriteWidthPxForCollision()
  {
      float datWidth = GetDatSpriteWidth();
      if (datWidth > 0f) return datWidth;
      return Sprite?.GetCurrentSpriteWidthPx() ?? 0f;
  }
  ```

  **Must NOT do**:
  - 不要修改 `LF2Sprite` 类
  - 不要删除 fallback 逻辑

  **References**:
  - `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs:894-898`
  - `Assets/NTSD/Scripts/Animation/LF2CharacterData.cs:16,63` (SpriteFileInfo.width)
  - FLF: `livingobject.js:40`, `mechanics.js:206`

  **Acceptance Criteria**:
  - [ ] `GetDatSpriteWidth()` 方法存在
  - [ ] `GetSpriteWidthPxForCollision()` 优先返回 Dat 宽度
  - [ ] 当 Data 为 null 时 fallback 到运行时宽度
  - [ ] LSP diagnostics: no new errors

  **Commit**: YES
  - Message: `fix(collision): use Dat sprite width instead of runtime texture width (FLF alignment)`
  - Files: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs`

---

## Success Criteria

### Verification Commands
```
LSP diagnostics on:
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs
```

### Final Checklist
- [ ] `GetDatSpriteWidth()` 从 `Data.characterData.files[0].width` 获取宽度
- [ ] `GetSpriteWidthPxForCollision()` 优先使用 Dat 宽度
- [ ] Fallback 到运行时宽度正常工作
- [ ] 无编译错误
