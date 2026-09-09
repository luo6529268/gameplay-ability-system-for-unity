# NTSD28 B5 hit-group eligibility / frozen-pair audit

## Authority live path

- `BattleWorld28::rebuild_geometric_hit_candidates`在每个几何重叠后冻结`HitCandidatePairSnapshot28`，随后在candidate append/nearest RNG之前调用`candidate_passes_native_group_filter`。
- `BattleWorld28::classify_ordinary_hit_eligibility`在消费时复用该冻结snapshot；live entity只用于target仍存活和runtime ITR解析，不重新定义已冻结group/state/facing裁决。
- `battle_world.cpp` SHA-256 `EB37E8EC1186BF0349A3B3B5759666C3A8F44F0E1640C159E06F805189FBCCB1`；`hit_candidates.cpp` SHA-256 `4168C78A23849EAA17DD749002B89DD335841DB6CECA8B04FAE8356B10F1B905`。两者在playable closure `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`内。

## Exact truth table order

1. 原始kind 4/8/50绕过ordinary group block。
2. target current state 10/13绕过。
3. system freeze-column OID212在不同OID或同OID action phase 0→5时绕过。
4. attacker group 0放行；非零group下，state190接受same/reject different，其他state接受different并让same进入例外表。
5. rejected side若selected background-mode `hurtable_18`为1/3，或attacker current state为18/180，且effect不是21/22，则放行。
6. target type 1/2/4/6放行。
7. attacker type0、target type3且facing不同放行；否则拒绝。

## Unity current production surface

- brute、loose、role-aware exact/fallback最终都进入`BruteForceSceneQuery.ItrAllowedCore`；shared runtime consumer通过`BattleHitCandidateSequenceRunner -> RuntimeConsumeItrAllowed`再次调用同一core。
- `SceneQueryHit`只保存target slot、bodyX、itrIndex/runtimeItr和两个consume flag；没有Authority group/state/action/type/facing/holder冻结snapshot。
- 当前core只在`RunsKindGroupFilters`旧kind集合内处理same-nonzero group；读取attacker collision/Prev2 frame state，不是current action frame state。
- target state10/13、OID212 override、target type1/2/4/6主体存在；但state190反转、state180、mode gate均缺失。
- type0→type3 opposing-facing分支当前`return false`，与Authority“放行”极性相反。
- raw kind集合也不等价：Authority只显式绕过4/8/50；Unity的旧`RunsKindGroupFilters`额外绕开kind5、kind9非type0及其他kind，可能在nearest/capacity之前保留不应存在的candidate。
- consumer用实时entity重算；低slot命中若先改group/action/facing，后续candidate会与Authority frozen pair产生同tick首差。

## Content / mode observations

- 当前Authority decoded runtime有state190：39个frame occurrence、19个DAT文件；冻结Unity `Assets/NTSD/Config`为0。该内容差异归H/B11，不允许本审计覆盖Config，但logic仍必须支持state190。
- 当前Authority decoded background-mode文件没有显式`hurtable`，因此正式缺省为0；playable host仍从selected background-mode record投影到world `active_mode_hit_group_gate_18`。非零内容生产归B8/H，但carrier/truth-table不能把该分支删除或硬编码成永久0。

## First difference and required split

`NATIVE_HIT_GROUP_ELIGIBILITY_AND_FROZEN_PAIR`是special-hit latch之后下一独立B5 first difference。下一包先执行
`NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNER-AUDIT-001`，冻结：

- exact kind-set与truth-table pure core边界；
- candidate-time三collector与consumer defensive gate的唯一owner；
- `SceneQueryHit`/candidate store冻结snapshot所需最小字段、copy/reset/容量/zero-allocation边界；
- world `active_mode_hit_group_gate_18` carrier与B8/H producer的拆分；
- HitPlan只消费已筛选candidate，不复制另一套group规则。

owner audit完成前不得只翻转opposing-facing一行，也不得仅在consumer补state190；这两种局部修复都会继续污染nearest RNG/candidate capacity或同tick冻结语义。
