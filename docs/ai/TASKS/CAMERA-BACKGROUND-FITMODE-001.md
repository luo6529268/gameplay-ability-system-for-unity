# CAMERA-BACKGROUND-FITMODE-001 — camera background fit mode contract

> 状态：`SUPERSEDED / USER REJECTED CROP`

## Goal

为`BattleCameraSafeArea`提供不拉伸背景的`ContainBackground`与`CoverViewport`两种尺寸策略，
并以`CoverViewport`为默认值消除当前上下镂空。

## Scope

- 仅修改`Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs`；
- 保留当前只写`Camera.orthographicSize`的边界；
- 不修改场景、背景、Transform、UI、URP、战斗 runtime 或 C++。

## Verification

- fresh Unity compile；
- 当前背景/相机的两种公式数值检查；
- 短 Play 与 filtered Console；
- Ledger validator 与 scoped diff check。

## Stop conditions

- 发现实现必须写 Camera Transform、viewport 或 SpriteRenderer scale；
- 发现现有场景无法解析新增 enum；
- 发现本改动影响战斗 runtime 或其他相机组件。

## Out of scope

任何实体跟随、安全区、视野裁切、背景拉伸、C++ battle alignment 和 scene migration。
