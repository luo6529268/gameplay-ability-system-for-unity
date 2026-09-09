# NTSD 2.8-Logan B4 F-04 physics owner matrix

> Change ID：`NTSD28-B4-F04-PHYSICS-OWNER-AUDIT-001`  
> 状态：`VERIFIED / PHYSICS_SPLIT_DEFINED`

| 片段 | Authority | Unity 当前 | 裁决 / 后续包 |
|---|---|---|---|
| hold / relation gate | signed motion hold先向0，再以negative interaction return | `FrameDelay`先向0；`LinkState<0`与额外cpoint gate | 主体同形；state10/cpoint精确边界另包 |
| X/Z directional block | 只阻止对应方向位移，随后消费四flag | `StepBattleLogic`/`WeaponDynamics`同形 | 公式匹配，统一owner仍待 |
| type4/120 与 101 X extra | 两个独立20% operation，blocked也执行 | shared path独立；derived weapon为`if/else if` | confirmed difference，identity包 |
| type3 hit_j depth | `hit_j>0`加`hit_j-50` | 两条路径均有 | 主体匹配，统一owner待 |
| friction plane | pre-step integer Y >= collision reference后，X/Z各向0一步 | Unity只以`YInt>=0` | missing carrier，floor/core包 |
| effective floor | negative collision reference覆盖flat floor；否则0 | 主要固定0 | missing carrier，floor/core包 |
| airborne gravity | type3=0；type4=.85；type6=1.1333；state1002 identity table；ordinary=1.7 | 常量和主要分流已存在 | identity alias与统一owner待 |
| type0 air/landing | post-gravity state12/18选择；ordinary strict crossing后94/215/hit_g/219 | 分散在character/shared landing resolver | 独立type0包；environment damage联动 |
| type1 landing | state1002只有motionY大于`5.2571022450498032e120`才action7 | `9.9`后action7 | 首个独立差异；下一小包 |
| type2 landing | strict penetration；>9反弹，否则action20 | 零地面crossed + >9分支 | carrier后闭合 |
| type4/6 landing | strict penetration/downward；8.5/10阈值，0.7 bounce | 主体同形，零地面假设 | carrier后闭合 |
| type3/OID999 | 特殊state clamp；OID999仅contactY<-9发射9/9、action101 | crossed zero floor后清零motion、action101 | confirmed difference，依赖carrier |
| integer sync | 每次有效step尾部trunc toward zero并保存previous integer | `SyncIntegerPosition`存在，多owner调用 | previous语义与单一owner另包 |

本表只定义实施拆分，不宣称 F-04 或任何 object type 已整体对齐。

