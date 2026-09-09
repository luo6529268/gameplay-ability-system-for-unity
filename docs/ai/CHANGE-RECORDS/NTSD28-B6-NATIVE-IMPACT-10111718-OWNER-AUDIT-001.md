# NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan SimulationTickDriver28 special-candidate dispatch and BattleWorld28::resolve_special_relation_hit kind10/11/17/18 atomic impact transaction; system weapon_flute_sky 201/202; EXE B1E13AE1, closure 39DDDA15.
evidence: complete character/object gate and write sets frozen; Unity dispersed actual plus HitPlan compared; missing environment/catch-source/impact-source carriers and kind17/18 dispatch confirmed; exact divide-vs-multiply and forbidden WeaponCount/legacy stats identified; current/release ITR counts 0/0/0/0 and 121/61/0/0; release Tayuya180 + Pain-d1 + OID838 type3 1; three packages defined; runtime blocked; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / RELEASE_CORPUS_WITNESS / PRODUCTION_HELD`

Authority impact事务已闭合：kind11读取`EnvironmentState320`，17/18分别限制character/object；character
先双owner preflight，再写environment=-20、encoded catch-source、physical impact-source、respond action；
object按type与weapon_flute_sky表裁决；两类均以精确`/1.07`阻尼并执行Y尾。事务不写WeaponCount、legacy
stats、rest、delay、HP或RNG。

Unity只识别10/11，错误用WeaponCount作gate并写-20及+11统计，漏三项exact字段与owner preflight，HitPlan
也没有相应carrier。release有121/61条正式记录，current为0/0；17/18两端均无producer。后续按HitPlan
carrier→pure core→actual+HitPlan atomic三包实施；现有B6 runtime栈未清，本轮无脚本/content/Scene改动。

## CORPUS CORRECTION（2026-09-08）

current kind10/11/17/18纠正为15/5/0/0，10/11均在OID36 Tayuya243..247。三包owner保留，
10/11已current reachable，不再依赖release内容导入后才能Play。
