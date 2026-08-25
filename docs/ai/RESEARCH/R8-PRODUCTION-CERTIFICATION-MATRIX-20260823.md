# R8 production certification matrix

> 日期：2026-08-23
> 状态：`OPEN / CURRENT-WORKTREE EVIDENCE COLLECTION`

## Fixed boundaries

- C++ authority只读：`J:\QQFile\NTSD2.4\ntsd_release`；
- production保持CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5 visual scale、fixed-world camera；
- Authority400只用于C++同槽合同；MobileExtended保持1050 slots/1000 active；
- DesktopExtended保持无固定产品cap、prebattle page reservation、sealed strict 0 B与overflow确定性拒绝；
- 30 Hz逻辑tick、FrameInputSet、SoA/ECS、pool/worker/0-GC边界不回退；
- T8默认stage.dat与Android真机排除；R1-WP02 full trace保持BLOCKED。

## Evidence matrix

| Certificate | Current-worktree evidence | Status | Cannot prove |
|---|---|---|---|
| Unity compile | 03:28基线clean；R8-TEST-001/002后Editor DLL fresh且dotnet 0 errors | PASS（test scripts） | gameplay正确性 |
| Full self-check | full suite后同域07:31:17、强制域重载后fresh 07:32:39 PASS | PASS | 未覆盖Play Mode路径 |
| EditMode regression | job `6a6336d0e1e94abd9585110358012ca5` 1357/1357 PASS，0 failed/skipped | PASS | 真实输入手感与像素表现 |
| Core movement/input Play Mode | 用户报告组合失败；source已定位C++ by-ref combo persistence与Unity local transaction discard差异D-INP-010 | D-INP-010 VERIFIED / D-INP-006 MANUAL EDGE PENDING | real-scene InputSystem L/S/K→DDJ1/2/3→frame271、objects8→20；L/D/J→DRA1/2/3→frame263、objects7→8；final self-check/EditMode/validator均PASS。用户实体键盘/窗口焦点edge仍独立。 |
| Interaction/opoint/lifecycle Play Mode | 01 live type0/1/3/5 birth/cursor/reuse PASS；02 live type1/2/4/6 pickup/held/throw/landing/no-immediate-hit PASS；focused/self-check/validator PASS | 01/02 VERIFIED（Unity S4）；03 APPROVAL PENDING；04～06 PENDING | full C++ trace、extended>399 Play、grab/CPoint/hit/death仍未覆盖 |
| CentralOnly visible rendering | 三个通用source差异已写入；all-DAT descriptor 5537/0差异；GPU probe 5537 entries/84,327,319 pixels/hash相同/0差异；visible partial dynamic Mesh 562/562 mean/max0/0；cleanup/focused/self-check PASS | RUNTIME_PENDING / GPU S4 VERIFIED | 真实Game/Scene挂点/层级/可见性、loaded data无state8000 authored witness、C++ full trace |
| 1000 active capacity/performance | 2026-08-20历史U9仅作baseline；待current-build复跑 | PENDING | C++ 400-slot行为权威 |
| Windows Mono Player | 历史通过；待current-build复跑 | PENDING | IL2CPP一致性 |
| Windows IL2CPP Player | 历史通过；待current-build复跑 | PENDING | 完整战斗行为 |
| C++ full trace | R1-WP02观察方式未确认 | BLOCKED | 不得由Unity证据替代 |

`B-R8-MCP-001`已解除：新Editor会话恢复并完成exact/class/full/self-check。下一阶段进入Play Mode/Player；
自动基线仍不能替代这些证据。

`R8-PLAY-001`已关闭gizmo异常污染（fresh compile、07:40:57 self-check、Play 15秒0 error/warning）。
随后用户报告的组合键失败属于独立`D-INP-006 / R8-INP-01`，不由自动基线或历史单序列注入覆盖。
继续只读authority后，physical crossed mapping已闭合为正确适配；确定first difference转为`D-INP-010`。
修复合同`R3-COMBO-01`已建立但尚未改脚本，R8-WP01B保持FAILED/PENDING。

## First execution order

1. 清空非项目MCP探测日志并确认Console clean；
2. fresh scripts refresh/compile；
3. full self-check；
4. current-worktree EditMode基线；
5. 只有基线全绿后进入真实Play Mode与Player；
6. 发现失败时将tick/pass/slot/field或可见对象绑定到现有D-ID；没有D-ID则新增差异项。

## Historical evidence boundary

`Assets/NTSD/Docs/unified-battle-u9-final-acceptance-20260815.md`记录的2026-08-20结果证明当时工作树曾在
Windows Player上达到五组1000实体约60 FPS、0 B/0 GC和central draw=1，并完成Mono/IL2CPP correctness。
R2～R7之后代码已经变化，因此该报告只能作为回归基线，不能直接关闭本次R8。
