# NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan EntityState28 owner_slot/object_ai_excluded_group_source_slot_2f8/object_ai_target_slot_3f8; NativeAi28 common and hit_Fa4/5/6/7 target paths; Unity PickerStableId/OwnerEntityIndex/OPointCreateTask.trackedTargetSlot and factories; EXE B1E13AE1, closure 39DDDA15.
evidence: three physical-slot fields proven independent; existing PickerStableId storage reclassified as +0x3F8 candidate with specialized OID124 positive control; generic common/4/7/11 and 5/6 child paths proven to misuse exact OwnerSlot +0x354; scan-miss stale-cache clearing and merged state14/render-phase gate differences frozen while raw inactive-slot behavior remains EXE-trace-gated; current 206 generic common frames, OID219 hit_Fa5-to-4 and nine hit_Fa3-to-7 chains prove reachability; carrier/producer/excluded-group consumer packages split; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / EXISTING_CARRIER_RECLASSIFIED / OWNER_354_CORRUPTION_CURRENT_REACHABLE / THREE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`

Authority的+0x354 owner、+0x2F8 excluded-group source与+0x3F8 target cache是三个独立slot字段。Unity
specialized OID124已把`PickerStableId`底层storage当+0x3F8使用，但generic hit_Fa却把target读写到
`OwnerSlotIndex`，导致206个current common frames、OID219 5→4与9条3→7链在保持表面追踪的同时污染归属。

后继复用现有int作canonical +0x3F8 carrier，再原子修common/child producers；既有+0x2F8 consumer包依赖其后。
完整closure、current matrix与验收见
`docs/ai/TASKS/NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001.md`。当前runtime栈未清，本轮无code/content/Scene。
