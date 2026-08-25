# R5-HOLD-002 — type-2 held throw must not stamp `SpawnerEntityIndex`

<!-- CHANGE-RECORD
id: R5-HOLD-002
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1597-1630,1977-2006
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-SHARED-WRITER-MISMATCH / UNITY-COMPILE-PASS / FULL-SELFCHECK-PASS / CPP-TRACE-AND-PLAYMODE-PENDING
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — 已完成合同内最小 writer / fixture 改动，并取得 Unity compile 与 full self-check 证据；C++ trace 与真实 Play Mode仍待。

## 差异

C++ release 的 type `1/4/6` held `dvx` throw 写 child `spawner_slot=holder slot`；相邻 type `2` branch
不写该字段。Unity `LF2WeaponHeldStateResolver.ThrowHeldWeapon` 被两类 branch共享，却无条件写
`SpawnerEntityIndex=holder slot`。

## 允许代码路径

| 文件 | 符号 | 允许内容 |
|---|---|---|
| `LF2WeaponHeldStateResolver.cs` | `Act`、private `ThrowHeldWeapon` | 为现有 helper增加显式 stamp 条件；type `1/4/6` 写 spawner，type `2` 保持已有字段。 |
| `BattleRuntimeSelfCheck.cs` | existing real weapon held step12 fixture | 断言 type `1/4/6` stamp 与 type `2` preseed value 保持，同时保留既有 throw断言。 |

## 禁止扩大

不改 `PickerStableId`（已单独登记为`D-HOLD-003`）、FrameDelay、ReleaseTick、PN/wait、random、OnThrown、
link release、held pass顺序、CPoint/WeaponSync、target-selection reader、slot/generation、input、AI、collision、
render、DAT、scene、resources、performance或C++ authority。

## 验收门槛

代码写入后必须同步 ledger、STATE、full diff、main plan 与 handoff，并实际运行 ledger validator、本包范围
diff check、Unity compile与full self-check。最高状态为`RUNTIME_PENDING`；C++ trace和真实 Play Mode不因本包而关闭。

## 本次代码写入

- `LF2WeaponHeldStateResolver.Act` 对 type `1/4/6` 调用`ThrowHeldWeapon(..., stampSpawnerSlot: true)`，对type `2`
  调用同一helper但传`false`；
- helper仅在`stampSpawnerSlot`为真时写`SpawnerEntityIndex`，没有为type2添加任何重置；
- existing real held fixture现在断言type1、type4、type6写holder runtime slot，并以type2预置sentinel `77`断言
  no-write保持；它继续断言type2随机frame、FrameDelay、速度、link与throwing state；
- 本包没有修改`PickerStableId`、ReleaseTick、FrameDelay、PN/wait、random、OnThrown或任何reader。

## 实际验证

| 层级 | 实际操作 | 结果 |
|---|---|---|
| 留痕 | `Tools/Validate-ChangeLedger.ps1` | PASS；31个Change Record、25个已有governed脚本改动均有归属。 |
| 文本差异 | 本包路径的 `git diff --check -- ...` | exit 0；仅LF→CRLF提示。未触碰用户已有场景trailing whitespace。 |
| Unity编译 | 当前已打开Unity 2022.3.62f3经UnityMCP `refresh_unity(mode=scripts)`，随后`read_console(filter=error CS)` | 0条C# compiler error。 |
| existing focused fixture / full self-check | 菜单`NTSD/验证/运行战斗运行时自检` | `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 08:25:40为`PASS`；已执行type1/4/6 spawner stamp与type2 sentinel保持fixture。 |

本包没有新建独立EditMode class，而是扩展了full self-check已经调用的真实held weapon fixture。C++ runtime trace、
same-scene first-difference和真实 Play Mode尚未取得，因此本记录只能是`RUNTIME_PENDING`，不是“已对齐”。
