# Task Contract — NTSD28-B5-TYPE3-TARGET-CONTINUATION-PREREQUISITE-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / RUNTIME_CARRIERS_READY`
> 依赖：`NTSD28-B5-TYPE3-ATTACKER-POST-HIT-ACTION-001 / VERIFIED`

## 目标

核验 type3 target generic continuation 所需的 battle group、owner slot、native control/cycle slot、special-hit latch、
pending impulse total/count 与 hit-plan projection 是否已有精确 Unity carrier；只读拆分下一实现，不修改脚本、内容或 Scene。

## Authority

- `battle_world.cpp::resolve_relation_hit(...)` 的 `0x0042F72B..0x0042FC78`。
- `battle_world.h::EntityState28` 的 `control_slot_000`、`owner_slot`、`battle_group`、
  `special_hit_latch_0eb` 与 `pending_hit_impulse`。
- `hit_response.h::HitImpulseAccumulator28` 与 C22/finalizer 的 contribution-count 消费。

## Unity

- `NTSDEntityRuntime.RelationTeam / OwnerSlotIndex / AnimCounter / HitConfirm2`。
- `NTSDEntityRuntime.KnockbackVx/Vy/Vz / HitCount` 与 `FramePostProcessAll`。
- `BattleDamageWriter.ApplyKind0Type3Tail / CopyRelation / ResetType3HitMotion`。
- `BattleEcsHitExecutionPlan.WriterEffectSnapshot`、capture/project/compare/commit。

## 验收与边界

- 依据已验证 B0 binding 判断 carrier，不凭字段名猜测。
- 明确 runtime carrier、hit-plan shadow carrier与生产 writer三个层次。
- kind catalog transform 保持独立；本审计不得把旧 OID 特判晋升为通用表实现。
- 不修改 DAT/资源、Scene、Prefab、ProjectSettings 或 authority。

## 结果

- runtime carrier 已齐备：battle group=`RelationTeam`，owner slot=`OwnerSlotIndex`，native
  `control_slot_000`=`AnimCounter`（共享 cycle/关联槽，不是玩家输入槽），latch=`HitConfirm2`。
- pending impulse total=`KnockbackVx/Vy/Vz`，contribution count=`HitCount`；C22 两端都在 count>0 时按
  `(count+1)` 归一写 Runtime velocity，再清 count/total。当前 `ResetType3HitMotion` 保留 HitCount 是正确的，
  但额外清 Runtime V 与 AttackingCounter 不正确。
- 当前 generic relation 只写 team/HolderCopySlot；应改为 team/OwnerSlotIndex/AnimCounter，且不能把
  HolderCopySlot 当 owner slot。
- hit-plan 已有 TargetRelationTeam/TargetAnimCounter/TargetHitConfirm2/Knockback/HitCount，但缺
  `TargetOwnerSlot` shadow field；这是投影缺口，不需要新增 runtime carrier。
- 下一包直接实施 `NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001`：state3005-only skip、active parent source、
  ownership transaction、只清 pending total、hit_Fj/hit_Uj 与 30/20 fallback、actual/hit-plan。kind transform独立后置。

## 回滚

删除本审计新增文档行即可；没有生产脚本、内容或 Scene 修改。

