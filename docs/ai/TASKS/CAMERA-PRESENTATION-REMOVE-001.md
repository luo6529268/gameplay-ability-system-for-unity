# CAMERA-PRESENTATION-REMOVE-001 — BattleCameraSafeArea cleanup contract

> 状态：`COMPLETE / VERIFIED`

## Goal

移除`BattleCameraSafeArea`的安全区、viewport布局、角色跟随、边界与调试职责，仅保留背景 bounds 驱动的
正交相机尺寸自适应；不改变Unity战斗逻辑真相、CentralOnly或场景资产。

## Scope

- 仅修改`Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs`；
- 删除安全区域、viewport布局、角色follow/边界、camera offset与调试逻辑；
- 保留类名、组件类型、`targetCamera`/`backgroundRenderer`字段名与Camera require contract，保护既有场景引用；
- 仅根据`max(background.extents.y, background.extents.x / cameraAspect)`写`orthographicSize`，绝不写相机Transform。

## Verification

- fresh compile；
- 静态确认没有残余的safe-area、viewport布局、follow、debug或camera-offset逻辑；
- 当前场景的背景 bounds→`orthographicSize`数值检查及短Play启动；
- Ledger validator与scoped diff check。

## Out of scope

场景YAML清理、相机位置重设、URP、BattleBootstrap、`NTSDRenderSpace`、战斗runtime与C++ authority。
