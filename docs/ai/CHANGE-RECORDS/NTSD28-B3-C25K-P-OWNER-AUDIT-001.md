# NTSD28-B3-C25K-P-OWNER-AUDIT-001 — C25k-p owner audit

<!-- CHANGE-RECORD
id: NTSD28-B3-C25K-P-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan simulation_tick_driver.cpp C25k-p live-slot tail and battle_world.cpp state18/weapon-piece/lifecycle/healing writers; EXE B1E13AE1, closure 39DDDA15.
evidence: ORDER-CLOSED / UNITY-OWNER-CROSSWALK-CLOSED / IMPLEMENTATION-SPLIT-DEFINED / NEXT-C25P / NO-CODE-WRITE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`

## 已观察事实

- Authority在同一slot内依次执行k→l→m→n→o→p；o的pending非零即消费该slot并跳过p。
- Unity当前顺序为frame tick→early frame exit→death opoint→OPoint→cleanup→mixed tail→Prev mirror，healing另在random-drop后的global scan。
- Unity OPoint已有AttackingCounter==0 gate；state18 branch已有基本7/1数量和四次随机tuple；Frame.Prev已有B0字段绑定。
- weapon pieces、正式terminal pending carrier和逐槽lifecycle消费仍不完整。

## 裁决

先迁C25p，因为它只依赖现有type0/HP/timer/frame-state载体；k～o按OPoint/history、state18 particles、weapon pieces/lifecycle分包，不能一次性重排。

## 边界

无代码、资源、Scene、Authority写入。完整矩阵见`docs/ai/MANIFESTS/NTSD28-B3-C25K-P-OWNER-AUDIT.md`。
