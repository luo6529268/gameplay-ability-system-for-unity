# R8-WP01C-01 — opoint birth / lifecycle runtime evidence

> 日期：2026-08-23  
> 证据层：`S4 Unity production Play Mode`  
> C++ full trace：`BLOCKED（R1-WP02）`

## Environment

- Unity：国际版 `2022.3.62f3`；
- scene：`Assets/NTSD/Scene/NTSD_Battle.unity`；
- presentation：当前production CentralOnly，dedicated simulation worker active；
- probe：`R8-OPLIFE-001 / BattleOpointLifecyclePlayModeProbeEditor`；
- result：`Temp/NTSD_R8_WP01C_01_OpointLifecycle.result.json`，09:05:09；
- Play tick：356→359；
- baseline/final：object 6→6、claimed slots 4→4、object pool active 2→2、logic pool active 4→4；
- cleanup：`true`，0 cleanup errors；Play结束后Console 0 error/warning。

## Four-type birth matrix

| OID | DAT type | CLR | Producer slot | Spawn slot:generation | frame/runtime | Prev2/runtime | Pool ownership | Release |
|---|---:|---|---:|---|---|---|---|---|
| 33 | 0 | LF2Character | 52 | 53:1 | 0/0 | 0/0 | CentralOnly logic pool +1；renderer false | old handle rejected，pools/world restored |
| 120 | 1 | LF2Weapon | 52 | 53:3 | 0/0 | 0/0 | CentralOnly logic pool +1；renderer false | old handle rejected，pools/world restored |
| 203 | 3 | LF2SpecialAttack | 52 | 53:5 | 0/0 | 0/0 | CentralOnly logic pool +1；renderer false | old handle rejected，pools/world restored |
| 999 | 5 | LF2OtherObject | 52 | 53:7 | 0/0 | 0/0 | CentralOnly logic pool +1；renderer false | old handle rejected，pools/world restored |

所有birth的`SpawnSemantic=LateOpoint(1)`、object delta=+1、actual DAT type与definition一致。每次release后
slot53立即失效；下一次最低空闲slot仍为53，但generation按1→3→5→7前进，没有旧handle复活。

## Scan cursor witnesses

### High-slot same-pass

- producer slot52；newborn slot53:generation9；
- creation tick357；
- creation tick结束时`attacking=1`；
- 结论：newborn位于当前late scan cursor之后，已在同一pass后续被访问。

### Low-slot next-pass

- producer slot53；预先释放低位filler形成slot52 hole；newborn slot52:generation5；
- creation tick358结束时`attacking=0`；
- consumer tick359结束时`attacking=1`；
- 结论：newborn位于当前cursor之前，创建pass不回扫，下一tick才执行。

## Regression evidence

- fresh Unity compile：新脚本导入，`Assembly-CSharp-Editor.dll` 09:01:25晚于源码，Tundra success，0 error；
- focused EditMode：job `3b8e08105d0946bca58d88e5ed6ef990`，
  `W05OpointLifecycleEditorTests` 8/8 PASS；
- full `BattleRuntimeSelfCheck`：09:06:51 PASS；
- Play Mode result：PASS；
- production gameplay/factory/pool/pass/DAT/scene没有修改。

## Scope conclusion

本证据把`D-OP-001`四类birth-history从S3推进到Unity S4，并为`D-SCHED-012`提供当前production低位动态
slot的high/low cursor S4 witness，同时补充R5-LIFE-01B的release/generation reuse S4证据。它没有覆盖：

- MobileExtended/DesktopExtended >399 的真实Play cursor（仍只有joint fixture）；
- pickup/held/throw、grab/CPoint/link、hit/damage、death/respawn；
- CentralOnly像素/阴影/排序（WP01D）；
- C++ executable full trace（继续BLOCKED）。

因此只能裁决`R8-WP01C-01`，不能宣称整个WP01C、R8或完整C++ runtime战斗对齐完成。
