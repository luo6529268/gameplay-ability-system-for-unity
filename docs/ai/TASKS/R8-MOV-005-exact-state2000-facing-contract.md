# R8-MOV-005 — exact character state2000 facing contract

> 日期：2026-08-23  
> 状态：`IN_PROGRESS`  
> D-ID：`D-MOV-005`  
> Change ID：`R8-MOV-005-001`

## Goal

把 C++ Release `frame_tick` 对 `state == 2000` 的通用 facing写入补齐到Unity exact character ECS
路径，使 exact与fallback在相同current-DAT/state/Vx下得到同一逻辑朝向；不得使用角色、OID或技能特判。

## Scope

- `BattleEcsCharacterFrameTickPass.ExecuteExactCharacter`：在C++对应时点按`Vx > 0 ? right : left`写朝向；
- `FrameAdvanceRuntimeSnapshotEditorTests`：增加type0 exact state2000的positive/zero/negative Vx矩阵，并保留fallback证据；
- 更新Change Record、Ledger、STATE、all-diff register、主计划与handoff。

## Authority / Evidence

- C++ `src/entity/frame_advance.cpp:884-887`：state2000无条件写`facing=(vx>0)?0:1`；
- C++ Makefile包含`frame_advance.cpp`；
- Unity fallback `LF2Entity.RunCommonFrameTick`已有同一规则；
- exact pass接管current type0但缺少该branch；当前正式type0 DAT不可达不否定通用source合同。

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterFrameTickPass.cs`
- `Assets/NTSD/Scripts/Test/Editor/FrameAdvanceRuntimeSnapshotEditorTests.cs`
- 本Task、Change Record、Ledger、STATE、register、主计划、handoff

## Deliverables

1. exact path通用state2000 facing写入；
2. Vx正/零/负矩阵；
3. fresh compile、focused/full self-check与治理证据；
4. 明确当前正式DAT仍只在type2/type4 fallback可达。

## Verification

- exact type0 state2000：Vx>0为right，Vx==0和Vx<0为left；
- wait/transition/counter顺序不变；
- fallback现有规则不改；
- Unity compile 0 error、full self-check PASS、validator和diff check PASS。

## Stop conditions

- 修复需要改变frame transition、physics、input、render或pass order；
- `SwitchDir`会反写非表现逻辑真值以外的状态并破坏worker；
- 回归失败指向本项以外合同且无法最小闭合。

## Out of scope

其他movement、weapon state1002/3000、landing、hit、collision、lifecycle、F1/F2 debug、C++ executable。
