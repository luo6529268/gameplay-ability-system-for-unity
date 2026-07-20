# Hit / Collision 全量静态对齐审计（2026-07-17）

## 1. 结论

本分区以 `J:\QQFile\NTSD2.4\ntsd_release_C#` 为唯一战斗逻辑权威，只审计正式 C# 源码和 Unity 当前实现。未读取或引用其他历史实现、反编译结果或伪代码。T8 默认 `stage.dat` 不在本分区范围内。

审计结果：

- 权威方法覆盖：`HitResolve.cs` 40/40，`CollisionCollect.cs` 21/21，共 61/61（100%）。
- Unity 映射覆盖：候选收集、角色消费、共享 Character-DAT 消费、SpecialAttack 消费、Weapon 消费，以及 damage/rest/hit-stop/owner-link/stats/sound/spark 直接契约均已覆盖。
- 源码级确认差异：33 簇。
- 需逐帧 trace、资产或生产可达性证实的风险：6 簇。
- 不能据此声明全量战斗已经对齐。尤其普通角色受击、对象受击和特殊 kind 分支仍会在同一输入、同 seed 下产生确定性的字段差异。

这里的“确认差异”表示两端源码在同一可达前置条件下明确写出不同结果；“风险”表示源码结构不同，但尚需证明对应数据或运行时形态能在正式资产中出现。

## 2. 审计快照

权威文件：

- `src/BattleCore/Interaction/HitResolve.cs`，SHA256 `846716FFE2A1B8EA2E2751FB614D53182AB94B1E97A57ADACDA3FACBD1D62834`
- `src/BattleCore/Interaction/CollisionCollect.cs`，SHA256 `8A82BFA998B4F0EE6BBD46A96FE226E62AD06D461CCCD413033C3E38C1322E4B`

Unity 最终复核 SHA：

- `BruteForceSceneQuery.cs`：`7690AFBC6530BE9223ECDE6FDDE06BB4B0AE43218EA47B1BF21D19C768DB3A02`
- `LF2CharacterInteractionResolver.cs`：`3201C53862F5D1D7B1B58C05F7C2265815533700C62175EB6D8D1B995D2CC2DF`
- `LF2CharacterDatInteractionResolver.cs`：`573D9754C50E119E51F7361EBB4B2C2E79F89C22DB01D93CF08F1F5F1C7C82F0`
- `LF2WeaponInteractionResolver.cs`：`A65F0D50195A7163861D786B75BED98D8893D1954330B6D27219BD7B2980359B`
- `LF2CharacterHitResolver.cs`：`D9A181DB2840FAA7C096C1E94CA4593FB74102842D99BDC19FEC133C09A9EC3B`
- `LF2CharacterDatHitResolver.cs`：`3D4B4ECD73435E43F0CF68E59AE7C02F4B537FDF4D1332343660F77A59C16EF5`
- `LF2Weapon.cs`：`41BB9C4024FBE7E7C02641A2E71458E4C3FBACD3736BBB4D78569E3A8A688A03`
- `LF2SpecialAttack.cs`：`5C66CDEDB5145BAFAEEB6516CADFE2522920C7F8C544A53A5F564C6839534363`

审计期间 `LF2CharacterHitResolver.cs` 曾被并发修改，因此所有结论在报告落盘前按上述最终 SHA 重新读取关键行；后续若 SHA 变化，应重新执行第 7 节场景。

## 3. 权威调用链与不变量

权威顺序为：

1. `GameTick.Run` 先冻结 `PrevFrame2`。
2. `CollisionCollect.CollectCandidates` 按固定槽 `i < j` 递减双向 `VRest`，再按 `i->j`、`j->i` 收集。
3. `HitResolve.ResolveLoop1` 只消费当前 Character DAT 攻击者。
4. 自然/F8 武器掉落位于两轮命中之间。
5. `HitResolve.ResolveLoop2` 消费非 Character DAT 攻击者。

候选必须保存 `victimSlot + itrIndex`，消费时按当前槽 occupant 重新解析；`PrevFrame2` 的 itr 表固定到本 tick。Unity 新增的 `SceneQueryHit.TargetSlot` 和四条 consumer 的 `ResolveCurrentTarget` 满足这条槽位语义。

权威普通 kind0 Character 伤害的核心顺序为：击杀统计判定 -> `HP -= injury` -> `HpMax -= injury/3` -> combo/damage stats -> fall/hit count/frame/knockback -> sound -> hit state/delay/rest -> caught hurt frame -> attacker state tails -> `ARest` -> spark。任何把表现层或 CLR 子类当作分支真值的实现都不能替代这个顺序。

## 4. 确认差异（33 簇）

### C-01 HitConfirm2 清零时点提前

- 权威：`CollisionCollect.CollectCandidates` 14-23 调用 `Entity.ResetHitCandidates`；`Entity.ClearHitCandidateCarriers` 在收集开始时才把 `HitConfirm2=0`。
- Unity：`SimulationWorld.Passes.partial.cs` 896-902 在上一 tick 的 `EntityPostFrameTail` 清零；`BruteForceSceneQuery.CollectCollisionCandidates` 236-253 只清候选计数/距离。
- 前置条件：上一 tick 命中把 `HitConfirm2=1`，下一 tick frame advance 或其前置 pass 读取该字段。
- 差异：权威在下一 tick 碰撞收集前仍保留 1；Unity 从上一 tick 末尾起已经为 0。即使当前 Unity 暂无生产 reader，schema v2 trace 也会在 tick 边界不同。

### C-02 共享 Character-DAT 攻击者遗漏 kind1/2/7

- 权威：`HitResolve.ApplyCandidate` 104-113 对所有攻击者按 kind1/3/2/7 分派。
- Unity：`LF2CharacterDatInteractionResolver.DispatchInteractionByKind` 95-113 只显式处理 kind3，其余只进入 `IsAttackKind`；`NTSDItrKindService` 把 1/2/7 归为 pre-interaction。
- 前置条件：CLR 对象不是 `LF2Character`，但当前 DAT `type=0`，当前 itr kind 为 1、2 或 7。
- 差异：权威抓取/拾取；Unity 无操作。身份替换后的 shared Character-DAT 对象可进入此路径。

### C-03 SpecialAttack 攻击者遗漏全部 pre-interaction kind

- 权威：`HitResolve.ApplyCandidate` 对非 Character DAT 的 loop2 同样处理 kind1/2/3/7。
- Unity：`LF2SpecialAttack.DispatchInteractionByKind` 366-385 只处理 kind8 和 `IsAttackKind`，没有 1/2/3/7。
- 前置条件：SpecialAttack DAT 带可相交的 kind1/2/3/7。
- 差异：权威执行抓取/拾取；Unity 丢弃候选。

### C-04 SpecialAttack kind8 错把 CLR 类型当作 Character-DAT 真值

- 权威：`CollisionCollect` 96-101 及 `HitResolve.ApplyKind8` 619-625 只要求 victim DAT `ObjType=Character`。
- Unity：`LF2SpecialAttack.DispatchInteractionByKind` 368-372 要求 `target is LF2Character`。
- 前置条件：目标是 shared Character-DAT（当前 DAT type0，但 CLR 不是 `LF2Character`）。
- 差异：权威写 heal timer、攻击者 frame/x/z；Unity拒绝。

### C-05 oid300 对后续候选的中止条件过宽

- 权威：`ApplyOid300SpecialHit` 1294-1314 只有 future/current bdy 存在且当前 `bdy.x>1000` 时才置 `AbortRemainingHitPairs`；否则只结束当前 candidate。
- Unity：四条 consumer 的 `ShouldAbortAfterSuccessful...`（Character 132-137、CharacterDat 127-130、Weapon 103-108、SpecialAttack 357-363）只要 kind0 且 oid300 就立即 return。
- 前置条件：oid300 candidate 命中，但专属 bdy 跳转前置条件不成立，且攻击者还有后续 candidate。
- 差异：权威继续消费；Unity丢弃同攻击者剩余候选。

### C-06 Character 普通受击额外加入三组状态免疫/改写

- 权威：kind0/9 最终统一进入 `ApplyDamageCandidate`；没有“state14 全免疫”或“victim state19 + attacker state3000 全免疫”。
- Unity：`LF2CharacterHitResolver.ResolveHit` 76-124 与 `LF2CharacterDatHitResolver.ResolveHit` 525-564 对 being-caught、lying、firen-specific 增加独立路径/early return。
- 前置条件：候选已经由权威 collect 接受，victim 当前 state 为 10、14 或 19。
- 差异：HP、fall、frame、rest、统计均可能被 Unity 跳过或按另一公式写入。

### C-07 未满足 WeaponCount 的 kind4 被 Unity 当普通伤害

- 权威：`PreprocessCandidate` 162-170 仅 `kind4 && WeaponCount>0` 转 kind0；否则 `ApplyCandidate` switch 没有 kind4，故无结算。
- Unity：两个 Character hit resolver 的主伤害条件显式包含 kind4（actual 141-143；shared 565），并直接结算。
- 前置条件：kind4 candidate，attacker `WeaponCount<=0`。
- 差异：权威无伤害；Unity扣血/硬直/rest/spark。

### C-08 actual Character 额外实现 itr.kind 5000/6000 段

- 权威：`ApplyCandidate` switch 对 5000..6999 无分支。
- Unity：`LF2CharacterHitResolver.ResolveHit` 126-139 将 kind5000 段直接扣 HP、kind6000 段直接改 frame；shared resolver又没有同等分支。
- 前置条件：authoring 产生这些 itr.kind。
- 差异：权威无操作；Unity actual Character 改生命/帧，且 actual/shared 彼此不一致。

### C-09 普通伤害错误按“重武器攻击者”减半

- 权威：`ApplyDamageCandidate` 260-508 不按 attacker type2 对 injury/fall/dvx/dvy 减半；只在 victim 是 IronBall 时于 `PreprocessCandidate` 235-239 将 dvx/dvy 除 2。
- Unity：actual 171-178 与 shared 591-598 在 attacker 是重武器时把 dvx/dvy、injury、fall 全部减半。
- 前置条件：重武器作为 attacker，victim 为 Character DAT，走标准而非 alternate hurt。
- 差异：伤害、fall 档位、击飞速度和统计值均不同。

### C-10 actual Character 的普通 injury 数据契约整体不同

- 权威：`ApplyDamageCandidate` 342-358 直接使用 `itr.Injury`，`HpMax -= injury/3`，不回 PP。
- Unity：`LF2CharacterHitResolver` 183-184 先做 `injury*100/Health.MaxMP`；`LF2LivingObject.Injury` 457-463 用 `CeilToInt(injury/3)` 降 `HPBound`；`LF2Character.Injury` 492-497 又回 `PP += injury/3`。
- 前置条件：普通 actual Character 受伤。角色初始化把 `Health.MaxMP=maxMp`，通常不是 100。
- 差异：HP、HPBound、PP 三个域同 tick 分叉；shared Character-DAT 又使用另一套数值。

### C-11 普通 kind0 Character 伤害统计副作用缺失

- 权威：326-358 在致死/伤害时写 holder `KillStat/ComboCountAtk`、victim `ComboCountVic`、world `KillStats/DamageStats`。
- Unity：actual/shared 标准路径只在 alternate hurt 和 kind16 写部分统计；普通 `ApplyHitInjury/Injury` 不写上述统计。
- 前置条件：标准 kind0（非 alternate）命中 Character DAT。
- 差异：计分、连击和结果统计不增长；致死归属也丢失。

### C-12 HitStateCount / HitCount / fall 反应契约不同

- 权威：363-368 先按类型增 `HitCount`、累加 fall；444 最终把 `HitStateCount=45`，标准路径不额外加 `bdefend`。
- Unity：actual/shared 先 `SetHitStateCount(45)`，非击飞再 `AddHitStateCount(bdefend)`；HitCount 依赖 dvx/HitFallDown，且 force 条件使用当前 state/`itr.fall==100`，不是权威 `PrevFrame state13`、`PrevFrame2 state12` 和对象类型规则。
- 前置条件：普通 Character hit，尤其 `dvx=0`、`bdefend!=0`、帧在 falling/frozen 过渡附近。
- 差异：hit counters、受伤帧和后续 alternate-hurt 门控发生漂移。

### C-13 标准 knockback X 与倒地帧的计算顺序不同

- 权威：`ApplyStandardDamageKnockbackX` 999-1067 先按 attacker state2000、victim velocity、effect22/23 和 flying type求最终 X，再以最终 `KnockbackVx` 选 180/186。
- Unity：actual 只按 attacker facing 应用 dvx；shared 虽然后补 `ResolveStandardDamageKnockbackX`，但 `HitFallDown` 已先用旧/基础 X 选过倒地帧。
- 前置条件：dvx=0 的固定推力、attacker state2000、effect22/23，或同 tick 已有 KnockbackVx。
- 差异：X 速度、倒地方向帧、后续反弹均可能不同。

### C-14 actual Character 的垂直越界钳制符号错误

- 权威：420-421 在 `(int)(KnockbackVy + YInt)>0` 时写 `KnockbackVy=+12.0`。
- Unity：`LF2CharacterHitResolver.HitFallDown` 当前实现用拆分 cast，并写 `-12.0f`；shared resolver写 `+12.0f`。
- 前置条件：击飞 dvy 使 y+knockbackVy 越过 0。
- 差异：actual Character 的垂直方向与权威、shared 都相反。

### C-15 被抓目标的 hurt-frame 尾处理不等价

- 权威：`ApplyCaughtVictimHurtFrame` 1197-1218 在普通伤害完成后，基于 `PrevFrame2.cpoint kind2`、active catcher pair 和双方 facing 选择 front/back hurt act。
- Unity：actual/shared 在入口 state10 提前按 `cpoint.hurtable` 分支，用 `Abs(injury)`、可能调用 HitFall，并以 attackerPos 推导 front；不走权威统一伤害后尾处理。
- 前置条件：被抓目标仍可成为 collision candidate。
- 差异：是否扣血、扣多少、fall、选择的 hurt frame 和 rest 顺序不同。

### C-16 负 link 攻击者向 holder 传播了错误的 FrameDelay

- 权威：457-465 / 957-965 在 `attacker.LinkState<0` 时令 holder `FrameDelay = attacker.FrameDelay`。
- Unity：actual 333-337、shared 756-760 以 `GrabbedBy<0` 找 parent，却写入 `victim.FrameDelay`（通常 -3），不是 attacker delay（通常 +3）。
- 前置条件：被 holder 链接的攻击对象命中。
- 差异：holder hit-stop/动作冻结方向与持续时间错误。

### C-17 标准命中遗漏 world.ARest 写入

- 权威：483 / 977 将 `world.ARest[attackerSlot]=itrArest`，同时另写 attacker residual `AttackExempt`。
- Unity：hit resolver 只写 `attacker.AttackExempt`；`ItrRest.Arest` 没有在标准 candidate hit 尾部同步。
- 前置条件：任意标准 kind0 命中。
- 差异：arest domain trace 不等；后续使用 `ItrArestTest` 的路径也可能比权威提前放行。

### C-18 Character 标准伤害额外执行冰火/掉武器后处理

- 权威：Character 标准 damage 分支不按 effect20/21/22/23/3/30 强制切 203/200 或掉持有武器；这些 effect frame 尾处理在 type3 object 的 `ApplyKind0Type3Tail`。
- Unity：actual/shared 的 `HitPostEffect` 对 Character 执行上述切帧，actual 还会 `DropWeapon`。
- 前置条件：Character victim 的 accepted kind0 带相应 effect。
- 差异：frame、held link、对象生成/掉落链不同。

### C-19 kind8 未完整写 frame 与整数位置

- 权威：619-625 无条件 `attacker.Frame=itr.Dvx`，同时写 X/Z 与 XInt/ZInt。
- Unity：actual 仅 `itr.dvx>0` 才 `ImmediateFrame`，写 PS.x/z；shared 写 Runtime.X/Z 但未同步 XInt/ZInt。
- 前置条件：kind8 命中，尤其 dvx=0 或同 tick 后续读取整数位置。
- 差异：frame 与 collision/snapshot position 同 tick 不一致。

### C-20 kind10/11 Character 统计和伤害逻辑不同

- 权威：1758-1788 对 type0 victim 写 `WeaponCount=-20`；每 12 tick（且非 step gate）给 holder `ComboCountAtk+=11`；无条件 `DamageStats[Unk344]+=11`；不扣 HP。
- Unity：actual 不写这两项统计，并在 victim state12 时额外 `injury=itr.injury*2` 进入普通扣血；shared 写周期 combo 但漏 `DamageStats+=11`。
- 前置条件：kind10/11 命中 Character DAT。
- 差异：HP、combo、damage stats 分叉。

### C-21 kind16 漏 world KillStats / DamageStats

- 权威：1644-1676 同时写 entity 统计和 world `KillStats/DamageStats`。
- Unity：actual/shared 只写 holder/victim 的 `KillStat/ComboCount`，不写 world arrays。
- 前置条件：kind16 命中 Character DAT，伤害或致死。
- 差异：结果统计和 checksum 不一致。

### C-22 kind16 释放重武器的 VRest 下标错误

- 权威：1693-1696 写 `[attackerSlot,targetHeldSlot]=45`、`[victimSlot,targetHeldSlot]=30`。
- Unity：两个 `ReleaseHeldTargetOnKind16` 都写 victim row `[victimSlot,attackerSlot]=45` 和 `[victimSlot,targetHeldSlot]=30`。
- 前置条件：kind16 命中 `LinkState=2` 且子物体为 `-2` 的 victim。
- 差异：第一项 rest 的 row/column 都与权威不同。

### C-23 held Weapon victim 被 Unity 全局免疫

- 权威：没有“victim 有 holder 则 Hit=false”的全局 gate；是否可命中由 candidate/team/kind/filter 决定。
- Unity：`LF2Weapon.Hit` 369-377 只要 `GetRuntimeHolderEntity()!=null` 就拒绝。
- 前置条件：敌方 itr 与 held weapon 的 bdy 相交且通过 collect。
- 差异：权威结算 weapon hurt/link/rest；Unity不消费伤害。

### C-24 Weapon 额外按 oid/state 拒绝普通命中

- 权威：进入 `ApplyDamageCandidate` 的 weapon victim 不因 oid201/202 或 heavy 当前非 2000/2004 而跳过。
- Unity：`LF2Weapon.Hit` 413-421 对 light oid201/202 拒绝，对 heavy 只接受 state2000/2004。
- 前置条件：这些 weapon 有有效 bdy 并被 accepted candidate 命中。
- 差异：权威有 durability/fall/rest/tail；Unity返回 false。

### C-25 未转换的 kind9 被 Weapon 当普通命中

- 权威：kind9 只有 victim Character、type3，或 victim state1002/2000 的预处理/专属效果；其他 weapon state 不进入 kind0 object tail。
- Unity：weapon consumer 将 kind9 交给 `LF2Weapon.Hit`，其在特殊 kind 分支后落入普通 `ApplyHitEffects`。
- 前置条件：kind9 命中非 type3 weapon，victim state 不是1002/2000。
- 差异：Unity错误扣 durability、写 frame/rest/spark。

### C-26 IronBall 低 fall 击飞错误加入垂直冲量

- 权威：`ApplyObjectHurtTail` 914-920 对 IronBall 仅 `itr.Fall>40` 才叠 dvy/default -7。
- Unity：`LF2Weapon.ApplyHitEffects` 552-556 对 heavy knockback 无条件叠 dvy/-7。
- 前置条件：IronBall victim，`fall<=40`；它仍因对象类型规则被强制 fall80。
- 差异：Y knockback 不同。

### C-27 Weapon 的 vrest/arest 矩阵和值不等价

- 权威：通用 object tail 先写 victim row authored `itr.Vrest`；type2 另写 holder/self 3或19；type4/6 另写 attacker self row30；attacker residual 与 `world.ARest` 使用 resolved arest。
- Unity：type2 漏 victim authored vrest；type4/6 把 victim row固定成30且漏 attacker self30；attacker `AttackExempt` 使用 raw `itr.arest`，也不写 `ItrRest.Arest`。
- 前置条件：weapon victim kind0，尤其 type2/4/6 或 `arest<4,vrest=0`。
- 差异：多项 rest domain 首 tick即不同，直接影响重复命中时长。

### C-28 Weapon 命中声音链不完整

- 权威：type1/2/4 标准 object 路径先 `RecordDamageEffectSound`，再由 `RecordStandardHurtSounds` 记录 weapon-hit sound；type6 reaction-only 不记录 lead effect sound。
- Unity：`LF2Weapon.ApplyHitEffects` 只 `PlaySound(WeaponHitSound)`，没有 type1/2/4 的 damage-effect cue，也没有攻击者 type3 broken-sound 侧效应。
- 前置条件：相应 weapon victim kind0。
- 差异：sound domain 数量、顺序、worldX 不同。

### C-29 Weapon 额外消费 encoded effect 5000/6000

- 权威：encoded effect 的 PP/frame 尾处理只在 `ApplyKind0Type3Tail`（victim type3）。
- Unity：`LF2Weapon.ApplyHitEffects` 593-597 对任何 weapon victim 调 `ApplyCommonEncodedHitEffectRange`。
- 前置条件：weapon victim 的 kind0 itr effect 位于5000..6999。
- 差异：Unity额外扣 weapon PP 或切 frame。

### C-30 SpecialAttack kind0 跳过整个通用 object-hurt tail

- 权威：非 Character victim 先执行 `ApplyObjectHurtTail`（hit/fall/knockback/sound/hitstate/delay/rest/attacker tails/ARest），之后 kind0 才执行 `ApplyKind0Type3Tail`。
- Unity：`LF2SpecialAttack.Hit` 484-488 直接调用 `ApplyKind0Type3Tail`。
- 前置条件：type3 victim 被 kind0 命中。
- 差异：上述通用字段和 sound 全部漏写；只剩 type3 identity/frame/effect 尾处理。

### C-31 SpecialAttack kind9 的 identity、motion 和 sound 写入不同

- 权威：269-303 记录 damage-effect cue和 victim broken sound；非 state3005 时复制 relation/holder、清 Attacking/Knockback/V、写 `AnimCounter=attackerSlot`，不替换 victim CharData。
- Unity：`LF2SpecialAttack.Hit` 449-475 会在 attacker 也是 SpecialAttack 时 `FrameCache.Load(attacker wrapper)`，却未完整清 Attacking/Knockback、未写 AnimCounter，也漏 damage-effect cue。
- 前置条件：kind9 命中 type3 victim，尤其非 state3005。
- 差异：对象身份、下一帧数据、速度、关系归属和 sound 都可能不同。

### C-32 SpecialAttack kind14 实现成 wait 修改而不是方向阻挡

- 权威：`ApplyKind14` 512-538 对任意 victim 按相对位置与 V/KnockbackV 设置四个 block flag。
- Unity：`LF2SpecialAttack.Hit` 478-482 仅 state3000 时 `Trans.SetWait(0,20)`，不写方向 block。
- 前置条件：kind14 命中 type3 victim。
- 差异：移动边界与 frame wait 均不同。

### C-33 actual/shared Character 两套标准 hit resolver 本身不等价

- 权威：是否为 Character 只由当前 `CharData.ObjType==0` 决定，所有 type0 victim 共用同一 `ApplyDamageCandidate`。
- Unity：actual 走 `LF2CharacterHitResolver`，identity-swap 后的 shared type0 走 `LF2CharacterDatHitResolver`；两者在 MaxMP 缩放、kind5000/6000、kind10/11、垂直 clamp、stats 与 `Injury` PP 副作用上有不同结果。
- 前置条件：构造相同 runtime snapshot，仅改变 CLR wrapper 为 actual/shared Character-DAT。
- 差异：Unity 的框架类型进入逻辑真值，违反权威当前 DAT 决策边界。

## 5. 待证实风险（6 簇）

### R-01 非正尺寸 itr/bdy 的 collect 语义

权威 union 和逐 bdy 循环不显式过滤 `w<=0/h<=0`；Unity `IsReleaseItrGeometry/IsReleaseBody` 会过滤。需要对 137 个 resolved frame table 统计非正尺寸项目，若存在且可达则升级为确认差异。

### R-02 Unity-only transition-smoke pair gate

`BruteForceSceneQuery.CandidateCollectionPairAllowed` 会用 `IsPureTransitionSmoke` 过滤特定 oid999/state3005/semantic；权威只看 Active/CharData/itr/bdy。当前 broken_weapon 多数展示帧无有效 body，看起来可能是冗余 gate；需要数据审计确认所有可达帧。

### R-03 当前 DAT type/oid 解析来源可能漂移

权威始终读 `entity.CharData.ObjType/Oid`；Unity 多处混用 `ObjectId`、`FrameCache.Wrapper.characterId`、`GameDataManager.GetObjectById` 和 `ReleaseEntityType`。身份替换或缺 ObjectDefinition 时可能把同一 candidate 分到不同 loop/kind/type tail。需用 identity-swap scenario 对拍。

### R-04 pickup 依赖 CLR `LF2WeaponBase`

权威按当前 DAT type1/2/4/6 直接建 link；Unity Character pickup 最终要求 `target as LF2WeaponBase`。若生产可出现“非 weapon CLR wrapper + 当前 weapon DAT”，则会漏拾取。现有明确 identity swap 主要指向 Character DAT，尚未证明 weapon DAT swap 可达。

### R-05 SceneQueryHit 的负槽 fallback

`ResolveCurrentTarget` 在 `TargetSlot<0` 时回退旧对象引用；权威所有 active entity 都在400固定槽。正常注册对象不应出现负槽，但需断言 candidate collect 时 target slot 永不为负，否则 RISK-4 修复仍有旁路。

### R-06 float/整数位置源混用

权威 collision 和 kind14/kind8 全部读写 XInt/YInt/ZInt 与 runtime double 的明确组合；Unity collision 大多读 Runtime Int，但 actual Character 的 kind8/kind14 和部分 grab 使用 PS float/dir。除 C-19 已确认项外，还需 trace 覆盖负小数、0附近和同 tick未同步快照，确认 cast/round 不产生额外首差。

## 6. 已确认等价或已正确修复的部分

以下静态映射在最终读取快照中与权威一致；仍需 trace 证明组合行为：

- 固定槽顺序：`GetAllEntities` 最终按 runtime slot 排序；pair 顺序为 `i<j` 后双向 collect。
- collect frame source：active frame 做存在性/kind8 lead-in，`PrevFrame2` 做 itr/bdy geometry，`Prev` 做 kind0 effect filters。
- pair gates：attacker AttackExempt、victim row vrest、oid205/9/301 特例。
- kind group/team/effect filters、zwidth 默认15、严格矩形相交、full-height body、type3 collision-Z offset。
- candidate 16项上限、nearest path、bodyX>=1000 gate、kind1 最近目标 tie RNG、kind2/7 jump edge。
- candidate 槽位快照：四条 consumer 已用 `TargetSlot` 重解析当前 occupant，不再直接消费旧 Target 引用。
- preprocessing：kind4（WeaponCount>0）转换与 dvx 反向、kind5 holder itr 字段替换、IronBall dvx/dvy 除2、kind9转换/attacker HP0、kind0 重武器 link2/-2 释放。
- alternate hurt 的 oid37/6/52/defend gate、`injury/10`、FallDamageDiv、统计、arest/vrest clamp、state1002/state2000/state3000 尾处理总体与权威一致。
- kind1/kind3 在 actual Character 且目标为 actual Character 时的朝向、frame、300 tick、fall reset 和对位公式。
- kind6 hit-confirm、kind15 对 Character/weapon 的移动、type3 kind0 identity/effect 尾处理主体、spark owner与两次 RNG顺序。

## 7. 对拍场景优先级

建议把确认差异直接变成最小 headless scenario，每个 scenario 只制造一个首差：

1. `standard-character-hit`：injury=31、fall=20、bdefend=7、arest=0、vrest=0，检查 HP/HPBound/PP、HitStateCount、HitCount、Kill/Combo/DamageStats、ARest。
2. `standard-character-wrapper-parity`：同一 snapshot 分别用 actual/shared Character-DAT，要求逐字段完全相同。
3. `held-projectile-hit`：attacker LinkState=-1，检查 holder FrameDelay 应等于 attacker FrameDelay。
4. `kind10-11`：tick=12 与 tick=13、step gate on/off，检查 WeaponCount、ComboCountAtk、DamageStats、HP不变。
5. `kind16-held-heavy`：检查两项 VRest 的精确 row/column，以及 world kill/damage stats。
6. `kind8-zero-frame`：dvx=0，目标坐标含小数，检查 frame、X/Z、XInt/ZInt。
7. `oid300-no-redirect`：bdy前置不成立且后面还有 candidate，检查第二 candidate仍被消费。
8. `weapon-type-matrix`：type1/2/4/6 各跑 kind0，检查 durability、dvy、authored vrest、extra30、自 arest、sound 顺序。
9. `special-kind0`：type3 victim 检查通用 object-hurt tail 后再进入 identity/effect tail。
10. `special-kind9`：state3005/非3005各一组，检查 CharData不被替换、AnimCounter和完整 motion/sound。
11. `caught-state-hit`：PrevFrame2 cpoint kind2、hurtable 0/1，检查统一伤害后 hurt frame。
12. `hitconfirm2-lifetime`：上一 tick weapon hit，下一 tick collect 前后各采样一次。

每个 scenario 必须比较 400 slot canonical snapshot、world stats、ARest/VRest、RNG call count 和 sound queue；只比较最终 HP 不足以定位这些差异。

## 8. 最终复核

已执行：

- 已重新计算所有 Unity 证据文件 SHA；与最终重读快照一致。
- 已用 `rg` 复核33个差异所涉及的关键赋值仍存在。
- `git diff --check -- .omc/research/hit-collision-audit-20260717.md` 通过。
- 本报告是只读静态审计，未改生产代码；它不代表编译、自检、Play Mode 或双端 trace 已通过。
