# Task Contract — NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001

> 状态：`VERIFIED / RED_1_PASS_7_FAIL_OF_8 / FOCUSED_16_OF_16 / PREINTERACTION_15_OF_15 / B6_CATEGORY_57_OF_57 / HITPLAN_185_OF_185 / NTSD28_183_OF_183 / TARGETED_PLAY_16_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED`
> 依赖：
> - `NTSD28-B6-CATCH-ADVANCE-SLOT-ORDER-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001 / VERIFIED`
> - `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001 / VERIFIED`

## 目标

把 Unity pre-interaction 中分离的全体kind1、全体kind2两个sweep改为Authority的单一slot升序mixed
advance，并把advance/settlement关系判断从compat `CatcherSlotIndex`迁到exact `CatchSourceSlot90`；同时
关闭reciprocal mismatch与negative-decrease release之后错误继续throw/dircontrol的控制流。

## Authority 合同

- `BattleWorld28::advance_catch_relations()`单个slot升序loop：snapshot kind1且motion-hold非负时处理catcher，
  否则仅在current kind2时处理orphan validation；两分支对同一slot二选一。
- 低槽kind2在高槽catcher改变action/throw前验证；高槽kind2在低槽catcher之后验证。settlement是advance
  完整结束后的第二个独立升序pass，不得合并进slot body。
- 三个关系consumer都读plain exact `CatchSourceSlot90`；`0x2000+slot` attribution tag不得解码为catch source。
- active kind1 reciprocal mismatch只写catcher action0并立即结束slot。
- negative decrease使timeout<0时写catcher action0、caught action181、双方`AttackingCounter=1`、caught
  Vx按相对X为-4/+4、Vy=-3，保留relation/negative timeout并立即结束slot。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Passes/Interaction/BattleInteractionPipeline.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- 新增本包 focused Editor/Play 测试及 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/PreInteractionNoOpProofEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointThrowAtomicProductionEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本 Change 治理文档与恢复入口。

## 不变量

- 保持settlement为advance后的完整独立pass；不实施后续vaction frame/kind2 preflight与held accounting。
- 不改变kind1 action selection、正常throw/resource/environment/Vz、正常dircontrol或caughtact combo。
- compat relation继续由producer/lifecycle同步，但不得裁决本包三个正式consumer。
- no-op/participant-filter必须按两个正式pass重新证明；derived virtual side effect仍通过既有kind1/kind2 hook中
  被mixed入口选择的一个及settlement hook观察，不能恢复三sweep。
- 不改content、Scene、Prefab、importer、Authority、RNG或shutdown顺序；warmed 4096迭代0 allocation。

## Test-first 验收

1. RED覆盖低caught/高catcher与低catcher/高caught的同tick极性；旧两sweep必须分别错误。
2. exact/compat故意冲突：kind1 reciprocal、kind2 orphan、settlement均只服从exact；encoded source不得解码。
3. mismatch + throw/dircontrol与negative-release + throw/dircontrol必须证明terminal；HitCount sentinel保持、
   AttackingCounter写1、relation/timeout保持、RNG0变化。
4. positive/zero decrease、input-selected kind1、normal throw/dircontrol、slot0/high、lifecycle reuse与kind1/2
   no-op/derived hooks回归。
5. compile、focused、B6/NTSD28/PreInteraction回归、SelfCheck、targeted Play、Console、Scene、Ledger按真实
   证据推进；无current mixed-order content witness时不得冒充真实玩家路径。

## 回滚

只恢复本包mixed traversal、exact consumer与terminal fence/diagnostic fixture；不得回退relation producer、
lifecycle cleanup、throw精确子集或用户工作树。
