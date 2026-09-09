# Task Contract — NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ADVANCE_PACKAGE_AMENDED / SETTLEMENT_PACKAGE_DEFINED / RELEASE_TREE_WITNESS / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-CATCH-ADVANCE-SLOT-ORDER-OWNER-AUDIT-001 / VERIFIED`；
> `NTSD28-B6-POST-THROW-SETTLEMENT-AUDIT-001 / ACCOUNTING_FACTS_RETAINED / ROUTING_CORRECTED`

## 目标

补齐 `advance_catch_relations()` 与 `settle_catch_relations()` 中此前未冻结的 terminal `continue`
边界，纠正 Unity 在关系失配、negative-decrease release 以及 `vaction` frame 无效后继续执行后续副作用的
控制流差异，并把现有错误 SelfCheck 断言、正式语料可达性和最小 production 包边界写清。

本任务只读 Authority、Unity runtime/test 与当前/release DAT；不修改 C#、Config、Scene、Prefab、资源、
ProjectSettings 或 Authority。

## Authority 控制流事实

### advance：两个 terminal branch

`BattleWorld28::advance_catch_relations()` 在单一 slot 升序 mixed pass 中：

1. snapshot kind-1 catcher 的 target 不存在、exact reciprocal source 不匹配、target snapshot frame 无效或首
   CPoint 不是 kind 2 时，只把 catcher current action 写 0，记录 broken relation，然后立即 `continue`。
   caught action/position/motion、双方 frame counter、timeout/relation、throw resource/environment、throw motion、
   dircontrol 与 RNG 均不再变化。
2. `decrease < 0` 使 timeout 穿过 0 后，依次写 catcher action 0、caught action 181、双方
   `frame.frame_counter=1`、caught horizontal motion 为按相对 X 选择的 `-4/+4`、Y motion `-3`，保留
   reciprocal 字段与负 timeout，然后立即 `continue`。该 tick 不再执行 input action、throw、dircontrol、
   resource/environment 或 RNG。

### settlement：vaction write 后重新预检

`BattleWorld28::settle_catch_relations()` 在 hurtable 条件成立时：

1. 无条件把 `vaction` 通过 `native_relation_action()` 写给 caught；负值先翻 caught facing 再取绝对 action，
   0 也必须真实写 action 0。
2. 随即从 caught 自身 definition 重新解析新 action frame；frame 不存在时记录失败并立即 `continue`。
3. frame 存在但首 CPoint 缺失或不是 kind 2 时同样立即 `continue`。
4. 只有新 frame 与新 kind-2 CPoint 均有效时，才允许继续 injury/resource、position/cover/facing、precise position
   与 catch anchor 写入。

失败后的 action/facing 已经提交，不回滚；被 terminal fence 阻止的是其后的 injury 与 placement 副作用。

## Unity 当前首差

- `BattleCpointWriter.RunKind1()` 在 reciprocal mismatch 后设置 `skipActions/useFallbackFrameForThrow`，但仍会在
  原 snapshot CPoint 的 `ThrowVx != 0` 时用 frame 0 fallback 执行 `ApplyThrow()`，并始终继续
  `ApplyDirControl()`。这与 Authority 的立即 `continue` 相反。
- negative-decrease release 写的是 `HitCount=1`，而已验证的 native `frame.frame_counter` carrier 是
  `AttackingCounter`；之后 Unity 仍可能 throw/dircontrol，并由 frame post-process 消费错误的 HitCount。
- `BattleRuntimeSelfCheck.CheckCpointEscapeAndMismatchControlFlow()` 当前明确断言“retain the C++ fallback-frame
  throw tail / dircontrol tail”。这些是与当前 B1E13AE1 Authority 源码相反的旧断言，后续 production 包必须先
  将其改成 Authority RED fixture，不能继续把现状绿灯当对齐证据。
- `BattleCpointWriter.SyncCaughtByCpoint()` 以 `cpoint.Vaction != 0` 作为额外 gate，且写 action 后没有重新验证
  target frame 与 kind-2 CPoint。若 frame 缺失，`DirectWriteRawFramePreserveWaitCounter()` 会留下新 frame id 和
  `Frame.D=null`，随后 `SyncHeldPosition()` 仍以全零 target geometry 执行 position/cover/facing 写入。

## 语料测量与可达性

逐 DAT frame 只取第一个 CPoint；settlement 统计另限定 frame state 9。跨定义 join 只使用各 corpus
`data.txt` 中的 indexed definition，并对每条 kind-3 ITR 联结其 source catching action 与每个 type-0 target
的 caught/vaction frame。该 join 证明静态正式内容组合，不冒充几何命中或真实 Play 已复现。

| Corpus | DAT files | kind-1 first CPoint | state-9 kind-1 | `z != 0` | `vaction <= 0` | negative decrease | negative decrease + throw | throw 或 dircontrol |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Direction-B Unity | 138 | 17 | 16 | 0 | 0 | 1 | 0 | 3 |
| 2.8 release decoded | 405 | 1709 | 1593 | 0 | 0 | 96 | 6 | 94 |

- 当前 3 条 throw/dircontrol 行为行是 Naruto clone 的 1 条 dircontrol 与 Naruto clone/criminal 的 2 条
  throw。前置 lifecycle audit 已证明 Unity 可在 slot release/reuse 后留下 stale catch relation，因此
  “existing target 但 reciprocal 不匹配”不是纯不可达 synthetic 状态；Authority lifecycle 本身会在 release
  前清关系。
- release 的 6 条 negative-decrease + throw 分别位于 `c\\gaa\\gaa.dat` 4 条、
  `c\\hay\\a\\blo.dat` 1 条、`c\\kon\\kon.dat` 1 条；
  只有 timeout 在进入这些 frame 时实际穿过 0 才触发 terminal branch，真实技能/时点仍
  `RUNTIME_WITNESS_PENDING`。
- indexed cross-definition join：Unity 为 137 definitions / 42 type-0 / 1 kind-3 ITR，唯一初始有效
  kind-2 pair 的 post-vaction 仍有效，invalid 数为 0。release 为 330 definitions / 158 type-0 /
  548 kind-3 ITR；494 条 source catching action 本身为 state-9 kind-1，得到 66372 个静态初始
  kind-2 pair，其中 134 个 post-vaction 无效。
- 这 134 个全部来自 `OID 555 c\\yam\\a\\tre.dat`：attack 73 的 kind-3 ITR 写
  `catchingact=72/caughtact=130`，action 72 CPoint 为
  `vaction=517/hurtable=1/injury=0/decrease=10/cover=11`。134 个 target 中 121 个缺 frame 517，
  13 个 frame 517 没有 CPoint。它们会产生同 tick action 一致、但 Unity 额外 placement/cover/facing 写入的
  first difference；正式 Yamato tree Play 路径仍待验。
- 两个 corpus 的 kind-1 首 CPoint 均没有 nonzero `z` 或 zero/negative `vaction`。这些规则必须保留 synthetic
  验收，但不能冒充当前内容首差。`z` 仍缺 Unity formal carrier，单独后置。

## 后续 production 包与依赖顺序

### 1. 扩充既有 advance 包

既有 `NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001` 必须同时完成：

- 前置 exact relation producer 与 entity-link lifecycle cleanup runtime 绿灯；
- 单一 slot 升序 mixed dispatch 与 exact `CatchSourceSlot90` consumer 迁移；
- reciprocal mismatch 立即结束该 slot，禁止 fallback throw/dircontrol；
- negative-decrease release 写 `AttackingCounter=1` 而不是 `HitCount=1`，随后立即结束该 slot；
- 将 `CheckCpointEscapeAndMismatchControlFlow()` 的旧 fallback-tail 断言先改为预期 RED，再实现修正；
- 不把 settlement 合入 advance，不更改 throw 正常成功路径、resource 策略、content 或 lifecycle 顺序。

### 2. 独立 settlement preflight 包

`NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001` 在上述包之后：

- actual-only 修改 `BattleCpointWriter.SyncCaughtByCpoint()`；无 HitPlan owner；
- 精确实现 signed/zero vaction write、新 frame 与首 kind-2 CPoint preflight、失败后的 action/facing commit 与
  injury/placement terminal fence；
- focused fixture覆盖 frame missing、frame 无 CPoint、other-kind、valid kind2、vaction 0、negative vaction、
  hurtable false/1+timer0/1+timer非0，以及 release OID555/action72→type0 action130/517 witness；
- 验证失败分支不改 HP/MP/score/KO/timer/position/cover anchors/RNG，且 valid 分支继续交给独立 held-injury
  accounting package。

### 3. 独立 dormant schema

`cpoint.z` 需要新增 formal data contract/parser/adapter carrier并接 settlement，不能塞进上述控制流包；因两端
现有 corpus 触发数均为 0，保持后续 dormant schema package，不阻塞本次 two-package owner 结论。

## 运行验收

- compile 0 error；focused advance/settlement tests、NTSD28/B6 regression 与 BattleRuntimeSelfCheck PASS；
- stale target release→reuse→advance witness证明无 erroneous throw/dircontrol 与无 slot ABA；
- 6-row negative-decrease/throw synthetic threshold matrix；
- release Yamato tree OID555 对可进入 action130 的 type-0 目标做定向 Play/trace；
- actual/legacy/no-op proof 路径一致，4096 warmed iterations 零 managed allocation；
- same seed/input/tick first-difference 为零后才可分别提升对应 production 包，不能用静态 134-pair join替代Play。

## 不变量 / 阻塞

- 不改 relation producer、throw 正常路径、held injury accounting、caughtact combo、WPoint、OPoint、content、
  Scene、Authority 或顶层 pass 顺序。
- 不将 release DAT 写入 Direction-B Config；content 策略仍等用户选择。
- 当前 throw 与 refill production 尚缺 Unity Test Runner/SelfCheck/Play 绿灯，exact relation/lifecycle/mixed-pass
  production 也已 held；本轮只记录 owner 与验收，不新增脚本 diff。

## 回滚

仅移除本治理记录及对应摘要；没有代码、content、Scene 或 Authority 回滚。

## Current corpus correction（2026-09-08）

current first-kind1/state9/negative/negative+throwvx/throw-or-dircontrol由`17/16/1/0/3`纠正为
`776/760/204/1/88`。current post-vaction invalid不再为0：OID417 tree产生40 pairs。terminal continue、
AttackingCounter与settlement preflight owner结论保留；current正式matrix扩大。详见multiline总correction。
