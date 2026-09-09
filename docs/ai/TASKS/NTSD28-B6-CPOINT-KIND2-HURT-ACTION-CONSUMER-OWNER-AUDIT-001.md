# Task Contract — NTSD28-B6-CPOINT-KIND2-HURT-ACTION-CONSUMER-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / UNITY_ONLY_HURT_OVERRIDE_CONFIRMED / ATOMIC_ACTUAL_SHADOW_LEGACY_RETIREMENT_DEFINED / CURRENT_CORPUS_WITNESS / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-CPOINT-27-SCALAR-SCHEMA-OWNER-AUDIT-001 / VERIFIED`

## 目标

穷尽 Unity 将 kind-2 CPoint `fronthurtact/backhurtact`（经旧 alias 后的 `Injury/Cover`）用于普通命中后
action override 的 actual、HitPlan、legacy 与测试调用，核对当前 B1E13AE1 playable closure 是否存在等价
consumer，冻结 production reachability、当前内容 witness 与最小原子退休包。

只读 Authority、Unity 与两端 DAT；不修改脚本、Config、Scene、Prefab、资源、ProjectSettings、package、
Server 或 Authority。

## Authority absence 证据

- playable 82-file closure 中 `CatchPointRecord28` 只由 decoder 和 `battle_world.cpp` 使用。
- `fronthurtact/backhurtact` 只在 `CombatRecordDecoder28::catch_point()` 中分别写
  `front_hurt_action/back_hurt_action`；整个 simulation/render/playable closure 没有对这两个 CPoint字段的读取。
- `first_catch_point()` 的 battle consumers只包括：type3 frame-HP drain 的 kind2 bypass、catch relation
  geometry、advance 与 settlement。命中/伤害 writer 不读取 target CPoint front/back，也不在普通命中尾按它覆盖
  target action。
- Authority 的普通命中 action、fall、effect/post-action 继续由 ITR、state、armor 与 system tables裁决；本审计
  不删除或改变这些真实 writer。

因此旧 `fronthurtact/backhurtact -> caught victim hurt action` 是 Unity/NTSD2.4历史行为，不是当前
NTSD 2.8-Logan battle rule。

## Unity production consumer inventory

### Shared helper

`LF2HitResolveRuntimeData.ApplyCaughtVictimHurtFrame()`：

- 跳过 fall 80；读取 target `PrevFrame2`；要求首 CPoint kind2；
- 以 compat `CatcherSlotIndex/CaughtSlotIndex`验证关系；
- `ResolveCaughtVictimHurtAction()` 按 target 与 physical attacker朝向差选择 `cpoint.Injury`或
  `cpoint.Cover`，非零时 raw-write target action。

由于 converter把front/back别名进Injury/Cover，这正是旧 hurt-action writer。

### Actual / shadow / legacy 调用

- `BattleDamageWriter.ApplyCharacterDamage()` 的默认 data-oriented ordinary-unarmored tail：standard rest后调用。
- `BattleDamageWriter.ApplyWeaponDamage()` 的 object/weapon damage tail：standard rest后调用。
- `BattleEcsHitExecutionPlan.TryResolveCaughtVictimHurtFrame()`：在非knockback projection中写
  `TargetFrame/TargetRuntimeFrame`，使shadow与错误actual一致而不能发现差异。
- `LF2CharacterDatHitResolver` legacy/direct path：standard damage且non-knockdown时调用。
- `LF2CharacterHitResolver` 仍构造但当前 `LF2Character` 没有调用其实例；其中同类调用/inline fallback仍是会被
  未来误接回的 dead compatibility code，退休包必须处理或加architecture guard。
- `CPointResolvedHurtActionAlignmentEditorTests` 的5个测试明确冻结 old source-order alias和hurt action选择，
  不能继续作为当前 Authority绿灯。

默认 DataOrientedCanonical 与正式可配置的 LegacyCanonical 都有实际调用面；这不是只删 dead helper即可关闭。

## 内容 witness 与可达边界

- Direction-B current：33个CPoint blocks中4条front/back均为
  `Character/naruto_clone.dat` kind2：action55=`55/55`、130=`132/131`、131=`132/132`、
  132=`131/131`。所有4条都没有explicit injury/cover。
- current唯一kind3 ITR在OID300 `chars/criminal.dat`，`catchingact=341/caughtact=130`；42个indexed
  type0 definitions中只有OID33 Naruto clone action130是初始kind2，因此存在一个正式当前静态pair。
- 该pair在关系存续、target `PrevFrame2`仍指kind2、fall非80且第三方普通非击飞命中时，Unity会在标准命中尾按
  朝向额外写132或131。选择无后续effect action override的ITR即可保留差异到tick末；精确角色/输入/命中Play
  尚`RUNTIME_WITNESS_PENDING`。
- release：398条front/back records全部kind2且同block无injury/cover，覆盖大量type0 definitions；完整场景矩阵
  等内容策略与后续B12。

静态 current pair证明content字段与relation组合存在，不等于第三方命中已实际复现；报告必须保留这一区分。

## 后续原子 production 包

`NTSD28-B6-CPOINT-KIND2-HURT-ACTION-RETIREMENT-PRODUCTION-001`：

1. 先把 current OID300→OID33 relation + third-attacker ordinary/nonknockdown fixture写成 Authority预期RED：
   target action只能来自现有ordinary hit链，不能被kind2 front/back覆盖。
2. 同一包删除默认 `BattleDamageWriter` 两处旧helper调用和 HitPlan projection，使 actual/shadow同时变化；禁止只
   改actual让shadow继续投影旧结果。
3. 同一包删除 `LF2CharacterDatHitResolver` legacy调用与 `LF2CharacterHitResolver` dead fallback，确保
   DataOrientedCanonical/LegacyCanonical和未来误接路径一致。
4. 删除无调用的 `ApplyCaughtVictimHurtFrame/ResolveCaughtVictimHurtAction` helper，或仅在仍有编译级compat caller时
   改为明确unsupported seam并加零production-call architecture guard；不得改读 `FrontHurtAct/BackHurtAct`。
5. 将 `CPointResolvedHurtActionAlignmentEditorTests` 改为 parser/schema 独立性测试或由后续27-scalar schema测试
   supersede；旧alias action断言必须消失，但本包暂不改变Direction-B normalized v1 projection。

## 验收矩阵

- default actual与HitPlan：same/opposite facing、fall0/79/80、knockback/nonknockback、valid/missing/mismatched
  relation、slot0/extended high、effect override present/absent。
- LegacyCanonical与direct `LF2Character.Hit()`：同一pair不产生CPoint-derived action。
- current Naruto clone action55/130/131/132全部作为content guard；release 398条只读 inventory不导入Config。
- ordinary ITR action/fall/effect、kind3 relation、settlement vaction、candidate skip、standard rest、damage、RNG与
  audio不变；actual/HitPlan diff mask只减少旧hurt override。
- compile、focused、HitPlan、B5/B6/NTSD28、BattleRuntimeSelfCheck、OID300/OID33 third-hit Play、same-tick trace、
  4096 warmed零managed allocation、Scene unchanged。

## 顺序 / 阻塞

- 该 retirement 是27-scalar schema migration的前置，否则修正converter alias会同时暗改四条production路径。
- exact relation producer/lifecycle consumer迁移仍按既有B6顺序；本包不声称解决stale relation或slot ABA。
- 当前throw/refill等B6代码仍无Unity runtime绿灯，避免继续叠加production；本包保持held。
- H内容策略未定不阻止current synthetic/Direction-B fixture设计，但release content Play仍后置。

## 不变量

- 不改 CPoint relation/advance/settlement、held injury、throw、resource、parser alias、19/27 schema、DAT、Scene、
  package/Server、candidate order或Authority。
- 不恢复NTSD2.4/C# authority，不把旧Server formal contract当当前battle rule。

## 回滚

仅移除本治理记录与摘要；没有代码、content、Scene、package或Authority回滚。

## Current corpus correction（2026-09-08）

current front/back kind2 rows由4纠正为170；“current唯一kind3/OID300”也由249条kind3 broader corpus取代。
Authority无hurt-action consumer与atomic retirement owner不变；focused/Play不得只覆盖OID300/OID33。
详见`NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001`。
