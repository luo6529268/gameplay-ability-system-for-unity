# NTSD28 B5 Special-Hit Latch Owner / Write-Surface Audit

## Authority live path

| 类别 | 函数 / 位置 | 精确条件与顺序 | 原子迁移义务 |
|---|---|---|---|
| consumer A | `BattleWorld28::classify_ordinary_hit_eligibility`，`battle_world.cpp:4851` | candidate/target解析后、tick frame与runtime ITR替换前；attacker latch=true且target type0则reject | shared runner必须在runtime ITR与writer前读新carrier |
| consumer B | `BattleWorld28::resolve_special_relation_hit`，`:5303` | 原始ITR确认后、kind2/3/8/9 dispatch前；同一predicate则终止candidate | 与consumer A共用同一Unity gate，不能在各writer内补门 |
| producer 1 | kind9 type3 state3005，`:5648` | attacker hold=-3后，target latch=true→action40→return | actual与HitPlan同时改新bool |
| producer 2 | kind9 type3 generic，`:5659` | group/owner/control transfer后，target latch=true，再清motion并选action | actual与HitPlan同时改新bool |
| producer 3 | kind0 locked kind transform，`:6906` | definition/identity/action-history事务内写target latch=true | actual与HitPlan同时改新bool |
| producer 4 | kind0 generic type3 continuation，`:6929` | ownership source解析并写group/owner/control后写target latch=true | actual与HitPlan同时改新bool |

Authority源码树无`special_hit_latch_0eb=false`运行时writer；false仅来自entity默认构造。

## Unity production owner matrix

| 面 | 当前唯一位置 | 下一包改法 |
|---|---|---|
| shared consumer | `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 的`attacker.HitConfirm2`门 | 改为`attacker.Runtime.SpecialHitLatch0EB`；返回true仍表示whole-attacker break |
| kind9 actual | `BattleDamageWriter.ApplySpecialAttackDamage` 两条`victim.HitConfirm2=1` | 两条改写新carrier，旧字段不写 |
| generic actual | `ApplyNativeType3TargetGenericContinuation` | 改写新carrier，旧字段不写 |
| locked actual | `ApplyNativeLockedKindTransform` | 改写新carrier，旧字段不写 |
| HitPlan state capture | `CaptureWriterEffectSnapshot` / `WriterEffectSnapshot` | 新增`TargetSpecialHitLatch0EB` bool，初值来自runtime |
| HitPlan producers | locked/generic `:3399/:3437`、kind9 `:3532`、D1 identity `:3656/:3778` | 五处改写新bool；不再改`TargetHitConfirm2` |
| HitPlan comparison | `ComputeDifferenceMask` target status bit group | 将新bool纳入可见difference，保持actual/shadow mask=0 |

HitPlan五处投影对应Authority四个producer，是因为Unity对generic type3 continuation另有两条D1 identity投影入口；
它们最终仍映射同一Authority target-type3 latch事务，不是新增第五条规则。

## Must-retain legacy surface

- `BattleDamageWriter.ApplyKind0WeaponVictimTail` 的type1/2/4三类`HitConfirm2=1`及standard object writer既有写入；
- HitPlan standard object `TargetHitConfirm2=1`；
- `BattleEcsCharacterPostFrameTailPass` 与`LF2Entity.ClearHitCandidateCarriers` 对旧字段的清零；
- 与weapon临时确认相关的既有测试。

因此下一包既不能删旧字段，也不能停止其clear；只把current-authority type3 producer与consumer身份迁到新bool。

## Test-first matrix

- 四条actual producer均：new latch false→true，旧`HitConfirm2`保持原值；
- 五条HitPlan producer投影与actual一致，difference mask=0；
- new latch=true + type0：exact/fallback/shared consumer均在writer前whole-attacker abort；
- new latch=true + non-type0：不得抑制；new latch=false + old HitConfirm2非零：不得抑制；
- C25与candidate clear后new latch仍true，下一tick仍抑制type0；full reset/reuse后false；
- ordinary weapon路径仍写并清旧`HitConfirm2`，且不得置new latch。
