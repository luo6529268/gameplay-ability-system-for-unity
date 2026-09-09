# NTSD28 B5 Effect Eligibility / Post-Action Crosswalk

> Change：`NTSD28-B5-EFFECT-ELIGIBILITY-POSTACTION-AUDIT-001`
> 结论：`AUDIT_VERIFIED / IMPLEMENTATION_SPLIT_DEFINED`

## 1. Direct candidate effect filter

| effect | NTSD 2.8-Logan candidate gate | Unity现状 | 结论 |
|---|---|---|---|
| 2 | attacker previous state=19且target previous state=18时拒绝 | `BruteForceSceneQuery.Kind0EffectAllowed`使用双方`Frame.Prev` state执行同一条件 | 已对齐 |
| 4 | target object type=0时拒绝 | 同入口按当前data object type拒绝Character | 已对齐 |
| 20 | 仅type0；target previous state不能为18/19 | 同入口按Character及`Frame.Prev` state过滤 | 已对齐 |
| 21 | target previous state不能为18/19 | 同入口按`Frame.Prev` state过滤 | 已对齐 |
| 30 | target candidate action不能在200..202 | 同入口按当前`Frame.N`过滤 | 已对齐 |

`BattleHitCandidateSequenceRunner`中runtime replacement后的effect21 current-state whole-attacker abort是已验证的
`R4-COL-003`消费门，不得与上述previous-state candidate filter合并或删除。

## 2. Effect 8..16 action override

Authority顺序位于unarmored ordinary reaction/rest/projectile/type3 continuation之后、direct post-effect之前：

1. target action-latch首个BDY kind 50/52、latched state 602/603、definition property 2/3会抑制整个override；
2. effect 8..16且caughtact为-2/-3也抑制；
3. target-type矩阵：8/13→type0；9/14→type3；10/15→type0或3；11/16→type1/2/4/6；12→任意；
4. positive `pickedact`要求target previous-action frame state相等；
5. positive `catchingact`写attacker action但不直接清frame counter；
6. positive `caughtact`只在target HP仍为正时写target action，不能覆盖致死反应；
7. 随后的direct post-effect action可再次覆盖target action。

Unity已解析并复制这些ITR字段，但actual/shared character、weapon/object dispatch与hit-plan没有上述普通kind0
action-override consumer。权威runtime decoded DAT中effect8有808条，常见`caughtact:615/232/397`，属于
`CONFIRMED_REACHABLE_DIFFERENCE`；effect9..16在当前runtime corpus计数为0，但仍必须由同一矩阵实现，
不能只为effect8写角色特例。

后续包：`NTSD28-B5-EFFECT-ACTION-OVERRIDE-001`。

## 3. Direct kind0 post-effect action

仅unarmored type0 continuation执行；selected-armor/guarded reduced branch明确跳过：

| effect | Authority action | gate |
|---|---|---|
| 3 / 30 | 200，frame counter=0 | target previous-action state !=13 |
| 2 / 21 / 22 | 203，frame counter=0，facing按最终pending X impulse：负→0，零/正→1 | 无额外gate（candidate filter已先执行） |
| 20 | 同203 | target previous-action state !=18 |

Unity `BattleDamageWriter.ApplyStandardCharacterDamage`在rest/caught/state1002之后直接返回，没有此tail；
hit-plan `ProjectStandardCharacterDamageWriterEffect`也没有对应projection。两个resolver内的私有`HitPostEffect`
没有生产调用者，而且使用current state、武器掉落、effect23等旧语义，不能提升为实现。结论为
`CONFIRMED_DIFFERENCE`。

后续包：`NTSD28-B5-KIND0-POST-EFFECT-ACTION-001`；必须在effect action override之后调用，并同时覆盖
actual/shared/hit-plan。Audio与spark事件仍归B10/B9，不能在该包顺手补齐。

## 4. Type3 target continuation

Authority在effect action override之前完成type3 ownership/kind-catalog transform及`hit_Fj`/`hit_Uj` action：

- state3005 target跳过；
- kind catalog可转移definition/object identity并选择record frame（0回退40）；
- generic路径根据attacker type/link及effect2/20选择`hit_Fj`或`hit_Uj`，0回退30/20；
- 之后才进入effect8..16 override及type0 direct post-effect。

Unity已有`ApplyKind0Type3Tail`与对应hit-plan投影，但它包含较多identity/holder/资源前置，不能在本包
宣称完全一致。后续另建`NTSD28-B5-TYPE3-POST-HIT-ACTION-AUDIT-001`做exact/shared/real type3核验。

## 5. Reduced/armor、audio/spark与旧范围

- selected-armor/guarded branch不执行unarmored input status/join，也不执行effect action override/direct
  post-effect；Unity alternate路径的整体armor选择、数值和声音仍受B5+B11/H约束。
- effect23在Authority这里不写action；其声音与spark属于后续公共tail，归B10/B9。
- Unity旧`effect 5000..5999`扣PP、`6000..6999`跳帧代码不能从当前portable battle live path获得同语义证明。
  Authority runtime corpus发现1条effect6500，因此不能简单按“未使用”删除；先做正式EXE/源码闭包专项确认，
  状态保持`UNKNOWN`。

## 6. 审计中发现的旁支首差

`BattleEcsHitExecutionPlan.ProjectStandardCharacterDamageWriterEffect`仍以raw `resolvedItr.injury`投影type0
HP/HPBound/combo/stat，而生产writer已经使用`target.IncomingDamageScale340 -> attacker.WeakTimer12C/2`。
这会使ShadowCompare/DataOriented在非100 scale或weak条件下产生首差。它不属于effect action，实现前先用
`NTSD28-B5-TYPE0-HITPLAN-DAMAGE-SCALE-CORRECTION-001`独立修正。
