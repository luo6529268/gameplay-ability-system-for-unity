# NTSD28 B5 hit-group eligibility owner / snapshot plan

> Change：`NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNER-AUDIT-001`  
> 状态：`VERIFIED / GOVERNANCE_ONLY / FOUR_PACKAGE_SPLIT_DEFINED`

## 1. Authority owner

- `HitCandidatePairSnapshot28`是唯一candidate-time pair carrier，完整字段为：valid、双方OID/type/group/action、
  current/previous/tick state、facing，以及attacker linked-holder存在性/group。
- `BattleWorld28::rebuild_geometric_hit_candidates`在几何重叠后、direct-effect/group过滤前构造完整snapshot；
  `candidate_passes_native_group_filter`在nearest替换、tie RNG和20-slot append之前消费它。
- `classify_ordinary_hit_eligibility`只在手写candidate的`valid=false`兼容路径读取live entity；正式candidate始终复用冻结snapshot。
- `GameSession28`把selected background-mode record的`hurtable_18`投影到
  `BattleWorld28::active_mode_hit_group_gate_18_`；相同原始字段还被system resource规则作为
  `selected_mode_full_restore_gate_18`消费，但两个消费语义不能互相覆盖。

## 2. Unity production ownership

### 2.1 World scalar

`BattleRuntimeState.NativeHitResourceRules`是现有selected-mode/runtime rule scalar owner；在不引入第二个重复
world规则对象的前提下，新增`ActiveModeHitGroupGate18`：default/reset为0，restore、core scalar snapshot、
full battle restore、lockstep checksum与full parity必须一起推进。它是确定性world state，因此schema必须从
core scalar `8→9`、battle state `17→18`、checksum `20→21`。本包只建carrier；从实际background-mode
content装载非零值仍归B8/H，当前正式content缺省0。

### 2.2 Frozen pair carrier

在`ILF2SceneQuery.cs`定义allocation-free readonly value carrier `BattleHitCandidatePairSnapshot`，字段逐项对应
Authority的18个payload字段加`Valid`。正式collector创建valid snapshot；手工/即时/兼容构造默认invalid，允许
沿现有live adapter运行。

以下复制边界必须完整保留snapshot，不能只补`SceneQueryHit`：

- `SceneQueryHit`两个构造器及`QueryBodyHits`缓存复制/重建路径；
- `CollisionCandidateStoreEntry`、store-first写入、store-authority回读和legacy/store shadow equality；
- `BattleHitCandidateSequenceRunner`解析runtime ITR后重建hit的路径；
- `BattleEcsHitExecutionPlan.Entry` capture及legacy candidate observation fingerprint/equality。

candidate slab仍固定每attacker 20条，carrier只做struct value copy，不增加managed collection或每tick分配。候选
是每tick重建的ephemeral state，不进入persistent battle snapshot/checksum schema；但store shadow和HitPlan必须
比较它，防止data-oriented authority静默丢字段。

### 2.3 Eligibility pure owner

新增一个无分配pure resolver，输入为原始kind/effect、`BattleHitCandidatePairSnapshot`和world
`ActiveModeHitGroupGate18`，严格实现已冻结的七步truth table。resolver不查询GameObject、Transform、Frame、
World或content；invalid snapshot由调用侧显式构造live snapshot后调用，resolver本身不藏第二套fallback规则。

### 2.4 Producer and consumer

- brute、loose、role-aware exact/fallback都在`TryRecordReleaseCandidate`汇合；该入口是正式snapshot创建与
  pre-nearest group gate的唯一生产owner。
- formal collector的早期`ItrAllowedCore`不得继续运行旧`RunsKindGroupFilters`，否则精确resolver尚未来得及执行
  就会误拒绝state190、state180、kind-set及type0→type3 opposing-facing组合。
- immediate/direct query没有正式candidate snapshot时，使用同一个snapshot factory从live pair采样并调用同一
  resolver；不得保留旧truth table。
- `BattleHitCandidateSequenceRunner`是四种正式shell共享的消费owner：valid candidate只用冻结snapshot进行防御
  gate，不重新读取group/action/state/type/facing。两个cached `QueryBodyHits`兼容消费路径也必须传递并使用
  同一snapshot。无snapshot的直接kind6/weapon helper保留live adapter，不宣称具备同tick冻结语义。
- HitPlan只验证/carry candidate snapshot身份，并观察已经筛选的candidate；禁止在HitPlan复制另一套group真值表。

## 3. Test-first package order

1. `NTSD28-B5-WORLD-HIT-GROUP-MODE-GATE-CARRIER-001`：world field、reset/restore、snapshot/checksum/parity及
   schema；不接行为或content producer。
2. `NTSD28-B5-HIT-CANDIDATE-PAIR-SNAPSHOT-CARRIER-001`：完整readonly snapshot跨SceneQuery/store/shadow/
   runner/HitPlan的无损复制；不改变eligibility结果。
3. `NTSD28-B5-HIT-GROUP-ELIGIBILITY-PURE-CORE-001`：七步真值表、state190、18/180、mode1/3、type/facing、
   kind4/8/50与zero-allocation；不接production。
4. `NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001`：一次性移除formal早期旧group gate，
   接candidate-time snapshot resolver、nearest/capacity/RNG顺序、shared consumer/cached兼容路径与HitPlan观察。
5. 独立exit audit确认该family无残余首差后，才继续B5下一项。

任何包都不得顺带装载background content、补state190 DAT、实现full-restore resource规则，或修改B6/B7/B8/
B9/B10/H行为。

## 4. Acceptance and rollback

- 每个脚本包先写会失败的focused tests，再实现；至少覆盖所有truth-table分支、三collector、nearest equal-distance
  RNG不被rejected pair推进、20-slot容量不被污染、低slot先命中后group/action/facing变化、store-authority/
  legacy shadow、HitPlan observation及warm zero-allocation。
- 原子接线包还需B5组、完整Unity侧NTSD28组、SelfCheck和真实collision-hit Play probe；Scene必须退出Play且
  dirty/hash/root保持原状。
- 回滚按上述四个独立Change ID逆序撤回；carrier在behavior未接线时必须保持行为中性。
