# Task Contract — NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001

> 状态：`FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_7_OF_7 / SELFCHECK_BLOCKED_BEFORE_PRESENTATION_ASSERT_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_4 / LOGIC_AND_PRESENTATION_PRODUCERS_WRITTEN / FRAGMENT_AND_LEGACY_BRANCHES_EXCLUDED`

## 目标

在 Unity world-owned late frame OPoint production 链中，于 child 第一次注册前把 parent 的 literal
`OwnerEntityIndex`写入生成task，使child owner严格等于parent owner，而不是parent physical slot、holder、
stable id或target。

## Authority 与 Unity 原状

- 当前正式 EXE SHA-256：
  `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；playable closure
  manifest：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- playable build closure中的`object_spawning.cpp::ObjectSpawnPlanner28::plan_frame()`对每个有效frame
  OPoint执行`intent.owner_slot = parent.owner_slot`；
  `battle_world.cpp::spawn_from_opoint_intents()`再原样写`request.owner_slot = intent.owner_slot`并spawn。
- Unity生产链为`BattleLateEntityLifecycleModule -> BattleStructuralWriter.ProcessLateOpointSegment()`，随后按
  world materialization mode进入`BattleLogicObjectPointRuntime.ProcessOneLateOpoint()`或Unity presentation
  adapter的`LF2ObjectPointFactory.ProcessOneLateOpoint()`；两个显式producer的task均已携带parent、team、
  position、semantic、required slot，但都没有写`ownerEntityIndex`，因此ordinary child仍为`-1`。
- `LF2ObjectPointModule.ProcessFrame()`在当前项目源码中没有生产调用者；本包不以dormant备用入口替代或
  扩大world-owned正式链，也不在factory按`parent!=null`全局推断owner。

## 修改范围

- `Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicObjectPointRuntime.cs`
  - 只在`ProcessOneLateOpoint()`构造ordinary task时写
    `task.ownerEntityIndex = spawner.OwnerEntityIndex`，位置在structural spawn前。
- `Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs`
  - 对同一world-owned structural segment的Unity presentation materializer做同构显式写入；不改通用
    `CreateObjectImmediate`/`PostInitLiving`或任何parent推断。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B0OpointOwnerPropagationProductionEditorTests.cs`
  - production `LateEntityUpdateAll`覆盖parent owner sentinel/self/nonself、kind1/kind2、type0/non-type0、
    single/multi、two-hop以及child owner与parent physical slot/holder的分离。
  - 回归实际built-in OID999 null-parent fragments仍owner`-1`；route3 F8与state9996由既有focused/SelfCheck保持。
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
  - 在既有late OPoint精度与state9996合同上补literal owner与raw backing断言。

## 不变量与排除

- 不修改OPoint候选、kind、facing/multi数量、slot allocation、birth visibility、frame、position、velocity、
  team/relation/holder、AttackExempt/VRest、RNG、pass顺序或factory通用行为。
- built-in OID999 broken fragments和state9996 five-clone保持owner`-1`；DAT `<weapon_piece>`完整
  parser/materializer及source-owner归B7。
- hit_Fa5/6继续使用已闭合的source owner与独立`+0x3F8` target；hit_Fa8/9/13与`+0x2F8`保持独立。
- 不修改`LF2ObjectPointModule` dormant fallback、F8、content、Scene、Prefab、ProjectSettings或shutdown。

## 验收

- RED先证明world-owned ordinary child在parent owner sentinel/self/nonself、single/multi/two-hop上仍错误为`-1`。
- GREEN后每个ordinary child owner恒等parent literal owner；nonself/two-hop明确不等于immediate parent slot。
- claimed active-slot entity runtime反映owner，独立raw-slot backing保持`-1`。
- kind2 holder/link仍绑定parent physical slot而owner绑定parent literal owner；两字段互不污染。
- built-in OID999 fragment与state9996 child仍owner`-1`；frame/position/velocity/slot/RNG相关既有断言不变。
- runtime/editor compile 0 error；focused Unity测试实际通过；SelfCheck、Play、joint trace状态如实记录。

## 回滚

只移除world-owned ordinary late OPoint task的单字段写入和本包测试/记录；不得回滚route1/2/3、
用户工作树、fragment/state9996或其他OPoint路径。
