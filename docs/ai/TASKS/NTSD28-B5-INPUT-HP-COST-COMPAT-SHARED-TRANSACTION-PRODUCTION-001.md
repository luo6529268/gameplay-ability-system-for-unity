# Task Contract — NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001

> 状态：`VERIFIED / RED_0_OF_6 / FOCUSED_6_OF_6 / RELATED_INPUT_187_OF_187 / RELATED_B5_926_OF_926 / TARGETED_PLAY_6_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / TWO_INPUT_WRITERS_RETIRED / NEGATIVE_RECOVERY_AUDIT_NEXT`
> 来源：`NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001 / VERIFIED`。

## 目标与Authority

将registered Legacy/DataOriented及unregistered compatibility generic frame-action入口统一到已经实现的
`BattleCharacterActionWriter.ApplyNativeInputAction` exact transaction；退休两个重复的
`ComboCountVic += hpCost` writer且不复制cost/fallback算法。Authority为playable闭包中的
`input_routing.cpp:317-428 apply_action(...)`。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- 新focused test及必要的既有CharacterInput/SelfCheck断言
- 本Task/Change、Ledger、STATE、handoff、CURRENT-AUTHORITY与总表

## 不变量与排除

- single exact core保留lock、signed/999、redirect、resource gate、adjusted MP、frame hp、fallback、
  +0x34C/+0x350、HPBound、+0x144与facing顺序。
- registered wrapper按`Attempt.Applied`返回；unregistered入口调用同一无状态core，不允许per-action分配writer。
- caller继续拥有combo/edge clear与frame-counter；不改input sample、two-pass、RNG、release/builtin selector、
  B6 linked weapon action、negative recovery、held CPoint、content、Scene、Prefab、ProjectSettings或schema。
- `LF2Character.TryInputFrameJump`只能删除或转发，不能保留第二transaction。

## Test-first验收

1. registered Legacy/DataOriented与unregistered对同一redirect+cost case产生相同frame、PP、HP、HPBound、
   +0x34C/+0x350、+0x144与facing，legacy ComboVic哨兵不变。
2. fallback/lock/missing/999保留exact attempt/applied边界，fallback不扣费、不写+0x144。
3. source closure证明两个`ComboCountVic += hpCost`归零、compat helper无资源算法复制、single core无热路径分配。
4. focused、NativeComboAction、CharacterInput、B2/B5 related、compile、full SelfCheck、Legacy override Play、
   Console与Scene不变性。

## 回滚

恢复三个入口的旧转发/compat helper；不得回退既有native exact action transaction或其他B5 stats retirement。
