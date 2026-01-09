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
