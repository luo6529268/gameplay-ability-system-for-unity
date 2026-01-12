# DEVLOG: Unity Project Progress

> **Last Updated**: 2026-01-08
> **Current Status**: ✅ P2+P3 Phase 1 已完成 - 等待 PlayMode 测试验证

---

## 1. 🎯 当前聚焦 (Active Context)

### 当前工作：✅ P2+P3 Phase 1 已完成（代码已完成，等待测试）

**任务概览**：
- **P2**: 跳跃/落地语义对齐 - 坐标系从 2D 改为 3D，引入 `ps.groundY` 机制
- **P3**: NoWalkZone 阻挡（地形阻挡）- 使用 BodyBox footprint Rect 检测，确定性位移解算

**当前状态**：
- 代码实现已完成，等待 PlayMode 测试验证（验证清单见「下一步行动计划」）

**核心变更**：
1. **坐标系变更（P2）**: Unity (X, Y, Z) = FLF (x, 跳跃高度, z)
   - 新增 `PhysicsState.groundY` 字段
   - 修改 `ToUnityPosition()` / `FromUnityPosition()`
   - 起跳时记录 `groundY`，落地判定基于 `ps.y <= 0`
2. **地形阻挡（P3）**: 3-step fallback 位移解算
   - 新增 `PhysicsState.GetFootprintRect()` 方法
   - NoWalkZone 检测集成到 `ApplyDynamics()`
   - 场景边界硬限制（临时方案）

---

## 2. 🧠 关键决策与已知事实 (Critical Memories)

### FLF 帧转换系统架构

**FLF 源码执行流程** (参考：`I:\C++Test\NTSD\F.LF-master\LF\livingobject.js`):

```javascript
// 游戏主循环
match.TU_trans()
  → emit_event('transit')    // 第287行
  → for_all('transit')
  → livingobject.transit()   // 第325行
  → trans.trans()           // 实际帧切换
```

**关键时序**：
1. `trans.frame(F, au)` - 只设置变量，不执行切换
   - 设置 `next = F`
   - 设置 `wait = 0`

2. `trans.trans()` - 实际执行帧切换 (Line 644-721)
   ```javascript
   this.trans = function () {
       if (wait === 0) {
           if (next !== 0) {
               $.frame.PN = $.frame.N
               $.frame.N = next        // Line 671: 先切换帧ID
               $.frame.D = $.data.frame[next]  // Line 680: 加载帧数据
               $.frame_update()        // Line 710: 然后调用帧更新
           }
       } else {
           wait--
       }
   }
   ```

**Unity 实现映射**：
- `TU_Update()` (Lines 385-407) - 等待计数器递减 + 触发帧更新
- `Frame_Update()` (Lines 415-497) - 实际帧切换逻辑

---

## 3. 🐛 当前已知问题 (Current Issues)

### 暂无

- 已修复/已完成的历史问题与任务记录已从本文件清理（可通过 git history 找回）

---

## 4. 📝 下一步行动计划 (Next Steps)

### 短期计划

1. **测试帧转换修复**
   - [ ] 测试站立状态的所有连招（防御、攻击、跳跃、行走、奔跑）
   - [ ] 测试奔跑状态的反向输入（应正确停止奔跑）
   - [ ] 测试跳跃攻击（应正确触发帧80）
   - [ ] 测试蹲伏二段跳（应正确触发4种跳跃类型）

2. **PlayMode 验证（P2/P3）**
   - [ ] **起跳瞬间**: 帧211→212 时，`ps.groundY` 被记录为当前 `transform.position.y`
   - [ ] **空中运动**: `ps.y` 递减（向上为负），`worldY = groundY + ps.y/100` 正确反映高度
   - [ ] **落地检测**: `ps.y > 0`（FLF 下落后）时，clamp 为 0，`vy=0`，触发落地事件
   - [ ] **坐标映射**: `ToUnityPosition()` 正确映射到 Unity 3D 坐标（X/Y/Z = FLF x/跳跃高度/z）
   - [ ] **Footprint Rect**: `GetFootprintRect()` 正确从 BodyBox[0] 计算地面矩形
   - [ ] **Overlap 检测**: 角色身体与 NoWalkZone 重叠时被阻挡（不允许半身进入）
   - [ ] **确定性解算**: 斜向撞墙时能滑墙（X-only 或 Z-only），不会卡死/抖动
   - [ ] **速度归零**: 完全阻挡（stop）时，`vx=0`，`vz=0`，保持上一合法位置
   - [ ] **场景边界**: 到达边界（0/10000, 0/5000）时，位置被 clamp，不穿模

3. **验证 FLF 行为一致性**
   - [ ] 对照 FLF 源码验证帧转换时机
   - [ ] 对照 FLF 源码验证等待时间设置
   - [ ] 对照 FLF 源码验证权限系统

4. **继续实现缺失功能**
   - [ ] 武器系统（影响 States 0, 1, 2, 4, 5, 15）
   - [ ] 重拳检测逻辑（State 0 的 `att` 事件）
   - [ ] 对角移动速度调整（State 1/2 的速度系数）
   - [ ] 等待时间设置（walking_frame_rate, running_frame_rate）

### 中期计划

5. **完善状态处理器**
   - [ ] State 3: 攻击状态（笛子攻击 Kind 10/11）
   - [ ] State 9/10: 抓取系统（抓取计数器、位置同步）
   - [ ] State 12: 倒地系统（弹起判定、起身逻辑）
   - [ ] State 13/18: 特效系统（冰冻、燃烧）

6. **实现物理系统**
   - [ ] Z轴移动（深度移动）
   - [ ] 摩擦力系统（`unit_friction()`）
   - [ ] 跳跃速度系统（起跳速度计算）
   - [ ] 冲刺速度系统（冲刺速度设置）

### 长期计划

7. **完整复刻 FLF 状态机**
   - [ ] 实现 id_update 机制（角色特定逻辑）
   - [ ] 实现 MP/HP 系统
   - [ ] 实现武器交互系统
   - [ ] 实现场景查询系统

---

## 5. 📚 技术文档参考

### FLF 源码分析文档

- **FLF States 3-19 完整分析**：`I:\C++Test\NTSD\FLF_States_3-19_完整分析.md`
- **CharacterStates 重构完成报告**：`I:\C++Test\NTSD\CharacterStates_FLF完全重构完成报告.md`

### 关键源码位置

**FLF 源码**：
- 游戏循环：`I:\C++Test\NTSD\F.LF-master\LF\match.js` Lines 285-300
- 帧转换：`I:\C++Test\NTSD\F.LF-master\LF\livingobject.js` Lines 644-721
- 状态处理：`I:\C++Test\NTSD\F.LF-master\LF\character.js` Lines 239-1404

**Unity 实现**：
- 帧转换器：`Assets\NTSD\Scripts\Animation\Character\FrameTransistor.cs`
- 动画播放器：`Assets\NTSD\Scripts\Animation\LF2CharacterAnimator.cs`
- 状态处理器：`Assets\NTSD\Scripts\Animation\Character\CharacterStates.cs`

---

## 6. 🔍 调试技巧与经验教训

### 发现时序Bug的过程

1. **用户观察**：调用 `trans.Frame(5, 5)` 后无法切换到目标帧
2. **初步假设**：权限参数（authority）冲突
3. **深入分析**：发现是字段同步时机问题
4. **对照 FLF**：FLF 先设置 `$.frame.N`，再调用 `frame_update()`
5. **定位根因**：Unity 实现先读旧值，再被覆盖，再同步

### 关键教训

1. **严格对照源码**：不能只看逻辑，还要看执行顺序
2. **注意字段同步时机**：分离的数据结构需要立即同步
3. **命名语义很重要**：
   - `PlayFrameByID()` - 立即播放（绕过等待）
   - `TransitionToFrame()` - 立即转换（同步字段）
   - `trans.Frame()` - 仅设置目标（延迟执行）

---

## Pending Plan: Plan A 完善路线（先记录，不立即改代码）

> 已完成的计划/任务（例如 P0-P3、历史阶段性计划）已从本文件移除；如需追溯请查 git history。

### P4: 碰撞与攻击判定的分阶段路线
**Goal**
- 所有水平速度符号/帧力 dvx/奔跑维持速度的方向，统一从 `ps.dir` 读取（FLF `dirh()` 语义）。
- `transform.localRotation = (left ? Y180 : identity)` 只负责显示，不作为逻辑真值来源。

**Rules**
- `dirh = (ps.dir == "left") ? -1 : +1`
- 禁止从 `transform.localScale`/`transform.localRotation` 反推方向来计算 dvx/vx
- 切向的唯一入口（switch_dir/TurnToDir）必须同步：
  - `ps.dir`（逻辑）
  - `unitActions.dir`（如使用）
  - `transform.localRotation`（表现）

**Acceptance**
- `Frame_Force()` 应用 dvx 时使用 `ps.dir` 决定正负号（不再依赖 scale/rotation 推断）。

---

## 2026-01-02：关卡边界编辑器（Walkable Area）讨论纪要

### 结论（已确定）
- 关卡平面统一为 **X/Y**（`Vector2 = (x,y)`），`z` 不参与边界/阻挡几何判断。
- 边界不再用“Path + Width 的墙条”表达，而是用 **可行走区域 Walkable Area** 表达。
- 采用 **方案 A：多个凹多边形的并集**。
  - 一个关卡对象支持 `List<Polygon>`。
  - 角色 footprint `Rect` **只要完整落在任意一个 Polygon 内** ⇒ 允许移动/跳跃位移。

### 当前实现为什么“点/拖拽无效”（根因）
- `BoundaryWall.cs` 存在字符串未闭合的编译错误（会导致 Unity Editor 脚本不运行）。
- `BoundaryWallEditor.cs` HelpBox 文本字符串未闭合（同样会导致编译失败）。
- `BoundaryWallEditor.cs` 命名空间引用不完整：`BoundaryWall` 在 `NTSD.LevelEditor`，Editor 在 `NTSD.LevelEditor.Editor`，需要 `using NTSD.LevelEditor;` 或 `typeof(NTSD.LevelEditor.BoundaryWall)`。
- 鼠标坐标取法错误：使用 `HandleUtility.GUIPointToWorldRay(...).origin` 会得到摄像机射线起点，不是鼠标落点，导致 hover/插点/删点基本全部失效。必须做 **ray 与编辑平面求交**。
- 轴不一致：旧实现以 X/Z 编辑，但项目关卡平面最终确定为 X/Y。

### 目标功能（验收标准）
- SceneView 可视化编辑：拖拽顶点、Shift 点边插点、Ctrl 点点删点。
- 支持凹多边形；一个对象包含多个 Polygon；只编辑当前 active polygon（避免满屏手柄）。
- 数据先存 Manager/组件内，提供导出 JSON 按钮（后续按关卡保存/加载）。
- 运行时 API：`IsRectWalkable(Rect rectXY)`，用于 Plan A 位移解算（移动/跳跃都限制）。

### 运行时判定规则（严格，防“半身越界”）
- `RectFullyInsidePolygon(rect, poly)`：
  1) Rect 四角都在 poly 内（point-in-polygon）
  2) poly 任意边不与 rect 任意边相交（segment intersection）
- `IsRectWalkable(rect)`：只要有一个 poly 满足 `RectFullyInsidePolygon` 就返回 true。

### 数据存储与导出（阶段性）
- 先存到 `BoundaryWallManager/BoundaryWall` 中，提供 `ExportToJson` 按钮。
- JSON 建议结构：
  - `polygons: [{ name, verticesWorld:[{x,y},...] }, ...]`

### 待 Claude 执行的改造点（脚本范围）
- `Assets/NTSD/Scripts/LevelEditor/BoundaryWall.cs`
  - 改为 `List<Polygon>` 数据结构（local XY），闭合多边形。
  - `GetWorldVertex/SetWorldVertex` 统一 XY 映射到 `Vector3(x,y,fixedZ)`。
- `Assets/NTSD/Scripts/LevelEditor/Editor/BoundaryWallEditor.cs`
  - 修复编译/命名空间。
  - 鼠标拾取改为 ray-plane（XY 平面）。
  - 支持 active polygon 的顶点拖拽/插点/删点，多 polygon 管理。
- `Assets/NTSD/Scripts/LevelEditor/BoundaryWallManager.cs`
  - 提供 `IsRectWalkable(Rect rectXY)`。
  - 增加导出 JSON 按钮。

### 追加待办（2026-01-02）：垂直边界（Unity Y）层级规则
**背景/问题**
- 当前边界阻挡仅约束地面平面位移（ps.x/ps.z），跳跃/击飞高度（Unity `transform.position.y`）不参与边界判断。
- 需求：左/右/底部属于“绝对不可越界”；上方（Unity Y 高度方向）存在“特殊层”，仅击飞/受击上抛可越过，普通跳跃/移动不可越过。

**TODO（Phase 0：最小闭环）**
- [ ] 定义并实现 `VerticalCeiling` 规则：`maxY_Normal` 与 `maxY_Knockback`（世界单位），超出时 clamp 并清零上升速度。
- [ ] 明确“击飞/受击上抛”判定来源（推荐：由状态机显式设置 flag，而不是靠速度阈值猜测）。
- [ ] 在 `ApplyDynamics()` 中加入垂直边界 clamp（在 `ps.y += ps.vy` 后、写回 `transform.position` 前）。
- [ ] Inspector 可配置：开关、两档 maxY、调试日志（必要时绘制 gizmo 线）。

**TODO（Phase 1：编辑器/数据）**
- [ ] 在边界数据导出 JSON 中增加垂直边界配置（或单独导出 `VerticalCeiling` 配置）。
- [ ] （可选）将“上方特殊层”升级为可编辑的高度区间（例如 `yMin/yMax` 的 volume/带状区域），而非全局单条 ceiling。

**UPDATE（2026-01-03）**
- 已确认采用 FLF 风格的 2.5D 映射：地面平面使用 `ps.x/ps.z`，而跳跃/击飞高度使用 `ps.y` 但只做“子节点（Sprite/Model）视觉偏移”。
- 因此不再需要“天花板/垂直边界”与“上方特殊层”方案；边界系统简化为单一 Walkable 多边形并集，仅约束地面平面（X/Z）。

---

## 7. Mechanics 对齐清单 V2（FLF `mechanics.js` -> Unity）

> 目的：把 FLF 的 `I:\C++Test\NTSD\F.LF-master\LF\mechanics.js` 中“适合数据层/运动学层”的职责，明确映射到 Unity 工程的脚本位置，避免把碰撞/渲染/状态机逻辑塞进 `CharacterMechanics`。

### 7.1 设计原则（必须遵守）
- `CharacterMechanics`：只做 `PhysicsState(ps)` 的运算（位置/速度/摩擦/重力/边界解算），不直接触发状态事件，不直接写 Unity Transform。
- `LF2CharacterAnimator`：负责每 Tick 调用 mech，负责把结果写回 Unity（Transform/UnitActions），并在需要时触发 `CharacterStates` 事件。
- 日志统一使用 `NTSD.Tools.Log`（禁止 `Debug.Log*`），并由外层 debug 开关控制是否输出。
- Tick 热路径禁止分配：`ApplyDynamics()` 不能每 Tick new lambda/delegate；`Context/Result` 用 struct；委托必须缓存。

### 7.2 映射表（按 FLF 函数维度）
| FLF `mechanics.js` | 用途 | Unity 对应位置（建议） | 状态 |
|---|---|---|---|
| `create_metric()` | 创建/初始化 `ps`（位置/速度/dir/fric…） | `Assets/NTSD/Scripts/Animation/Character/PhysicsState.cs` | 已有（ps 数据结构已建立） |
| `reset()` | 重置 `ps` 到初始值 | `PhysicsState` 增加 `Reset()`（或等价方法） | 可选（按需要补） |
| `set_pos(x,y,z)` | 直接放置脚底点 + 边界 clamp | `CharacterMechanics.SetPos(...)`（内部调用边界规则） | TODO |
| `dynamics()` | 主动力学：位移/边界/重力/摩擦/落地修正 | `CharacterMechanics.Step()` + `LF2CharacterAnimator.ApplyDynamics()` 写回 | 已有（但需去掉 Tick 分配 + 换 Log） |
| `unit_friction()` | 单位摩擦（每 tick -1 的简化摩擦） | `CharacterMechanics.UnitFriction(ps)` | TODO |
| `linear_friction(x,z)` | 指定摩擦量（用于落地/倒地等特殊刹车） | `CharacterMechanics.LinearFriction(ps, x, z)` | TODO |
| `speed()` | 速度标量（FLF 默认只算 vx/vy） | `CharacterMechanics.SpeedXY(ps)`（明确不含 vz） | TODO |
| `blocking_xz()` | 基于 itr:14（障碍）判定前方阻挡 | 未来：`LF2CollisionSystem` / 场景查询系统；不是 WalkableArea | 暂不做（除非要对齐 itr:14） |
| `project()` | Sprite 投影到屏幕坐标 + z 排序 | Unity：Animator/渲染写回（Transform/Sorting） | 不放 mech |
| `body()/volume()/body_body()` | 构造 bdy/itr 的体积数据（碰撞/判定） | Unity：`PhysicsState` 的 volume/rect/volume builder + `LF2CollisionSystem` | 不放 mech（保持分层） |
| `make_point()/coincideXZ()/coincideXY()` | 抓取/武器跟随/点对齐（依赖 sprite.w/centerx/centery） | 建议新增 `AttachmentKinematics`（或放 Animator/Weapon 系统） | 视后续抓取/武器对齐需求决定 |

### 7.3 映射表（按“职责”维度，方便 Claude 拆分）
- **运动学/动力学（进入 `CharacterMechanics`）**：位移、边界解算、落地修正、地面摩擦、空中重力、`unit_friction/linear_friction/speed`。
- **Unity 写回（留在 `LF2CharacterAnimator`）**：Transform 更新、UnitActions 赋值、视觉偏移、落地事件触发（`fell_onto_ground`）。
- **碰撞/体积（不要放进 `CharacterMechanics`）**：bdy/itr 体积生成、scene query、受击/击飞结算（`LF2CollisionSystem` 等）。
- **投影/排序（不要放进 `CharacterMechanics`）**：渲染排序、阴影、sprite 相关。

### 7.4 当前实现需要立即补齐的点（给 Claude 的行动清单）
1) `LF2CharacterAnimator.ApplyDynamics()` 热路径去分配：缓存 `isPointWalkable` 委托；日志委托用静态 method-group；不允许每 Tick new lambda。
2) `LF2CharacterAnimator` 与 `CharacterMechanics` 的所有日志改为 `NTSD.Tools.Log`。
3) 在 `CharacterMechanics` 中补齐可复用 helper：`UnitFriction`、`LinearFriction`、`SpeedXY`、（可选）`SetPos`。
4) `blocking_xz()` 暂不对齐：当前 Unity 的 WalkableArea/BoundaryWall 不是 FLF itr:14 机制；除非后续要实现 itr:14 障碍物，再另开任务。

> 备注：`NTSD.Tools.Log.Warn(string, params object[])` 不能直接作为 `Action<string>` 缓存（delegate 签名不匹配），需要写一个 wrapper，例如 `static void Warn1(string msg) => Log.Warn(msg);` 再把 `Warn1` 缓存为 `Action<string>`。

---

## 8. StateUpdateFrame 设计与实现说明（FLF `state_update` 对齐）

> 目标：在 Unity 的 `CharacterStates` 中实现一个“可返回 frameId 的 state_update 通道”，用于 `fell_onto_ground` / `fall_onto_ground` 两个事件，严格对齐 FLF `livingobject.state_update` 的顺序与返回策略。
>
> FLF 参考：`I:\C++Test\NTSD\F.LF-master\LF\livingobject.js:292-305`
> - 顺序：先 `states.generic(...)`，再 `states[currentState](...)`
> - 返回：`res1 || res2`（generic 的 truthy 返回优先）

### 8.1 为什么不能直接用 `HandleStateEvent`
- `HandleStateEvent` 是“事件分发 + bool handled”语义，并且会触发 generic TU/Frame/Combo 等整套流程。
- FLF 的 `state_update('fall_onto_ground')` 仅用于“generic + specific 的一次性可覆盖判定”，并且需要返回 frameId（或表示已接管）。
- 所以必须新增一个**专用的** state_update-return-frame 通道，且只用于 `fell_onto_ground/fall_onto_ground`。

### 8.2 新增结果类型：支持 frameId 或 handled
在 `Assets/NTSD/Scripts/Animation/Character/CharacterStates.cs` 中新增：

```csharp
public readonly struct StateUpdateFrameResult
{
    public readonly bool handled;
    public readonly int? frameId;

    public StateUpdateFrameResult(bool handled, int? frameId)
    {
        this.handled = handled;
        this.frameId = frameId;
    }

    public static readonly StateUpdateFrameResult None = new StateUpdateFrameResult(false, null);
}
```

### 8.3 新增 `StateUpdateFrame(...)`：严格按 FLF 顺序 generic→specific，并按 `res1 || res2` 合并
约束：**不要复用 `HandleStateEvent`**，避免把 generic TU/Frame 的逻辑混进去。

```csharp
private StateUpdateFrameResult StateUpdateFrame(LF2CharacterAnimator character, string eventType)
{
    if (character == null || character.CurrentFrame == null) return StateUpdateFrameResult.None;

    // 只允许这两个事件走该通道
    if (eventType != "fell_onto_ground" && eventType != "fall_onto_ground")
        return StateUpdateFrameResult.None;

    // 1) generic 先执行
    var res1 = InvokeGenericStateUpdateFrame(character, eventType);

    // 2) specific 再执行
    var res2 = InvokeSpecificStateUpdateFrame(character, eventType);

    // 3) 返回策略对齐 FLF：res1 || res2
    int? frameId = res1.frameId ?? res2.frameId; // generic 优先
    bool handled = res1.handled || res2.handled;

    return new StateUpdateFrameResult(handled, frameId);
}
```

其中 `InvokeGenericStateUpdateFrame/InvokeSpecificStateUpdateFrame` 的实现方式建议参考你现有 `GetNextFrameId()` 的思路（用一个 eventData 容器让 handler 写回 frameId）：

```csharp
private sealed class StateUpdateFrameData
{
    public int? frameId;
    public bool handled;
}

private StateUpdateFrameResult InvokeGenericStateUpdateFrame(LF2CharacterAnimator character, string eventType)
{
    // 只做 generic 对该 eventType 的处理；默认返回 None。
    // 注意：不要调用 GenericStateHandler 的 TU/frame/combo 分支，只在 generic 中单独加这两个 eventType 的分支。
    return StateUpdateFrameResult.None;
}

private StateUpdateFrameResult InvokeSpecificStateUpdateFrame(LF2CharacterAnimator character, string eventType)
{
    // 只调用当前 state 的 handler；handler 可以：
    // - 设置 data.frameId 来请求 generic TU 统一 TransitionToFrame(frameId, 15)
    // - 或者直接在 handler 内调用 TransitionToFrame 并设置 handled=true
    return StateUpdateFrameResult.None;
}
```

### 8.4 改造 generic TU 的 3 段落地逻辑：用 `StateUpdateFrame(...)` 替代 `state_update(...)`
修改位置：`Assets/NTSD/Scripts/Animation/Character/CharacterStates.cs` 的 `HandleGenericTU()` 落地相关分支。

#### A) `fell_onto_ground`（ps.y==0 && ps.vy>0）
结构对齐 FLF：
- 先 `res = StateUpdateFrame(character, "fell_onto_ground")`
- `res.frameId.HasValue` → `TransitionToFrame(res.frameId.Value, 15)`
- `else if (res.handled)` → do nothing（表示已接管，不走默认）
- `else` → 默认：`ps.vy=0` + 落地瞬间摩擦：
  - `fricX = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, ps.vx)`
  - `fricZ = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, ps.vz)`
  - `CharacterMechanics.LinearFriction(ps, fricX, fricZ, NTSDGlobal.Gameplay.MinSpeed)`

#### B) `fall_onto_ground`（ps.y+ps.vy>=0 && ps.vy>0）
结构对齐 FLF：
- 先 `res = StateUpdateFrame(character, "fall_onto_ground")`
- `res.frameId.HasValue` → `TransitionToFrame(res.frameId.Value, 15)`
- `else if (res.handled)` → do nothing
- `else` → 默认：Frozen 不动；JumpingAir→Crouch(215,15)；其它→Crouch2(219,15)

### 8.5 实施约束（必须遵守）
- 新通道只用于 `fell_onto_ground/fall_onto_ground`，其它事件不要改。
- 不要新增 `Debug.Log*`；如需日志只用 `NTSD.Tools.Log`，但本任务不要求加日志。
- 允许不同 state 返回不同 frameId（不得写死到 generic TU 里）。
- 通过 `res1.frameId ?? res2.frameId` 实现 generic 优先（对齐 FLF `res1 || res2`）。


---

## 9. 通用 StateUpdate（对齐 FLF `state_update`）改造要求

> 背景：当前 `StateUpdateFrame` 仅覆盖 `fell_onto_ground/fall_onto_ground`。但 FLF 的 `state_update(event, ...)` 是 **对所有事件通用** 的（不同状态内会在多处用不同 eventType 调用），因此 Unity 侧也必须提供一个通用入口，支持未来扩展。
>
> FLF 参考：`I:\C++Test\NTSD\F.LF-master\LF\livingobject.js:292-305`
> - 执行顺序：先 `states.generic(...)`，再 `states[currentState](...)`
> - 返回策略：`res1 || res2`（generic 的 truthy 返回优先）

### 9.1 目标
在 `Assets/NTSD/Scripts/Animation/Character/CharacterStates.cs` 中实现一个**通用**的 `StateUpdate(...)`（或等价命名），可在任意逻辑处（generic TU / specific state handler / 其它）调用，语义对齐 FLF：
- 支持任意 `eventType`（字符串）
- 先执行 generic，再执行 specific
- 返回策略对齐 `res1 || res2`（generic 优先）
- 能携带“返回帧（frameId）”能力，用于落地等场景

### 9.2 设计约束（必须遵守）
- 不能复用 `HandleStateEvent(...)` 来实现 `StateUpdate(...)`（避免把 generic TU/frame/combo 等整套逻辑混入 state_update 语义）。
- 必须使用 **引用类型容器（class）** 传递返回结果（禁止 `readonly struct`），否则 handler 无法写回。
- generic 与 specific 都需要被调用（对齐 FLF），但最终返回值合并必须保持 generic 优先。
- 不新增 `Debug.Log*`；如需日志只用 `NTSD.Tools.Log`（本改造不强制加日志）。
- `CharacterStates.cs` 是运行时代码：移除或 `#if UNITY_EDITOR` 包裹 `UnityEditor.*` 引用，避免运行时/打包编译失败。

### 9.3 推荐实现方式（最稳，严格对齐 FLF）
新增一个结果容器：

```csharp
public sealed class StateUpdateData
{
    public bool handled;
    public int? frameId;
}
```

新增一个通用入口（示意）：
- 创建 `genericData` 与 `specificData` 两份（避免互相覆盖）
- 分别调用 generic handler 与 specific handler（顺序必须是 generic→specific）
- 最终合并：
  - `frameId = genericData.frameId ?? specificData.frameId`（generic 优先）
  - `handled = genericData.handled || specificData.handled`

```csharp
private StateUpdateFrameResult StateUpdate(LF2CharacterAnimator character, string eventType, object eventData = null)
{
    // 1) generic
    // 2) specific
    // 3) merge: generic-first
}
```

> 注：返回类型可以继续用现有的 `StateUpdateFrameResult`（handled + frameId），但 eventData 写回必须用 class。

### 9.4 落地逻辑的使用方式（保持现有功能）
`HandleGenericTU()` 中 `fell_onto_ground/fall_onto_ground` 的逻辑继续使用通用 `StateUpdate(...)`：
- 若返回 `frameId`：统一 `TransitionToFrame(frameId, 15)`
- 若 `handled==true`：不走默认
- 否则走默认（包括落地瞬间摩擦：`LookupAbs(FrictionFell)` + `CharacterMechanics.LinearFriction`）


---

## 10. FLF `blocking_xz`（itr:kind:14）阻挡体：真实数据从哪里来（计划）
> 目标：保持与 FLF 一致的阻挡语义（kind:14 + blocking_xz），同时保持项目不依赖 Unity Collider，便于后续 ECS/大规模实体。
> 参考：`I:\C++Test\NTSD\F.LF-master\LF\mechanics.js` 的 `mech.prototype.blocking_xz()` 与 `mech.prototype.dynamics()`。

### 10.1 最终目标（行为一致）
- 阻挡数据来自 DAT 解析后的 `LF2FrameData.itrs`，并筛选 `InteractionArea.kind == 14`。
- 阻挡判定对齐 FLF：
  - 用“当前帧 bdy 体积 + offset(vx,vz)”预测下一步位置（等价 FLF `body(..., offset)`）
  - blocking 查询时将 body 的 `zwidth = 0`（等价 FLF `body[i].zwidth = 0`）
  - 若与任意 `itr:14` 相交，则 `blocking_xz == true`
  - dynamics 中被阻挡时：只移动 `vx/vz` 的 `0.1`（速度不变，靠摩擦衰减）

### 10.2 当前项目对齐点（后续接入时不要破坏）
- `CharacterSim.SimTick()` 顺序为 `Transit()` -> `TU_Update()`（与 FLF 分阶段一致）。
- `LF2CharacterAnimator.ApplyDynamics()` 负责计算 `blockedMoveScale` 并传入 `CharacterMechanicsContext`（dynamics 层只做位移/摩擦/重力）。
- `LF2CollisionSystem.BlockingXZ(...)` 是 blocking_xz 的唯一入口（必须保持无 per-tick 分配）。

### 10.3 “真实数据来源”接入：推荐结构（与当前架构兼容）
障碍物/可生成物体的阻挡体（itr:14）需要的最小输入：
- `datKey`：该物体对应的 DAT（或 objectId -> datKey 的映射）
- `frameKey`：具体使用哪一帧作为阻挡帧（建议优先使用 `frameId`；必要时支持 `frameName/state` 解析）
- `spriteWidthPx`：用于 facingLeft 的镜像公式（FLF：`localX = sp.w - itr.x - itr.w`）
- `facingLeft`：是否需要镜像（静态障碍物通常固定为 false，但接口上必须可扩展）

为了不把“解析/IO/资源读取”塞进 collision/mechanics 层，建议新增 Provider/Repository：
- `ILF2FrameDataProvider`：`TryGetFrame(datKey, frameId, out LF2FrameData frame)`
- `ILF2SpriteMetricsProvider`：`TryGetSpriteWidthPx(spriteKey, out float widthPx)`（或 `sp.w`）
- `LF2FrameDataRepository`：缓存所有 dat->frame，关卡加载/启动时构建，运行时只读

注意事项（必须遵守）：
- Provider 必须只读缓存/引用：禁止在 `BlockingXZ` / `ApplyDynamics` / `Step` 内做解析/磁盘 IO/反序列化。
- `spriteWidthPx` 的定义必须与角色一致（角色当前用 `SpriteRenderer.sprite.textureRect.width`），否则 facingLeft 镜像将产生偏差。
- 日志统一使用 `NTSD.Tools.Log`，不要 `Debug.Log`。

### 10.4 接入流程（把重活放到初始化/加载阶段）
推荐流程（后续真正改时按这个做）：
1) 关卡加载阶段：通过 `LF2FrameDataRepository` 预加载并缓存所需 DAT（按关卡引用清单）。
2) 障碍物实例化后：阻挡体组件仅持有 key（datKey/frameId/spriteKey/facingLeft）。
3) `RefreshFromProviders()`：
   - 从 Provider 获取 `LF2FrameData`（包含 `centerx/centery/itrs`）
   - 从 Provider 获取 `spriteWidthPx`
   - 缓存到本地字段（仅一次），并在缺失时 `Log.Warn`（只打印一次，避免刷屏）
   - 动态生成对象：实例化后应尽快调用 `LF2BlockingObstacle.Configure(frameData, spriteWidthPx, facingLeft)`（或等价的 Set* 接口）完成赋值
4) 运行时 tick：`LF2CollisionSystem.BlockingXZ` 只做：
   - `actor.ps` 生成预测 body volumes（offset=(vx,vz)）并令 `zwidth=0`
   - 遍历障碍物的 `itr:14` volumes（由组件把 kind14 itrs 转成 `FlfVolume` 写入复用 list）
   - 用 `Intersect(body, itr14)` 判断是否阻挡

### 10.5 关键实现细节（对齐 FLF 的计算规则）
- 坐标系：Unity ground plane (X/Y) ↔ FLF (x/z)，转换使用 `SimulationConstants.PIXELS_PER_UNIT`。
- `sx/sy/sz` 的 origin 计算需与 `PhysicsState.UpdateSpriteOrigin(...)` 的语义一致：
  - right：`sx = x - centerx`
  - left ：`sx = x + centerx - spriteWidthPx`
- `FlfVolume` 的矩形定义：`(x+vx, y+vy, w, h)`，深度区间：`[z-zwidth, z+zwidth]`。

### 10.6 ECS 方向注意点（提前约束）
- 阻挡体数据应可烘焙为纯数据（frameId/center/itrs(kind14)/spriteWidth），运行时查询不依赖 Mono/Unity 组件。
- `LF2CollisionSystem` 的 registry 未来可替换为 ECS 世界的空间索引，但 `BlockingXZ` 的语义不变。
