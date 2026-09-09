# NTSD 2.8-Logan B4 frame-motion entry

> Change ID：`NTSD28-B4-ENTRY-FRAME-MOTION-AUDIT-001`  
> 状态：`VERIFIED / FIRST_DIFFERENCE_DEPTH_INTENT`

| 轴/分支 | Authority | Unity当前 | 裁决 |
|---|---|---|---|
| X `dvx>500` | `vx=dvx-550`，不看facing | 同 | MATCHING |
| X `dvx!=0 && <=500` | 按facing-space只向目标阈值clamp | `ApplyFrameAxisVelocity`同形 | MATCHING，需矩阵锁定 |
| Y `dvy>500` | `vy=dvy-550` | 同 | MATCHING |
| Y ordinary nonzero | `vy+=dvy` | 同 | MATCHING |
| Z `dvz>500` | `vz=dvz-550`，不看intent | 同 | MATCHING |
| Z ordinary nonzero | intent positive→`+dvz`，negative→`-dvz`，none不写 | Unity用Up/Down+cooldown比较 | DIFFERENCE：双按不应写 |

下一包只统一公式owner和X/Y/Z矩阵；linked platform、physics integration、landing/bounce与revival另包。
