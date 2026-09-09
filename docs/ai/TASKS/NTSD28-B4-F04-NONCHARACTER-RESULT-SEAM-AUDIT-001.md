# Task Contract — NTSD28-B4-F04-NONCHARACTER-RESULT-SEAM-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / SHARED_FIRST_SPLIT`

## 结论

`CharacterMechanics.WeaponDynamics`目前只有`bool crossedGround + oldVy`，会丢失Authority落地分支
需要的`previousPreciseY`、`contactY`、`effectiveFloorY`与strict penetration结果。直接改bool语义会同时
影响shared non-character、derived `LF2WeaponBase.OnLanded`和旧direct SelfCheck，风险过大。

## 实施顺序

1. 新增无分配`BattleNonCharacterMechanicsStepResult`和reference-aware core；保留旧bool wrapper。
2. 先让`RunSharedNonCharacterDatFrameAdvance`消费新result，闭合type1/2/4/6 shared landing坐标。
3. derived path另包将contact result穿过`RunFrameAdvancePhysics`/`OnLanded`，再统一owner。
4. type3/OID999因`contactY<-9`独立包。

旧bool wrapper不得被误当作Authority strict predicate；它只保留direct compatibility直到全部caller迁移。

