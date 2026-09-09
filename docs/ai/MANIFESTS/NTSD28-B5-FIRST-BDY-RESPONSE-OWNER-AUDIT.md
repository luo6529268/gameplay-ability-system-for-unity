# NTSD28 B5 first-BDY response owner audit

## Data owner

- Authority只读目标**当前action帧的第一个BDY**；因此不得把响应数据加入candidate frozen-pair，也不得使用
  实际overlap body index。
- Unity `LF2FrameData.primaryBodyKindForEffectSuppression`已经保存同一个raw first-kind，但命名只描述旧用途。
  carrier包保留该序列化字段和现有effect-suppression caller以避免迁移风险，新增一般化只读入口，并新增
  `primaryBodyRespondForHitResponse`（缺失=0）。
- `Lf2DatConverter`在第一个`bdy`分支同时捕获kind/respond；duplicate property遵循现有subblock最后值覆盖。
- `BattleBodyBoxValue(X/Y/W/H)`继续只表示几何；legacy `BodyBox`及其adapter不扩字段，避免把first-body元数据
  错误复制到每个geometry value或破坏既有formal seam/fingerprint。

## Pure decision owner

- 新`BattleFirstBodyResponseResolver`只接收first kind/respond、raw injury、attacker group、可选roll；不读
  `LF2Entity`、frame cache、world或RNG。
- 第一阶段返回`None / ActionRange / EncodedNeedsRoll / EncodedChanceRejected / EncodedApplied`及所有写值。
  chance 1..99且尚无roll时只报告needs-roll；roll由调用者从同步流取得后再次纯resolve。
- 1xxx/2xxx strict bounds、1999→-1、respond投影、encoded十进制拆分、action<999、effect 0..7组合和
  manual damage raw injury均由此resolver唯一决定；4096+循环须0 B allocation。

## Production eligibility and order

shared `BattleHitCandidateSequenceRunner`是四种attacker shell的唯一production owner。仅当resolved runtime ITR
为kind0且disposition为Damage时，在任何`ApplyConsumeEffects`之前尝试响应：

- character target先复用`BattleOrdinaryCharacterDamageRouteResolver`；只有`UsesUnarmoredHit`进入。
  `ReducedDefense`与active `ReducedType1Armor`跳过；type1 bypass/resource-fallback/broken-fallback进入。
- non-character target若存在selected armor record，对齐Authority的更早feedback-only return，不进入响应；其余
  standard unarmored target可进入。
- chance 1..99从`world.NativeRandom.SynchronizedNext((uint)chance, 100)`消费一次；0或>=100不消费。
- chance失败只提交RNG cursor，随后继续原consume-effects与ordinary dispatch；成功先提交RNG（若有），再写动作/
  counter/group/hold/manual HP+统计，且不执行consume effects、普通writer、combo或OID300尾部。
- 成功返回whole-attacker termination给runner的candidate loop；外层下一个attacker继续。

## Actual writer owner

新增`BattleFirstBodyResponseWriter`，只执行已解析plan：

- 1xxx/2xxx target action用现有raw frame write并保留frame counter；encoded action写后显式counter=0；
- group写`Runtime.RelationTeam`；hold写attacker `FrameDelay=3`、target `FrameDelay=-3`；
- manual damage为`target HP=max(0,HP-raw injury)`，但`InputHpConsumedTotal34C += raw injury`且物理attacker
  `InputScoreTotal348 += raw injury`，不改HPBound、不走owner credit、不触发普通伤害/音频/资源/命中统计。

## HitPlan owner

- 不能把response藏在`BattleDamageWriter.ApplyStandardCharacterDamage`：现有HitPlan会误投影普通伤害，且runner
  无法获知abort。
- atomic包在HitPlan增加独立的`FirstBodyResponseAttempt` prepare/observe阶段，使用
  `NTSD28SynchronizedRandomCursor`从相同pre-attempt游标预演，不提交world RNG。
- attempt snapshot至少覆盖双方action/runtime frame/frame counter、group、FrameDelay、target HP、target
  HP-consumed、physical attacker score，以及同步counter/index/calls/last-callsite/table generation identity。
- chance失败也必须被attempt阶段验证；完成后普通writer observation从post-attempt状态开始，避免漏记同步RNG。
- success attempt标记该entry终止，HitPlan跳过同attacker余项但继续下一attacker；不得复用只识别OID300的
  `ProjectAbortAfterSuccessfulDispatch`。

## Package and acceptance matrix

1. carrier：parser default/last-wins、first-only、现有kind50/52 suppression不变、criminal正式22帧可读取。
2. pure：1000/1998/1999/2000/2998/2999；respond -1/0/positive；encoded chance 0/1/99/100；
   target/attacker action 998/999；effect 0..7；negative/large injury按Authority unchecked/HP clamp语义。
3. atomic：四shell共享路径、secondary BDY忽略、chance success/failure、同步callsite、type1三种fallback与两种
   reduced排除、non-character armor排除、manual stats、consume-effects suppression、per-attacker abort、
   next-attacker continuity、HitPlan ShadowCompare/DataOriented identity、0-allocation。
4. final：compile 0、focused tests、B5/NTSD28 broad、SelfCheck、Console、Scene dirty/root/hash不变，以及至少
   criminal 1xxx真实Play；encoded真实Play在当前Unity内容缺失时用明确synthetic fixture，不伪装成正式内容见证。
