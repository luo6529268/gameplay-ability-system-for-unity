# NTSD28-B3-C25I-ARMOR-RECOVERY-OWNER-AUDIT-001 — C25i owner audit

<!-- CHANGE-RECORD
id: NTSD28-B3-C25I-ARMOR-RECOVERY-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan battle_world.cpp advance_armor_recovery_slot and battle_world_tests; EXE B1E13AE1, closure 39DDDA15.
evidence: GATES-CLOSED / TIMER-STATE-MACHINE-CLOSED / PROGRAMMATIC-SEAM-DEFINED / FORMAL-CONTENT-BLOCKED-B5-H-B11 / NO-CODE-WRITE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / CORE_SEAM_DEFINED`

## 事实

- timer<0 no-op；negative relation或ordinary nonzero hold冻结，type3例外。
- timer==0立即reload；timer>0只在runtime armor HP<=0时递减，到0同tickreload；armor HP>0时timer保持。
- reload从definition首个armor block读hp/recover；无block或hp0写timer=-1，positive recover原值，否则-1。
- Unity两runtime carrier已存在但无owner与armor schema；正式content/hit不能在本包启用。

## 路由

下一`NTSD28-B3-C25I-ARMOR-RECOVERY-001`只实现programmatic profile seam、core和h→i→j placement；正式profile parser/content与hit writer留B5+H/B11。
