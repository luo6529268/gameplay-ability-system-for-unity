# R5-HOLD-003 — held throw must preserve `PickerStableId`

<!-- CHANGE-RECORD
id: R5-HOLD-003
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1597-1630,1977-2006;src\entity\frame_advance.cpp:215-271,695-735
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-SHARED-WRITER-MISMATCH / UNITY-COMPILE-PASS / FULL-SELFCHECK-PASS / CPP-TRACE-AND-PLAYMODE-PENDING
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — 已完成合同内唯一 writer removal与existing fixture扩展，并取得Unity compile和full self-check证据；C++ trace与真实Play Mode仍待。

## 差异

C++ reset默认 `picker_idx=-1`，normal pickup和两轮held throw不写它；其唯一正常后续写入在
frame-advance target selection。Unity shared `ThrowHeldWeapon` 却对type1/2/4/6全部写
`PickerStableId=holder runtime slot`。

## 允许代码路径

| 文件 | 符号 | 允许内容 |
|---|---|---|
| `LF2WeaponHeldStateResolver.cs` | private `ThrowHeldWeapon` | 删除唯一无条件PickerStableId writer。 |
| `BattleRuntimeSelfCheck.cs` | existing real held step12 fixture | 对type1/2/4/6输入不同picker sentinel并断言throw后保持，同时保留已有字段合同。 |

## 禁止扩大

不改 SpawnerEntityIndex、FrameDelay、ReleaseTick、PN/wait、random、OnThrown、pickup、target selection reader、
CPoint/WeaponSync、pass order、slot/generation、input、AI、collision、render、DAT、scene、resources、performance或C++ authority。

## 验收门槛

代码写入后必须同步ledger、STATE、full diff、main plan和handoff，并实际运行validator、scoped diff、Unity compile
和full self-check。最高状态为`RUNTIME_PENDING`；C++ trace和真实Play Mode不因本包而关闭。

## 本次代码写入

- 从 shared `ThrowHeldWeapon` 删除唯一无条件的 `PickerStableId=holder slot` write；
- existing real held fixture为type1、type4、type6、type2分别预置picker sentinel `71/72/73/74`，throw后断言保持；
- 同一fixture继续锁定R5-HOLD-001的type2 FrameDelay与R5-HOLD-002的type1/4/6 spawner stamp、type2 spawner sentinel；
- 未改 SpawnerEntityIndex、ReleaseTick、PN/wait、random、OnThrown、target reader / target selection或其它battle path。

## 实际验证

| 层级 | 实际操作 | 结果 |
|---|---|---|
| 留痕 | `Tools/Validate-ChangeLedger.ps1` | PASS；32个Change Record、25个已有governed脚本改动均有归属。 |
| 文本差异 | 本包路径的 `git diff --check -- ...` | exit 0；仅LF→CRLF提示。未触碰用户已有场景trailing whitespace。 |
| Unity编译 | 当前已打开Unity 2022.3.62f3经UnityMCP `refresh_unity(mode=scripts)`，随后`read_console(filter=error CS)` | 0条C# compiler error。 |
| existing focused fixture / full self-check | 菜单`NTSD/验证/运行战斗运行时自检` | `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 08:39:22为`PASS`；已执行type1/2/4/6 picker sentinel保持fixture。 |

本包没有新建独立EditMode class，而是扩展full self-check已经调用的真实held weapon fixture。C++ runtime trace、
same-scene first-difference和真实 Play Mode尚未取得，因此本记录只能是`RUNTIME_PENDING`，不是“已对齐”。
