# Draft: LF2LivingObject 与 FLF livingobject 对齐分析

## 当前项目结构

### 继承层次
```
ISimObject (模拟接口)
    └── ILF2Object (LF2对象接口)
            └── LF2LivingObject (活动对象基类 - 纯C#)
                    ├── LF2Character (角色专用)
                    ├── LF2LightWeapon
                    ├── LF2HeavyWeapon
                    └── LF2SpecialAttack
```

### 核心模块分布

| 模块 | LF2LivingObject | LF2Character | 对应 FLF |
|------|-----------------|--------------|----------|
| PS (PhysicsState) | ✅ | 继承 | $.mech.create_metric() |
| Trans (FrameTransistor) | ✅ | 继承 | $.Trans |
| Frame (LF2FrameInfo) | ✅ | 继承 | $.frame |
| FrameCache | ✅ | 继承 | 帧数据缓存 |
| ItrRest | ✅ | 继承 | $.itr |
| Effect | ✅ | 继承 | $.effect |
| Health | ✅ | 继承 | $.health |
| Sprite | ✅ | 继承 | $.sp |
| ComboBuffer | 虚方法(null) | ✅ 实现 | $.combo_buffer |
| HitCounters | 虚方法(null) | ✅ 实现 | $.health.fall/bdefend |
| ObjectPointModule | ❌ | ✅ | opoint |
| WeaponPointModule | ❌ | ✅ | wpoint |

## 用户确认的决策

1. **LF2CharacterAnimator** → **已废弃，可以删除**
2. **CharacterStates 状态处理器** → **需要恢复启用**
3. **目标** → **先完善 FLF 对齐，表现一致后再考虑简化架构**

---

## 完整架构分析

### SimulationTickDriver 调用流程（已确认）

```
FixedUpdate() {
    while (accumulator >= SIM_DT) {
        RunOneSimTick(tickIndex) {
            1. World.TransitTickAll()    // 所有对象执行 SimTransit
            2. FlushTasks()              // OPoint 任务刷新
            3. World.TUTickAll()         // 所有对象执行 SimTU
            4. World.LateTick()          // 后期处理
        }
    }
}
```

### FLF 原版 vs 当前实现对比

#### FLF 原版 livingobject.js 生命周期
```javascript
// match.TU_trans() 调用顺序
for each object:
    object.transit()     // Transit 阶段

for each object:
    object.TU()          // TU 阶段

// livingobject.prototype.transit()
transit() {
    if (stuck) return;
    this.Trans.trans();           // 帧转换
    this.effect.timein--;
    this.state_update('transit'); // 状态更新 → mech.dynamics() + wpoint()
}

// livingobject.prototype.TU()
TU() {
    this.TU_update();
}

// livingobject.prototype.TU_update()
TU_update() {
    this.ps.fric = 1;             // 重置摩擦力
    this.state_update('TU');      // 状态 TU 事件
    // ... 效果处理、生命值检查等
}
```

#### 当前项目实现（LF2LivingObject）
```csharp
// SimTransit() - 对应 FLF transit()
SimTransit(tickIndex) {
    if (Effect.TimeIn < 0 && Effect.Stuck) return;
    Trans.Trans();                // ✅ 帧转换
    Effect.TimeIn--;              // ✅ 效果时间
    StateUpdate("transit");       // ✅ 状态更新
}

// SimTU() - 对应 FLF TU()
SimTU(tickIndex) {
    TUUpdate();                   // ✅ 调用 TU 更新
}

// TUUpdate() - 对应 FLF TU_update()
TUUpdate() {
    StateUpdate("TU_force");      // ✅
    ProcessEffects();             // ✅
    StateUpdate("TU");            // ✅
    ItrRest?.Tick();              // ✅
}
```

### 发现的问题

#### 问题 1: CharacterStates 未启用
**位置**: `CharacterStates.cs`

**现状**:
- `stateHandlers` 字典声明了但未初始化
- `genericHandler` 未设置
- 所有状态处理器（Standing, Walking, Running 等）被注释

**影响**:
- `HandleStateEvent()` 调用时找不到处理器
- 状态逻辑无法执行
- `dynamics()` 和 `wpoint()` 不会被调用

#### 问题 2: Transit 阶段缺少 dynamics/wpoint
**FLF 原版**: `state_update('transit')` 内部调用 `mech.dynamics()` 和 `wpoint()`

**当前实现**: 
- `LF2Character.Transit_DynamicsAndWPoint()` 存在
- 但 `CharacterStates.HandleGenericTransit()` 被注释，不会调用它

#### 问题 3: LF2LivingObject.Transit() 与 SimTransit() 重复
**位置**: `LF2LivingObject.cs`

```csharp
// 方法 1: Transit() - 第 385-413 行
public virtual void Transit() {
    ComboUpdate();
    Trans.Trans();
    CharacterStates.Instance.HandleStateEvent(this, "transit");
}

// 方法 2: SimTransit() - 第 341-358 行
public virtual void SimTransit(int tickIndex) {
    Trans.Trans();
    Effect.TimeIn--;
    StateUpdate("transit");  // 注意：这里调用的是 StateUpdate，不是 CharacterStates
}
```

**问题**: 
- 两个方法功能重叠
- `Transit()` 调用 `CharacterStates.Instance`
- `SimTransit()` 调用 `StateUpdate()`（本地方法，默认返回 false）

#### 问题 4: 初始化时序
**位置**: `LF2Character.ModuleBind()`

```csharp
ModuleBind(wrapper, characterId) {
    FrameCache.Load(wrapper);
    Frame.D = FrameCache.GetFrameDataById(0);  // 设置第一帧数据
    // ❌ 但没有设置初始 sprite
    // ❌ Trans 初始 wait=1，第一次 Trans.Trans() 会等待
}
```

---

## 需要修复的清单

### 优先级 1: 恢复 CharacterStates
- [ ] 取消注释 `RegisterDefaultHandlers()`
- [ ] 取消注释 `genericHandler = GenericStateHandler`
- [ ] 取消注释所有状态处理器（0-19）
- [ ] 确保 `HandleGenericTransit()` 调用 `Transit_DynamicsAndWPoint()`

### 优先级 2: 统一 Transit 调用路径
- [ ] 决定使用 `Transit()` 还是 `SimTransit()`
- [ ] 确保 `ComboUpdate()` 在正确位置调用
- [ ] 确保 `dynamics()` 和 `wpoint()` 被调用

### 优先级 3: 修复初始化时序
- [ ] 在 `ModuleBind()` 末尾设置初始 sprite
- [ ] 或在第一次 `SimTransit()` 时强制触发帧更新

### 优先级 4: 清理废弃代码
- [ ] 删除 `LF2CharacterAnimator.cs`（用户确认可删除）
- [ ] 清理 `LF2LivingObject` 中重复的 `Transit()` 方法

---

## FLF 原版确认（已读取源码）

### 1. ComboUpdate 调用位置 ✅ 已确认

**FLF 原版 (livingobject.js:315-333)**:
```javascript
livingobject.prototype.transit = function () {
    if ($.con) {
        $.combo_update()           // ← 在 Transit 阶段调用
    }
    if (!(stuck)) {
        $.trans.trans()            // 帧转换
    }
    $.effect.timein--
    if (!(stuck)) {
        $.state_update('transit')  // 状态更新
    }
}
```

**结论**: ComboUpdate 应该在 **Transit 阶段**调用，当前实现是正确的。

### 2. state_update 调用顺序 ✅ 已确认

**FLF 原版 (livingobject.js:292-305)**:
```javascript
livingobject.prototype.state_update = function (event) {
    // 1. 先执行 generic 处理器
    const tar1 = $.states.generic
    if (tar1) { var res1 = tar1.apply($, arguments) }
    
    // 2. 再执行当前状态的特定处理器
    const tar2 = $.states[$.frame.D.state]
    if (tar2) { var res2 = tar2.apply($, arguments) }
    
    return res1 || res2
}
```

**但 combo_update 是特例 (character.js:1802-1848)**:
```javascript
character.prototype.combo_update = function () {
    // 1. 先执行当前状态的处理器
    const tar1 = $.states[$.frame.D.state]
    if (tar1) { var res1 = tar1.call($, 'combo', K) }
    
    // 2. 如果未处理，再执行 generic
    const tar2 = $.states.generic
    if (!res1) { if (tar2) { var res2 = tar2.call($, 'combo', K) } }
    
    // 3. 两者都执行 post_combo
    if (tar1) { tar1.call($, 'post_combo') }
    if (tar2) { tar2.call($, 'post_combo') }
}
```

**结论**: 
- 普通事件: generic 先执行，specific 后执行
- combo 事件: specific 先执行，generic 后执行（如果 specific 未处理）

### 3. Transit 阶段的 dynamics 和 wpoint ✅ 已确认

**FLF 原版 (character.js:185-190)**:
```javascript
case 'transit':
    $.mech.dynamics()  // 物理更新
    $.wpoint()         // 武器点更新
    break
```

**结论**: `dynamics()` 和 `wpoint()` 在 `state_update('transit')` 的 **generic** 处理器中调用。

---

## 需要修复的完整清单

### 优先级 1: 恢复 CharacterStates 状态处理器

**文件**: `CharacterStates.cs`

需要取消注释:
1. `RegisterDefaultHandlers()` 方法
2. `genericHandler = GenericStateHandler` 赋值
3. 所有状态处理器 (0-19)
4. `HandleGenericTransit()` 方法 - 确保调用 `Transit_DynamicsAndWPoint()`

### 优先级 2: 修复 state_update 调用逻辑

**文件**: `CharacterStates.cs`

当前 `HandleStateEvent()` 的逻辑需要调整:
- 普通事件: generic → specific (当前实现正确)
- combo 事件: specific → generic (需要特殊处理)

### 优先级 3: 统一 Transit 调用路径

**文件**: `LF2LivingObject.cs`

问题: 存在两个方法
- `Transit()` - 调用 `CharacterStates.Instance.HandleStateEvent()`
- `SimTransit()` - 调用本地 `StateUpdate()`

**解决方案**: 
- 删除 `Transit()` 方法
- 修改 `SimTransit()` 调用 `CharacterStates.Instance.HandleStateEvent()`

### 优先级 4: 修复初始化时序

**文件**: `LF2Character.cs`

在 `ModuleBind()` 末尾添加:
```csharp
// 设置初始 sprite
if (_spriteRenderer != null && _sprites != null && Frame.D != null)
{
    int picIndex = Frame.D.pic;
    if (picIndex >= 0 && picIndex < _sprites.Count)
    {
        _spriteRenderer.sprite = _sprites[picIndex];
    }
}
```

### 优先级 5: 清理废弃代码

- 删除 `LF2CharacterAnimator.cs` (用户确认可删除)

---

## FLF 完整生命周期对照表

```
FLF 原版                              你的项目
─────────────────────────────────────────────────────────────
match.TU_trans() {                    SimulationTickDriver.RunOneSimTick() {
  for each object:                      World.TransitTickAll() {
    object.transit()                      for each obj: obj.SimTransit()
                                        }
  
  // (无 FlushTasks)                    FlushTasks()
  
  for each object:                      World.TUTickAll() {
    object.TU()                           for each obj: obj.SimTU()
                                        }
}                                     }

─────────────────────────────────────────────────────────────
livingobject.transit() {              LF2LivingObject.SimTransit() {
  if ($.con) $.combo_update()           // ❌ 缺少 ComboUpdate
  if (!stuck) $.trans.trans()           Trans.Trans()  ✅
  $.effect.timein--                     Effect.TimeIn--  ✅
  if (!stuck) $.state_update('transit') StateUpdate("transit")  ⚠️ 调用错误方法
}                                     }

─────────────────────────────────────────────────────────────
livingobject.TU() {                   LF2LivingObject.SimTU() {
  $.TU_update()                         TUUpdate()  ✅
}                                     }

─────────────────────────────────────────────────────────────
livingobject.TU_update() {            LF2LivingObject.TUUpdate() {
  if (!state_update('TU_force'))        if (!StateUpdate("TU_force"))
    $.frame_force()                       FrameForce()  ✅
  // 效果处理                            ProcessEffects()  ✅
  if (!stuck) $.state_update('TU')      StateUpdate("TU")  ⚠️
  // itr rest tick                       ItrRest?.Tick()  ✅
}                                     }

─────────────────────────────────────────────────────────────
character.states.generic['transit']:  CharacterStates.HandleGenericTransit():
  $.mech.dynamics()                     // ❌ 被注释，不会调用
  $.wpoint()                            // ❌ 被注释，不会调用
```

## 修复后的目标流程

```csharp
// LF2LivingObject.SimTransit() - 修复后
public virtual void SimTransit(int tickIndex) {
    // 1. 输入处理（仅角色）
    ComboUpdate();  // ← 添加
    
    // 2. 帧转换
    if (!(Effect.TimeIn < 0 && Effect.Stuck)) {
        Trans.Trans();
    }
    
    // 3. 效果时间
    Effect.TimeIn--;
    
    // 4. 状态更新（包含 dynamics + wpoint）
    if (!(Effect.TimeIn < 0 && Effect.Stuck)) {
        CharacterStates.Instance.HandleStateEvent(this, "transit");  // ← 修改
    }
}

// CharacterStates.HandleGenericTransit() - 取消注释后
private bool HandleGenericTransit(ILF2LivingObject character) {
    character.Transit_DynamicsAndWPoint();  // dynamics + wpoint
    return true;
}
```

---

## 确认清单

在生成工作计划前，请确认以下内容：

- [x] ComboUpdate 在 Transit 阶段调用（FLF 原版确认）
- [x] state_update 普通事件: generic → specific
- [x] combo 事件特例: specific → generic
- [x] dynamics/wpoint 在 transit 的 generic 处理器中
- [x] LF2CharacterAnimator 可以删除
- [x] CharacterStates 需要恢复启用
- [x] 目标是完全对齐 FLF 行为
