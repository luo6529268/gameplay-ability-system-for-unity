# NTSD28 B5 Standard Hit Rest / Recover Crosswalk

## Authority

- EXE：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- playable closure：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- 核心：`battle_world.cpp:3854-3920`；caller为initial matching pair `6630`与普通unarmored tail `6819`。

## Exact contract

| 输出 | gate/formula |
|---|---|
| attacker hold | 当前hold>=0，ITR recover非1/3，attacker definition effect非2/3时，写`max(0,3-reduction)` |
| target hold | ITR recover非2/3，target definition effect非2/4时，写`min(0,-3+reduction)` |
| arest | `arest<4 && vrest==0`固定4；否则arest>1时`max(1,arest-reduction)`，0/1原值 |
| vrest | 仅输入>0；先转native `uint8`，byte>1时`max(1,byte-reduction)`，否则byte原值 |
| world reduction | `0..5` clamp；战前菜单cursor4+Defend循环0..5，GameSession建World时传入；默认0 |

## Carrier/reachability audit

| 项 | Authority runtime | Unity | 结论 |
|---|---|---|---|
| ITR recover | typed int，missing默认0；405个正式decoded DAT无显式recover | `InteractionArea`无typed字段；rawProperties可保留但Copy/HitPlan/consumer不读 | missing carrier |
| definition `<bmp> effect` | typed definition bmp property，missing默认0；正式405 DAT无非零 | `LF2CharacterData`与converter无字段；冻结138 DAT也无非零 | missing carrier |
| timing reduction | world int clamp0..5；正式menu可选，default0 | world/runtime/snapshot/checksum均无字段 | confirmed observable difference for nondefault setting |
| arest/vrest byte | authority正式ITR显式vrest为3/10/15/20/30，均在byte范围 | 冻结Unity ITR显式arest15/16/20、vrest1/5/10/17/20，均在byte范围 | default0/current content byte cast无首差；规则仍需实现 |
| actual/HitPlan | single authority owner被2个caller复用 | DamageWriter多条路径与HitPlan多处重复raw 3/-3/arest/vrest | shared resolver/integration required |

## Implementation order

1. `NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001`：typed ITR recover、definition effect、world
   timing-reduction 0..5及copy/parser/snapshot/checksum/parity；无行为接线、无selection UI。
2. 后续pure resolver：输出attacker/target hold、arest/vrest；覆盖recover/definition effect/reduction/uint8边界。
3. 后续actual/HitPlan integration：统一character/weapon/special/other与initial early branch，删除重复公式。

完整原生选择流程是用户明确排除项；本计划只提供battle runtime的确定性world carrier和可配置入口，不实现菜单。
Direction B下不覆盖任何Config DAT。

## Carrier implementation result

`NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001 / VERIFIED`已闭合typed ITR recover、definition effect、world
timing reduction及其parser/copy/HitPlan fingerprint/reset/snapshot/restore/checksum/parity，schema为8/15/18。
自动证据为focused5、related74、B5+HitPlan383、exact104/758、SelfCheck/Scene/Ledger PASS。行为仍未接线，
下一pure resolver。

## Pure resolver result

`NTSD28-B5-STANDARD-HIT-REST-PURE-RESOLVER-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`已将
recover、双方definition effect、0..5 reduction、arest特例和native uint8 vrest语义收敛到allocation-free
`BattleStandardHitRestResolver`。证据为red20、focused21（含warm 4096 zero-allocation）、B5+HitPlan404、
exact105/779、SelfCheck/Scene/Ledger PASS。actual与HitPlan生产调用者尚未接入，下一包统一替换重复公式。

## Production integration result

`NTSD28-B5-STANDARD-HIT-REST-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION_CONNECTED /
STANDARD_REST_ALIGNED`已用两个薄适配器把四类actual owner、initial matching early branch与六个HitPlan
standard投影统一接到pure resolver；alternate/reduced/armor保持独立。证据为red7、focused7、
B5+HitPlan411、NTSD28 broad692、SelfCheck/Scene/Ledger PASS。下一步重新审计standard-rest family退出条件。

## Exit audit

`NTSD28-B5-STANDARD-HIT-REST-EXIT-AUDIT-001 / VERIFIED / STANDARD_REST_FAMILY_EXIT_READY`确认Authority
仅有initial matching与normal unarmored两个caller；Unity四个actual分支和六个HitPlan投影均由一个适配器调用
同一pure resolver。残留固定公式属于OID300/kind9、alternate/reduced/armor或unreachable legacy tail。
standard-rest specific family允许退出，下一B5 owner为armor/reduced-hit审计。
