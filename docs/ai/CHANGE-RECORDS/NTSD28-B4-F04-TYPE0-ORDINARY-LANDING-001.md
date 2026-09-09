# NTSD28-B4-F04-TYPE0-ORDINARY-LANDING-001 — type0 ordinary landing

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-TYPE0-ORDINARY-LANDING-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4Type0OrdinaryLandingEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan PhysicsIntegrator28 ordinary type0 landing branch; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-RED2-OF-6 / COMPILE0 / FOCUSED6 / RELATED77 / BROAD504 / SELFCHECK-OLD-STATE13-FIXTURES-CORRECTED / SELFCHECK-PASS-2026-09-05T07:08:54Z / FINAL-BROAD504 / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / TYPE0_ORDINARY_SINGLE_BODY / STATE12_18_PENDING`

边界来自 `NTSD28-B4-F04-TYPE0-ACTION-AUDIT-001`。只做ordinary single body；state12/18及
environment transaction不改。回滚见Task Contract。

focused red job `f6bbe369c56c41f5a1b5afd3a04c8219`：2/6失败，exact/shared均
expected hit_g630、actual219；94/215优先级和grounded noop四项通过。红灯精确定位duplicate
ordinary handler的hit_g遗漏，不扩大为已正确部分的失败。

实现后focused6/6、related77/77、broad504/504；SelfCheck `2026-09-05T07:03:34Z`
捕获shared state13高速度落地仍期待旧Frozen伤害/反弹，exact同类断言也存在。当前Authority将
state13归ordinary分支，故两条夹具应改为action219、HP/HPBound不变；不恢复旧特判。

首次更正后SelfCheck `2026-09-05T07:06:23Z`继续捕获raw-frame矩阵中exact/shared state13
仍期待action185并保留counter29；同一矩阵项按ordinary action219/counter reset0更正，raw writer
的PN/wait保留断言继续执行。

## 实际改动

- `BattleMechanicsStepResult`在同调用栈携带effective floor，不新增snapshot字段。
- exact `LF2Character` 与 transformed/shared character在strict ordinary landing edge进入同一body。
- single body保留negative floor，写Vy0、Vx/3，并严格按state100→94、action212/state6→215、
  frame hit_g、fallback219选action与reset counter。
- state12/18仍走原路径；airborne selector、environment/credit/hit-motion、Audio/effect未改。

## 最终验证

- focused `44a5158f1a27420c8761b1182a504f31`：6/6。
- related `2939ca82e36646fb9c4ab67cb25b54ff`：77/77。
- final broad `a5b1390277f84ee4afaad8817287e26b`：504/504。
- SelfCheck `2026-09-05T07:08:54.3112667Z` PASS。
- Scene `0D74E174...D77 / 203477 / 2026-09-04T13:12:45.1526434Z` 未变化；Console0。

## 未关闭

state12/18 airborne selector、contact-side environment/credit/hit-motion transaction、collision-Y producers、
legacy owner retirement与B10 Audio仍待；不得宣称F-04/B4完成。
