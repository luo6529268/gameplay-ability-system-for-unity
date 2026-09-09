# Task Contract — NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / GLOBAL_RETIREMENT_BLOCKED / FOUR_PREREQUISITE_ROUTES`
> 依赖：`NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001 / VERIFIED`、
> `NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001 / VERIFIED`。

## 目标

在启动`NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-001`前，逐路径确认旧
`ComboCountVic/ComboCountAtk/KillStat/DamageStats/KillStats`写入是否已有Authority exact
`InputHpConsumedTotal34C/InputScoreTotal348/KnockoutCount358`或native combo替代。缺替代者必须拆出
前置production package，禁止直接删除旧写入造成行为缺口。

## 只读结论

1. standard/reduced actual与HitPlan：HP consumed与score exact writer已存在，但lethal KO仍只写
   `holder.KillStat/world.KillStats`；必须先补同一attribution的`KnockoutCount358` actual+HitPlan。
2. type3/weapon ordinary damage：exact HP consumed已存在，Authority不写legacy entity/world stats；可在独立
   retirement包移除`ComboCountVic/DamageStats` actual+HitPlan。
3. kind10/11 flute：Authority只有weapon-count/frame/motion，不写`ComboCountAtk += 11`或
   `DamageStats += 11`；两套compat actual与HitPlan可随同上项退休。
4. held CPoint：当前writer仍只写legacy holder/victim/world stats，缺exact HP consumed/score/KO；必须先完成
   `NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001`，且该包仍受CPoint throw runtime gate约束。
5. input HP cost：formal `BattleCharacterActionWriter`已写exact HP consumed，但
   `TryInputFrameJump/TryCharacterDatInputFrameJumpCompatibility`仍只写`ComboCountVic`；需独立compat seam决定
   是补exact还是证明dead path后退休。
6. negative-WeaponCount recovery：legacy与data-oriented各只写`ComboCountVic += 9`，没有exact
   HP consumed/score/KO；其Authority source field仍归B4/B7 audit，不能在B5机械删除。
7. arrays/fields/store/snapshot/checksum/parity是route7 schema disposition；本审计不触碰。

## 当前生产写入面

- `ComboCountVic`行为写入13处：input2、recovery2、HitPlan4、DamageWriter4、CPoint1。
- `ComboCountAtk`行为写入8处：HitPlan3、DamageWriter2、CPoint1、两套flute compat2。
- actual `KillStat`写入3处；HitPlan另有standard/reduced holder与world projection。
- actual `DamageStats`写入7处、`KillStats`写入3处；HitPlan保留对应projection carrier。
- 以上不含reset/copy/store/snapshot/checksum/parity和test/editor。

## 后继严格顺序

1. `NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001`：补同一standard-credit attribution的
   exact KO actual+HitPlan，正反gate、owner chain、lethal/nonlethal、slot reuse。
2. `NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001`：只退已有exact或Authority明确无写入的
   B5 target/flute legacy镜像。
3. `NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001`与negative-recovery owner audit：分别确认
   compat seam与B4/B7 source，不能混入第2包。
4. CPoint throw runtime gate通过后，执行已定义的B6 held accounting；随后才能完成全局
   `NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-001`及route7用户方向。

## 修改与回滚

本审计只修改治理文档，不修改C#、content、Scene、Prefab、ProjectSettings或Authority。回滚仅移除本记录，
不得恢复已验证的+0x2F4 correction。
