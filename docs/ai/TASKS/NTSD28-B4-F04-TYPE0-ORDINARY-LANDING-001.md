# Task Contract — NTSD28-B4-F04-TYPE0-ORDINARY-LANDING-001

> 状态：`VERIFIED / TYPE0_ORDINARY_SINGLE_BODY / STATE12_18_PENDING`

## 目标

扩展type0 mechanics result承载effective floor，并让exact `LF2Character` 与transformed/shared
character在ordinary strict crossing上调用单一正式landing body。

## 不变量

- 仅state非12/18且`Landed` strict edge进入；already-grounded不重触发。
- 顺序固定：clamp effective floor、Vy=0、post-friction Vx/3，再按state100→94、action212或state6→215、hit_g、219选action并reset counter。
- state12/18现有路径、environment/credit/hit-motion、airborne selector、Audio/effect不改。
- 更正SelfCheck中把state13高速度落地固定为旧Frozen伤害/反弹的历史断言；按Authority ordinary action219且HP不变。
- 不改Authority、Scene、DAT、资源或snapshot schema。

## 验收

test-first覆盖exact/shared negative floor+hit_g、94/215优先级与grounded noop；编译、focused、
related、broad、SelfCheck、Scene、Ledger闭合。

## 回滚

删除single body/focused test，恢复exact/shared旧landing handler；无数据迁移。

## 验证结论

- test-first red `f6bbe369c56c41f5a1b5afd3a04c8219`：2/6（exact/shared hit_g630→219）。
- focused `44a5158f1a27420c8761b1182a504f31`：6/6；related `2939ca82e36646fb9c4ab67cb25b54ff`：77/77。
- 首轮 broad `0a5ca87c2bb44332b534dd3a2446bdb1` 504/504；SelfCheck依次捕获旧state13 Frozen damage与raw-frame185期望并按Authority更正。
- SelfCheck `2026-09-05T07:08:54.3112667Z` PASS；最终 broad `a5b1390277f84ee4afaad8817287e26b` 504/504。
- Scene不变、Console 0 error、Ledger PASS。
