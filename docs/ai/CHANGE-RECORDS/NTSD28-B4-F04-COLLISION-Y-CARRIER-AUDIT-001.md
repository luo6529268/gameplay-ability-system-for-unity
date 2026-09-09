# NTSD28-B4-F04-COLLISION-Y-CARRIER-AUDIT-001 — collision-Y carrier audit

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-COLLISION-Y-CARRIER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan EntityState28::collision_y_reference writers/readers in battle_world.cpp, physics_integrator.cpp and input_routing.cpp; EXE B1E13AE1, closure 39DDDA15.
evidence: WRITER-RESET-COPY-CONSUMER-MATRIX-CLOSED / UNITY-BINDING-MISSING-CONFIRMED / NEXT-CARRIER-ONLY / NO-CODE-WRITE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / CARRIER_BOUNDARY_DEFINED`

## 已观察事实

- Authority physics在C06消费上一轮C11 rebuild产生的reference；C11随后先清0再由operation30挑选最低
  reference，不能在physics内现算或用当前Y代替。
- state400/401 teleport把目标reference写入source integer Y；no-target写source自身reference。
- linked platform dvy更新precise/integer Y后同步更新reference；defusion复制reference。
- next999、state19/301 grounded input、state4 airborne input、fusion eligibility和hit response都读取该字段。
- Unity raw contract已经把`combat.collisionYReference`标为missing/null；项目内无等价生产字段。

## 裁决

先建独立signed int carrier和确定性闭包，再按阶段逐个接producer/consumer。`platformSourceSlot`与
`renderShadowOffset`仍是operation30原子writer的相邻缺口，但不塞入本次collision-Y carrier小包。

