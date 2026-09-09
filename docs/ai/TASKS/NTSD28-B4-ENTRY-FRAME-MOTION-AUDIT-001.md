# Task Contract — NTSD28-B4-ENTRY-FRAME-MOTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIRST_DIFFERENCE_DEPTH_INTENT`

## 目标与结论

闭合B4入口及F-02 frame motion的Authority/Unity逐字段矩阵。`dvx/dvy/dvz`的阈值500、偏置550与X facing clamp主体基本匹配；首个可证明差异为depth intent：Authority只在Up/Down current严格互斥时写Z，双按恒none；Unity当前按`CdUp/CdDown`比较，双按仍会写Z。

## 下一包

`NTSD28-B4-F02-FRAME-MOTION-KERNEL-001`建立单一纯kernel并接入production C04；保留legacy direct helper，不改physics、frame step、revival、input producer、Scene/内容/Authority。
