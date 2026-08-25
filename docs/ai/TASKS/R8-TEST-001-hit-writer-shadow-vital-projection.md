# R8-TEST-001 — hit writer ShadowCompare vital/stat projection sync

> 日期：2026-08-23
> 状态：`VERIFIED / DIAGNOSTIC-ONLY`
> Change ID：`R8-TEST-001`

## Goal

使`BattleEcsHitExecutionPlan`的writer-effect ShadowCompare投影覆盖已经由R4-HIT-001与R4-HIT-003
按C++ authority落地的type3 / normal weapon HP、HPBound、ComboCountVic与DamageStats写入；不修改
production damage writer。

## Observed failure

- current-worktree full EditMode job `20fcc884b4114ee9a1a3b7f1667c641c`：1357/1357执行完成，状态FAILED；
- failure list至少25项（MCP capped），全部位于`BattleHitExecutionPlanEditorTests` ShadowCompare；
- 统一`writerDiff=0x70000000000000`，即bit52/53/54：TargetHp、TargetHpBound/TargetPp、TargetComboCountVic；
- fresh-domain exact job `8d6f29aa8d8043958b29abcf58096e6e`：2/2 FAILED，同一mask；排除测试顺序污染。

## Authority / evidence

- R4-HIT-001：C++ `collision.cpp:561-585,644-918`、`hit.cpp:81-488,631-636`，type3 kind0写
  HP/HPBound/ComboCountVic/DamageStats；
- R4-HIT-003：C++ `collision.cpp:559-585`、`hit.cpp:107-167`，normal type1/2/4 weapon按
  FallDamageDiv调整后写vital/stat；type6/Drink不进入该子合同；
- production `BattleDamageWriter`已实现并由对应self-check覆盖；本包只同步shadow oracle。

## Scope

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsHitExecutionPlan.cs`
  - normal weapon projection：Light/Heavy/Throw写adjusted vital/stat，Drink不写；
  - type3 kind0 projection：standard、state-sync、D1 identity、active D1 identity写raw injury vital/stat；
  - non-converted kind9继续不写上述vital/stat；
- 不修改tests断言，先让现有red tests证明修复。
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
  - 仅修正`ShadowCompare_StandardType3DamageSupportsDeadAirTarget`的旧HP=0断言；kind0 injury10的C++
    vital合同要求实际HP=-10。其余断言不变。

## Acceptance

1. fresh exact two-case job PASS；
2. dead-air exact与`BattleHitExecutionPlanEditorTests`整类PASS；
3. full EditMode 1357项无失败；
4. fresh Unity compile 0 error；
5. full self-check PASS；
6. validator与scoped diff check PASS。

## Stop conditions

- mask出现52/53/54以外的新字段；
- existing production结果与R4 focused/self-check冲突；
- 需要改`BattleDamageWriter`、hit/collision pass、候选、关系、RNG、声音或生命周期；
- C++ source合同无法闭合。

## Out of scope

- Play Mode、Player、C++ runtime trace；
- 修改C++ authority；
- 更改damage数值、顺序或任何production gameplay。
