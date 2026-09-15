<!-- CHANGE-RECORD
id: NTSD28-Q06-HELD-QUERY-NONHOLDER-RELATION-PRESERVATION-001
status: VERIFIED
change-kind: NONHOLDER_QUERY_RELATION_PRESERVATION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterWeaponLinkResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06HeldQueryRelationEditorTests.cs
authority: Formal source140 sameworld fulltick preserves negative child interaction_state and zero linked-child after terminal. BattleWorld28 slot relationship cleanup writeslinked_child_slot0. Native CPoint/held pass doesnot clear a negative entity relation merely to query whether it holds a weapon.
evidence: source140 afterbinding before0/immediate0/following90 perprofile;78 type0child errors plus2 perother type(parent child0->-1). Independent read-only trace: BattleInteractionPipeline.RunWeaponSyncHeldStep10 -> LF2Character override -> LF2CharacterWeaponLinkResolver.GetHeldEntity -> LinkState<=0 callsClearStaleHeldReference and mutatesformal fields.
-->

# Non-holder query relation preservation

IN_PROGRESS / TEST_FIRST. Independent dependency of HELD-NATIVE-FRAME-BINDING, not broader relation lifecycle redesign.

Exact change: GetHeldEntity split Runtime null/LinkState<=0 into cache-only null return. Preserve Runtime.LinkState/TargetSlotIndex/HolderStableId. Keep positive holder invalid-slot/reciprocal stale logic exactly asbefore; no new preinteraction skip or source rewrite. Managed HeldWeaponReferenceInternal may clear; runtime remains authoritative. Do not remove explicit release/unregister behavior.

Focused newfixture invokes GetHeldWeapon and actual RunWeaponSyncHeldStep10 onnegative/zero character, positivevalid andpositiveinvalid controls. Source140 fulltick bothprofiles mustzero; realPlay bothfactories/replay/SelfCheck/slotreuse regression andorderedshutdown required beforeVERIFIED. Oldslotreuse oracle mustnot change without separateevidence.

Risk: cache lookup formerly repaired nonholderrelationship accidentally, othercaller may have relied onwrite; test rolevariants andactualfulltick. No new field/schema/manager/queue; shutdown11stages untouched. Rollback only thisdiff withrequiredapproval. No Scene/resource/GAS/nonbattle/Server/computer-use/commit/push/delete.

精确RED job39545de730bd42cf8b0d5252a07e7b55：负关系和零关系两FAIL，正valid/invalid两PASS，XML已归档。现在仅拆GetHeldEntity非持有者分支，保留其余代码。

本Record单测试文件追加slotreuse聚焦：明确holder0/child50，真实Unregister后先断言Target0/HeldWeaponStableId-1，再注册same-slot无反向关系newborn，getter仍null/managedcache不继承且Target0保持。配合独立SelfCheck单oracle纠正；原140/4PASS不重做。


最终本轮状态：VERIFIED / DECLARED_SCOPE。源140两profile before/立即/完整tick均0差异；query含slotreuse5/5、旧fixture91/91；同World280场景560重放tick PASS；完整SelfCheck20:01:02Z PASS；真实Play560(140×两profile×两factory)全部PASS、Scene checksum保持/borrowers2→2；最终20:02:44Z关闭restore4→4/worldslots两pool0/两帧Stopped。Scene SHA BCD1047B…0E9FB6保持。三个独立子包只按声明查询或测试范围VERIFIED，初次读帧父包仍需后继投掷/drop/refill读帧/RNG相关证据，不标完整held/Q06。原失败及纠正均保留。
