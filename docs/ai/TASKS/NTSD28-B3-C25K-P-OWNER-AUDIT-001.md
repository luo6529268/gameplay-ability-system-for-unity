# Task Contract — NTSD28-B3-C25K-P-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`
> 依赖：`NTSD28-B3-C25I-ARMOR-RECOVERY-001 / VERIFIED`

## 目标

只读闭合当前 NTSD 2.8-Logan C25k～p 的逐槽调用顺序、Unity生产owner、结构副作用和内容依赖，形成可独立验证的实施拆分；不以旧2.4/C#尾段替代当前权威。

## 权威事务

同一动态升序live-slot内固定执行：frame-zero OPoint（k）→state18/19 broken-weapon particles（l）→`previous_action_078=action`（m）→weapon pieces（n）→pending lifecycle resolve（o）→仅未被o消费的活体type0 healing（p）。k/l/n创建的更高slot可在同tick继续执行完整C25；o消费slot后不得执行p。

## 结论

- C25p已有正确HealTimer/CatchTimer/HP载体和近似算法，但错误地位于loop后的`EntityPostFrameTailAll`全局scan；可先独立迁回逐槽尾部。
- C25k已有counter-zero gate和即时structural writer，但当前被early frame exit/death-opoint前置影响，且额外character FrameDelay gate需要后续行为核验。
- C25l与Unity `SpawnLateTransitionEffects`部分重合，但混入非C25l的state13/200 branch，且执行晚于cleanup；必须先拆方法与RNG/slot证据。
- C25m绑定`Frame.Prev`已由B0证明，但当前commit晚于cleanup/tail；移动前必须拆开仍读取旧Prev的transition/input消费者。
- C25n只实现broken sound+pending destroy，缺built-in OID999、DAT weapon_piece和精确RNG/slot分配；受B7/B10/B11/H约束。
- C25o当前`HandleFrameTickExit`位于k前，11xx/12xx reset与terminal free过早；正式pending carrier仍缺，不能把`PendingFlushDestroy`伪装为native terminal pending。

## 实施拆分

1. `NTSD28-B3-C25P-HEALING-OWNER-001`：只迁移现有healing算法到逐槽survivor尾部，并从global post-tail移除重复owner。
2. C25k/m：先建立terminal classification与旧Prev reader边界，再归位frame-zero OPoint和previous-action commit。
3. C25l：独立拆出state18/19 branch，验证global-delay gate、sync RNG tuple与高slot birth。
4. C25n/o：在B7生命周期carrier与B10/B11/H资源依赖闭合后实现weapon pieces和逐槽消费。

## 不做

本审计不修改C#、Config/DAT、PNG/WAV、Scene、Prefab或Authority；不改变用户批准的随机掉武器例外。
