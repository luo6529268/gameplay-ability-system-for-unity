# Task Contract — NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001

> 状态：`VERIFIED / RED_4_FAIL_4_PASS_OF_8 / FOCUSED_8_OF_8 / B6_CATEGORY_65_OF_65 / NTSD28_191_OF_191 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / TARGETED_PLAY_8_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED`
> 依赖：
> - `NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001 / VERIFIED`
> - `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001 / VERIFIED`

## 目标

对齐 `BattleWorld28::settle_catch_relations()` 在hurtable条件命中后的vaction提交与二次preflight：
先按signed/zero语义写caught action，再从新action重新解析frame与首个CPoint；frame缺失或非kind2时
立即结束当前catcher settlement，禁止继续injury、position与cover副作用。

## Authority 合同

- settlement进入前仍要求catcher当前state9/kind1、exact reciprocal与caught当前frame首CPoint kind2。
- `hurtable==0`或`hurtable==1 && caught.motion_hold_timer==0`时，无条件提交
  `native_relation_action(vaction, caught.facing)`；`vaction==0`也必须写action0，负值翻转caught facing后取绝对值。
- action提交后必须重新解析caught frame与其第一个CPoint；缺frame、无CPoint或kind非2均立即`continue`。
- preflight失败时已提交的action/facing保留，但不得写HP/HPBound、resource/stat、frame counter、hold timer、
  X/Y/Z、cover facing或其他settlement尾副作用。
- hurtable条件未命中时不得读取vaction目标作二次preflight，继续使用已经验证的当前kind2 frame。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs`
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchSettlementVactionPreflightProductionEditorTests.cs`
  及其 `.meta`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本 Change 的治理文档与恢复入口。

## 不变量

- 不实现held injury exact resource/accounting、CPoint 27-scalar schema、drain/gain或content切换。
- 不改变mixed advance、settlement pass位置/slot顺序、exact reciprocal、正常injury/position/cover算法。
- 不把Authority diagnostic success flag移植成新的Unity gameplay分支；本包只对齐可观察状态与terminal边界。
- 不改Scene、Prefab、Config、PNG/WAV/importer、RNG、shutdown或HitPlan（settlement为actual-only）。
- warmed 4096次必须0 managed allocation。

## Test-first 验收

1. RED：missing vaction frame、existing no-CPoint、existing non-kind2三类均证明旧Unity错误继续injury/position。
2. signed negative与zero vaction必须证明action/facing提交语义；有效kind2目标继续正常settlement。
3. hurtable条件未命中时，无效vaction不得误阻断当前kind2 settlement。
4. preflight失败保留relation/action/facing与sentinel副作用，且不写HP/HPBound/counters/position/cover。
5. compile、focused、B6/NTSD28/PreInteraction/HitPlan回归、SelfCheck、targeted Play、Console、Scene与Ledger
   按实际证据推进；synthetic Play不得冒充当前content自然事件。

## 回滚

只反向恢复本包settlement vaction提交/preflight与新增测试；不得回退前置mixed/exact relation包或用户文件。
