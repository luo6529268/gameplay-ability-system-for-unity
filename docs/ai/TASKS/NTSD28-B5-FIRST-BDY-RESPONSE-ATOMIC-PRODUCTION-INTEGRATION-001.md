# Task Contract — NTSD28-B5-FIRST-BDY-RESPONSE-ATOMIC-PRODUCTION-INTEGRATION-001

> 状态：`VERIFIED / FIRST_BDY_RESPONSE_PRODUCTION_ALIGNED / FORMAL_CRIMINAL_PLAY_PASS`
> 依赖：carrier与pure core均`VERIFIED`

## 目标

把first-current-BDY响应原子接入四种attacker shell共享的candidate runner：只在kind0 Authority
unarmored continuation、且任何consume-effects/普通damage前执行；精确提交同步RNG、action/counter/group/hold/
manual damage，并在成功时只终止当前attacker余下candidate。HitPlan必须以独立attempt阶段投影/比较同一事务。

2026-09-07正式criminal Play见证准备时更正：C++ `resolve_confirmed_unarmored_hit`在first-BDY响应之后才进入
非角色普通尾；Unity的OID300提前分类是适配层语义，不能阻断响应。因此Unity eligible disposition必须覆盖
`Damage`与`Oid300Redirect`，仍排除其他kind/disposition。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleFirstBodyResponseWriter.cs`及`.meta`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5FirstBodyResponseAtomicProductionIntegrationEditorTests.cs`及`.meta`
- `Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs`（只扩展formal criminal
  first-BDY矩阵与结果证据）
- 必要时同一Change内的`BattleRuntimeSelfCheck.cs`
- 本Change的Task/Record/Ledger/STATE/handoff/总表

## 不变量

- shared runner仍是character/DAT/weapon/special四shell唯一消费入口；不得复制四套分支。
- 仅resolved runtime ITR kind0 + unarmored attack disposition可尝试：`Damage`或Unity提前分类的
  `Oid300Redirect`；character必须复用ordinary route且仅UsesUnarmoredHit进入。
- ReducedDefense与active ReducedType1Armor排除；type1 bypass/resource/broken fallback进入。
- non-character只有selected first type0 armor record时排除；无selected armor仍可响应。
- 读取target当前action帧的first kind/respond，不读overlap body。
- chance 1..99只调用一次`NativeRandom.SynchronizedNext((uint)chance,100)`；失败保留RNG并继续普通路径。
- 成功按pure plan写入，跳过consume-effects/普通writer/combo/OID300尾，终止当前attacker余项但不阻止下一attacker。
- 1xxx/2xxx raw action不重置FrameWaitCounter；encoded action `<999`时显式重置。
- manual damage只写target HP clamp0、target input HP consumed、physical attacker input score；不写HPBound、
  owner credit、普通统计、声音或资源副作用。
- HitPlan独立attempt snapshot覆盖动作/runtime frame/counter/group/delay/HP/统计及同步RNG identity/state；
  ShadowCompare与DataOriented不得改变结果，warm path不得分配。
- 不改Scene、Prefab、Config、资源、ProjectSettings、Authority或既有Slot容量模型。

## 风险与验证

- 风险：pre-consume顺序错误会误做zero-HP/heavy-release；chance失败漏提交RNG会造成后续全局漂移；
  early return若未通知HitPlan会把同attacker余项误报missing；raw/encoded frame counter语义不同。
- TEST-FIRST focused覆盖上述边界、四shell、type1 route、selected nonchar armor、成功/失败RNG callsite、
  manual stats、consume suppression、per-attacker abort/next-attacker、ShadowCompare/DataOriented及零分配。
- 再跑编译、B5/NTSD28 broad、SelfCheck、Console、Scene hash/dirty/root与Ledger validator；正式criminal
  1xxx需真实Play，encoded可使用明确synthetic fixture但不得冒充正式内容见证。

## 回滚

删除new writer/test；撤销runner调用、HitPlan attempt阶段、World桥接和NativeRandom generation scalar carrier。
carrier/pure core保留，不触及内容与场景。
