# Task Contract — NTSD28-B4-F02-FRAME-MOTION-KERNEL-001

> 状态：`VERIFIED / PRODUCTION_NATIVE_KERNEL / STRICT_DEPTH_INTENT`
> 依赖：`NTSD28-B4-ENTRY-FRAME-MOTION-AUDIT-001 / VERIFIED`

## 目标

建立单一`BattleNativeFrameMotionKernel`并接入production C04，精确实现dvx/dvy/dvz阈值500、偏置550、facing clamp与strict XOR depth intent；消除双Up/Down时由cooldown决定Vz的差异。

## 修改范围

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- 新focused Editor test
- 治理文档

## 不变量

- 只改`ApplyNativeFrameMotionForWorldPass`生产入口；legacy `ApplyNonCharacterFrameVelocityForFrameAdvance`与direct SimTU保持。
- 不改输入producer、key/cooldown carrier、physics、teleport、frame step、revival、Scene、DAT/资源或Authority。
- `dvx/dvy/dvz==0`不写对应轴；`>500`覆盖且不看facing/intent。
- ordinary dvz仅strict Up XOR Down写入；双按或双未按均none。

## 验收

红灯缺kernel/双按差异；XYZ纯矩阵与production双按/单按；C04/输入/physics相关测试；broad、SelfCheck、Scene/Console/Ledger。
