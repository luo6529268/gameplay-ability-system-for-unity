# Task Contract — NTSD28-B2-NATIVE-TYPE0-GROUND-BUILTINS-001

> 状态：`FOCUSED_TEST_PASS / GROUND_CORE_READY / HIGH_FRAME_READY / PRODUCTION_UNCONNECTED / AIR_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / NATIVE-TYPE0-BUILTINS`  
> 建立日期：2026-09-04

## 目标

在已通过focused的数据seam上，实现无分配native ground built-ins core：action110朝向、state19/301
落地纵深、state0/1普通与relation-state2 heavy walk/run/attack/jump/defend、state2普通与heavy
running动作、direct side effects、资源策略、linked stats和同步RNG `0x82/0x83/0x84`。本包只提供
writer core与focused证据，不连接two-pass，不关闭legacy release owner。

## Authority 与当前事实

- 正式`input_routing.cpp:630-719,808-893,897-933,992-1331`在playable build闭包；formal
  `input_routing_tests.cpp`的1、2、11～14、29～38项均由`main`调用且fresh authority binary通过。
- `control_slot_000`对应Unity `AnimCounter`；`input.run_accumulator`对应`AnimSub`；standing
  jump/defend清`AnimSub`与frame counter、保留`AnimCounter`，walk/run frame write通常保留frame counter。
- ordinary diagonal walk/run只除X，divisor分别1.4/1.2；custom sequence优先、空sequence用
  5..8/9..11/12..15/16..18 cyclic fallback，walk/run共享同一`AnimCounter`。
- standing attack interaction0使用world synchronized RNG `0x82`；interaction101使用`0x83`，其他
  positive x01使用`0x84`；不得调用legacy `BattleRandInt`。
- standing/ordinary airborne攻击使用clamp-on-overdraw；ordinary running attack85使用affordability
  gate；linked direct action绕过input lock/generic action cost与`InputLastAction144`。
- `LinkState`映射authority interaction state，`TargetSlotIndex`映射positive linked child slot；linked
  definition来自该slot的current DAT。`HitConfirmEa`复用authority kind6 input timer `+0x0EA`。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs`
- `Assets/NTSD/Scripts/Animation/Character/LF2FrameCache.cs`（仅把当前formal authority实测最大
  frame 856纳入固定索引边界；不得改lookup语义）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅将DATA-01B/C旧600上限守卫重基线到
  `MaxFrameIdExclusive=857`与856/857边界）
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0GroundBuiltinsEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改two-pass/resolver/legacy owner、authority、Config/DAT、Scene/Prefab、ProjectSettings、Packages、
air/dash/state85/86/action215/rowing、frame/hit/spawn或B11内容值。本包不得标为production connected。

## 不变量

- core入口只拥有action110、grounded state19/301、state0/1/2；其他state返回not-owned且不得改变状态。
- direct action不检查`InputActionLock130`、不写`InputLastAction144`、不走generic HP cost；只有authority
  标记restart的branch清`AttackingCounter`。
- `AnimSub`每次owned route先向0衰减；double-tap切run时清`AnimSub/AnimCounter`，无standing
  action pending才在同tick进入state2；同时方向+attack必须保留walk motion并由standing attack覆盖action。
- standing attack要求current J且buffer>0；standing K/L同样要求current+buffer，L还要求reentry cooldown0；
  running/heavy branches按authority仅检查buffer的入口不得擅自加current gate。
- movement sequence按`(counter+1) % (count*rate)`选择；fallback 6-phase/4-phase镜像循环；rate最小1。
- formal decoded runtime实测最大frame id为856；frame cache必须可加载/查询856，857仍为exclusive边界，
  不得把高编号sequence action静默变成空frame。
- linked字段0或linked entity/definition缺失使用调用点默认；不得从旧`NTSDSpec`推导动作。
- hot route不得分配managed memory；同步RNG由world唯一stream按ascending caller顺序直接消费。

## 验收

- test-first覆盖custom/shared cycle、action110/state19/301、current+buffer、defend cooldown、double-tap
  same-tick、1.4/1.2 scaling、direct side effects、clamp/gated resource、HitConfirmEa action70、
  linked selectors/default、0x82/83/84及heavy ground/run；
- warm 4096 route为0 B；Unity compile0、focused/related、B2 broad、SelfCheck、Console0、Ledger PASS；
- production仍未连接，air core与最终integration/joint trace另包。

## 回滚

移除`BattleCharacterActionWriter`中的ground built-ins core/helper与新focused test；不触碰已连接的
combo/direct/hold/direction或已通过的数据seam；`LF2FrameCache.MaxFrameIdExclusive`恢复600。

## Test-first 证据

- 2026-09-04：新增11项focused ground contract。首轮除37条missing writer API外，发现当前NUnit
  不支持`Is.AnyOf`，已先修为等价`Or`约束；重新编译后最后编译段恰为37条唯一CS1061，全部只因
  `RouteNativeGroundBuiltins`尚不存在，新测试之外错误0。
- writer ground core及其direct/resource/sequence/linked helper已写，`git diff --check`通过；
  two-pass/resolver仍未修改，Unity compile/focused待跑。
- 首轮focused为10/11：唯一失败在authority custom action650被`LF2FrameCache`现有600 exclusive cap
  静默丢弃。只读扫描formal decoded runtime得到最大frame id 856（Asuma `asu.dat`），因此本Task在
  修改前纳入精确857 exclusive frame-cache seam，并补856/857边界断言。
- frame-cap修复后compile0、focused12/12、B2 broad259/259；15:39:36 full SelfCheck唯一失败为
  `DATA-01B/C`仍硬编码0..599/600。该旧自检与formal max856冲突，已在修改前纳入本Task精确重基线。
- SelfCheck依次暴露并修正同源的DATA-01B/C、FT-02与LC-02旧600夹具；最终15:46:50 full
  SelfCheck `PASS`。Console仅7条预期rest-binding负向日志，清除后error0；Ledger135/91与diff check通过。
- ground core保持production unconnected；air/dash/redirect与最终owner integration/joint trace仍待。
