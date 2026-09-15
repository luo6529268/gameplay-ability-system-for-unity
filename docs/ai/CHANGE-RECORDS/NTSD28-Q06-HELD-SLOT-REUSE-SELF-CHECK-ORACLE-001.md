<!-- CHANGE-RECORD
id: NTSD28-Q06-HELD-SLOT-REUSE-SELF-CHECK-ORACLE-001
status: VERIFIED
change-kind: HELD_SLOT_REUSE_ORACLE_ONLY
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Formal BattleWorld28 cleanup_slot_references writes linked_child_slot0 andinteraction_state0; Unity BattleEntityLinkLifecycleWriter already same. Negative/zero query now correctly preserves formal fields.
evidence: Fresh SelfCheck19:56:28Z fails CheckHeldReferenceSlotReuseContracts; its joint assertion expectsTargetSlotIndex-1 but formal cleanup andcurrentUnitywriter set0. Native source140 terminal immediate/following also expects0. Managedcache/nullgetter/HeldWeaponStableId-1 requirements remain.
-->

# Held slot reuse self-check oracle

IN_PROGRESS / TEST_ONLY. Only CheckHeldReferenceSlotReuseContracts same-slot assertion TargetSlotIndex-1 becomes0. Preserve allsame-slot/different-slot/reverse-link/cache/TrackerParent assertions. No productionchanges. Priorfailure copiedtoartifact. Parentnonholder-query fixture adds directslot-reuse test independently to confirmbefore/after getter preservesnativezero andrejectsnewborn.

Acceptance: directslotreusefocused plusfullSelfCheck, compileCS0, ledger anddiff. No newruntimeowner/schema/shutdownstage orScene/resource/nonbattle/GAS/Server changes. Rollbackonlyoneassertdiffwithapproval, preserveotherSelfCheckchanges.


最终本轮状态：VERIFIED / DECLARED_SCOPE。源140两profile before/立即/完整tick均0差异；query含slotreuse5/5、旧fixture91/91；同World280场景560重放tick PASS；完整SelfCheck20:01:02Z PASS；真实Play560(140×两profile×两factory)全部PASS、Scene checksum保持/borrowers2→2；最终20:02:44Z关闭restore4→4/worldslots两pool0/两帧Stopped。Scene SHA BCD1047B…0E9FB6保持。三个独立子包只按声明查询或测试范围VERIFIED，初次读帧父包仍需后继投掷/drop/refill读帧/RNG相关证据，不标完整held/Q06。原失败及纠正均保留。
