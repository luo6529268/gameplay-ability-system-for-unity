# Task Contract — NTSD28-B2-NATIVE-ACTION-DATA-CARRIERS-001

> 状态：`FOCUSED_TEST_PASS / DATA_CARRIERS_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

在不连接动作执行器、timer producer或跨阶段状态生产者的前提下，为2.8原生动作路由建立完整、
可快照、可校验、可零分配复制的数据合同：runtime动作/重映射/bound/resource字段，frame的HP、
直接动作与hold/direction字段，以及definition的`use_ai/recmp/caughtact`。

## Authority 与当前事实

- authority `src/battle/battle_world.h:128-170,217-235,245-289`定义输入动作、重映射、绑定、
  资源和跨阶段状态槽位。
- authority `src/input/input_routing.cpp:287-428,446-620,1537-1559`消费这些字段并执行
  action gate、direct action、hold/direction与资源事务。
- authority `src/battle/battle_world.cpp:958-1022,3080-3125`定义完整entity初始化、输入块清理
  与carrier字段持久边界。
- `docs/ai/MANIFESTS/NTSD28-B2-NATIVE-ACTION-ROUTING.md`已冻结20项carrier的type、default、
  producer owner，以及46个由authority `main`实际调用的routing tests。
- Unity目前只有combo selector与0x21 input block；缺少本包所需动作、remap、bound/resource carrier，
  frame/definition parser也未保留全部2.8字段。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Animation/LF2CharacterData.cs`
- `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs`
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs`及`.meta`
- `Assets/NTSD/Scripts/Test/Editor/BattleWorldEntityRuntimeSnapshotEditorTests.cs`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改authority、`Assets/NTSD/Config`、DAT、Scene/Prefab、ProjectSettings、Packages、动作resolver、
timer pass、RNG、status/hit/fusion/stage生产逻辑。不得在B2提前实现归属B3/B5/B7/B8的producer。

## 数据合同

### Runtime carrier

- `InputActionLock130: int = 0`
- `InputLastAction144: int = 0`
- `InputRemapState138: int = 0`
- `InputRemapIndices13C: byte[7] = {0,1,2,3,4,5,6}`，每个runtime独立持有
- `BoundState198: int = 0`
- `InputGlobalRecordState20: int = 0`
- `InputModeCostMultiplier30: int = 0`
- `InputDoubleCost19C: int = 0`
- `InputCostWaived1B4: int = 0`
- `InputSpecialGate194: int = 0`
- `InputModeFallbackActionB8: int = 0`
- `InputLocalResourceEnabled49D034: bool = true`
- `InputHpConsumedTotal34C: int = 0`
- `InputMpConsumedTotal350: int = 0`
- `FeatureGate4A8428: bool = false`
- `InputLinkedDefinitionId324: int = -1`

已有`Unk328/Unk338`保持原字段，不创建重复carrier，也不在本包更改其producer或初始语义。

### Frame 与 definition

- frame新增`hp`；直接动作`hit_f/hit_b/hit_uz/hit_dz`；hold/direction字段
  `hold_a/hold_d/hold_j/hold_f/hold_b/hold_uz/hold_dz`。
- character definition新增`use_ai/recmp/caughtact`，只从对应definition级属性读取；不得把itr等
  同名局部字段误当成definition carrier。

## Reset、copy、snapshot 与 checksum 不变量

- `ResetInputState`只清理authority 0x21 input block；本包carrier必须保持不变。
- 完整`Reset`恢复全部carrier默认值和identity remap。
- canonical copy与snapshot对remap做深拷贝，不得共享数组；非法/缺失storage fail closed。
- entity runtime snapshot schema `2→3`、aggregate snapshot `4→5`、checksum `7→8`。
- checksum以固定顺序覆盖每个scalar及7个remap byte；任一字段变化都必须改变checksum。
- warm copy/checksum路径零托管分配；本包不产生动作、扣血/扣MP、timer推进或状态转换。

## 验收

- test-first红灯覆盖缺失字段、默认值/reset/copy/no-alias、frame/definition parser、snapshot、
  checksum敏感度、schema version与warm-path zero-allocation；
- 实现后运行本包focused tests、entity snapshot regression、相关B2回归、compile0、full SelfCheck、
  Console Error0、`git diff --check`和Change Ledger validator；
- 复核`Assets/NTSD/Config`/DAT未改，动作路由仍未接线，B3/B5/B7/B8 producer仍待后续包。

## 回滚

移除本包新增runtime/frame/definition carrier、parser复制、snapshot/checksum字段与测试，并恢复三个schema
version。不得回滚已完成的B2 input block、two-pass、combo selector或dual RNG工作。

## 当前结果

- test-first Unity compile红灯捕获128条预期缺失API `CS1061`；实现后compile 0 error。
- runtime新增carrier已覆盖input-only/full reset、canonical deep-copy、invalid storage fail-closed、entity
  snapshot与逐字段checksum；remap固定7字节、identity default且不同runtime不共享。
- frame与definition converter已保留本包全部字段并接入character data build；Config/DAT/Scene未由本包修改。
- 初轮focused job `87ddf9dac99c4d41b72b068f7cc5d91f` 9/9；snapshot/input migration定向
  `9bda007457124db882228ba30709ebcf` 17/17；B2 broad
  `582a493f4a5949bab7112053304fd452` 255/255；snapshot/checksum下游
  `0fcd26fe722744238af1bc3c84172c4d` 31/31；补强invalid snapshot storage后最终focused
  `3e4f7c1847f34b17afd8e0e4f97cd449` 10/10。
- 最终full SelfCheck于2026-09-03 13:55:53写入`PASS`；7条已知负向夹具日志清除后Console
  Error 0。`git diff --check`通过，Change Ledger 131 records / 80 governed code files通过。
- action resolver、资源事务、timer、remap producer、bound producer和B3/B5/B7/B8跨阶段生产仍未接；
  本状态不表示完整action routing已对齐。
