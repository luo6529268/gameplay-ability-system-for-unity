# Task Contract — NTSD28-B4-F04-STATE12-18-ENVIRONMENT-CREDIT-001

> 状态：`VERIFIED / DAMAGE_CREDIT_COUNT_ALIGNED / B8_EVENT_DEFERRED`
> 依赖：`NTSD28-B4-F04-STATE12-18-CONTACT-ACTION-001 / VERIFIED`

## 目标

在state12/18 effective-floor contact的action事务之前，接入NTSD 2.8-Logan environment damage、
source decode、最多两跳owner credit、score与lethal knockout count写入；B8 knockout event feed仍后置。

## Authority 合同

- 仅type0 state12/18 effective-floor contact且`EnvironmentState320!=0`触发；先damage，后action。
- damage先绝对值；`IncomingDamageScale340>0`时改为`damage*100/scale`整数除法。
- `CatchSourceSlot90>=0`才解析credit；值`>=0x2000`先减`0x2000`，随后最多沿`OwnerSlotIndex`两跳；下一跳不存在时保留当前credit。
- lethal前置为victim HP>0且HP-damage<=0且credit存在；只在此时credit `KnockoutCount358++`。
- victim HP、HPBound和InputHpConsumedTotal均减/加同一final damage；credit InputScoreTotal348加同一值；最后EnvironmentState320写1。

## 不变量

- 本包不创建knockout event list/sequence/source-object/four-owner记录；该可观察事件属于B8。
- 不写world KillStats/DamageStats，不改action选择、status producer或hard-motion kernel。
- invalid/missing credit不阻止victim damage；owner最多两跳，不扩成递归。
- 非state12/18、airborne、cpoint kind2、EnvironmentState320==0均不触发。
- 不修改Authority、Scene、DAT、资源、Prefab、ProjectSettings或网络协议。

## 验收

test-first覆盖abs/scale、encoded source、invalid credit、两跳/缺失owner、nonlethal/lethal/already-dead、
exact/shared、contact gate与action前写入；compile、focused/related/NTSD28 broad、SelfCheck、Scene/Console/Ledger闭合。

## 回滚

移除environment transaction method及两个production调用与focused test；contact action/carrier保持不变。

## 验证结论

- test-first behavior red：1/7，仅non-contact gate通过，其余6项因事务缺失失败。
- fresh compile error0；focused `d5a0e8a3e419457ba07d85aaefcc8b00` 7/7。
- related `ae1683de8f654facaa48d21922b4f152` 71/71；精确NTSD28 broad `f74a0c6baff14a3cb54e1d96dba63262` 558/558。
- SelfCheck `2026-09-05T08:45:39Z` PASS；Scene/Console/Ledger通过。
- B8 knockout event feed与B5 status producer仍未实现，不包含在本完成结论中。
