# Task Contract — NTSD28-B5-EFFECT-ELIGIBILITY-POSTACTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`
> 依赖：`NTSD28-B5-LEGACY-KIND16-PRODUCTION-RETIREMENT-001 / VERIFIED`

## 目标

逐项闭合NTSD 2.8-Logan kind0 effect的candidate eligibility、8..16 action override、2/3/20/21/22/30
post-effect action、type3 post-hit action及死亡/防御抑制条件，并映射Unity actual/shared/hit-plan生产所有者；本包只审计和拆包，
不修改战斗脚本。

## Authority 合同入口

- `battle_world.cpp::candidate_passes_native_direct_effect_filter(...)`
- `battle_world.cpp::native_effect_action_override_is_suppressed(...)`
- `battle_world.cpp::native_effect_action_target_type_matches(...)`
- `battle_world.cpp::apply_native_effect_action_override(...)`
- `battle_world.cpp::apply_native_kind0_post_effect_action(...)`
- `battle_world.cpp::apply_native_type3_post_hit_action(...)`
- `battle_world.cpp::resolve_relation_hit(...)`中effect/action tail的真实调用顺序

## Unity 审计入口

- `Animation/Character/BruteForceSceneQuery.cs`
- `Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
- `Animation/LF2Objects/LF2CharacterHitResolver.cs`
- `Animation/LF2Objects/LF2CharacterDatHitResolver.cs`
- `Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`

## 验收

- 每个effect分支记录前置字段、对象类型、previous/current/action-latch语义、写入顺序及可达性。
- 区分candidate rejection、armor selection、damage、action override、post-effect、audio/spark，不混包。
- 标出Unity已对齐、缺失、旧扩展或错误顺序，并为可独立实施项给出后续Change边界。
- Direction B下不修改DAT/PNG/WAV/Prefab/Scene/importer，不把armor/content缺口伪装为默认值。

## 回滚

删除本审计新增文档行即可；本包不修改生产脚本或内容。

## 结果

- direct candidate effect2/4/20/21/30 gate已对齐。
- effect8..16 action override缺失且effect8在Authority runtime corpus有808条，确认可达差异。
- unarmored type0 direct post-effect action缺失；resolver内旧`HitPostEffect`无生产调用者且语义不等价。
- type3 continuation已有部分实现，需独立核验；reduced armor、audio/spark和effect6500分别路由。
- 旁支发现type0 hit-plan仍用raw injury，与已对齐production scale不一致；先建独立修正包。
- 完整矩阵见`docs/ai/MANIFESTS/NTSD28-B5-EFFECT-ELIGIBILITY-POSTACTION.md`。
