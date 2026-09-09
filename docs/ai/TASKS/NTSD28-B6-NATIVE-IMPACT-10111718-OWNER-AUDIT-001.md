# Task Contract — NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / RELEASE_CORPUS_WITNESS / PRODUCTION_HELD`
> 来源：`NTSD28-B6-NTSDSPEC-PRODUCTION-OWNER-INVENTORY-AUDIT-001 / VERIFIED`

## 目标

冻结 NTSD 2.8-Logan `resolve_special_relation_hit()` 中 kind 10/11/17/18 的完整 impact事务、Unity
actual/HitPlan owner、缺失carrier、正式语料边界与后续最小实施顺序。此任务只读，不修改脚本、content、
Scene、Prefab、资源或 Authority。

## Authority 调用链与顺序

- `SimulationTickDriver28::step()` 的两个hit-consume sweep对非0/4/5/50 ITR调用
  `BattleWorld28::resolve_special_relation_hit()`；处理后继续同一attacker的下一candidate，不进入ordinary
  damage/armor/defense/combo/audio/spark尾。
- consumer先执行共同special-hit latch；随后kind 10/11/17/18进入同一impact family。
- kind 11第一道gate是`target.environment_state_320 < 0`；非负时无副作用reject。它不读取weapon count。
- kind 17只接受character target；kind 18只接受non-character target。kind 10/11由后续target family裁决。

### Character target原子事务

1. preflight `attacker.owner_slot`指向active first owner；再读取first owner的`owner_slot`作为credit slot，
   credit也必须active。任何一步失败都在写入前以unsupported退出。
2. `environment_state_320 = -(impact_environment_damage_94 > 0 ? value : 20)`；正式host默认值为20。
3. `catch_source_slot_90 = 0x2000 + creditSlot`；`impact_source_slot_164 = attackerSlot`。
4. `motion.x /= 1.07`、`motion.z /= 1.07`，并把结果写入pending-hit-impulse X/Z。
5. action写`respond == 0 ? 182 : respond`，不重置frame counter。

### Object target原子事务

1. 仅type 1/4/6和type 2接受；其他non-character type无副作用reject。
2. type 1/4/6先要求正式`weapon_flute_sky`表已审计且非空；ID在表内则免疫并无副作用。正式表为
   201/202。type 2不经过该表。
3. current frame state不是type2的2000、或type1/4/6的1000时，action写0且保留frame counter；匹配时
   action保持不变。
4. 同样以除法`/1.07`阻尼X/Z并覆盖pending-hit-impulse X/Z。

### 共同Y尾

- 若integer Y `>= -2`，同时写integer/precise Y=-2、motion Y=-6；此分支不写pending Y。
- 随后若motion Y仍`> -6`，character减3.0，object减2.3，并把新motion Y写入pending-hit-impulse Y。
- 全事务不写HP/PP、weapon count、legacy kill/combo/damage stats、vrest/arest、frame delay、facing或RNG。

## Unity 当前与首差

### 分派与owner

- `BattleHitCandidateDisposition.Kind10Or11`、`LF2CharacterDatHitResolver.ResolveCandidateDisposition()`和
  HitPlan只识别10/11；17/18被标Unsupported，`NTSDItrKindService.IsAttackKind()`也遗漏17/18。
- actual分散在`LF2CharacterHitResolver`、`LF2CharacterDatHitResolver`、`LF2Weapon.Hit()`与
  `BattleDamageWriter.ApplyGenericWeaponTypedHit()`；HitPlan另有独立`ProjectKind10Or11WriterEffect()`。
- 正确production owner应是一个world/runner可调用的shared impact writer；shell-specific旧分支退休或只
  委托shared writer，不能继续存在四份规则真相。

### 已接近但仍不精确的部分

- Unity以乘`0.9345794392523364`近似native除`1.07`；数值上接近但不保证bit-exact，生产应使用同一除法。
- action182 raw-write、state1000/2000 preserve、Y=-2/-6和3.0/2.3步长的总体结构与Authority相近。
- current `KnockbackVx/Vy/Vz`是pending-hit-impulse映射，可作为目标writer；X/Z须覆盖为阻尼后motion，
  Y仅在native实际写入时覆盖。

### 明确错误/遗漏

- kind11错误读取`WeaponCount >= 0`；必须改为`EnvironmentState320 >= 0`。
- character实际错误写`WeaponCount=-20`、holder combo +11和legacy DamageStats +11；object也错误写
  WeaponCount。Authority没有这些副作用。
- character漏写`EnvironmentState320`、encoded `CatchSourceSlot90`、`ImpactSourceSlot164`，且没有双owner
  active preflight；`respond`被硬编码182。
- HitPlan writer snapshot没有target environment/catch-source/impact-source三个字段，无法观察上述原子写集。
- kind17/18完全缺失；special/type3与unsupported object必须保持无副作用，而不是落入ordinary damage。

## 正式语料与可达边界

- Direction-B当前Unity ITR：kind10/11/17/18=`0/0/0/0`，所以当前冻结内容没有直接witness。
- release ITR：kind10/11/17/18=`121/61/0/0`。182条全为`respond=0, vrest=1, arest=0,
  effect=0, dvx/dvy/dvz=0`；180条injury1、2条injury0，而impact consumer不读取injury。
- 其中Tayuya OID36与OID62在flute/xflute/xflute2帧提供180条；Pain-d OID509 frame287/kind10和
  type3 OID838 frame168/kind11各一条。Tayuya角色攻击者的self-owner链可满足character credit preflight；
  对character/weapon的真实碰撞Play仍待运行，不把DAT存在夸大为已复现。
- 正式`weapon_flute_sky`为201/202；release catalog中201无对象entry、202为type3，因此“listed
  type1/4/6 target”在锁定catalog没有直接positive witness，保留synthetic/system-table contract。
- kind17/18为通用规则但current/release均无ITR producer，只能以synthetic fixture验证，不能称正式场景可达。

## owner与三包顺序

### 1. `NTSD28-B6-NATIVE-IMPACT-HITPLAN-CARRIERS-001`

- 扩展HitPlan `WriterEffectSnapshot` capture/expected/actual/compare mask，至少加入target
  `EnvironmentState320`、`CatchSourceSlot90`、`ImpactSourceSlot164`。
- carrier包不改变disposition、actual或projection；默认/roundtrip/故意不同值/zero-allocation先验收。

### 2. `NTSD28-B6-NATIVE-IMPACT-PURE-CORE-001`

- 新增无Unity对象写入的纯decision/result，覆盖kind gate、target family、owner/table preflight、action、
  exact `/1.07` motion、Y/pending-write masks及完整no-op结果。
- owner slots/active flags、current state、ID/table membership与规则值作为显式输入；不得从class name或
  legacy WeaponCount推断。

### 3. `NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-001`

- disposition与kind service纳入17/18，并将枚举重命名为准确impact family；更新所有observer/tests。
- shared writer在shell-specific dispatch前原子preflight并写actual；旧character/weapon/generic flute分支删除
  或只委托唯一writer，`FluteForce()` dead API另由NTSDSpec retirement处理。
- HitPlan projection消费同一pure decision和新carrier；actual/DataOriented/shadow compare同时落地，不能
  分两个turn留下错误shadow基线。
- production正式固定damage20与system IDs201/202；未来H内容策略若改变system table，须另建可校验
  runtime carrier，不能在本包猜测alternate data。

## 验收矩阵

- kind10/11/17/18 × target type0/1/2/3/4/5/6；kind17 character-only、kind18 object-only。
- kind11 environment=-1/0/positive；证明WeaponCount任意值不影响结果且impact不改WeaponCount。
- direct self-owner、two-hop owner、missing first owner、missing credit、slot0/high slot、stale generation/reuse；
  preflight失败零写入。
- character damage规则positive/nonpositive fallback20、respond0/nonzero、action counter保持、encoded credit与
  physical impact source分离。
- type1/4/6 table audited/un-audited、empty、listed/unlisted；type2绕表；state1000/2000匹配/不匹配。
- X/Z正负/零/非整值bitwise除法；Yint -1/-2/-3与Vy -7/-6/-5/positive；验证pending Y write mask。
- 证明HP/PP、WeaponCount、combo/kill/damage stats、rest、delay、facing、RNG和audio/spark均不变。
- real character、generic character DAT shell、real weapon、generic type1/2/4/6、type3/other；actual与HitPlan
  compare零差；legacy/data-oriented/no-op proof一致，4096 warmed零managed allocation。
- release Tayuya/Pain-d synthetic-definition fixture，内容策略允许后再做正式Tayuya Play；compile、focused
  B6/HitPlan/NTSD28、SelfCheck和first-difference trace。Play前最多`RUNTIME_PENDING`。

## 不变量 / 阻塞

- 当前两个B6代码包尚未进入Unity Test Runner，production继续held；不重试已穷尽的许可路径。
- 不修改Direction-B DAT或复制release角色/武器内容；正式Play witness受后续H内容策略约束。
- 不改ordinary hit、armor/defense/combo、catch/held settlement、OPoint/lifecycle、render/audio或Authority。
- 本审计纠正W06 impact owner，不回退B5已验证的ordinary-hit packages，也不声称整个B6完成。

## 回滚

仅移除本治理记录及Ledger/STATE/handoff/总表增量；无脚本、content、Scene或Authority回滚。

## Current corpus correction（2026-09-08）

Direction-B kind10/11/17/18由`0/0/0/0`纠正为`15/5/0/0`；20条10/11均在OID36 Tayuya
actions243..247。三包owner不变，但10/11已经current-content reachable，Play不再依赖release DAT导入。
详见`NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001`。
