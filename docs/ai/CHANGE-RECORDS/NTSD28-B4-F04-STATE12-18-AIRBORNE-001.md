# NTSD28-B4-F04-STATE12-18-AIRBORNE-001 — state12/18 airborne selector

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-STATE12-18-AIRBORNE-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4State1218AirborneEditorTests.cs
authority: NTSD 2.8-Logan PhysicsIntegrator28 state12/18 airborne selector and BattleWorld28 upcoming resource phase; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-COMPILE-RED-CS0103 / COMPILE0 / INTERMEDIATE16-OF-17-FIXTURE-CORRECTED / FOCUSED17 / RELATED99 / NTSD28-BROAD521 / SELFCHECK-PASS-2026-09-05T07:26:33Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / SINGLE_AIRBORNE_SELECTOR / LANDING_TRANSACTION_PENDING`

边界来自 `NTSD28-B4-F04-STATE12-18-AIRBORNE-AUDIT-001`。只改airborne selector；
contact/environment transaction后置。回滚见Task Contract。

导入focused tests后编译得到`CS0103`缺少pure kernel；这是预期test-first接口红灯，尚不作为
行为红灯。下一实现kernel/result/single owner后运行全部17项。

首次实现job `d1cd19b497814d06bcba512e787a1284` 16/17；唯一失败的WeaponCount夹具
post-gravity Vy恰落普通182区间，无法区分普通selector与错误override。只把初始Vy改为-5，
使普通结果181、错误override仍182；production不改。

## 实际改动

- pure kernel锁定state12两个action族与strict post-gravity bands、negative Env320 phase override、state18 `>1` gate。
- type0 result新增同调用栈`Airborne`，negative collision floor contact不再被绝对y<0误判。
- exact/shared production共用single selector；registered world读取`(NativeResourcePhase12+1)%12`，
  direct无world入口仅用规范化tickIndex fallback。
- selector raw-write action且不reset AttackingCounter；旧duplicate promotion方法保留但退出production owner。
- landing/environment/credit/hit-motion transaction未改。

## 验证

- focused `4434df25145e4f24b846f7a1488d0d7c`：17/17。
- related `f9f2c07dd9bd441689f5019e6afc1751`：99/99。
- broad `d8efede34c6343e4ba25968697ff664e`：521/521。
- SelfCheck `2026-09-05T07:26:33.8812585Z` PASS。
- Scene `0D74E174...D77 / 203477 / 2026-09-04T13:12:45.1526434Z`不变；Console0。

## 未关闭

state12/18 contact-side landing/environment/credit/hit-motion、collision-Y producers、legacy retirement与
B10 Audio仍待；不得宣称F-04/B4完成。
