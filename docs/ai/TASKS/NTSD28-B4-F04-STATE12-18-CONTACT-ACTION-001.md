# Task Contract — NTSD28-B4-F04-STATE12-18-CONTACT-ACTION-001

> 状态：`VERIFIED / CONTACT_ACTION_SINGLE_OWNER / ENVIRONMENT_DAMAGE_DEFERRED`
> 依赖：`NTSD28-B4-F04-HARD-MOTION-CONSUMER-001 / VERIFIED`

## 目标

为exact/shared type0 production接入state12/18 effective-floor contact事务：覆盖already-grounded contact、
soft 230/231与hard 185/191/picked/picking动作、速度处理、counter reset和status defer/consume。

## Authority 合同

- state12/18只要本tick在effective floor contact且未被cpoint kind2抑制就进入事务；不限于严格crossing landing。
- hard条件：state18，或post-physics Vy严格`>11`，或Vx严格`>9`/`<-9`。
- state12 soft：Vy=0、Vx/=3、current action `<186`选230，否则231；reset frame counter；status gain保持待后续hard消费。
- hard先写Vy=-3.5并把Vx钳到[-7,7]。gain==1时调用已验证hard-motion consumer；state12且current>=186选picked，其余选picking。gain!=1时，state12 current>=186选191，其余选185；不reset counter。
- action使用进入事务前的current action与frame state。

## 不变量

- 本包不处理contact前的EnvironmentState320 damage、HP/HPBound、score、KO count/event；下一独立事务补齐。
- 非state12/18、airborne与cpoint kind2路径保持现状；ordinary landing single body不变。
- exact LF2Character与shared type0走同一production owner；不新增snapshot字段。
- 不接B5 producer或B8 event，不改Authority、Scene、DAT、资源、Prefab或ProjectSettings。

## 验收

test-first覆盖pure contact flag、already-grounded、soft边界与defer、hard阈值/无gain/gain动作选择、
consumer后速度与清理、exact/shared同源；compile、focused/related/NTSD28 broad、SelfCheck、Scene/Console/Ledger闭合。

## 回滚

移除result contact flag、production method/call与focused test；carrier及pure consumer保持不变。

## 验证结论

- test-first compile red：`CS1061`缺少effective-floor contact result。
- fresh compile error0；final focused `1b1cc44b273643b7800f8fdbfdff638f` 9/9。
- related `0e1940ff4c214f18bbae491b7f97fa3b` 64/64；精确NTSD28 broad `264eca5ec3864fc6a1a5530d062a8743` 551/551。
- SelfCheck先后捕获shared/exact旧WeaponCount landing damage夹具；只更正为不得把WeaponCount当environment damage，最终`2026-09-05T08:32:06Z` PASS。
- Scene/Console/Ledger通过；environment damage/credit仍明确后置。
