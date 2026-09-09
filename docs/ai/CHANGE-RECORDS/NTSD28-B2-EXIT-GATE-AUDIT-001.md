# NTSD28-B2-EXIT-GATE-AUDIT-001 — 输入、双RNG与AI阶段退出审计

<!-- CHANGE-RECORD
id: NTSD28-B2-EXIT-GATE-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: Formal NTSD2.8-Logan.exe plus playable input_routing/native_ai/native_random/GameSession/host live path; alignment matrix I-01..I-10, R-01..R-07, A-01..A-08.
evidence: TASK-CONTRACT-CREATED / HUMAN-SOURCE-JOINT-EQUAL / AI-TICKS1-2-SOURCE-JOINT-EQUAL / FORMAL-EXE-HUMAN-AI-HEADLESS-BEHAVIOR-PASS / I-01-10-R-01-07-A-01-08-AUDITED / INPUT-PROXY-COMBO-AI-FOUNDATION-CLOSED / FRESH-AUTHORITY-NONAI-SYNC-50-TEXT-EXPRESSIONS-DIRECT-CRT-2 / OLD-43-INVENTORY-CORRECTED / NONAI-RNG-CROSSWALK-CLOSED / FUNCTION-KEY-F1-F12-CROSSWALK-CLOSED / FUNCTION-KEY-ROUTE-CARRIER-PRODUCTION-CLOSED / FUNCTION-KEY-REAL-PLAY-PHYSICAL-PASS / I08-B8 / I09-CONDITIONAL-ADAPTER / A06-B3-B4 / F4-B8 / F11-F12-B10 / DOWNSTREAM-OWNERS-ROUTED / B2-EXIT-READY / NEXT-B3-ENTRY-AUDIT / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / B2-EXIT-READY / FUNCTION-KEY-PHYSICAL-PASS / DOWNSTREAM-OWNERS-ROUTED / NEXT-B3-ENTRY-AUDIT`

> 2026-09-04 addendum：`NTSD28-B2-FUNCTION-KEY-ROUTE-CROSSWALK-001` 已闭合F1～F12清单与效果owner；
> 当前阻断从“未知路由库存”推进为pure route→Session carrier→production integration实施链。

> 2026-09-04 addendum 2：`NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001` 已通过focused；当前阻断推进到
> Session state/event-byte carrier，之后才允许production integration。

> 2026-09-04 addendum 3：`NTSD28-B2-FUNCTION-KEY-SESSION-STATE-CARRIER-001` 已通过focused并闭合
> reset/snapshot/restore/checksum；当前阻断推进到统一production integration与typed downstream handoff。

> 2026-09-04 addendum 4：`NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001` 已通过focused；I-07/I-10/A-08的
> B2 route/carrier/production code闭合。F4/F6～F12 effect仍按B3/B5/B8/B10/B11执行；当前B2唯一剩余门为
> 真实Play physical F3～F12 probe，不把typed handoff误报为effect已完成。

> 2026-09-04 addendum 5：`NTSD28-B2-FUNCTION-KEY-PHYSICAL-PLAY-PROBE-001` 已在真实`NTSD_Battle`、
> Keyboard device1、tick0→5完成九组PASS并退出Play，Scene SHA不变。I-07/I-10/A-08的B2职责现已关闭；
> 下游effect和NONAI consumer继续由B3/B5/B8/B10/B11/B12承担。下一步进入B3 entry audit。

## 计划

- 逐项核对25个I/R/A差异条目及其authority/Unity当前生产调用。
- 审核全部B2 Record的旧`UNCONNECTED/PENDING`是否被后续integration包实质关闭。
- 将下游producer与B2基础接口分开，不把B3+职责偷算为B2完成，也不让可延期字段永久卡死阶段推进。
- 冻结唯一下一包与验收门。

## 当前事实

- joint trace与formal smoke分别覆盖内部source-model字段和正式EXE可观察行为，但B2还没有逐项exit audit。
- I-07/I-10功能键、I-09 LFR、R-03/R-05非AI producer、A-06 slot-tail computer timer、A-08 mode/resource
  可能仍含真实缺口；必须按生产owner拆分。

## I / R / A 逐项退出审计

| ID | B2审计状态 | 证据 / 缺口 | Owner与B2结论 |
|---|---|---|---|
| I-01 | `B2_FOUNDATION_CLOSED` | 1tu=0、2tu=1/0 phase已进runtime/snapshot/checksum与测试。 | B2闭合。 |
| I-02 | `B2_FOUNDATION_CLOSED` | human current仅phase0更新；joint common/standing全等。 | B2闭合。 |
| I-03 | `B2_FOUNDATION_CLOSED` | FrameInputSet在tick前冻结并进入两遍producer/sample。 | B2闭合。 |
| I-04 | `B2_FOUNDATION_CLOSED` | exact 0x21 block、control lifecycle、两遍proxy production已接。 | B5 writer/B3 tail只写既有carrier。 |
| I-05 | `B2_FOUNDATION_CLOSED` | host→AI previous、AI current、human/link ownership与升序两遍已闭合。 | B2闭合。 |
| I-06 | `B2_FOUNDATION_CLOSED` | combo10、action transaction、direct/hold/direction、type0 builtins已接；DDJ/DRA Play通过。 | B11 action内容差异不反判B2。 |
| I-07 | `B2_BLOCKER` | F1/F2/F5已接；F3/F4/F6～F10正式route缺失，Unity F7/F8/F9仍为旧合同。 | 先补B2 route/session/host carrier；F4 effect→B8、F11/12 effect→B10。 |
| I-08 | `DOWNSTREAM_OWNER` | Results continue必须在正式timer gate消费。 | B8 BattleFlow/Results；不阻断B2基础退出。 |
| I-09 | `CONDITIONAL_NOT_REQUIRED` | Unity确定性FrameInput/journal可复现；formal行为已由内置smoke观察，未要求文件格式互通。 | B12若需要直接消费LFR再建adapter；不宣称格式兼容。 |
| I-10 | `B2_BLOCKER` | 缺F1～F10 auto-repeat、battle/main-state、F3 lock、global-delay与Ctrl维护命令统一路由。 | 与I-07同一function-key系列。 |
| R-01 | `B2_FOUNDATION_CLOSED` | NTSD28NativeRandom持有CRT+3001-byte synchronized两流。 | B2闭合。 |
| R-02 | `B2_FOUNDATION_CLOSED` | call-site-aware synchronized primitive/cursor/trace已接。 | B2闭合。 |
| R-03 | `B2_BLOCKER_PARTIAL` | AI、input0x82/83/84、direct-battle BGM已迁移；battle/stage/lifecycle/game-session未交叉。 | 先做NONAI crosswalk，再按B3+ owner实施。 |
| R-04 | `USER_EXCEPTION_ISOLATED` | Unity随机武器仍用legacy `world.Rng`，不推进NTSD28NativeRandom。 | 保留用户例外；需在B7/B12继续证明slot副作用隔离。 |
| R-05 | `B2_BLOCKER_PARTIAL` | AI/input fixture逐次顺序相等；fresh重计其余50个非AI文本表达式尚未建立统一顺序ledger。 | NONAI crosswalk阻断B2。 |
| R-06 | `B2_FOUNDATION_CLOSED` | seed/reset/direct bootstrap/snapshot/restore/checksum均覆盖native双流。 | 各下游producer只消费该owner。 |
| R-07 | `B2_FOUNDATION_CLOSED_WITH_FORMAL_BOUNDARY` | v3 joint comparator可定位逐次首差；formal smoke验证行为但不暴露内部calls。 | source live path定义内部字段；不伪称formal per-call certificate。 |
| A-01 | `B2_FOUNDATION_CLOSED` | first valid tick、AI-before-routing、accepted commit与两遍输入已闭合。 | B2闭合。 |
| A-02 | `B2_FOUNDATION_CLOSED` | native_ai live branch/call-site crosswalk、canonical kernel及focused corpus已闭合。 | 全角色/场景覆盖继续B12。 |
| A-03 | `B2_FOUNDATION_CLOSED` | AI synchronized 6/7/8逐次相等；legacy CRT owner隔离。 | B2闭合。 |
| A-04 | `B2_FOUNDATION_CLOSED` | physical-slot shared rows、full/indexed shadow及tie-break tests已具备。 | birth/hit造成的候选变化由B5/B7复验。 |
| A-05 | `B2_FOUNDATION_CLOSED` | AI输出经canonical store→exact proxy/routing，history roundtrip已闭合。 | B2闭合。 |
| A-06 | `DOWNSTREAM_OWNER_WITH_CARRIER` | `nativeComputerState1b8`已存在；state7000～7999 tail producer尚未对齐。 | slot tail顺序归B3，frame条件归B4；不在B2偷接。 |
| A-07 | `B2_SEAM_CLOSED_DOWNSTREAM_MATRIX` | AI sensing按active physical slots与canonical rows；对象birth/lifecycle仍会改变集合。 | B7/B12做全量可见性矩阵。 |
| A-08 | `B2_BLOCKER_PARTIAL` | battle mode carrier已分离，但function-key route仍缺；资源/mode effects未按2.8闭合。 | route→B2，资源/effect→B8/B10/B11。 |

## 旧B2状态 supersede 审核

- proxy block/carrier/control的`PRODUCTION_UNCONNECTED`已被`AI-SAMPLE-PROXY-TWO-PASS`与
  `NATIVE-INPUT-PRODUCER-MIGRATION`实质关闭。
- AI各RNG分支的`PRODUCTION_UNCONNECTED`已被`AI-SYNC-RNG-PRODUCTION-COMMIT`及v3 joint trace关闭。
- ground/air builtins的`PRODUCTION_UNCONNECTED`已被`NATIVE-TYPE0-BUILTINS-PRODUCTION-INTEGRATION`关闭。
- environment-state carrier的producer仍真实未接，但其writer分别属于B3/B4/B5/B6，不能在B2凭空写值。
- 尚未被supersede的真实缺口：非AI RNG consumer/call-order与完整function-key route/gate。

## 下一包

`NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001`（governance only）：冻结authority fresh重计的50个非AI synchronized
表达式、2个direct CRT调用的live owner、条件、顺序、下游阶段和Unity候选；输出可实施拆包，不直接改行为。

## 后续状态更新

NONAI crosswalk现已完成：50+2逐项manifest，未迁移consumer均有B3～B8/B11/B12 owner。由于这些调用必须
随各自pass/single-writer行为一起迁移，它们不再作为“未知库存”阻断B2；B2当前下一阻断改为F3～F12
function-key route/reject与effect handoff交叉。

## Git / 交接

- production代码：零修改。
- authority：只读。
- validator：`PASSED / Records 151 / governed code files 104`；本包`code-path: NONE`。
