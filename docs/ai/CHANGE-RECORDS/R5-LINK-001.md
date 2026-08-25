# R5-LINK-001 — positive-link invalidation preserves forward slot fields

<!-- CHANGE-RECORD
id: R5-LINK-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsPositiveLinkValidationPass.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsPositiveLinkValidationPassEditorTests.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1828-1845
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-STATIC-MISMATCH / UNITY-COMPILE-PASS / FULL-SELFCHECK-PASS / FOCUSED-EDITMODE-PASS / CPP-TRACE-AND-PLAYMODE-PENDING
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — 已完成合同内最小改动，并取得Unity compile、full self-check和focused EditMode证据；C++ trace与真实Play Mode仍未关闭。

## 差异

C++ T11 invalid positive-link branches只写`link_state=0`。Unity Legacy和DataOriented writer额外清
`TargetSlotIndex`和`HeldWeaponStableId`，并把此extra behavior写入shadow expected和tests。

## 允许代码路径

| 文件 | 符号 | 允许内容 |
|---|---|---|
| `SimulationQueryAndLinkModule.cs` | `RunLegacyPositiveLinkValidation` | 删除invalid branch的两个extra clear。 |
| `BattleEcsPositiveLinkValidationPass.cs` | `CaptureExpected`、`ExecuteDataOriented` | invalid expected和writer仅写LinkState。 |
| `BattleRuntimeSelfCheck.cs` | `CheckValidatePositiveLinksMatrix` | 更新invalid正向link字段保持断言。 |
| `BattleEcsPositiveLinkValidationPassEditorTests.cs` | positive-link mode/parity tests | 验证Legacy/Shadow/DataOriented字段一致及event witness。 |

## 禁止扩大

不改negative link、target reverse fields、CPoint/WeaponSync、held process/release、slot/generation、scheduler、input、AI、render、DAT/scene/resource或C++ authority。

## 验收门槛

代码写入后必须同步Ledger/STATE/full diff/main plan/handoff，并实际运行ledger validator、diff check、Unity compile、full self-check与focused EditMode tests。最高状态为`RUNTIME_PENDING`。

## 本次代码写入

- `RunLegacyPositiveLinkValidation` 无效分支只写`LinkState=0`，保留`TargetSlotIndex`和`HeldWeaponStableId`；
- `BattleEcsPositiveLinkValidationPass` 的DataOriented writer与ShadowCompare expected改为同一字段保持语义；
- `CheckValidatePositiveLinksMatrix` 覆盖无效slot、inactive target和reciprocal mismatch的前向字段保持；
- focused Editor tests锁定 Legacy / ShadowCompare / DataOriented parity、live-link路径和structural witness的`AfterTargetSlot`/`AfterHeldWeaponSlot`。

## 实际验证

| 层级 | 实际操作 | 结果 |
|---|---|---|
| 留痕 | `Tools/Validate-ChangeLedger.ps1` | PASS；22个governed diff均被Record覆盖，R5四个路径归`R5-LINK-001`。 |
| 文本差异 | R5范围的`git diff --check -- <R5 paths>` | exit 0；只输出LF→CRLF提示。最终全工作区重跑为exit 1，但仅报告用户已有`Assets/NTSD/Scene/NTSD_Battle.unity`的trailing whitespace；没有R5路径差异错误，未触碰场景。 |
| Unity编译 | 当前已打开的Unity 2022.3.62f3经UnityMCP `refresh_unity(mode=scripts)`，随后`read_console(filter=error CS)` | 0条C# compiler error。 |
| 完整自检 | 菜单`NTSD/验证/运行战斗运行时自检` | `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 07:32:40为`PASS`；Console仅含两个既有rest-binding negative control。 |
| 聚焦EditMode | UnityMCP `run_tests(EditMode, NTSD.Test.BattleEcsPositiveLinkValidationPassEditorTests)`，job `edc22b2fd5314fb685c59d1b04f97c7a` | 8/8 passed，0 failed，0 skipped，0.675s。 |

该证据只证明Unity脚本和聚焦测试闭环；C++ runtime trace、同场景first-difference和真实Play Mode尚未取得，本记录不得写为“已对齐”。
