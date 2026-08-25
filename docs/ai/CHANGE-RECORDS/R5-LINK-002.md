# R5-LINK-002 — negative-link invalidation preserves child holder slot

<!-- CHANGE-RECORD
id: R5-LINK-002
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationQueryAndLinkModuleEditorTests.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1441-1457,1860-1872
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-STATIC-MISMATCH / UNITY-COMPILE-PASS / FULL-SELFCHECK-PASS / FOCUSED-EDITMODE-PASS / CPP-TRACE-AND-PLAYMODE-PENDING
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — 已完成合同内single-field writer、self-check与focused Editor test，并取得Unity compile、full self-check和focused EditMode证据；C++ trace与真实Play Mode仍未关闭。

## 差异

C++两轮invalid negative-held relation分支只写child `link_state=0`。Unity shared
`SimulationQueryAndLinkModule.HeldObjectProcessAll`额外写child `HolderStableId=-1`，从而在两个pass中都扩大了
invalid cleanup范围。

## 允许代码路径

| 文件 | 符号 | 允许内容 |
|---|---|---|
| `SimulationQueryAndLinkModule.cs` | `HeldObjectProcessAll` | 删除invalid relation branch的`HolderStableId=-1`。 |
| `BattleRuntimeSelfCheck.cs` | 新增或扩展negative-link invalidation fixture | 锁定child link清零和holder slot保持。 |
| `SimulationQueryAndLinkModuleEditorTests.cs` | focused EditMode tests | 锁定out-of-range、active-holder mismatch和第二pass不重清。 |

## 禁止扩大

不改first/second pass顺序、valid `RunStep12`、`BattleHeldObjectWriter`、release/throw、positive link、
CPoint/WeaponSync、slot/generation、scheduler、input、AI、collision、render、DAT/scene/resource或C++ authority。

## 验收门槛

代码写入后必须同步Ledger/STATE/full diff/main plan/handoff，并实际运行ledger validator、R5范围diff check、
Unity compile、full self-check与focused EditMode。最高状态为`RUNTIME_PENDING`。

## 本次代码写入

- `HeldObjectProcessAll` invalid branch仅保留child `LinkState=0`与runtime snapshot refresh；
- full self-check新增out-of-range holder、第二shared pass与active-holder target mismatch三种字段保持断言；
- 新增focused Editor tests，覆盖out-of-range holder跨两次pass和active-holder mismatch。

## 实际验证

| 层级 | 实际操作 | 结果 |
|---|---|---|
| 留痕 | `Tools/Validate-ChangeLedger.ps1` | PASS；23个governed diff均被Record覆盖，R5三条路径归`R5-LINK-002`。 |
| 文本差异 | R5范围的`git diff --check -- <R5 paths>` | exit 0；仅LF→CRLF提示。全工作区仍仅被用户已有场景trailing whitespace阻塞，未触碰场景。 |
| Unity编译 | 当前已打开Unity 2022.3.62f3经UnityMCP `refresh_unity(mode=scripts)`，随后`read_console(filter=error CS)` | 0条C# compiler error。 |
| 完整自检 | 菜单`NTSD/验证/运行战斗运行时自检` | `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 07:46:36为`PASS`；Console仅含两个既有rest-binding negative control。 |
| 聚焦EditMode | UnityMCP `run_tests(EditMode, NTSD.Test.SimulationQueryAndLinkModuleEditorTests)`，job `161af4674f524a388233e9e89865065c` | 2/2 passed，0 failed，0 skipped，0.499s。 |

该证据只证明Unity脚本和聚焦测试闭环；C++ runtime trace、同场景first-difference和真实Play Mode尚未取得，本记录不得写为“已对齐”。
