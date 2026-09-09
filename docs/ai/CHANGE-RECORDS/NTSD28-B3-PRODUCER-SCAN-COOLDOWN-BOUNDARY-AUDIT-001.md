# NTSD28-B3-PRODUCER-SCAN-COOLDOWN-BOUNDARY-AUDIT-001 — C02/C03与Cooldown边界审计

<!-- CHANGE-RECORD
id: NTSD28-B3-PRODUCER-SCAN-COOLDOWN-BOUNDARY-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD 2.8-Logan C02 producer/object-hit_Fa/AI sample scan and C03 proxy/route scan after verified C01; Unity actual first difference is Cooldown.
evidence: TASK-CONTRACT-CREATED / READ-ONLY / USER-CONFIRMED-BUGFIXED-EXE-B1E13AE1 / PLAYABLE-CLOSURE-39DDDA15 / CAPTURE-MANIFEST-07CD47A0 / C00-C03-UNCHANGED / C02-SLOT-INTERLEAVED-HITFA-AND-CHARACTER-PRODUCERS / C03-SECOND-ROUTE-SCAN / UNITY-COOLDOWN-AREST-EARLY / UNITY-ATTACKEXEMPT-CLEAR-EARLY / NATIVE-C11-CLEAR / NATIVE-C25J-DECREMENT / FRAMELOGIC-IS-NONCHAR-HITFA / NEXT-C02-C03-PRODUCTION-PLACEMENT / NO-PRODUCTION-CHANGE / NO-AUTHORITY-WRITE
-->

> 状态：`VERIFIED / C02-C03-WRITERS-CLOSED / COOLDOWN-CLASSIFIED / NEXT-PRODUCTION-PLACEMENT`

## 起点事实

- C01 production integration已验证，actual full/partial sequence为31/6。
- 共同`BattleFlow → NativeSparkAdvance`之后，Authority下一checkpoint是`CoreProducerSampleScan`，
  Unity下一occurrence是`Cooldown`。
- Unity B2已经在`CharacterInputAll`内部建立producer/sample barrier与proxy/route第二遍；当前问题不是重新实现
  B2输入算法，而是明确其前置pass和C02非输入职责的正确位置。

## 审计计划

按Task Contract逐项闭合Authority、Unity writer与traversal，再选定下一实现包。当前不修改脚本。

## 结果

读取C02/C03入口时发现当前物理EXE/source不再匹配固定authority身份；立即停止把当前source用于裁决。
本包没有完成Cooldown职责分类，也没有产生脚本修改。恢复条件由
`GOVERNANCE-NTSD28-AUTHORITY-IDENTITY-DRIFT-001`定义。

## 可安全保留的Unity侧只读库存

以下只描述当前Unity，不把它映射为未确认C++的正式语义：

- C01后当前顺序为`Cooldown → optional HumanInput → CharacterInput → RuntimeMaintenance`；battle-entry
  input-clear partial则为`Cooldown → HumanInput → RuntimeMaintenance → InputClear → return`。
- `BattleEcsCooldownPass`默认DataOriented writer只做两类写入：对每个active slot的
  `RuntimeRestStore.ARest`减1；当当前frame无Itr，或held frame对应holder WPoint不再attacking时，把
  `AttackExempt`清0。方法名`VrestTickAll`是旧命名，实际此pass不遍历或递减victim-vrest map。
- `AttackExempt`还有独立的later frame writer：`LF2Entity.RunCommonFrameTick()`在FrameDelay/data-type gate后、
  LinkState/cpoint gate前执行正值减1。因此不能把当前Cooldown整体当成一个可直接移动的单counter pass。
- `PostCooldownHumanInputAll`只遍历已绑定active human roster，调用controller poll并刷新runtime；它是human
  producer，不执行AI、proxy或action route。
- `CharacterInputAll`已经由B2建立两遍character-only scan：第一遍AI/human producer与sample freeze，第二遍
  exact proxy copy、native sampled-state处理和action route。它不覆盖non-character object `hit_Fa`。
- `Oid5152RuntimeMaintenance`在普通路径位于CharacterInput之后，在input-clear partial又会在return前运行；
  其职责是OID 7/8→51 merge及split lifecycle，不属于输入producer，且当前位置会影响structural visibility。
- HumanInput、CharacterInput与多个maintenance使用`BeginDeferredMutationEntityPass()`，在pass scope尾部才flush
  structural mutation；先前固定authority表要求C02 non-character `hit_Fa`后重新取得当前slot，并允许新高slot
  在同一scan可见。此差异必须等authority身份恢复后重新确认，当前不下实现裁决。

恢复后不需要重新搜索上述Unity入口；下一步只需用被确认的正式source重新建立C02/C03 writer表，再决定
Cooldown拆分、non-character producer pass和OID51/52 placement的实施顺序。

## 恢复进度

用户已明确晋升Bug修复版。当前source确认C00～C17前半仍保留producer→proxy/route→motion→teleport→physics→
revival→geometry/fusion→type0 hit→drop→non-type0 hit→catch链，但C18以后与旧表存在实质变化。必须先完成新版
全pass manifest与contract修正，再继续选择Cooldown extraction包；不能仅在旧52项序列上移动C02。

## 新版权威C02/C03闭环

- C02仍是一个升序live-slot scan。non-character先执行正`hit_Fa`行为，调用后重新取得当前slot；若仍active，
  同一slot继续physical/AI producer sample。新生更高slot可在本scan继续被访问。
- C03仍是第二个升序scan：先做精确proxy 0x21-byte状态复制，再处理sampled input/action route。
- C00～C03与旧身份的语义一致；新57项contract中索引6～9保持input phase、spark、producer、route。

## Unity职责分类

| Unity职责 | 新版权威位置 | 裁决 |
|---|---|---|
| `PostCooldownHumanInputAll` controller poll | C02 scan前的physical/controller sample | 必须移到Cooldown前。 |
| `CharacterInputAll`第一遍character producer/freeze | C02 | 可复用，但须与non-character hit_Fa按slot合并到production专用第一遍。 |
| `CharacterInputAll`第二遍proxy/route | C03 | 可复用，保持独立第二遍barrier。 |
| `FrameLogicBeforeAdvanceAll` non-character hit_Fa | C02 | 从后续FrameLogic phase移入production producer第一遍；不得重复执行。 |
| `RuntimeRestStore.ARest`减1 | C25j attacker-rest尾部镜像 | 当前Cooldown过早；本placement包只把Cooldown整体推到C03后，后续tail包与`AttackExempt`一起移到C25j。 |
| current-frame无Itr/state1001 holder WPoint时清`AttackExempt` | C11 geometry prelude | 当前Cooldown过早；归B5 candidate prelude，不在C02/C03包重写。 |
| `RunCommonFrameTick`减`AttackExempt` | C25j，且受motion-hold/type3 gate | 当前随Unity FrameAdvance发生在candidate前；归B3 tail/B4-B5联合包。 |
| `Oid5152RuntimeMaintenance` | C12 fusion/lifecycle相关 | 不属于输入producer，保持后续独立处理，最终归B5/B7。 |

`ActiveEntitiesByRuntimeSlot`会动态读取高slot，pending unregister也会被`IsActiveForCurrentPass`跳过；但deferred
unregister使“销毁后同/低slot立即复用”仍与native不同。C02 placement包只保留这一已披露边界，精确slot复用归B7
birth/lifecycle trace，不借本包扩大修改。

## 下一实现包

`NTSD28-B3-C02-C03-PRODUCTION-PLACEMENT-001`：新增production专用producer/route入口，把human poll、按slot
interleaved non-character hit_Fa + character producer、第二遍proxy/route移到Cooldown前，移除后续FrameLogic重复
occurrence。先不拆Cooldown内部writer；关闭后下一首差预期下移为`Authority CoreFrameMotion / Unity Cooldown`。
