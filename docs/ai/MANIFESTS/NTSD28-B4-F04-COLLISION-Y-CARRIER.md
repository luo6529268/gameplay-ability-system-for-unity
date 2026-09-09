# NTSD 2.8-Logan collision-Y reference owner map

> Change ID：`NTSD28-B4-F04-COLLISION-Y-CARRIER-AUDIT-001`  
> 状态：`VERIFIED / CARRIER_BOUNDARY_DEFINED`

| 边界 | Authority | Unity当前 | 路由 |
|---|---|---|---|
| 默认/重置 | spawn default0；C11 rebuild每active slot清0 | 无字段 | carrier package |
| operation30 producer | strict previous/current交叉；只保留更低reference | 无kind30 platform producer | B5 |
| linked dvy producer | grounded linked platform motion后reference=integer Y | linked platform motion未实现 | B4后续 |
| defusion copy | partner复制primary reference | 未复制 | B6/B8 |
| physics consumer | friction plane/effective floor/landing predicates | 固定零地面 | B4 core |
| teleport consumer | state400/401 target/self reference写source Y | 当前按目标/自身Y路径 | B4 teleport correction |
| next999 consumer | type0且Y!=0且Y!=reference才212 | 当前缺reference | B4 frame body correction |
| input consumers | state19/301 grounded相等；state4 airborne小于 | 当前主要按Y==0/<0 | B2/B4 |
| hit consumers | unarmored/reduced above-reference predicates | 当前缺字段 | B5 |
| raw trace | signed int32 | missing/null | carrier package绑定 |

下一包只闭合carrier deterministic lifecycle，不接任何producer/behavior consumer。

