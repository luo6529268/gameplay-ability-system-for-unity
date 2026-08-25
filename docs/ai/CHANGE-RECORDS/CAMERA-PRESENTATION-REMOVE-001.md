# CAMERA-PRESENTATION-REMOVE-001 — remove BattleCameraSafeArea non-background runtime behavior

<!-- CHANGE-RECORD
id: CAMERA-PRESENTATION-REMOVE-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs
authority: USER-DIRECTED-20260824 / UNITY-NATIVE-PRESENTATION-ADAPTER
evidence: UNITY-COMPILE-SCENE-PLAY-20260824
-->

> 创建日期：2026-08-24  
> 当前状态：`VERIFIED`  
> 类型：presentation / camera / cleanup

## 1. 需求与范围

用户要求从`BattleCameraSafeArea`移除：

- 安全区域、reserved UI root与安全区域叠加逻辑；
- viewport布局、角色跟随、舞台边界与非背景来源的视野逻辑；
- 相机跟随、目标收集、边界钳制与presentation camera offset写入；
- Editor preview、OnGUI、Gizmo和其他调试逻辑。

用户随后明确保留“相机尺寸自适应背景”这一项。因此改后组件只保留背景 bounds → 正交相机 contain size；
它不改写相机位置，不收集角色、不跟随角色、不钳制舞台、
不改 viewport、不写 presentation camera offset，也没有安全区或调试行为。

不得修改场景、DAT、战斗runtime、CentralOnly、`NTSDRenderSpace`、C++或其他相机组件。

## 2. Unity 原状与影响评估

- `BattleCameraSafeArea.cs`共1271行；其运行路径全部属于本次删除类别；
- `Assets/NTSD/Scene/NTSD_Battle.unity`仅序列化挂载该组件与其旧字段；
- 全项目脚本未发现对`TargetCamera`、`SafeAreaScreenRect`、`SafeAreaWorldRect`或`HasSafeAreaWorldRect`的直接引用；
- 旧字段不修改scene时会作为未知序列化数据留在YAML中，Unity会忽略它们；这是为保留用户当前dirty scene的最小变更。

## 3. 计划改动

| 文件 | 改前职责 | 改后职责 |
|---|---|---|
| `Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs` | 安全区、viewport、自动正交尺寸、背景对齐、follow、边界、camera offset、Editor/GUI/Gizmo调试 | 只保留背景 bounds 驱动的 orthographic contain size；其余逻辑删除 |

## 4. 不可回退边界

- 只允许按背景 bounds 修改正交相机的`orthographicSize`；不改 Transform、`rect`、enabled状态或URP配置；
- 不写`NTSDRenderSpace.PresentationCameraOffset`；
- 不修改scene序列化、prefab、GameConfig、BattleBootstrap、战斗tick或输入；
- 不删除组件类型或meta，避免已有场景引用断裂。

## 5. 验收

1. Unity fresh scripts compile为0 C# error；
2. `BattleCameraSafeArea.cs`中不再保留safe area、viewport布局、角色follow、舞台边界、debug/Editor/GUI/Gizmo、camera offset写入路径；
3. 有有效背景 sprite 时，正交尺寸等于完整容纳该背景所需的`max(extents.y, extents.x / cameraAspect)`，且不改写相机 Transform；
4. 现有`NTSD_Battle`场景保留组件引用而不产生Missing Script；
5. Change Ledger validator与scoped `git diff --check`通过。

## 6. 风险与回滚

- 移除后，原先由该脚本提供的移动相机、裁切viewport、安全区和调试取景将不再生效；这是用户明确要求的结果；
- 背景尺寸适配不会跟随角色或改写相机位置；它只在背景 bounds 或显示 aspect 改变时更新`orthographicSize`；
- 场景YAML中的旧字段本轮不清理；若用户后续希望物理删除这些字段，应作为单独scene migration处理；
- 回滚仅限此脚本和本Change的文档记录，需用户明确授权。

## 7. 实际改动与范围修正

- 初版曾将脚本收缩为无运行逻辑兼容标记；在用户追加“保留相机尺寸自适应背景”后，该中间状态不作为最终交付；
- 最终实现已恢复背景 `SpriteRenderer.bounds` 的 contain-size：`max(extents.y, extents.x / aspect)`；
- `Update`与`OnValidate`只重算`orthographicSize`，不读取实体、输入、safe-area、stage边界或任何战斗状态，也不改写相机 Transform；
- 仍删除safe-area、reserved root、viewport布局、角色follow、camera bounds、`PresentationCameraOffset`写入及Editor/GUI/Gizmo调试；
- 保留类名、`DisallowMultipleComponent`、`RequireComponent(Camera)`以及原有`targetCamera`/`backgroundRenderer`序列化字段名，避免现有场景组件和字段绑定失效；
- 未修改`.unity`场景、`NTSDRenderSpace`、任何战斗脚本或资源。

## 8. 验证状态

| 层级 | 实际结果 | 状态 |
|---|---|---|
| 静态调用审计 | 无外部脚本直接引用该组件公开成员；场景仍序列化组件 | `PASS` |
| Unity compile | `Assembly-CSharp.dll`/Editor DLL于`2026-08-24 14:44:27`重新生成；当前`Editor.log`无`error CS`或本脚本编译失败 | `PASS` |
| 静态职责审计 | 没有`OnGUI`、Gizmo、Editor callback、viewport、safe-area、follow、camera offset或 Transform writer；只保留 bounds contain-size | `PASS` |
| 现有场景引用 | `NTSD_Battle`中找到1个可解析的`BattleCameraSafeArea`，绑定`ScenesCamera`和`Bg (2)`，无Missing Script | `PASS` |
| 背景尺寸行为 | `Bg (2)` bounds=`30.1100025 × 14.1140633`，aspect=`1.77777779`；`max(7.0570316, 15.0550013 / aspect)=8.468438`，等于运行中`ScenesCamera.orthographicSize=8.46843815`；相机 Transform保持`(-1.77,-6.69,-10)`未由本脚本改写 | `PASS` |
| 短 Play 启动 | UnityMCP进入/退出Play成功；`[BattleTestBootstrap] === Test bootstrap complete ===`；按`BattleCameraSafeArea`过滤的Console error=0 | `PASS` |
| Ledger validator | `Tools/Validate-ChangeLedger.ps1`通过；本脚本已被本Change覆盖 | `PASS` |
| Scoped diff | `git diff --check -- <scoped paths>`通过 | `PASS` |

通用Console仍有MCP客户端断连噪声和一次既有场景关闭清理警告；两者均不含`BattleCameraSafeArea`，不作为本Change失败证据。

## 后续范围修正

`CAMERA-BACKGROUND-FITMODE-001`随后新增可切换的背景尺寸模式，并使`CoverViewport`成为默认值。
本 Record 仅裁决安全区、viewport布局、follow、debug与camera offset删除完成；不再单独定义最终背景尺寸策略。
