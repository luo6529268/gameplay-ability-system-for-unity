# CAMERA-BACKGROUND-NOCROP-FIT-001 — no-crop full-background fit mode

<!-- CHANGE-RECORD
id: CAMERA-BACKGROUND-NOCROP-FIT-001
status: ROLLED_BACK
code-path: Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs
authority: USER-DIRECTED-20260824 / UNITY-NATIVE-PRESENTATION
evidence: ROLLBACK-UNITY-COMPILE-PLAY-20260824
-->

> 创建日期：2026-08-24  
> 当前状态：`ROLLED_BACK`  
> 类型：presentation / camera / background framing

## Goal

替代已被用户拒绝的裁切 Cover 模式，为`BattleCameraSafeArea`提供两种可切换类型：

1. `完整显示（可能留空）`：背景保持原始比例，显示全部内容；
2. `全面覆盖（保留全部内容）`：背景不裁切，通过必要轴向的非等比伸缩填满相机视野。

## Scope

- 仅修改`Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs`；
- 删除`CoverViewport`与其`Mathf.Min`裁切尺寸分支；
- 保留当前`ContainBackground`相机尺寸公式；
- 新的全面覆盖模式先以原始背景 base local scale 计算完整相机尺寸，再按需要只伸缩 X 或 Y，使背景 bounds 精确填满相机宽高；
- 保留用户可切换的 Inspector enum，默认全面覆盖；
- 禁止裁切、禁止改相机 Transform/rect、禁止安全区/viewport/follow/debug/camera offset、禁止改资源或scene YAML。

## Behavior contract

对于背景初始 bounds `(W,H)` 与相机 aspect `A`：

- 相机完整显示尺寸：`S = max(H / 2, W / (2A))`；
- 全面覆盖目标尺寸：`targetWidth = 2SA`，`targetHeight = 2S`；
- 背景 scale 的 X/Y 分别乘`targetWidth / W`与`targetHeight / H`；
- 因`S`来自`max`，一个轴保持 1，另一个轴拉伸；全部原始内容保留，不裁切。

## Risks / user-visible tradeoff

- 宽背景在16:9下将纵向拉伸；当前`Bg (2)`约为`16.936876 / 14.114063 = 1.20`；
- 模式切回完整显示或组件停用时必须恢复原始背景 scale，避免污染场景/对象池重用状态；
- 所有 base scale 捕获、恢复和 background reference 更换必须最小且可逆。
- 组件为`ExecuteAlways`，全面覆盖模式会修改已加载场景**内存**中的背景`localScale`；agent不直接修改或保存scene YAML。
  若用户选择保存场景，Unity会自然序列化当前mode、base scale和背景scale；当前`NTSD_Battle.unity`的既有大范围diff
  不包含本Change的`backgroundFitMode`、captured-scale或`y: 1.2`字段。

## Acceptance

1. Unity compile无本脚本 C# error；
2. Inspector仅有上述两个无裁切模式；
3. 当前`Bg (2)`在全面覆盖模式下完整显示且`bounds`等于相机 view bounds；
4. 切换回完整显示时背景恢复 base scale，且相机回到`Contain`尺寸；
5. 不写 Camera Transform、rect、SpriteRenderer asset、战斗runtime或scene文件；
6. 短 Play、filtered Console、Ledger validator与scoped diff通过。

## Rollback

只回退本脚本与本Change文档；不操作scene、DAT、资源或战斗脚本，需用户明确授权才可执行回退。

## 实际改动

- 已将原`CoverViewport`替换为`StretchToViewport`，Inspector显示为“全面覆盖（保留全部内容）”；
- 默认值为`StretchToViewport`；现有值`1`保持映射到新的第二枚举项，不需要agent修改scene YAML；
- 已加入背景 base local scale 的捕获、按背景引用切换的恢复、全面覆盖时的轴向缩放、切回完整显示及组件停用/销毁时的恢复；
- 背景尺寸计算始终以 base bounds 派生，避免每帧按已拉伸 bounds 再次缩放产生累积；
- 没有相机 Transform/rect writer，没有safe-area、viewport、follow、debug、camera offset或战斗runtime改动。

## 验证状态

| 层级 | 实际结果 | 状态 |
|---|---|---|
| Unity compile | 待最终代码重新编译 | `PENDING` |
| 当前Scene全面覆盖 | 待确认background scale与camera view bounds相同 | `PENDING` |
| 完整显示恢复 | 待在可逆的临时检查中确认base scale/contain size恢复 | `PENDING` |
| 短 Play / Console | 待执行 | `PENDING` |
| Ledger / diff | 待最终代码后执行 | `PENDING` |

## 最终验证

| 层级 | 实际结果 | 状态 |
|---|---|---|
| Unity compile | `Assembly-CSharp.dll`/Editor DLL于`2026-08-24 15:14:34`重新生成；当前`Editor.log`无`error CS`或本脚本编译失败 | `PASS` |
| 无裁切静态审计 | `CoverViewport`与`Mathf.Min`已不存在；无 camera Transform/rect、safe-area、viewport、follow、debug或camera-offset writer | `PASS` |
| 全面覆盖默认值 | 当前组件`backgroundFitMode=1`（`StretchToViewport`），Camera size=`8.46843815`，背景 scale=`(1,1.2,1)`；bounds=`30.1100025 × 16.9368763`，与相机视野`(2×8.46843815×16/9) × (2×8.46843815)`一致 | `PASS` |
| 全部内容保留 | 相机仍采用完整显示尺寸`8.46843815`，背景宽度保持`30.1100025`，只将高度由`14.1140633`拉伸至`16.9368763`；没有横向裁切或图集/资源修改 | `PASS` |
| 完整显示切换 | 在临时 Play 场景将 mode 设为`0`后，只读快照显示背景 scale恢复`(1,1,1)`、bounds恢复`30.1100025 × 14.1140633`，Camera size保持完整显示值 | `PASS` |
| 全面覆盖恢复 | 在同一临时 Play 场景切回 mode`1`后，scale/bounds恢复`(1,1.2,1)`/`30.1100025 × 16.9368763` | `PASS` |
| Transform 边界 | 两次切换前后`ScenesCamera`均为`(-1.77,-6.69,-10)` | `PASS` |
| 短 Play / Console | `BattleTestBootstrap` complete；按`BattleCameraSafeArea`过滤的 Console error=`0` | `PASS` |
| Ledger / diff | `Tools/Validate-ChangeLedger.ps1`通过（98 Records / 123 governed code files）；scoped `git diff --check`通过 | `PASS` |

说明：UnityMCP的`manage_components.set_property`在 Play Mode 返回“cannot be used during play mode”，
但每次调用后的只读组件资源都显示 mode 已实际变更并完成对应 scale/bounds 切换。该工具返回值与实际 Unity
状态矛盾，故本 Record 以只读快照和数值为验收证据，而不把 MCP 返回文本误报为 gameplay/script 异常。

agent未调用scene save。当前`NTSD_Battle.unity`本来就有大量用户UI/layout修改；只读检查确认其Git diff
不包含本Change的mode、captured scale或背景`y:1.2`序列化字段，因此没有将场景资产变更纳入本交付。

## User constraint correction / rollback plan

用户明确禁止修改背景缩放，理由是后续联机对战不能让共享背景`Transform`成为会被运行时写入的状态。
因此本Change的`StretchToViewport`、base-scale捕获/恢复和所有背景`localScale` writer不再可交付，
必须在本轮删除并回到仅写`Camera.orthographicSize`的完整显示行为。无裁切全覆盖若要恢复，
只能另行设计为不写背景Transform的纯渲染层方案；该方案不在本轮实现。

### Rollback code written

- 已删除`BackgroundFitMode`、`StretchToViewport`、base-scale隐藏字段、`OnDisable`/`OnDestroy`恢复和全部背景scale计算/writer；
- 脚本已恢复为只读取`backgroundRenderer.bounds`并写`Camera.orthographicSize=max(extents.y, extents.x / aspect)`；
- 未尝试实现新的纯渲染层全覆盖方案。

### Rollback verification

| 层级 | 实际结果 | 状态 |
|---|---|---|
| Unity compile | `Assembly-CSharp.dll`/Editor DLL于`2026-08-24 15:33:58`重新生成；无`error CS`或本脚本编译失败 | `PASS` |
| 静态边界 | 没有`localScale`、`transform`、`BackgroundFitMode`、`StretchToViewport`、`CoverViewport`、`Mathf.Min`或旧follow/debug/camera-offset writer | `PASS` |
| 编辑器运行时对象 | 当前组件属性仅余`targetCamera`与`backgroundRenderer`；背景`localScale=(1,1,1)`、bounds恢复`30.1100025 × 14.1140633` | `PASS` |
| 短 Play | `BattleTestBootstrap` complete；运行时背景仍为`scale=(1,1,1)`，相机完整显示尺寸为`8.46843815`，filtered Console error=`0` | `PASS` |
| Ledger / diff | `Tools/Validate-ChangeLedger.ps1`通过（98 Records / 123 governed code files）；scoped `git diff --check`通过 | `PASS` |

结论：该无裁切缩放方案已按用户架构约束完整回退，不能作为后续全覆盖方案复用。

## Out of scope

无形变的边缘延展 shader、额外美术补图、背景平铺、camera follow、安全区、viewport布局、战斗逻辑与C++ authority。
