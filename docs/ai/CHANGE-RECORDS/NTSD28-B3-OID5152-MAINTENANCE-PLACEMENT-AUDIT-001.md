# NTSD28-B3-OID5152-MAINTENANCE-PLACEMENT-AUDIT-001 — OID51/52 maintenance owner审计

<!-- CHANGE-RECORD
id: NTSD28-B3-OID5152-MAINTENANCE-PLACEMENT-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: Promoted NTSD 2.8-Logan C12 advance_native_fusions and C25h advance_reaction_timers_slot, EXE B1E13AE1, playable closure 39DDDA15.
evidence: READ-ONLY / AUTHORITY-C12-FUSION-PREDECREMENT-TIMER-READ / AUTHORITY-C25H-TIMER-DECREMENT / UNITY-COMBINED-EARLY-MAINTENANCE / TIMER1-SAME-TICK-SPLIT-DIFFERENCE / NEEDCLEAR-MAINTENANCE-DIFFERENCE / DIRECT-COMPAT-ENTRY-MUST-REMAIN / NEXT-NTSD28-B3-OID5152-PRODUCTION-SPLIT-001 / NO-PRODUCTION-CHANGE / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C12-C25H-OWNERS-CLOSED / NEXT-PRODUCTION-SPLIT`

## 证据

- Authority `simulation_tick_driver.cpp:700-714`在candidate后调用fusion；`battle_world.cpp:2697-2818`
  merge/split判断直接读取尚未递减的`input_special_timer_338`。
- Authority `simulation_tick_driver.cpp:1025`调用C25h reaction timers；`battle_world.cpp:3722`附近负责
  `+0x338`正值递减，位于同slot frame后。
- Unity `BattleOid5152RuntimeModule.RunMaintenance`在slot0..19内先`Unk338--`再`TrySplit/TryMerge`，且当前
  production位于C03之后和NeedClear return之前。

## 裁决

combined入口不能整体搬到C12，也不能整体搬到tail。下一包新增production-only fusion scan与per-slot tail timer
writer；旧`Oid5152RuntimeMaintenanceAll`行为保持给直接兼容测试，不再作为production owner。
