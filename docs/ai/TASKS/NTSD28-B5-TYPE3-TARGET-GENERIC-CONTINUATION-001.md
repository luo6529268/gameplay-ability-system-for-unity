# Task Contract — NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001

> 状态：`VERIFIED / TYPE3_TARGET_GENERIC_CONTINUATION_ALIGNED / HOLDER_COPY_CORRECTION_VERIFIED / WHOLE_CONTINUATION_RESTORED`
> 依赖：`NTSD28-B5-TYPE3-TARGET-CONTINUATION-PREREQUISITE-AUDIT-001 / VERIFIED`

## 目标

对非 kind-transform candidate 的 type3 target 实现正式 generic continuation：仅 state3005 跳过；从 attacker
或 active negative-link parent 复制 battle group/owner/control source slot；设置 special-hit latch；只清 pending
impulse total、保留 contribution count；按 attacker type/link/effect 读取 target 当前帧 hit_Fj/hit_Uj，零值回退30/20。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3TargetGenericContinuationEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表与 type3 manifest

## 不变量

- kind.dat bound/respond candidate 与 definition/object identity transform 不在本包改写；现有 legacy route 暂时保留，
  下一独立 kind-catalog Change 取代。
- ownership：RelationTeam←source.RelationTeam；OwnerSlotIndex←source.OwnerSlotIndex；
  AnimCounter←source physical SlotIndex。HolderCopySlot 不作为 owner。
- target current response frame 必须在动作写入前读取；state3005 或 frame null 不写 generic transaction。
- pending total 只清 Knockback XYZ；HitCount、Runtime XYZ、AttackingCounter 在 ownership/impulse 阶段保留，
  随后 action write只将 AttackingCounter清0。
- 不修改 effect8..16 override、type0 direct post-effect、pair/hold tail、kind transform、DAT/资源或 Scene。

## 验收与回滚

test-first覆盖Fj/Uj及fallback、direct/negative-parent ownership、state3005 skip、state3006可达、pending total/count
与runtime velocity preservation、non-kind candidate gate及hit-plan TargetOwnerSlot projection。之后 fresh compile、focused、
B5+hit-plan、exact broad、SelfCheck、filtered compile errors、Scene hash与Ledger。

回滚移除 generic helper、非-kind actual分流、TargetOwnerSlot shadow projection及新测试；保留前一攻击者包。

## 最终证据

- test-first：`a9c51fe81f1f42db97855f208b251133`，11项中7项按预期失败、4项基线通过。
- fresh compile：runtime 18:27:56Z、Editor 18:23:23Z；filtered `error CS`=0。
- focused：`35a8c525a12e4526829de33274fb2c32` 11/11。
- HitPlan：`fa3927cd838344d9ab414a2338e45e31` 182/182；B5+HitPlan：
  `49e59345777646cfbe5c70598cf3330c` 359/359。
- exact 100-class broad：`30500081d6a3436b88dfd18481b6cf60` 735/735；SelfCheck 18:29:12Z PASS。
- Console中的7条error-type均为既有预期拒绝/回滚测试日志；Ledger268/233 PASS；Scene并发基线
  `D4266C6D...583B` unchanged。
- 未关闭：locked kind catalog/definition identity transform；下一独立审计
  `NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-AUDIT-001`。

## 后继额外字段纠正（2026-09-08）

`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`确认：本包已验证的
`RelationTeam/OwnerSlotIndex/AnimCounter`、pending impulse、action和motion保留；但
`BattleDamageWriter.CopyRelation()`与HitPlan仍额外复制旧`HolderCopySlot`，Authority type3
transaction没有该写入。因此本包只能证明core exact子集，不再证明whole continuation
完全对齐。后继由`NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001`删除
actual/HitPlan extra write并重跑原矩阵。
