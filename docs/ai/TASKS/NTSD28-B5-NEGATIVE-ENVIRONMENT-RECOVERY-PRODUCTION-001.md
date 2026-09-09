# Task Contract — NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-PRODUCTION-001

> 状态：`VERIFIED / RED_11_OF_12 / FOCUSED_12_OF_12 / RELATED_154_OF_154 / RELATED_B5_981_OF_981 / TARGETED_PLAY_12_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SINGLE_EXACT_TRANSACTION / FLUTE_FALSE_POSITIVE_RETIRED`
> 依赖：
> - `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B5-NEGATIVE-ENVIRONMENT-CLAMP-CORRECTION-AUDIT-001 / VERIFIED`
> - `NTSD28-B5-NEGATIVE-ENVIRONMENT-RULE-CARRIER-001 / VERIFIED`

## 目标

将 legacy virtual 与 data-oriented character recovery 中两套错误的 negative-WeaponCount 分支替换为
一个共享的 exact negative-environment transaction。两条生产入口必须读取同一个 world phase/rule和
同一个slot-owner resolver，最终状态一致。

## Authority 合同

- 只处理当前 DAT type0、active/claimed且有Health/Runtime的实体；native resource per-slot loop在
  definition type0门内。
- eligibility为 `EnvironmentState320 < 0 && NativeResourcePhase12 == 0`；不读取WeaponCount，
  不使用传入tick `% 12`，不受BattleStepMode/Gate影响。
- rule为world `NegativeEnvironmentDamage90`；raw值 `>0` 时原样使用，否则fallback `9`。
- actual damage默认等于rule；`IncomingDamageScale340 > 0`时改为整数 `900 / scale`，不是rule比例，
  也不读取FallDamageDiv。
- credit从`CatchSourceSlot90`开始；值`>=0x2000`先减`0x2000`，再最多跟随两次当前physical
  `OwnerSlotIndex`。某一hop无实体时保留最后一个已解析credit；root无实体则credit为空但damage仍执行。
- lethal判断在HP mutation前；仅credit存在且`HP>0 && HP-actual<=0`时
  `credit.KnockoutCount358++`。B8 knockout event payload/feed仍独立后置。
- victim：`HP -= actual`、`HPBound -= actual/3`、`InputHpConsumedTotal34C += rule`；随后HP与HPBound
  各自clamp到0。credit：`InputScoreTotal348 += actual`。
- 不写ComboCountVic/ComboCountAtk/KillStat/world DamageStats/KillStats，不重置EnvironmentState320，
  不修改WeaponCount/FallDamageDiv/ImpactSourceSlot164。
- 同一个phase内，既有普通HP regen仍先于本transaction，PP regen仍在其后；本包不重写其他regen规则。

## Unity 原状

- `LF2Entity.RunPreCollisionRecoveryPhase` 与
  `BattleEcsCharacterRecoveryPass.ApplyAuthorityRecovery` 都以
  `WeaponCount<0 && tick%12 && !stepWait`触发，使用FallDamageDiv/旧常量，clamp后写
  `ComboCountVic += 9`。
- data-oriented pass在tick既非HP也非PP周期时直接ProvenNoOp，会漏掉phase carrier与tick不同步的
  exact negative-environment transaction。
- flute命中可正式留下`WeaponCount=-20`，因此旧false-positive在现有内容可达。

## 修改范围

- 新 `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNegativeEnvironmentRecoveryWriter.cs`：
  单一eligibility、resolver与atomic writer。
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`：legacy/derived入口调用共享writer。
- `Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs`：exact入口调用共享writer，
  no-op gate纳入native phase/environment eligibility。
- 新 focused Editor test与request/Play runner；更新`FrameAdvanceRuntimeSnapshotEditorTests`和
  `BattleRuntimeSelfCheck`中的旧negative-WeaponCount期望。
- 治理恢复入口与Ledger。

## 不变量

- 不实现B6 kind10/11/17/18 negative EnvironmentState producer；synthetic carrier可用于本包验收。
- 不实现B8 knockout event feed，不改event/schema/raw capture。
- 不改变普通HP/PP regen、step-wait对它们的既有作用或late pass顺序。
- 不修改Authority、content、Scene、Prefab、ProjectSettings、Input Actions或网络协议。
- transaction warm path不得产生managed allocation。

## 验收

1. test-first RED证明两路仍由WeaponCount触发且未执行EnvironmentState exact transaction。
2. DataOriented与Legacy/derived在phase0、非周期tick、step-wait打开时结果一致。
3. 覆盖rule positive/fallback、900/scale、root decode、两hop、missing root、lethal KO、overkill clamp。
4. 覆盖slot reuse读取当前occupant；flute `WeaponCount=-20 + EnvironmentState320=0`不得伤害。
5. exact与legacy stats、EnvironmentState、WeaponCount/FallDamageDiv/ImpactSource保持预期。
6. production source只保留一个exact transaction body，两入口不再包含旧WeaponCount/FallDamageDiv/
   ComboVic negative-recovery写入。
7. 4096次warm transaction 0 B；focused、相关recovery/B5、构建、targeted Play、SelfCheck、Console、
   Scene与Ledger按实际证据记录。

## 实际结果

- test-first RED：旧production上job `a649def072834bfe8347dd1e6354c5c9`实际执行12项，
  `11 failed / 1 passed`；失败覆盖两profile、derived、rule fallback、slot reuse、scale-zero、flute误伤、
  单一writer source gate与warm transaction，唯一通过项为positive Environment/nonzero phase inert control。
- GREEN focused：job `f3878759aa014f0397ee550fdbb3f93c`，`12/12`通过；覆盖DataOriented、Legacy、
  derived、非周期tick、step-wait、raw rule positive/fallback、`900/scale`、root decode、two-hop、missing
  root、lethal KO、overkill clamp、slot reuse、flute control、source uniqueness与4096次warm 0 B。
- related recovery：job `046d5223665e4176ab35171b67656849`，`154/154`通过。
- B5广回归：job `89df898c7f4146299a8c857c781b2aa6`，73个B5/HitPlan/FrameAdvance/LateTail
  test class共`981/981`通过。
- 编译：runtime `0 error / 47 warnings`，Editor `0 error / 104 warnings`。
- 定向真实Play：`NTSD_Battle`内12 cases全部通过，覆盖DataOriented、Legacy与derived入口；Console清理后
  `0` error，退出Play后Scene dirty为false。
- Scene不变：验收前后SHA-256均为
  `32451F1A311476C036AC7D28514FDE34AE53E06B3CC28B7263835A65569AA928`，长度`214471`，mtime
  `2026-09-09T01:26:42.8284084Z`。
- full `BattleRuntimeSelfCheck`未通过：仍在本包之前已存在、与本包无关的CPoint throw Vz断言
  （`BattleRuntimeSelfCheck.cs:10939`）停止；因此该项如实保留`SELFCHECK_BLOCKED_UNRELATED`，不扩大为
  full-B5或全战斗对齐结论。
- 本包没有实现negative EnvironmentState producer、B8 knockout event、联合schema/content，也没有修改
  Scene、Prefab、ProjectSettings或Input Actions。

## 回滚

删除共享writer与focused test/runner；将两个入口恢复为本包前实现，并恢复旧fixtures。无schema/content/
Scene迁移；回滚会重新暴露flute false-positive，必须在记录中标明。
