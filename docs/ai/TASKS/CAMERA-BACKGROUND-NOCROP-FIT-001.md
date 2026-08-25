# CAMERA-BACKGROUND-NOCROP-FIT-001 — no-crop background fit contract

> 状态：`ROLLED_BACK / USER-REQUIRED`

## Goal

提供“完整显示”与“全面覆盖但保留全部内容”两种类型，取代裁切背景的 Cover 模式。

## Scope

- 仅`BattleCameraSafeArea.cs`；
- 全面覆盖时只拉伸背景必要轴；
- 相机位置、rect、资源、scene与战斗runtime均不写入。

## Verification

- fresh compile；
- base scale、Contain恢复、Stretch完整覆盖的数值与组件检查；
- 短 Play / filtered Console；
- Ledger validator / scoped diff。

## Stop conditions

- 必须裁切原背景内容才能实现；
- 无法无副作用地恢复背景 base scale；
- 需要新增shader、资源、scene YAML或战斗代码。
