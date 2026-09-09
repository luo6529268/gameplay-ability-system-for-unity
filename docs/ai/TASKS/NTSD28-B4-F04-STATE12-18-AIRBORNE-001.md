# Task Contract — NTSD28-B4-F04-STATE12-18-AIRBORNE-001

> 状态：`VERIFIED / SINGLE_AIRBORNE_SELECTOR / LANDING_TRANSACTION_PENDING`

## 目标

新增无状态state12/18 airborne selector，并由exact/shared type0 production在mechanics后调用，
使用result Airborne、EnvironmentState320与upcoming NativeResourcePhase12。

## 不变量

- action write不reset AttackingCounter；非state12/18不处理。
- state12/18 landing/environment/credit/hit-motion transaction不改。
- ordinary single body、all-nonchar、producer、Audio/content不改。
- 不新增snapshot字段；Airborne只存在同调用栈result。
- Authority、Scene、DAT、资源不改。

## 验收

test-first覆盖pure strict bands、registered world upcoming phase、Env320而非WeaponCount、negative-floor
grounded noop、exact/shared与state18；focused/related/broad/SelfCheck/Scene/Ledger闭合。

## 回滚

删除kernel/test并恢复两套旧promotion调用；无数据迁移。

## 验证结论

- test-first compile red：CS0103 missing kernel。
- 首次实现16/17，唯一失败为不可区分普通182/错误override的夹具输入；仅将初始Vy改为-5后，focused `4434df25145e4f24b846f7a1488d0d7c` 17/17。
- related `f9f2c07dd9bd441689f5019e6afc1751` 99/99；broad `d8efede34c6343e4ba25968697ff664e` 521/521。
- SelfCheck `2026-09-05T07:26:33.8812585Z` PASS；Scene不变、Console0、Ledger PASS。
