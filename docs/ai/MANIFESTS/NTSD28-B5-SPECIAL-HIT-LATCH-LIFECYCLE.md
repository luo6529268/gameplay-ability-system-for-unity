# NTSD28 B5 Special-Hit Latch Lifecycle Crosswalk

## 1. Authority identity

- 正式 EXE：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- playable closure：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- field：`battle_world.h::EntityState28::special_hit_latch_0eb`，独立 bool，default false。
- trace：`scenario28.cpp` 的 entity JSON 字段 `specialHitLatch0eb` 与 deterministic state 均直接读取。

## 2. Authority producer/consumer matrix

| 顺序 | Authority 位置 | 条件 | 写入/行为 |
|---|---|---|---|
| consumer A | `classify_ordinary_hit_eligibility`，`battle_world.cpp:4851` | attacker latch=true，target type0 | writer前 reject queued standard hit |
| consumer B | `resolve_special_relation_hit`，`battle_world.cpp:5303` | attacker latch=true，target type0 | relation/special dispatch前终止该candidate |
| producer 1 | kind9 state3005 type3，`:5648` | target type3/state3005 | latch=true，action40 |
| producer 2 | kind9 generic type3，`:5659` | target type3/non3005 | relation/identity/motion后 latch=true |
| producer 3 | locked kind transform，`:6906` | bound/respond catalog命中 | identity/action-history事务内 latch=true |
| producer 4 | generic type3 continuation，`:6929` | target type3/non3005 | group/owner/control事务内 latch=true |

源码树中没有 `special_hit_latch_0eb=false` 的运行时 writer；false 只来自新实体默认状态。因此该值是
object-lifetime latch，不是下一 tick 自动清理的 candidate scratch。

## 3. Unity current mapping and first difference

| Unity 面 | 当前行为 | 与 Authority 的关系 |
|---|---|---|
| `BattleHitCandidateSequenceRunner.TryConsumeCandidate` | `attacker.HitConfirm2!=0 && target type0` 时返回whole-attacker abort | consumer形状相似，但读错共享临时字段 |
| type3 kind9/generic/transform actual | 写 `HitConfirm2=1` | producer值相似，但载体身份/生命周期错误 |
| HitPlan | 只有 `TargetHitConfirm2` projection | 缺独立 latch shadow |
| `BattleEcsCharacterPostFrameTailPass.ApplyAuthorityMaintenance` | 每个C25 slot tail清 `HitConfirm2=0` | confirmed premature clear |
| `LF2Entity.ClearHitCandidateCarriers` | candidate collect前清 `HitConfirm2=0` | confirmed premature clear |
| ordinary weapon tails | type1/2/4等命中也写 `HitConfirm2=1` | 证明不能把现有字段整体永久化 |
| snapshot/checksum/parity | 仅有 legacy `hitConfirm2` | 缺 current-authority独立字段 |
| raw trace | 未输出 `specialHitLatch0eb` | C++已输出，joint schema缺项 |

可观察首差：type3 target 在 tick N 置 latch，Unity 最迟在其 C25/下一轮 C11 清零；tick N+1 作为
attacker面对已排队type0 candidate时，Authority在writer前终止，Unity会继续消费并产生damage/rest/status等副作用。

## 4. Required carrier contract

新 carrier 名固定为 `SpecialHitLatch0EB`：

- 类型 bool；full/birth/reuse reset=false；input-only reset保留；canonical copy保留。
- entity snapshot、full battle snapshot、restore、checksum和parity精确保留。
- raw entity projection使用 current-authority key `specialHitLatch0eb`，不得继续用`hitConfirm2`冒充。
- carrier包不改任何 producer/consumer，防止半接线；atomic behavior包再同时迁actual、HitPlan和runner。
- `HitConfirm2` 暂作为独立legacy临时字段保留，直到其普通weapon writer family按current authority独立退出。

## 5. Validation gates

- carrier red必须只因缺 `SpecialHitLatch0EB`/schema而失败；
- full reset false、input reset/copy/snapshot/restore true、checksum/parity差异可见；
- raw C++/Unity schema能同时观察 `specialHitLatch0eb`；
- atomic包必须证明跨tick保持、entity reuse清零、type0抑制、non-type0不抑制，以及旧
  `HitConfirm2` 的变化不能再控制current-authority latch gate；
- actual/HitPlan difference mask=0，随后运行B5、Unity侧NTSD28自动回归、SelfCheck、必要Play和Ledger。
