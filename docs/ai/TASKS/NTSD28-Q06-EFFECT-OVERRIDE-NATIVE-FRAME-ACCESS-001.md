> 2026-09-21 VERIFIED / DECLARED_SCOPE_ONLY。联合30/30、SelfCheck、代表replay与Play22/Q05关闭PASS。准确范围/未覆盖项见artifacts/diagnostics/NTSD28-Q06-EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001/ACCEPTANCE.md。下文保留实施前计划，不代表当前尚未修改。

# NTSD28-Q06-EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。正式playable BattleWorld28::resolve_ordinary_unarmored_standard_hit→resolve_confirmed_unarmored_hit→apply_native_effect_action_override，抑制gate/action_type/pickedact previous-state/post-damage HP与attacker先target后顺序继承已VERIFIED NTSD28-B5-EFFECT-ACTION-OVERRIDE-001。旁边Kind0PostEffectAction及Type3PostHit是独立事务，不混改。

剩余准确访问边界：BattleDamageWriter.ResolveNativeEffectActionOverride两reader、ApplyNativeEffectActionOverride两binder；条件性HitPlan.ProjectNativeEffectActionOverride identity投影分支两reader。旧LF2FrameCache上限857排斥显式857..999，不是只缺implicit：latch900/state602或BDY50会漏抑制，previous900/state12+pickedact12会错误拒绝。审阅初稿声称已声明读取等价已纠正，不沿用该错误scope。隐式state0/null在gate中的等价仅适用未声明帧。

第一写域仅Tools/NTSD28AuthorityTrace/effect_override_native_frame_access_witness.cpp。主16代表：8正动作binder(双方错开低隐式/857/900/998/999声明缺失/1000/显式)，4reader控制(低/高隐式latch、positive picked配implicit拒绝/显式900state12匹配)，2高位抑制(900首BDY50、900state602)，2positive/death控制。kind0/effect8，普通type0双方；真实geometric candidate→resolve_ordinary_unarmored_standard_hit，不直接匿名helper。完整before/after/following，双方descriptor/历史latch/previous/snapshot区分、HP/pending/RNG/calls，记录真实result override/posteffect字段。不得修改权威源码/资源。身份转换source/Unity证据独立后继，普通16不能关闭HitPlan条件分支。

正式330字段审计effect8=801，其余8..16为0，catchingact/pickedact全0，caughtact多个正值及-3；仅静态token域不证明目标可达。报告remaining-reader/FORMAL330-EFFECT-OVERRIDE-FIELDS.json。延用用户代表验证策略，不做全乘积，不重测未变化B5全矩阵。

在源实测后准确新增Unityfixture及最小production符号，当前生产未改。验收compile、source independent/Unity RED、相关回归、闭包出口一次SelfCheck/代表replay/Play/Q05关闭；若发现其他hit事务差异另拆任务，不改expected掩盖。保护schema/Kernel/Scene/资源/非战斗/framework；回滚仅本ID精确diff并保留别的改动。Q06未完/Q07未迁移。
