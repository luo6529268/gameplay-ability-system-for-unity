# Task Contract — NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001

> 状态：`VERIFIED / RED_0_OF_3 / FOCUSED_4_OF_4 / RELATED_284_OF_284 / TARGETED_PLAY_3_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / FOUR_TYPE3_WRITERS_RETIRED / TYPE3_SPECIFIC_FAMILY_EXIT_READY / ORDINARY_CREDIT_GATE_2F4_NEXT`
> 依赖：`NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001 / VERIFIED`、
> `NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001 / VERIFIED`。

## 目标

退休 type3 damage transaction 中不存在于 Authority 的 legacy `HolderCopySlot`额外写入：actual kind9 copier与
HitPlan kind9/D1/active-D1 projection均必须保留target原值。已验证的RelationTeam、OwnerSlotIndex、AnimCounter、
action、special-hit latch、pending impulse、motion、sound、rest、identity transform与pair tail必须完全不变。

## Authority 与现状

- Authority type3 kind9/generic/kind-transform transaction分别写各自明确字段，但没有root HolderCopy carrier，
  不复制source `linked_parent_slot`、owner root或任何平行slot到target。
- Unity `BattleDamageWriter.CopyRelation()`仍在kind9非-flying分支执行
  `target.HolderCopySlot = source.HolderCopySlot`。
- HitPlan还在`ProjectType3Kind9DamageWriterEffect()`、legacy D1 identity projection与active-D1 projection中
  写`TargetHolderCopySlot`；后两者即使被当前locked-kind/generic优先路径遮蔽，也仍是可错误复活的shadow writer，
  必须与actual退休保持结构闭合。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3LegacyHolderCopyWriterRetirementEditorTests.cs`及`.meta`
- 仅为preserve-sentinel更正的
  `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`、
  `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本Task/Change与治理恢复文档。

## 不变量与排除

- actual只删`CopyRelation`中的HolderCopy write；RelationTeam写入保留，方法名可保持以避免扩大调用重构。
- HitPlan只删三处`TargetHolderCopySlot` projection；pickup、CPoint、held、OPoint、stats等其他HolderCopy写入不动。
- 不改type3 gate/branch顺序、active holder解析、group/owner/control、definition、frame、motion、RNG、sound、damage、
  hit record、rest或effect tail。
- 不删HolderCopy carrier/reset/copy/ECS/snapshot/checksum/parity；schema仍需用户方向。
- 不改content、Scene、Prefab、ProjectSettings、Authority目录或用户例外。

## Test-first 验收

1. RED以attacker/root HolderCopy=77、target HolderCopy=99覆盖actual+HitPlan kind9；Standing branch必须暴露99→77，
   flying分支为preserve基线。
2. source-closure guard锁定BattleDamageWriter与三个HitPlan extra assignment均归零，同时确认pickup等非type3
   writer仍存在。
3. focused、完整type3/HitPlan/B5相关回归通过；target HolderCopy sentinel不变，而group/owner/control/action/
   motion及diagnostic difference mask保持原绿灯。
4. 两套build、full SelfCheck与真实`NTSD_Battle` Play实际运行；Console/Scene不变。
5. 通过后只恢复type3-specific whole continuation exit；B5 `+0x2F4`、legacy stats及carrier/schema仍后置。

## 回滚

恢复actual一处与HitPlan三处legacy assignment，并回退本包fixture；不得回退linked-parent correction或其他type3核心。
