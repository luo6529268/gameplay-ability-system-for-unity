# Task Contract — NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001

> 状态：`VERIFIED / RED_0_OF_4 / PRODUCTION_CHILD_SCAN_REMOVED / FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELF_RESET_PRESERVED / CURRENT_ITACHI_1250_COVERED / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`
> 依赖：`NTSD28-B3-LEGACY-STATE501-TRANSFORM-RETIREMENT-001 / VERIFIED`

## 目标

保留Unity late-lifecycle对当前实体`1100..1299`的action0与`runtime_state_code=1100-code`映射，同时退休
Authority不存在的`KillCount == owner physical slot`全世界child `HitStun`传播。

## Authority与reachability

- `BattleWorld28::resolve_pending_lifecycles_range()`对1100..1299只写当前active实体：
  `runtime_state_code=1100-code`、action/tick snapshot=0、清pending/code；不遍历其他slot、不读owner。
- Direction-B normalized projection有1个current witness：`Character/itachi.dat` action167/state9/next1250。
- Unity `BattleLateEntityLifecycleModule.HandleFrameTickExit()`先遍历world并写所有KillCount匹配child，再写self
  `HitStun=1100-frameId`与frame0；额外child写入当前可达。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B3LegacyLifecycleChildRetirementEditorTests.cs`及`.meta`
- 本Task/Change、Ledger、STATE、handoff与总表。

## 不变量

- self `1100-code`、frame0、pending/lifecycle清理及slot存活语义不变。
- 不改普通`<0 || >=857`释放、state501、CPoint、KillCount其他reader/writer、runtime carrier或schema。
- 不改content、Scene、Prefab、ProjectSettings、Authority、RNG或pass顺序。

## 验收

1. RED覆盖1100/1200/1250/1299：self结果保持Authority值，KillCount匹配child与相同初始状态的不匹配child
   经过各自正常late update后结果相同，且frame/identity均不被owner lifecycle改写；必须含current Itachi 1250 witness。
2. production只删除world traversal与child HitStun write；运行focused、相关late-lifecycle回归、build、SelfCheck、
   目标Play、Console/Scene与Ledger validator。
3. full SelfCheck若仍被更早CPoint阻塞须明确记录；本包不冒充完整B3或full parity。

## 回滚

只恢复`HandleFrameTickExit`中本Change删除的KillCount child traversal及对应旧断言；不得回退self lifecycle逻辑。

## 实施与验证（2026-09-09）

- 新增1100/1200/1250/1299四组test-first矩阵：self必须得到`1100-code`和frame0；相同初始状态的
  KillCount匹配/不匹配child在各自正常late update后必须得到相同HitStun，identity/frame/slot不变。
- 02:25:46 +08最终RED为`0/4`；unmatched child正常由41推进到40，旧matched child分别被额外改成
  `0/-99/-149/-198`。1250行即Direction-B Itachi action167当前见证。
- production只从`BattleLateEntityLifecycleModule.HandleFrameTickExit()`删除owner slot获取、world entity遍历和
  matched child `HitStun=1100-frameId`；self写入、frame0、snapshot refresh、存活与其他release路径未改。
- SelfCheck GT-08把matched child sentinel设为41，并要求它只执行自身late update至40；不再以1100/default0
  偶然相等掩盖额外写入。
- fresh builds：`Assembly-CSharp.csproj` 0 error/47 warning；`Assembly-CSharp-Editor.csproj`
  0 error/104 warning。Unity focused于02:27:19 +08通过`4/4`。
- 02:34:43 +08最终真实`NTSD_Battle` Play通过：cases=4、selfResets=4、childOwnerWritesAbsent=8、
  currentItachi1250=true。Play前后active Scene dirty=false/root=13，Scene SHA-256保持
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`，清空后的Console error=0。
- 02:29:05 +08 full SelfCheck仍提前停在独立既有CPoint mode0 victim-Vz断言，未到GT-08；
  该失败不被写成通过，专项Play直接覆盖GT-08目标语义。
- static grep只剩self `entity.HitStun=1100-frameId`与frame0；已无`other.KillCount==ownerSlot`或child write。
  本包只关闭B3该legacy child propagation；严格下一包为
  `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001`。
