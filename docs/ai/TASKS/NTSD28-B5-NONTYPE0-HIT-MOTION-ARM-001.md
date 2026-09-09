# Task Contract — NTSD28-B5-NONTYPE0-HIT-MOTION-ARM-001

> 状态：`VERIFIED / TYPE1_6_HIT_MOTION_ARM_ALIGNED`
> 依赖：`NTSD28-B5-NONTYPE0-ENCODED-JOIN-001 / VERIFIED`

## 目标

在type1/2/3/4/5/6 confirmed unarmored hit中，于reaction之后、horizontal response之前接入
已验证的`ArmNativeUnarmoredHitMotion`。

## Authority 合同

- arm位于type6 damage/status skip块之外，因此type1..6全部执行。
- 同一strict bypass：`Fall==80 && -5<Vx<5 && itr.dvx==0`。
- type1/2/4/6 insertion：`ApplyWeaponDamage`的hurt/reaction之后、horizontal之前。
- type3/5 insertion：`ApplySpecialObjectHurtTail`的hurt/reaction之后、horizontal之前。

## 不变量

- 复用既有arm helper，不复制算法。
- type6仍不执行encoded/status/join RNG；arm本身不消费RNG。
- 不改Config/DAT/Scene/Authority/资源/snapshot schema或各类型专用post-hit tail。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5NonType0HitMotionArmEditorTests.cs`

## 验收

test-first behavior red；覆盖type1..6 armed production、type6 arm-with-status-skip、type6
stabilization bypass；相关完整hit、精确NTSD28 broad、SelfCheck、Scene/Console/Ledger。

## 回滚

移除weapon/special hurt seam的两个arm调用与focused test；保留type0 arm和encoded/join。

## 验证结论

- test-first behavior red：armed 7项失败、strict bypass负例1项通过。
- fresh compile 0 error；focused `e166e420bcb44278a564cbaba7296d70` 8/8。
- 完整hit related `d212caf385584d58bf2f648013a586d6` 241/241。
- 精确NTSD28 broad `b485759c256d4804a94b1fa6dd797f7a` 606/606。
- BattleRuntimeSelfCheck `2026-09-05T11:45:00Z` PASS；Scene unchanged。
