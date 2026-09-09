# NTSD28-B5-TYPE1-ARMOR-BREAK-VERTICAL-ORDER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE1-ARMOR-BREAK-VERTICAL-ORDER-001
status: VERIFIED
change-kind: TEST_FIRST_ORDER_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorBreakVerticalOrderEditorTests.cs
authority: NTSD 2.8-Logan battle_world.cpp resolve_confirmed_unarmored_hit lines 6793-6832 broken armor/vertical/post-hit order; EXE B1E13AE1, closure 39DDDA15.
evidence: red 5e7d050684b84dc8848ba9d74a8334de 2 expected failures/3; focused 192b5cd55a1240afa42ccb8e1cd9d081 3/3 pass; HitPlan f78f42d5c0a640179dfc420a0af9729b 184/184 pass; B5 c8b0f7625de44d32b923d860bdca6c44 356/356 pass; NTSD28 broad 173f12539be545f48bd1cac7cdd71381 821/821 pass; BattleRuntimeSelfCheck PASS 2026-09-06T03:01:45Z; Console only 7 intentional rest-binding negative-path errors; scene D4266C6D...583B unchanged; diff-check and ledger PASS.
-->

> 状态：`VERIFIED / BREAK_VERTICAL_POSTHIT_ORDER_ALIGNED`

退出审计确认只剩type1-specific broken fallback order残差。本包不改变数值选择或跨族尾。

## 实际修改

- `ApplyStandardFall(...)`新增明确defer参数；仅broken fallback命中时暂缓既有vertical mutation。
- actual在horizontal与`CompleteNativeBrokenArmorFallback`之后、attacker post-hit之前补交一次vertical。
- HitPlan把attacker post-hit投影移到broken/vertical块之后，与Authority和actual顺序一致。
- 非broken路径仍在原位置提交vertical，最终行为保持。

## 验证

- 首次测试`6f2bce6d7b9d43a69d83c00e7d6340f8`含一个错误的固定`-7.0`测试假设；按真实初值改为delta断言后，
  有效red `5e7d050684b84dc8848ba9d74a8334de`为2项顺序失败、最终状态1项通过。
- focused `192b5cd55a1240afa42ccb8e1cd9d081` 3/3、HitPlan
  `f78f42d5c0a640179dfc420a0af9729b` 184/184、B5
  `c8b0f7625de44d32b923d860bdca6c44` 356/356、broad
  `173f12539be545f48bd1cac7cdd71381` 821/821通过。
- SelfCheck PASS `2026-09-06T03:01:45.6260151Z`；Console仅7条已知负路径日志；Scene基线、diff-check、Ledger均PASS。

下一步为type1 armor exit audit 002；跨族resource/audio/spark/content仍不在本包。
