# Task Contract — NTSD28-B2-NATIVE-TYPE0-AIR-DASH-REDIRECT-BUILTINS-001

> 状态：`FOCUSED_TEST_PASS / GROUND_AIR_CORE_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / NATIVE-TYPE0-BUILTINS`  
> 建立日期：2026-09-04

## 目标

按NTSD 2.8-Logan正式playable `FUN_00412E80`闭合Unity action215、state4、state5、state85/86
和action182/188 rowing redirect的built-in核心。实现保持Unity数据与对象适配，但动作优先级、当前键/
buffer门、朝向、直接动作写、资源策略、frame counter、速度和同tick重入必须与authority一致。

本包只建立可直接测试的writer核心，不接入`NTSD28InputTwoPassModule`或legacy resolver切换；ground+air
统一生产接线另建包，避免在air覆盖未闭合前形成混合所有权。

## Authority 与锁定语义

- `source/ntsd28_core/src/simulation/input_routing.cpp:726-805,938-990,1333-1491`属于playable构建闭包。
- `source/ntsd28_core/tests/input_routing_tests.cpp:1861-2233`锁定state4/5、action215、state85/86、
  rowing redirect可观察结果。
- action215：current+buffer defend先直写102；current+buffer jump按当前水平键或既有Vx选择213/214并
  写dash X/Y；depth dash独立且始终处理；随后重读新frame并允许同tick进入state5 normalization。
- state5：先按exclusive水平键定向，再以当前朝向/Vx规范到213/214（匹配侧216/217保护）；forward且
  当前J按住即可触发，无需新edge。interaction0对action90使用adjusted、afford-before-write MP策略；
  linked x01用jump_attack，kind4 mixed direction与kind6 any direction用sky_light_throw，直写、counter归0、
  `Vy -= 1`。
- state4：仅`Y < groundY`；exclusive水平键定向；只要求attack buffer。interaction0直写action80并使用
  clamped adjusted MP；x01 neutral用jump_attack/30，x01 directional及4/6用sky_light_throw/52；重启动作。
- state85/86：均按exclusive水平键定向；state85仅在action非0且Vx相对新朝向为forward时action+1；
  state86不推进。
- action182/188：要求`EnvironmentState320 >= 0`、current jump、positive jump edge、HP>0、
  `ComboState[8] != 1`；分别读取action100/108的raw MP，不adjust、不更新`InputMpConsumedTotal350`；
  不足则拒绝。目标100/108由朝向与原Vx选择，counter归0，Vy向上限clamp，Vx按±1阈值和
  rowing_distance重定向。
- built-ins common prelude每次只衰减一次run accumulator；不得添加ObjType gate。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0AirDashRedirectBuiltinsEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改`NTSD28InputTwoPassModule`、legacy resolver、ground core、environment producer/raw projection、
Config/DAT、Scene/Prefab、ProjectSettings、Packages或authority。不得把direct-test core写成production-connected。

## 不变量

- direct built-ins不写`InputLastAction144`、不受`InputActionLock130`阻断；只有authority明确要求时清
  `AttackingCounter`。
- action90使用adjusted MP且成功扣费才更新累计；rowing使用raw MP且永不更新普通累计。
- stale action215 jump/defend buffer没有current键时不得触发；其exclusive depth仍可更新Vz。
- state5攻击读取current J而非edge；state4读取edge而不额外要求current J。
- state4/5 linked字段必须使用已完成的数据carrier与exact fallback，不回退到旧C#规则。
- `EnvironmentState320`为独立carrier；不得用`Unk328`替代。
- 本包不改变production pass顺序、Config/DAT/资源、Slot模型或用户批准的Unity表现例外。
- warm focused route保持0 B。

## 验收

- test-first红灯覆盖缺失air core入口或enum selector；
- focused tests覆盖上述所有正/负路径、priority、resource、motion、counter、facing、same-tick和0 B；
- compile、ground回归、B2相关回归、snapshot/checksum、SelfCheck、Console0、Ledger/diff通过；
- 状态最多为`GROUND_AIR_CORE_READY / PRODUCTION_UNCONNECTED`，生产接线另包完成。

## 回滚

移除air/dash/redirect入口及私有helper、linked enum两项和新测试；不触碰已闭合ground core、
environment carrier或数据carrier。

## 当前证据

- 2026-09-04：新增10个focused tests后请求Unity编译；仅因
  `BattleCharacterActionWriter.RouteNativeAirDashRedirectBuiltins`不存在而产生CS1061，test-first红态成立。
- 生产实现后Unity脚本编译最新段0 error；新air 10/10、ground+air 22/22、NTSD28 group159/159、
  CharacterInput37/37、snapshot/checksum/ring31/31全部通过。
- `BattleRuntimeSelfCheck`于2026-09-04 16:15:04生成PASS；7条预期负向日志清理后Console error=0。
- `git diff --check`和`Validate-ChangeLedger.ps1`通过（137 records、93 governed code files）。
- `NTSD28InputTwoPassModule`与legacy resolver未修改；真实生产路径仍未接入本core。
