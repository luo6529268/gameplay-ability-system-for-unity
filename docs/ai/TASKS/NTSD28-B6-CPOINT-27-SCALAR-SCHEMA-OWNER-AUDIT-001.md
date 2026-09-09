# Task Contract — NTSD28-B6-CPOINT-27-SCALAR-SCHEMA-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / AUTHORITY_27_SCALARS_REQUIRED / RELEASE_DRAIN_WITNESS / LEGACY_ALIAS_CONSUMER_AUDIT_REQUIRED / CONTENT_STRATEGY_GATED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001 / VERIFIED`

## 目标

比较当前 B1E13AE1 Authority `CatchPointRecord28`/decoder/live consumers 与 Unity 既有 19-scalar
`BattleCatchPointValue`、legacy `CatchPoint`、converter、canonical writer、Direction-B normalized projection 和
actual/HitPlan consumers，冻结完整字段差异、正式 corpus witness、旧 alias 行为与跨包迁移顺序。

本任务只读；不修改 C#、package/manifest、Server、Config、Scene、Prefab、资源、ProjectSettings 或 Authority。

## Authority 27-scalar 合同

`combat_records.h::CatchPointRecord28` 与 `CombatRecordDecoder28::catch_point()` 独立解码：

```text
kind, x, y, injury, cover,
vaction, aaction, jaction, daction, taction,
faction, baction, uzaction, dzaction,
throwvx, throwvy, hurtable, fronthurtact, backhurtact,
decrease, dircontrol, throwinjury, throwvz,
z, recover, drain, gain
```

关键语义：

- `fronthurtact/backhurtact` 与 `injury/cover` 是四个独立字段；当前 decoder 不做 alias 或 source-order覆盖。
- advance 消费 A/J/D/T/F/B/UZ/DZ、throw、decrease 和 dircontrol。
- throw positive-injury 与 settlement nonzero-injury 均把 `drain/gain` 传给 native resource transaction。
- settlement 消费 `z`；`recover` 当前只进入 decoded record，playable battle source 没有 CPoint recover consumer。
- 未显式写入的字段精确默认为 0；sentinel-looking explicit int 必须保留。

## Unity 现状

- `BattleCatchPointValue`、`BattleCatchPointCatalog.CopyCanonicalScalars()`、focused corpus 与
  `UnityPresentContentAuthorityEditorTests.FormatCatchPoint()`冻结为 19 scalar；缺
  `Faction/Baction/Uzaction/Dzaction/Z/Recover/Drain/Gain`。
- `BattleCatchPointValueAdapter.FormalPropertyNames` 不接纳上述 8 个名称；任一显式字段会在
  `Lf2DatConverter.ConvertToCatchPoint()` 阶段抛 `InvalidOperationException`，无法建立 world。
- mutable `CatchPoint` 同样没有 8 个字段。converter 在读取 `fronthurtact` 时还写
  `cpoint.injury=fronthurtact`，读取 `backhurtact` 时写 `cpoint.cover=backhurtact`；这是旧合同 alias，
  与当前 Authority decoder 相反。
- 旧 alias 不只影响内容 identity：`LF2HitResolveRuntimeData.ResolveCaughtVictimHurtAction()`、
  `LF2CharacterHitResolver`、`BattleDamageWriter` 的 actual calls 与
  `BattleEcsHitExecutionPlan` shadow projection 都会把 kind-2 CPoint 的 `Injury/Cover`当作 caught hurt action。
  当前 Authority playable source 中 CPoint `fronthurtact/backhurtact` 没有对应 hit consumer；不能在 schema
  migration 中默默保留、删除或改字段而不先审计这些生产调用。
- 19-scalar canonical writer 与 Direction-B normalized formatter也是冻结证据接口。直接把常量19改27会改变
  corpus identity/consumer ABI；必须有显式 schema version/supersede，而不能覆盖旧 fingerprint。

## 完整语料测量

对每个 `cpoint: ... cpoint_end:` block 统计显式字段与非零值；这不是只取 primary 的行为计数，而是完整内容
schema inventory。

| Field group | Direction-B Unity（33 blocks） | 2.8 release（4019 blocks） | 结论 |
|---|---:|---:|---|
| 8 omitted fields 中 `drain` | explicit 0 / nonzero 0 | explicit 3 / nonzero 3 | 正式 release witness；值均为600。 |
| `faction/baction/uzaction/dzaction/z/recover/gain` | 全部0/0 | 全部0/0 | 当前 corpus dormant，但 Authority schema/live规则仍需保留。 |
| `fronthurtact/backhurtact` rows | 4 / 4 nonzero each | 398 / 398 nonzero each | 全部kind2，且同block均无explicit injury/cover。 |
| `injury` | 9 explicit / 9 nonzero | 648 / 484 nonzero | 正式kind1 held damage。 |
| `cover` | 0 / 0 | 438 / 303 nonzero | 正式settlement placement/timer。 |
| `aaction` | 1 / 1 | 0 / 0 | Direction-B Naruto clone当前内容存在。 |
| `taction` | 1 / 1（负值） | 0 / 0 | Direction-B Naruto clone当前内容存在。 |

因此旧“4019 条 corpus 无 A/T”的说法只适用于 release decoded corpus，不能外推到 Direction-B 当前内容；
当前 A/T production 路径仍需保持。

### drain 正式 witness

3 条均为 indexed type-0 definition 的 action 414、state 9、首 CPoint kind 1：

| OID | Definition | CPoint |
|---:|---|---|
| 63 | `c\\hir\\hir.dat` | `injury=100, vaction=132, hurtable=1, decrease=7, drain=600` |
| 27 | `c\\min\\min.dat` | 同上 |
| 449 | `c\\min\\sag.dat` | 同上 |

三者均由同 definition 的 kind-3 ITR `catchingact=403/caughtact=130` 建立关系，action403开始的 state-9
catch sequence 经 next 链推进到414。因此 `drain=600` 不是孤立未引用 frame；在 local resource transaction
启用、target type0 且 target MP 至少600时，Authority先按resource顺序处理 injury reward，再扣 target MP600并
累加 `input_mp_consumed_total`，之后才执行 held HP accounting。精确技能输入/命中/目标MP Play仍
`RUNTIME_WITNESS_PENDING`。

## 迁移依赖与 package split

### 1. 先审计旧 caught-hurt consumers

下一只读包为 `NTSD28-B6-CPOINT-KIND2-HURT-ACTION-CONSUMER-OWNER-AUDIT-001`：

- 沿 shared actual、legacy fallback、BattleDamageWriter 与 HitPlan 全部调用冻结生产 reachability、调用顺序与
  当前 Authority absence；
- 确认哪些调用必须退休、哪些只是 dead compatibility；
- focused matrix覆盖 current Naruto clone 4 条与 release 398 条 kind2 front/back数据；
- 不在未审计时把 consumer机械改读 `FrontHurtAct/BackHurtAct`，因为 Authority没有该 hit rule。

### 2. versioned 27-scalar schema/carrier

consumer owner闭合且H内容策略允许后，建立独立 schema package：

- mutable authoring boundary、immutable value、adapter、converter、catalog、tests一次性表达27个独立scalar；
- 旧19-scalar Direction-B normalized/canonical artifact保持可辨识的v1，不覆盖其SHA；新增v2 exact writer按
  Authority顺序输出27值，并显式记录schema/version；
- `fronthurtact/backhurtact`不得再污染`Injury/Cover`；旧consumer若仍需过渡，必须用具名compat seam，不能冒充
  exact field；
- current 138-DAT v1/v2 normalized projection应只出现解释性schema差异，raw内容、GUID与资源不变；
- release三条drain必须能够解析并在primary value中保留600；所有8字段unknown-admission测试更新；
- package/local shared-kernel依赖、constructor call sites、canonical buffers及零分配验收必须整体盘点，禁止只改
  `CatchPoint` class。

### 3. drain/gain behavior integration

schema carrier后，held-injury与throw resource production分别消费 exact `Drain/Gain`；但完整 transaction仍依赖：

- B7 child suppression；
- B8 selected-mode/local resource rules；
- B11/H authoritative baseMax与内容策略；
- 既有 B5 pure resource transaction/attacker resolver。

OID63/27/449 action414 drain600是 held-injury resource integration的formal Play witness；当前 Direction-B未含这些
字段，不能把release内容直接写入Config来让测试变绿。

### 4. dormant fields

- `Z` 在27-scalar carrier后由已路由的 settlement placement package消费；当前/release explicit数均0。
- F/B/UZ/DZ action仍由 advance exact规则消费；当前/release explicit数均0。
- `Recover`无当前 playable consumer，只保留content identity，不发明行为。
- `Gain`进入同一 resource transaction；现有 corpus为0，但 synthetic all-or-nothing测试保留。

## 验收

- schema owner audit后，先完成 kind2 hurt consumer owner audit；不得跳过直接改19→27。
- 后续 schema package：test-first unknown drain RED、27-value equality/hash/default/signed/canonical-v2、v1 freeze、
  raw/current normalized projection、release drain witness与4096 warmed zero-allocation。
- 后续 behavior：compile、focused、B6/NTSD28、SelfCheck、same-tick resource/HP order、OID63/27/449定向Play、
  first-difference为零。
- 任何静态 parse/corpus通过都不能替代运行时 skill entry、catch establishment、action414、target MP gate与transaction
  结果。

## 不变量 / 阻塞

- 不修改 Direction-B Config、release DAT、Scene、Prefab、资源、Authority或Server S0-S9 formal marker。
- 不改变当前A/T production、catch relation/settlement order、resource arithmetic、content fingerprint或package ABI。
- 用户尚未选择H内容策略；涉及normalized authority版本和release内容消费的production保持 gated。
- 当前B6 runtime栈仍未清；本轮不新增脚本 diff。

## 回滚

仅移除本治理记录与状态摘要；没有代码、content、Scene、package或Authority回滚。

## Current corpus correction（2026-09-08）

本记录的current `33 blocks / front-back4 / injury9 / cover0 / A-T各1`来自单行block漏计，已由
`NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001`纠正为CPoint1426、front/back170、raw
injury explicit298/nonzero223、cover142/106、A/T各9。8个missing字段current explicit仍0；schema、alias与三阶段
owner结论保留，但测试矩阵以总correction为准。
