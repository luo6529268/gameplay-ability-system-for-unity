# R7-FRAME-01 — FrameTick / recovery data-oriented pass 重新认证预检

> 日期：2026-08-22  
> 状态：`READ_ONLY_PREFLIGHT / NO_NEW_SCRIPT_CHANGE`  
> 前置：`R7-PERF-001`仍为唯一活跃代码包；本文件不授权并行修改脚本。

> 2026-08-22认证更新：后续`R7-FRAME-001`已按no-code conditional certification执行；fresh focused
> `FrameAdvanceRuntimeSnapshotEditorTests`为22/22 PASS，warmed 0 B与20:42:47 full self-check均PASS。
> current asset/state2000与identity两项watch也已完成；仍保留INFERRED/UNKNOWN边界。

## 1. C++ authority

- `Makefile:11-35`将`game_tick.cpp`、`frame_advance.cpp`纳入release build；
- `game_tick.cpp:140-173`定义late frame tick之前的character recovery：period12 HP/negative weapon injury、
  period3 PP、step-wait gate、kill/PP/hit-stop gate及current `char_data->oid` 51/52半HP rate；
- `game_tick.cpp:584`按slot调用`frame_tick`；
- `frame_advance.cpp:802-995`定义exact顺序：frame-delay gate→attack_exempt→negative link→current frame/
  kind2→special drain→hit/fall/confirm counters→frame/wait sound与attacking→state0/state2000/state14→
  wait/next→caught-exit hit-stop→jump212→PP/turn→110/114/202 tail→wait_counter。

## 2. Unity optimized mapping

### 2.1 Recovery

`BattleEcsCharacterRecoveryPass.Execute`只接管CLR exact `LF2Character` + current type0 DAT；其它shell回退
virtual route。period12/period3与C++常量、step gate、negative weapon injury、HP/HPBound clamp、combo+9及PP
公式顺序一致；非周期tick的`ProvenNoOp`只跳过C++同样无writer的tick。

C++ 51/52判断读取current `char_data->oid`；Unity读取`entity.ObjectId`。已审计的production runtime identity
switch（state transform、TryApplyRuntimeIdentity、CPoint transform、child propagation）均在加载新FrameCache时
同步写`ObjectId`，当前未找到exact character产生identity分离的production writer。该点为`INFERRED safe`，
不是C++ trace证明；未来若允许exact shell identity与current DAT分离，必须改读current DAT identity并重开合同。

### 2.2 FrameTick

`BattleEcsCharacterFrameTickPass`只接管exact character/current type0 DAT。除已登记`D-MOV-005`外，当前
source mapping如下：

- frame-delay、attack-exempt、negative link、null/kind2 gate顺序一致；
- exact character不进入C++ special-DAT HP drain或type2 state2000→frame20分支；
- counter、frame-vs-wait sound、attacking reset/increment顺序一致；
- state0 below-ground→raw212且禁止jump init；state14 death hit-stop按kill/team/slot gate一致；
- next999对type0以`YInt!=0`选212，否则0，并禁止jump init；signed next先翻向再raw frame；
- previous wait frame state14、new state13 exception、difficulty/mode/current OID skip由现有helper保持；
- explicit next212读取本tickcurrent keys写jump velocity；PP/PpDisplay与turn使用同一负mp合同；
- frame110/114、202及wait-counter tail一致；
- C++ `jump_init_pending/suppress_jump_init`在有效静态DAT的一次调用内使用并在tail清零；Unity局部
  `allowJumpInit/suppressJumpInit`对当前有效DAT路径等价。缺frame导致的C++字段残留没有当前可恢复DAT writer，
  仅能标`INFERRED`。

## 3. 已知未关闭项

- `D-MOV-005`不变：C++对任意state2000按Vx写facing；optimized exact path漏该branch。当前Unity/C++
  inventory中literal state2000只出现在type2/type4 weapon，均走fallback，因此现状仍是
  `INFERRED current exact route not reachable`，不是已修复或VERIFIED；
- `ObjectId`与current DAT oid的未来分离风险；
- invalid/mutable DAT与jump flags的可恢复性当前UNKNOWN；
- existing data-oriented-vs-legacy Editor tests只能证明两条Unity路径在已有fixture一致，不能单独定义C++规则；
- C++ full trace、真实PlayMode及current asset之外的mod DAT均未验收。

## 4. Existing evidence

- `FrameAdvanceRuntimeSnapshotEditorTests`已有recovery periodic/no-op/fallback、FrameTick transition/counter/
  derived fallback与warmed 0 B矩阵；
- `BattleRuntimeSelfCheck`覆盖frame212、state14、PP display、current-key lifetime、raw-frame history等分支；
- 这些历史证据在R7-PERF-001新程序集刷新前不重新计为fresh R7证据。

## 5. Decision / future package

本预检没有发现除`D-MOV-005`外的新confirmed source difference，不修改脚本。完成R7-PERF-001后，若
进入`R7-FRAME-001`，应作为no-code/conditional certification运行现有focused tests并补两项最窄watch：

1. exact character current DAT oid51/52与shell identity一致性；
2. 当前DAT inventory继续不存在type0 state2000。

任一watch失败时必须另建独立Change Record；不得把本预检写成Frame/AI SoA整体已对齐。AI sensing/
decision kernel不在本文件范围，必须单独审计。
